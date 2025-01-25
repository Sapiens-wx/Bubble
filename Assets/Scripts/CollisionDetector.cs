using UnityEngine;

public class CollisionDetector : MonoBehaviour {
    void OnCollisionStay2D(Collision2D collision){
        //collides with wall
        if(GameManager.IsLayer(GameManager.inst.wallLayer, collision.collider.gameObject.layer)){
            if(collision.collider.CompareTag("WallWhite")){
                Bubble.inst.player.OnReturnToBubbleInterrupted();
            } else if(collision.collider.CompareTag("WallRed")){
                Bubble.inst.player.OnReturnToBubbleInterrupted();
                Bubble.inst.Die();
            }
        }
    }
}