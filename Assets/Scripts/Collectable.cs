using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectable : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collider){
        Debug.Log($"collect {collider.gameObject.layer}");
        if(GameManager.IsLayer(GameManager.inst.playerLayer, collider.gameObject.layer)){
            GameManager.inst.collected=true;
            Destroy(gameObject);
        }
    }
}
