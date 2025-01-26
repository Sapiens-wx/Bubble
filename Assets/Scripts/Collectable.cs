using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectable : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collider){
        if(GameManager.IsLayer(GameManager.inst.playerLayer, collider.gameObject.layer)){
            GameManager.inst.levels[SceneLoader.inst.currentLevel].collected=true;
            Destroy(gameObject);
        }
    }
}
