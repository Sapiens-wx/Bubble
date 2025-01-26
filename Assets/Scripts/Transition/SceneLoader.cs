using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public string menuScene;
    public Level[] levels;

    public static SceneLoader inst;
    [HideInInspector] public int currentLevel, currentSublevel;
   public event System.Action onLevelComplete;
    void Awake(){
        if(inst!=null){
            Destroy(gameObject);
            return;
        }
        inst=this;
        DontDestroyOnLoad(gameObject);
    }
    public void LoadLevel(int level, int sublevel){
        LoadScene(levels[level].sublevels[sublevel]);
        currentLevel=level;
        currentSublevel=sublevel;
    }
    public void LoadScene(string scene){
        SceneManager.LoadScene(scene);
    }
    public void LoadNextLevel(){
        if(currentSublevel<levels[currentLevel].sublevels.Length-1){ //load next sublevel
            LoadLevel(currentLevel, currentSublevel+1);
        } else{ //completes the current level, return to menu scene
            onLevelComplete?.Invoke();
            LoadScene(menuScene);
        }
    }
}
