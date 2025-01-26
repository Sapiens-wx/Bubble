using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public LayerMask wallLayer; //relative to the enemy
    public LayerMask projectileDestroyLayer;
    public LayerMask playerLayer;
    [Header("Level Info")]
    public int level;
    public GameSceneSO[] levels;

    [HideInInspector] public bool collected;

    public static GameManager inst;
    void Awake()
    {
        inst=this;
    }
    void OnEnable(){
        TeleportPoint.inst.onLevelComplete+=OnLevelComplete;
    }
    void OnDisable(){
        TeleportPoint.inst.onLevelComplete-=OnLevelComplete;
    }
    public static bool IsLayer(LayerMask mask, int layer){
        return (mask.value&(1<<layer))!=0;
    }
    public void OnLevelComplete(){
        levels[level].tag=2;
        if(collected)
            levels[level].tag=3;
        if(level<levels.Length-1 && levels[level+1].tag==0)
            levels[level+1].tag=1;
    }
}