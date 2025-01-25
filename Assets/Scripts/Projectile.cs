using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    Rigidbody2D rgb;
    public Rigidbody2D Rgb{
        get{
            if(rgb==null) rgb=GetComponent<Rigidbody2D>();
            return rgb;
        }
    }
    void OnCollisionEnter2D(Collision2D collision){
        if(GameManager.IsLayer(GameManager.inst.projectileDestroyLayer, collision.collider.gameObject.layer)){
            ProjectileManager.inst.ReleaseProjectile(this);
        }
        if(GameManager.IsLayer(GameManager.inst.playerLayer, collision.collider.gameObject.layer)){
            Bubble.inst.Die();
        }
    }
}
