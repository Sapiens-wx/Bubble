using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

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
    public LayerMask wallLayer;

    public static Bubble inst;
    Camera mainCam;
    bool mouseDown, insideRadius, lastFrameInsideRadius;
    float lastFrameShootDist;
    Vector2 mouseWorldPos;
    Vector2 shootDir, shootOrigin;
    float shootDist;
    [HideInInspector] public bool insideBubble;
    public float actualRadius;
    Sequence shootSyncPosSequence;

    public float ShootDist{
        get=>shootDist;
    }
    public float ActualRadius{
        get=>actualRadius;
        set{
            actualRadius=value;
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
        if(Input.GetKeyDown(KeyCode.E))
            player.OnReturnToBubble();
        if(insideBubble){
            if(Input.GetMouseButtonDown(0)&&MouseInsideRadius(radius)){
                mouseDown=true;
                if(shootSyncPosSequence!=null && shootSyncPosSequence.IsPlaying())
                    shootSyncPosSequence.Kill();
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
            //if collides with wall, then die
            if(CollidesWithWall()){
                mouseDown=false;
                Die();
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
    void Shoot(){
        mouseDown=false;
        //moves with bubble (player stays in the bubble)
        if(IsInsideRadius(radius1, transform.parent.position, mouseWorldPos)){ //move
            //animation
            if(shootSyncPosSequence!=null && shootSyncPosSequence.IsPlaying())
                shootSyncPosSequence.Kill();
            shootSyncPosSequence=DOTween.Sequence();
            shootSyncPosSequence.Append(transform.DOScaleX(1, animDuration).SetEase(Ease.OutCirc));
            if(shootDist>radius)
                shootSyncPosSequence.Join(transform.DOLocalMove(Vector3.zero, animDuration));
            shootSyncPosSequence.Join(center.DOLocalMove(Vector3.zero, animDuration).SetEase(Ease.OutCirc));
            //movement
            rgb.velocity=shootDir*(spd*shootDist/radius);
        } else{ //get out from the bubble
            //animation
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
    void Die(){
        Sequence s=DOTween.Sequence();
        s.Append(DOTween.To(()=>actualRadius, (value)=>ActualRadius=value, 0, shrinkDuration).SetEase(Ease.InBack));
        s.AppendCallback(()=>Revive(Vector2.zero));
    }
    void Revive(Vector2 pos){
        rgb.velocity=Vector2.zero;
        transform.parent.position=pos;
        transform.localPosition=Vector3.zero;
        transform.localScale=Vector3.one;
        center.transform.localPosition=Vector3.zero;
        FullyChargedEffect.inst.Stop();
        player.Start(); //reset player
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
            hit=Physics2D.Linecast(start, end, wallLayer);
            //Debug.DrawLine(start, end, Color.white);
            if(hit)
                return true;
            //left
            Vector2 r2lOffset=right2leftOffset*y;
            start+=r2lOffset;
            end+=r2lOffset;
            hit=Physics2D.Linecast(start, end, wallLayer);
            //Debug.DrawLine(start, end, Color.white);
            if(hit)
                return true;
        }
        return false;
    }
    float MaxDistToReachWall(){
        Vector2 origin = transform.position;
        Vector2 right=new Vector2(shootDir.y, -shootDir.x);

        //RaycastHit2D _hit=Physics2D.Linecast(origin, mouseWorldPos, wallLayer);
        //return _hit?_hit.distance:float.MaxValue;

        float a,b;
        GetEllipseAB(out a, out b);
        float minDist=float.MaxValue;
        int steps=4;
        for(int i=0;i<steps;++i){
            float y=(float)i/steps*radius;
            float xSquared = a * a * (1 - (y * y) / (b * b));
            float x = Mathf.Sqrt(xSquared);
            Vector2 origin_right = origin+right*y;
            RaycastHit2D hit=Physics2D.Linecast(origin_right, origin_right-shootDir*x, wallLayer);
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
