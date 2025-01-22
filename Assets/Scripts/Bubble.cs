using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Bubble : MonoBehaviour
{
    public Transform player;
    public float radius, radius1, radius2;
    public float spd;
    [Header("Animation")]
    public float animDuration;
    [Header("Components")]
    public Rigidbody2D rgb;

    public static Bubble inst;
    Camera mainCam;
    bool mouseDown, insideRadius, lastFrameInsideRadius;
    Vector2 mouseWorldPos;
    Vector2 shootDir, shootOrigin;
    float shootDist;

    public float ShootDist{
        get=>shootDist;
    }
    void OnDrawGizmosSelected(){
        Gizmos.DrawWireSphere(transform.position, radius);
        Gizmos.color=Color.green;
        Gizmos.DrawWireSphere(transform.position, radius1);
        Gizmos.color=Color.red;
        Gizmos.DrawWireSphere(transform.position, radius2);
    }
    void Awake(){
        inst=this;
    }
    // Start is called before the first frame update
    void Start()
    {
        mainCam=Camera.main;
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
        } else if(Input.GetMouseButtonUp(0)&&mouseDown){
            mouseDown=false;
            //animation
            transform.DOScaleX(1, animDuration).SetEase(Ease.OutCirc);
            if(shootDist>radius)
                transform.DOLocalMove(Vector3.zero, animDuration);
            player.DOLocalMove(Vector3.zero, animDuration).SetEase(Ease.OutCirc);
            //movement
            if(IsInsideRadius(radius1, transform.parent.position, mouseWorldPos)){ //move
                rgb.velocity=shootDir*(spd*shootDist/radius);
            } else{ //get out from the bubble
            }
        }
        if(Input.GetKeyDown(KeyCode.A))
            rgb.velocity=Vector2.left*spd;
        else if(Input.GetKeyDown(KeyCode.W))
            rgb.velocity=Vector2.up*spd;
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
            //adjust rotation
            transform.eulerAngles=new Vector3(0,0,Vector2.SignedAngle(Vector2.right, shootDir));
            if(!insideRadius){ //distort
                transform.localPosition=(mouseWorldPos+shootOrigin+shootDir*radius)/2-(Vector2)transform.parent.position;
                Vector3 scale=Vector3.one;
                scale.x=(shootDist+radius)/2/radius;
                transform.localScale=scale;
            }
            player.transform.position=mouseWorldPos;
            lastFrameInsideRadius=insideRadius;
        }
    }
}
