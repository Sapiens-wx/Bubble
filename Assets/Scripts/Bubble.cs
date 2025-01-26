using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Cinemachine;

public class Bubble : MonoBehaviour
{
    public Transform center;
    public Player player; //normally, when the player is inside the bubble, 'player' is the child of 'center'
    public float radius0, radius, radius1, radius2;
    public float spd, shootSpd;
    [Header("Animation")]
    public float animDuration;
    public float shrinkDuration;
    [Header("Components")]
    public Rigidbody2D rgb;
    public CircleCollider2D circleCollider;
    public CinemachineTargetGroup cinemachineTargetGroup;
    [Header("Find Tune")]
    public int phy_steps;

    public static Bubble inst;
    Camera mainCam;
    bool mouseDown, insideRadius, lastFrameInsideRadius;
    float lastFrameShootDist;
    Vector2 mouseWorldPos;
    Vector2 shootDir, shootOrigin;
    float shootDist;
    [HideInInspector] public bool insideBubble;
    [HideInInspector] public float actualRadius;
    Sequence shootSyncPosSequence, dieSeq;

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
            circleCollider.radius=actualRadius;
        }
    }
    void OnDrawGizmosSelected(){
        Gizmos.DrawWireSphere(transform.position, radius);
        Gizmos.color=Color.green;
        Gizmos.DrawWireSphere(transform.position, radius1);
        Gizmos.color=Color.red;
        Gizmos.DrawWireSphere(transform.position, radius2);
        Gizmos.color=Color.black;
        Gizmos.DrawWireSphere(transform.position, radius0);
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
    public void Shrink(){
        DOTween.To(()=>actualRadius, value=>ActualRadius=value, radius0, shrinkDuration).SetEase(Ease.OutQuint);
    }
    public void Expand(){
        DOTween.To(()=>actualRadius, value=>ActualRadius=value, radius, shrinkDuration).SetEase(Ease.InOutBack);
    }
    // Update is called once per frame
    void Update()
    {
        if(!player.mouseDown&&Input.GetMouseButtonDown(1))
            player.OnReturnToBubble();
        if (insideBubble){
            if(Input.GetMouseButtonDown(0)&&MouseInsideRadius(radius)){
                cinemachineTargetGroup.m_Targets[0].weight=0;
                mouseDown=true;
                if(shootSyncPosSequence!=null && shootSyncPosSequence.IsPlaying()){
                    shootSyncPosSequence.Kill();
                    shootSyncPosSequence=null;
                }
                player.animator.SetTrigger("charge");
            } else if(Input.GetMouseButtonUp(0)&&mouseDown){
                Shoot();
            }
        }
    }
    void FixedUpdate(){
        if(mouseDown){
            mouseWorldPos=MouseWorldPos();
            //update shoot parameters
            shootOrigin=transform.parent.position;
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
            if(!IsInsideRadius(radius2, transform.parent.position, mouseWorldPos)){ 
                mouseWorldPos=shootOrigin-shootDir*radius2;
                shootDir=shootOrigin-mouseWorldPos;
                shootDist=shootDir.magnitude;
                shootDir/=shootDist;
            }
            //avoid player dragging through the wall
            float maxDist=MaxDistToWall_DirectRay();
            if(maxDist<shootDist){
                mouseWorldPos=shootOrigin-shootDir*maxDist;
                shootDist=maxDist;
            }

            Vector2 idealMousePos;
            maxDist=MaxDistToWall_Player(1, out idealMousePos); //calculates the max shootDist
            if(maxDist<shootDist){
                //DrawPoint(mouseWorldPos, Color.red);
                //DrawPoint(idealMousePos, Color.green);
                shootDist=maxDist;
                mouseWorldPos=idealMousePos;
                shootDir=(shootOrigin-mouseWorldPos).normalized;
            }

            //detect if shootDist<radius1 or not and get the moment when shootDist changes from <radius1 to >=radius1 and vice versa
            //the moment when shootDist changes from >=radius1 to <radius1
            if(shootDist<radius1&&lastFrameShootDist>=radius1){ //stop the animation
                FullyChargedEffect.inst.Stop();
            } else if(shootDist>=radius1&&lastFrameShootDist<radius1){ //play the animation
                FullyChargedEffect.inst.Play();
            }
            //adjust rotation
            transform.eulerAngles=new Vector3(0,0,Vector2.SignedAngle(Vector2.right, shootDir));
            if(!insideRadius){ //distort
                transform.localPosition=(mouseWorldPos+shootOrigin+shootDir*radius)/2-(Vector2)transform.parent.position;
                Vector3 scale=Vector3.one;
                scale.x=(shootDist+radius)/2/radius;
                transform.localScale=scale;
            }
            center.transform.position=mouseWorldPos;
            lastFrameInsideRadius=insideRadius;
            lastFrameShootDist=shootDist;
        }
    }
    void DrawPoint(Vector2 point, Color color){
        Debug.DrawLine(point+Vector2.one, point-Vector2.one, color);
        Debug.DrawLine(point+new Vector2(-1,1), point-new Vector2(-1,1), color);
    }
    void Shoot(){
        cinemachineTargetGroup.m_Targets[0].weight=1;
        mouseDown=false;
        //moves with bubble (player stays in the bubble)
        if(IsInsideRadius(radius1, transform.parent.position, mouseWorldPos)){ //move
            //animation
            player.animator.SetTrigger("idle");
            if(shootSyncPosSequence!=null && shootSyncPosSequence.IsPlaying()){
                shootSyncPosSequence.Kill();
                shootSyncPosSequence=null;
            }
            shootSyncPosSequence=DOTween.Sequence();
            shootSyncPosSequence.Append(transform.DOScaleX(1, animDuration).SetEase(Ease.OutCirc));
            if(shootDist>radius)
                shootSyncPosSequence.Join(transform.DOLocalMove(Vector3.zero, animDuration));
            shootSyncPosSequence.Join(center.DOLocalMove(Vector3.zero, animDuration).SetEase(Ease.OutCirc));
            //movement
            rgb.velocity=shootDir*(spd*shootDist/radius);
        } else{ //get out from the bubble
            //music switch for getting out of bubble
            AudioManager.instance.OutBubbleMusicPlay();

            //animation
            player.animator.SetTrigger("sprint");
            Sequence sequence=DOTween.Sequence();
            sequence.Append(transform.DOScaleX(1, animDuration).SetEase(Ease.OutElastic));
            if(shootDist>radius)
                sequence.Join(transform.DOLocalMove(Vector3.zero, animDuration));
            center.localPosition=Vector3.zero;
            //stop fully charged effect
            FullyChargedEffect.inst.Stop();
            //movement
            rgb.velocity=Vector2.zero;
            player.OnShot(shootDir*(shootSpd*shootDist/radius));
            //disable collider
            circleCollider.enabled=false;
            //enable collider after the animation
            sequence.AppendCallback(()=>{
                circleCollider.enabled=true;
            });
        }
    }
    public void Die(){
        if(dieSeq!=null && dieSeq.IsActive() && dieSeq.IsPlaying()) return;

        //play death sound effects
        AudioManager.instance.SetEventEmitter(FMODEvents.instance.fishDeath, AudioManager.instance.fishChannel5);
        //switch music for inside bubble
        AudioManager.instance.InBubbleMusicPlay();

        dieSeq =DOTween.Sequence();
        dieSeq.Append(DOTween.To(()=>actualRadius, (value)=>ActualRadius=value, 0, shrinkDuration).SetEase(Ease.InBack));
        dieSeq.Join(player.transform.DOScale(Vector3.zero, shrinkDuration).SetEase(Ease.InBack));
        dieSeq.AppendCallback(()=>Revive(Vector2.zero));
    }
    void Revive(Vector2 pos){
        //reset animator
        player.ResetAnimation();

        rgb.velocity=Vector2.zero;
        transform.parent.position=pos;
        transform.localPosition=Vector3.zero;
        transform.localScale=Vector3.one;
        center.transform.localPosition=Vector3.zero;
        FullyChargedEffect.inst.Stop();
        player.Start(); //reset player
        player.transform.localScale=Vector3.one;
        ActualRadius=radius;
        mouseDown=false;
    }
    bool CollidesWithWall(){
        RaycastHit2D hit = new RaycastHit2D();
        return CollidesWithWall(ref hit);
    }
    bool CollidesWithWall(ref RaycastHit2D hit){
        Vector2 origin = transform.position;
        Vector2 right=new Vector2(shootDir.y, -shootDir.x);
        Vector2 right2leftOffset=-right*2;

        float a,b;
        GetEllipseAB(out a, out b);
        float aa=a*a, bb=b*b;
        int steps=4;
        for(int i=0;i<steps;++i){
            float y=(float)i/steps*radius;
            float xSquared = aa * (1 - (y * y) / bb);
            float x = Mathf.Sqrt(xSquared);
            Vector2 origin_right = origin+right*y;
            Vector2 start=origin_right+shootDir*x, end=origin_right-shootDir*x;
            //right
            hit=Physics2D.Linecast(start, end, GameManager.inst.wallLayer);
            //Debug.DrawLine(start, end, Color.white);
            if(hit)
                return true;
            //left
            Vector2 r2lOffset=right2leftOffset*y;
            start+=r2lOffset;
            end+=r2lOffset;
            hit=Physics2D.Linecast(start, end, GameManager.inst.wallLayer);
            //Debug.DrawLine(start, end, Color.white);
            if(hit)
                return true;
        }
        return false;
    }
    bool CollidesWithWall_Player(float r, ref RaycastHit2D hit){
        Vector2 origin = player.transform.position;
        Vector2 dir=Vector2.up;

        float deltaTheta=Mathf.PI*2/phy_steps;
        for (int i = 0; i < phy_steps; ++i) {
            dir=MathUtil.Rotate(dir, deltaTheta);
            Vector2 end=origin+dir*r;

            // right
            hit = Physics2D.Linecast(origin, end, GameManager.inst.wallLayer);
            Debug.DrawLine(origin, end, Color.white);
            if (hit)
                return true;
        }

        return false;
    }
    float MaxDistToWall_DirectRay(){
        RaycastHit2D hit=Physics2D.Linecast(shootOrigin, mouseWorldPos, GameManager.inst.wallLayer);
        return hit?hit.distance:float.MaxValue;
    }
    float MaxDistToWall_Player(float r, out Vector2 idealMousePos){
        idealMousePos=mouseWorldPos;

        RaycastHit2D hit;
        Vector2 origin = mouseWorldPos;
        Vector2 dir=Vector2.up;
        float maxDist=float.MaxValue;

        float deltaTheta=Mathf.PI*2/phy_steps;
        for (int i = 0; i < phy_steps; ++i) {
            dir=MathUtil.Rotate(dir, deltaTheta);
            Vector2 end=origin+dir*r;

            hit = Physics2D.Linecast(origin, end, GameManager.inst.wallLayer);
            //Debug.DrawLine(origin, end, Color.white);
            if (hit){
                Vector2 _idealMousePos=hit.point-dir*r;
                float a=(_idealMousePos-shootOrigin).magnitude;
                if(a<maxDist){
                    maxDist=a;
                    idealMousePos=_idealMousePos;
                }
            }
        }
        return maxDist;
    }
    float MaxDistToReachWall(){
        Vector2 origin = transform.position;
        Vector2 right=new Vector2(shootDir.y, -shootDir.x);

        //RaycastHit2D _hit=Physics2D.Linecast(origin, mouseWorldPos, wallLayer);
        //return _hit?_hit.distance:float.MaxValue;

        float a,b;
        GetEllipseAB(out a, out b);
        float minDist=float.MaxValue;
        for(int i=0;i<phy_steps;++i){
            float y=(float)i/phy_steps*radius;
            float xSquared = a * a * (1 - (y * y) / (b * b));
            float x = Mathf.Sqrt(xSquared);
            Vector2 origin_right = origin+right*y;
            RaycastHit2D hit=Physics2D.Linecast(origin_right, origin_right-shootDir*x, GameManager.inst.wallLayer);
            Debug.DrawLine(origin_right,origin_right-shootDir*x, Color.white);
            if(hit){
                float a_new=CalculateA(hit.distance,y,b);
                if(a_new<minDist)
                    minDist=a_new;
            }
        }
        return minDist*2-radius;
    }
    public void GetEllipseAB(out float a, out float b){
        a = (shootDist+radius)/2;
        b = radius;
    }
    static float CalculateA(float x, float y, float b)
    {
        float denominator = 1 - (y * y) / (b * b);

        float aSquared = (x * x) / denominator;

        return Mathf.Sqrt(aSquared);
    }
}
