using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using DG.Tweening;

public class ProjectileManager : MonoBehaviour
{
    [Header("Bullet")]
    [SerializeField] GameObject projPrefab;


    ObjectPool<Projectile> proj_pool;
    //ObjectPool<GameObject> hit_pool;
    public static ProjectileManager inst;
    void Awake(){
        inst=this;
        proj_pool=new ObjectPool<Projectile>(Proj_CreateFunc, Proj_OnGet, Proj_OnRelease, Proj_OnPoolObjDestroy);
        //hit_pool=new ObjectPool<GameObject>(Hit_CreateFunc, Hit_OnGet, Hit_OnRelease, Hit_OnPoolObjDestroy);
    }
    #region object pool funcs
    Projectile Proj_CreateFunc(){
        Projectile go = Instantiate(projPrefab).GetComponent<Projectile>();
        go.gameObject.SetActive(false);
        return go;
    }
    static void Proj_OnGet(Projectile go){
        go.gameObject.SetActive(true);
    }
    static void Proj_OnRelease(Projectile go){
        go.Rgb.velocity=Vector2.zero;
        go.gameObject.SetActive(false);
    }
    static void Proj_OnPoolObjDestroy(Projectile go){
        Destroy(go.gameObject);
    }
    /*
    GameObject Hit_CreateFunc(){
        GameObject go = Instantiate(hitPrefab);
        go.SetActive(false);
        return go;
    }
    static void Hit_OnGet(GameObject go){
        go.SetActive(true);
    }
    static void Hit_OnRelease(GameObject go){
        go.SetActive(false);
    }
    static void Hit_OnPoolObjDestroy(GameObject go){
        Destroy(go.gameObject);
    }
    */
    #endregion

    public Projectile CreateProjectile(){
        return proj_pool.Get();
    }
    public void ReleaseProjectile(Projectile go){
        proj_pool.Release(go);
    }
    /*
    public void HitAnim(Projectile go){
        GameObject hit = hit_pool.Get();
        hit.transform.position=go.transform.position;
        Sequence s = DOTween.Sequence();
        s.AppendInterval(hitAnimDuration);
        s.AppendCallback(()=>{ hit_pool.Release(hit); });
    }
    */
}