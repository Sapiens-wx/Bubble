using UnityEngine;

public class CollisionDetector : MonoBehaviour {
    void OnCollisionStay2D(Collision2D collision){
        //collides with wall
        if(GameManager.IsLayer(GameManager.inst.wallLayer, collision.collider.gameObject.layer)){
            //if(collision.collider.CompareTag("Wall")){
                Bubble.inst.player.OnReturnToBubbleInterrupted();
            //}
        }
    }
}