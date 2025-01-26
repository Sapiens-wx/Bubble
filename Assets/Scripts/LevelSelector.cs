using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class LevelSelector : MonoBehaviour
{
    public Transform center;
    public float radius, levelRadius, levelNormalScale, levelLargeScale;
    [Header("Animation")]
    public float animDuration;
    public float scalingDuration;
    [Header("Prefabs")]
    public GameObject level_unfinished;
    public GameObject level_finished;
    public GameObject level_collectable;

    public static LevelSelector inst;
    LevelInfo[] levels;
    int selectedLevel;
    Vector2[] points;
    Camera mainCam;
    bool mouseDown, insideRadius, lastFrameInsideRadius;
    Vector2 mouseWorldPos;
    Vector2 shootDir, shootOrigin;
    float shootDist;
    [HideInInspector] public bool insideBubble;
    [HideInInspector] public float actualRadius;
    Sequence shootSyncPosSequence, dieSeq;

    int SelectedLevel{
        get=>selectedLevel;
        set{
            if(selectedLevel!=value){ //value changed
                if(selectedLevel!=-1)
                    ScaleToValue(levels[selectedLevel], levelNormalScale);
                if(value!=-1)
                    ScaleToValue(levels[value], levelLargeScale);
            }
            selectedLevel=value;
        }
    }
    void ScaleToValue(LevelInfo level, float val){
        DOTween.To(()=>level.GetUIScale(), (value)=>level.SetUIScale(value), val, scalingDuration);
    }

    public float ShootDist{
        get=>shootDist;
    }
    public float ActualRadius{
        get=>actualRadius;
        set{
            actualRadius=value;
            #if UNITY_EDITOR
            if(CircleGen.inst!=null)
                CircleGen.inst.radius=value;
            #else
                CircleGen.inst.radius=value;
            #endif
        }
    }
    void OnDrawGizmosSelected(){
        Gizmos.DrawWireSphere(transform.position, radius);
        Gizmos.color=Color.green;
        Gizmos.DrawWireSphere(transform.position, levelRadius);
    }
    void OnValidate(){
        ActualRadius=radius;
    }
    void Awake(){
        inst=this;
    }
    // Start is called before the first frame update
    void Start()
    {
        mainCam=Camera.main;
        ActualRadius=radius;
        CircleGen.inst.anchor=center;
        CircleGen.inst.self=transform;
        UpdateLevelUI();
    }
    Vector2 MouseWorldPos(){
        return mainCam.ScreenToWorldPoint(Input.mousePosition);
    }
    /// <summary>
    /// center: transform.position; point: Input.mousePosition
    /// </summary>
    bool MouseInsideRadius(float r){
        Vector2 mousePos=mainCam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dir=mousePos-(Vector2)transform.position;
        float distSqr=dir.x*dir.x+dir.y*dir.y;
        return distSqr<=r*r;
    }
    /// <summary>
    /// center: transform.position
    /// </summary>
    bool IsInsideRadius(float r, Vector2 mousePos){
        Vector2 dir=mousePos-(Vector2)transform.position;
        float distSqr=dir.x*dir.x+dir.y*dir.y;
        return distSqr<=r*r;
    }
    /// <summary>
    /// center: transform.position
    /// </summary>
    bool IsInsideRadius(float r, Vector2 center, Vector2 mousePos){
        Vector2 dir=mousePos-center;
        float distSqr=dir.x*dir.x+dir.y*dir.y;
        return distSqr<=r*r;
    }
    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0)&&MouseInsideRadius(radius)){
            mouseDown=true;
            if(shootSyncPosSequence!=null && shootSyncPosSequence.IsPlaying()){
                shootSyncPosSequence.Kill();
                shootSyncPosSequence=null;
            }
        } else if(Input.GetMouseButtonUp(0)&&mouseDown){
            Shoot();
        }
    }
    void FixedUpdate(){
        if(mouseDown){
            mouseWorldPos=MouseWorldPos();
            //update shoot parameters
            shootOrigin=transform.position;
            shootDir=shootOrigin-mouseWorldPos;
            shootDist=shootDir.magnitude;
            shootDir/=shootDist;

            insideRadius=IsInsideRadius(radius, mouseWorldPos);
            if(!insideRadius&&lastFrameInsideRadius){ //the bubble begins to distort: compute origin
                //shootOrigin=transform.position;
            } else if(insideRadius&&!lastFrameInsideRadius){ //if the player changes from outside radius to inside radius, then reset the position of the bubble
                transform.localPosition=Vector3.zero;
                transform.localScale=Vector3.one;
            }
            //limit the drag distance
            if(!IsInsideRadius(radius, transform.position, mouseWorldPos)){ 
                mouseWorldPos=shootOrigin-shootDir*radius;
                shootDir=shootOrigin-mouseWorldPos;
                shootDist=shootDir.magnitude;
                shootDir/=shootDist;
            }

            //adjust rotation
            transform.eulerAngles=new Vector3(0,0,Vector2.SignedAngle(Vector2.right, shootDir));
            if(!insideRadius){ //distort
                transform.localPosition=(mouseWorldPos+shootOrigin+shootDir*radius)/2-(Vector2)transform.position;
                Vector3 scale=Vector3.one;
                scale.x=(shootDist+radius)/2/radius;
                transform.localScale=scale;
            }
            center.transform.position=mouseWorldPos;
            lastFrameInsideRadius=insideRadius;
        }
        DetectLevel();
    }
    void Shoot(){
        mouseDown=false;
        //animation
        if(shootSyncPosSequence!=null && shootSyncPosSequence.IsPlaying()){
            shootSyncPosSequence.Kill();
            shootSyncPosSequence=null;
        }
        shootSyncPosSequence=DOTween.Sequence();
        shootSyncPosSequence.Append(transform.DOScaleX(1, animDuration).SetEase(Ease.OutCirc));
        if(shootDist>radius)
            shootSyncPosSequence.Join(transform.DOLocalMove(Vector3.zero, animDuration));
        shootSyncPosSequence.Join(center.DOLocalMove(Vector3.zero, animDuration).SetEase(Ease.OutCirc));
        //load level if applicable
        if(selectedLevel!=-1 && SceneLoader.inst.levels[selectedLevel].tag>0){
            SceneLoader.inst.LoadLevel(selectedLevel, 0);
        }
    }
    void DetectLevel(){
        float radiusSqr=levelRadius*levelRadius;
        for(int i=points.Length-1;i>-1;--i){
            Vector2 dir=points[i]-mouseWorldPos;
            float distSqr=dir.x*dir.x+dir.y*dir.y;
            if(distSqr<radiusSqr){ //inside the level
                SelectedLevel=i;
                return;
            }
        }
        SelectedLevel=-1;
    }
    void UpdateLevelUI(){
        levels=new LevelInfo[SceneLoader.inst.levels.Length];
        points=new Vector2[levels.Length];
        Vector2 dir=Vector2.up;
        float deltaTheta=-Mathf.PI*2/levels.Length;
        for(int i=0;i<levels.Length;++i){
            Vector2 pos=dir*radius+(Vector2)transform.position;
            levels[i]=new LevelInfo(SceneLoader.inst.levels[i], level_unfinished, level_finished, level_collectable);
            levels[i].SetUIPos(pos);
            levels[i].SetUIScale(levelNormalScale);
            levels[i].SetState(SceneLoader.inst.levels[i].tag);
            points[i]=pos;
            dir=MathUtil.Rotate(dir, deltaTheta);
        }

    }
    class LevelInfo{
        public Level scene;
        public GameObject level_unfinished;
        public GameObject level_finished;
        public GameObject level_collectable;
        public LevelInfo(Level scene, GameObject level_unfinished, GameObject level_finished, GameObject level_collectable){
            this.level_unfinished=GameObject.Instantiate(level_unfinished);
            this.level_finished=GameObject.Instantiate(level_finished);
            this.level_collectable=GameObject.Instantiate(level_collectable);
            this.scene=scene;
            this.level_unfinished.gameObject.SetActive(false);
            this.level_finished.gameObject.SetActive(false);
            this.level_collectable.gameObject.SetActive(false);
        }
        public void SetUIPos(Vector3 pos){
            level_unfinished.transform.position=pos;
            level_finished.transform.position=pos;
            level_collectable.transform.position=pos;
        }
        public float GetUIScale(){
            return level_unfinished.transform.localScale.x;
        }
        public void SetUIScale(float scale){
            level_unfinished.transform.localScale=new Vector3(scale,scale,scale);
            level_finished.transform.localScale=new Vector3(scale,scale,scale);
            level_collectable.transform.localScale=new Vector3(scale,scale,scale);
        }
        /// <summary>
        /// 0: invisible; 1: unfinished; 2: finished; 3: finished and collected
        /// 0: invisible; 1: black hollow circle; 2: black hollow circle and white circle inside
        /// </summary>
        /// <param name="state"></param>
        public void SetState(int state){
            switch(state){
                case 0:
                    level_unfinished.gameObject.SetActive(false);
                    level_finished.gameObject.SetActive(false);
                    level_collectable.gameObject.SetActive(false);
                    break;
                case 1:
                    level_unfinished.gameObject.SetActive(true);
                    level_finished.gameObject.SetActive(false);
                    level_collectable.gameObject.SetActive(false);
                    break;
                case 2:
                    level_unfinished.gameObject.SetActive(false);
                    level_finished.gameObject.SetActive(true);
                    level_collectable.gameObject.SetActive(false);
                    break;
                case 3:
                    level_unfinished.gameObject.SetActive(false);
                    level_finished.gameObject.SetActive(false);
                    level_collectable.gameObject.SetActive(true);
                    break;
            }
        }
    }
}