using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CircleGen : MonoBehaviour
{
    public Bubble bubble;
    public GameObject linePrefab;
    public Transform lineParent;
    public int numOfPoints;
    public int numOfLines;
    [Header("line params")]
    public float lineWidth_v;
    public AnimationCurve lineWidthEase;

    public static CircleGen inst;
    [HideInInspector] [SerializeField] Line[] lines;
    float centerDist;
    void OnValidate(){
        InitLineRenderers();
        UpdateCircles();
    }
    void Start(){
        InitLineRenderers();
    }
    void Awake(){
        inst=this;
    }
    [ContextMenu("re init")]
    void InitCompletely(){
        for(int i=0;i<numOfLines;++i){
            lines[i] = Instantiate(linePrefab, lineParent).GetComponent<Line>();
            lines[i].lineRenderer.positionCount=numOfPoints;
            float width=lineWidth_v*lineWidthEase.Evaluate((i+1f)/numOfLines);
            lines[i].lineRenderer.widthCurve=AnimationCurve.Linear(0,width,1,width);
        }
    }
    void InitLineRenderers(){
        if(lines!=null){
            foreach(Line line in lines){
                if(line==null){
                    lines=new Line[numOfLines];
                    for(int i=0;i<numOfLines;++i){
                        lines[i] = Instantiate(linePrefab, lineParent).GetComponent<Line>();
                    }
                    break;
                }
            }
        }
        if(lines==null){
            lines=new Line[numOfLines];
            for(int i=0;i<numOfLines;++i){
                lines[i] = Instantiate(linePrefab, lineParent).GetComponent<Line>();
            }
        } else if(lines.Length<numOfLines){
            Line[] oldLines=lines;
            lines=new Line[numOfLines];
            Array.Copy(oldLines, lines, oldLines.Length);
            for(int i=oldLines.Length;i<lines.Length;++i){
                lines[i] = Instantiate(linePrefab, lineParent).GetComponent<Line>();
            }
        } else if(lines.Length>numOfLines){
            Line[] oldLines=lines;
            lines=new Line[numOfLines];
            Array.Copy(oldLines, lines, lines.Length);
            for(int i=lines.Length;i<oldLines.Length;++i){
                if(oldLines[i]!=null){
                    if(Application.isPlaying)
                        Destroy(oldLines[i].gameObject);
                    else
                        DestroyImmediate(oldLines[i].gameObject);
                }
            }
        }
        for(int i=0;i<numOfLines;++i){
            lines[i].lineRenderer.positionCount=numOfPoints;
            float width=lineWidth_v*lineWidthEase.Evaluate((i+1f)/numOfLines);
            lines[i].lineRenderer.widthCurve=AnimationCurve.Linear(0,width,1,width);
        }
    }
    void UpdateCircles(){
        centerDist=((Vector2)(bubble.center.position-bubble.transform.position)).magnitude;
        //0-numOfLines: inside to outside
        for(int i=0;i<numOfLines;++i){
            DrawEllipsePolar(i, (i+1f)/numOfLines, 1-((i+1f)/numOfLines));
        }
    }
    void DrawEllipsePolar(int lineIdx, float scale, float distortionDamp)
    {
        LineRenderer line=lines[lineIdx].lineRenderer;
        lines[lineIdx].Alpha=scale;

        float a_unscaled=transform.localScale.x;
        float a = Mathf.Lerp(transform.localScale.x,1,distortionDamp)*scale; // 长半轴
        float b = transform.localScale.y*scale;  // 短半轴
        float aa=a*a;
        float bb=b*b;
        float aabb=a*a*b*b;

        line.positionCount = numOfPoints + 1;
        for (int i = 0; i <= numOfPoints; i++)
        {
            float theta = 2 * Mathf.PI * i / numOfPoints;
            float cos=Mathf.Cos(theta), sin=Mathf.Sin(theta);
            float r = Mathf.Sqrt( aabb / (bb * cos*cos + aa * sin*sin));
            float xpos = r * cos;
            float ypos = r * sin;
            xpos+=(a-a_unscaled)*Mathf.Clamp01(centerDist/bubble.actualRadius);
            Vector2 pos=MathUtil.Rotate(new Vector2(xpos,ypos)*bubble.actualRadius, transform.eulerAngles.z*Mathf.Deg2Rad);
            line.SetPosition(i, transform.position+(Vector3)pos);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        UpdateCircles();
    }
}
