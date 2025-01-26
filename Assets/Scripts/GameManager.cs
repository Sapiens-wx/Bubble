using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public LayerMask wallLayer; //relative to the enemy
    public LayerMask projectileDestroyLayer;
    public LayerMask playerLayer;
    [Header("Level Info")]
    public Level[] levels;


    public static GameManager inst;
    void Awake()
    {
        inst=this;
    }
    void OnEnable(){
        SceneLoader.inst.onLevelComplete+=OnLevelComplete;
    }
    void OnDisable(){
        SceneLoader.inst.onLevelComplete-=OnLevelComplete;
    }
    public static bool IsLayer(LayerMask mask, int layer){
        return (mask.value&(1<<layer))!=0;
    }
    public void OnLevelComplete(){
        levels[SceneLoader.inst.currentLevel].tag=2;
        if(levels[SceneLoader.inst.currentLevel].collected)
            levels[SceneLoader.inst.currentLevel].tag=3;
        if(SceneLoader.inst.currentLevel<levels.Length-1 && levels[SceneLoader.inst.currentLevel+1].tag==0)
            levels[SceneLoader.inst.currentLevel+1].tag=1;
    }
}