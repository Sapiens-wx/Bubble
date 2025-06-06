using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Player : MonoBehaviour
{
    public Rigidbody2D rgb;
    public CircleCollider2D cc;
    public Animator animator;
    public float spd;
    public float radius, radius1;
    public float dieInterval;

    Camera mainCam;
    [HideInInspector] public bool mouseDown;
    bool insideRadius, lastFrameInsideRadius;
    Vector2 mouseWorldPos;
    Vector2 shootDir, shootOrigin;
    float shootDist;
    Sequence returnToBubbleSeq;
    Coroutine dieCoroutine;
    void OnDrawGizmosSelected(){
        Gizmos.DrawWireSphere(transform.position, radius);
        Gizmos.color=Color.green;
        Gizmos.DrawWireSphere(transform.position, radius1);
    }
    public void Start(){
        mainCam=Camera.main;
        rgb.simulated=false;
        Bubble.inst.insideBubble=true;
        cc.enabled=false;
    }
    void Update(){
        //sync player position with the bubble
        if(Bubble.inst.insideBubble){
            transform.position=Bubble.inst.center.position;
            transform.rotation=Bubble.inst.center.rotation;
        }
        //interaction
        if(!Bubble.inst.insideBubble){
            if(Input.GetMouseButtonDown(0)&&MouseInsideRadius(radius)){
                animator.SetTrigger("charge");
                mouseDown=true;
            } else if(Input.GetMouseButtonUp(0)&&mouseDown){
                mouseDown=false;
                //animation
                animator.SetTrigger("sprint");
                //movement
                rgb.velocity=shootDir*(spd*shootDist/radius);
            }
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
            } else if(insideRadius&&!lastFrameInsideRadius){ //if the player changes from outside radius to inside radius, then reset the position of the bubble
            }
            //limit the drag distance
            if(!IsInsideRadius(radius1, shootOrigin, mouseWorldPos)){ 
                mouseWorldPos=shootOrigin-shootDir*radius1;
                shootDir=shootOrigin-mouseWorldPos;
                shootDist=shootDir.magnitude;
                shootDir/=shootDist;
            }
            //adjust rotation
            transform.eulerAngles=new Vector3(0,0,Vector2.SignedAngle(Vector2.right, shootDir));
            lastFrameInsideRadius=insideRadius;
        }
    }
    void OnCollisionEnter2D(Collision2D collision){
        //collides with wall
        if(GameManager.IsLayer(GameManager.inst.wallLayer, collision.collider.gameObject.layer)){
            if(collision.collider.CompareTag("WallRed")){
                Bubble.inst.Die();
            }
        }
    }
    public void OnShot(Vector2 shootForce){
        Bubble.inst.insideBubble=false;
        rgb.simulated=true;
        rgb.velocity=shootForce;
        Bubble.inst.Shrink();
        //enable circle collider: didn't implement here. has to delay the enable after the player passes the center of the bubble. implemented in Bubble.cs: Update().
        cc.enabled=true;
        //start die coroutine;
        dieCoroutine=StartCoroutine(DieAnim());
    }
    public void OnReturnToBubble(){
        if (Bubble.inst.insideBubble) return;
        //play bubble expand sound
        AudioManager.instance.SetEventEmitter(FMODEvents.instance.callBackPt1, AudioManager.instance.bubbleChannel1);
        rgb.velocity=Vector2.zero;
        rgb.simulated=false;
        returnToBubbleSeq=DOTween.Sequence();
        returnToBubbleSeq.Append(Bubble.inst.rgb.DOMove(transform.position, 1));
        returnToBubbleSeq.AppendCallback(()=>{
            //play bubble expand sound
            AudioManager.instance.SetEventEmitter(FMODEvents.instance.callBackPt2, AudioManager.instance.bubbleChannel2);
            AudioManager.instance.InBubbleMusicPlay();

            Bubble.inst.Expand();
            Bubble.inst.insideBubble=true;
            //disable circle collider
            cc.enabled=false;
            //stop die coroutine
            if(dieCoroutine!=null)
                StopCoroutine(dieCoroutine);
            //animation
            ResetAnimation();
        });
    }
    public void OnReturnToBubbleInterrupted(){
        if(Bubble.inst.insideBubble) return;
        if(returnToBubbleSeq==null || !returnToBubbleSeq.IsPlaying()) return;
        returnToBubbleSeq.Pause();

        Bubble.inst.insideBubble=false;
        rgb.simulated=true;
        //enable circle collider: didn't implement here. has to delay the enable after the player passes the center of the bubble. implemented in Bubble.cs: Update().
        cc.enabled=true;
    }
    //-------------------
    IEnumerator DieAnim(){
        yield return new WaitForSeconds(dieInterval);
        Bubble.inst.Die();
    }
    public void ResetAnimation(){
        if(!animator.GetCurrentAnimatorStateInfo(0).IsTag("idle")){
            animator.SetTrigger("idle");
            animator.ResetTrigger("charge");
            animator.ResetTrigger("sprint");
        }
    }
    //-------------------mouse input utility--------------------
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
}