using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CircleGen : MonoBehaviour
{
    public Bubble bubble;
    public LineRenderer line;
    public int numOfPoints;
    void OnValidate(){
        DrawEllipsePolar();
    }
    void DrawEllipsePolar()
    {
        float a = transform.localScale.x; // 长半轴
        float b = transform.localScale.y;  // 短半轴
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
            Vector2 pos=MathUtil.Rotate(new Vector2(xpos,ypos)*bubble.radius, transform.eulerAngles.z*Mathf.Deg2Rad);
            line.SetPosition(i, transform.position+(Vector3)pos);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        DrawEllipsePolar();
    }
}
