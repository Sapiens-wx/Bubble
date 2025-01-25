using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : MonoBehaviour
{
    public float shootInterval, shootSpd;
    void Start()
    {
        StartCoroutine(Shoot());
    }

    IEnumerator Shoot(){
        WaitForSeconds wait=new WaitForSeconds(shootInterval);
        while(true){
            Projectile go=ProjectileManager.inst.CreateProjectile();
            go.transform.position=transform.position;
            go.Rgb.velocity=transform.up*shootSpd;
            yield return wait;
        }
    }
}
