using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Bubble : MonoBehaviour
{
    public Transform player;
    public float radius, radius1, radius2;

    public static Bubble inst;
    Rigidbody2D rgb;
    Camera mainCam;
    bool mouseDown, lastFrameInsideRadius;
    Vector2 mouseWorldPos;
    Vector2 shootDir, shootOrigin;
    float shootDist;
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
        rgb=GetComponent<Rigidbody2D>();
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
    bool InsideRadius(float r, Vector2 mousePos){
        Vector2 dir=mousePos-(Vector2)transform.position;
        float distSqr=dir.x*dir.x+dir.y*dir.y;
        return distSqr<=r*r;
    }
    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0)&&MouseInsideRadius(radius)){
            mouseDown=true;
            shootOrigin=transform.position;
        } else if(Input.GetMouseButtonUp(0)&&mouseDown){
            mouseWorldPos=MouseWorldPos();
            mouseDown=false;
            //animation
            transform.DOScaleX(1, .4f);
            if(shootDist>radius)
                transform.DOMove(shootOrigin, .4f);
            player.DOLocalMove(Vector3.zero,.4f);
            //movement
            rgb.velocity=shootDir*(shootDist/radius);
        }
    }
    void FixedUpdate(){
        if(mouseDown){
            mouseWorldPos=MouseWorldPos();
            bool insideRadius=InsideRadius(radius, mouseWorldPos);
            if(!insideRadius&&lastFrameInsideRadius){ //the bubble begins to distort: compute origin
                //shootOrigin=transform.position;
            } else if(insideRadius&&!lastFrameInsideRadius){ //if the player changes from outside radius to inside radius, then reset the position of the bubble
                transform.position=shootOrigin;
                transform.localScale=Vector3.one;
            }
            shootDir=shootOrigin-mouseWorldPos;
            shootDist=shootDir.magnitude;
            shootDir/=shootDist;
            if(!insideRadius){ //distort
                transform.eulerAngles=new Vector3(0,0,Vector2.SignedAngle(Vector2.right, shootDir));
                transform.position=(mouseWorldPos+shootOrigin+shootDir*radius)/2;
                Vector3 scale=Vector3.one;
                scale.x=(shootDist+radius)/2/radius;
                transform.localScale=scale;
            }
            player.transform.position=mouseWorldPos;
            /*
            if(InsideRadius(radius1, mouseWorldPos)){ //move
            } else{ //get out from the bubble
            }*/
            lastFrameInsideRadius=insideRadius;
        }
    }
}
