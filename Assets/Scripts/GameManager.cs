using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public LayerMask wallLayer; //relative to the enemy
    public LayerMask projectileDestroyLayer;
    public LayerMask playerLayer;

    public static GameManager inst;
    void Awake()
    {
        inst=this;
    }
    public static bool IsLayer(LayerMask mask, int layer){
        return (mask.value&(1<<layer))!=0;
    }
}