using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

public class SceneLoader : MonoBehaviour
{
    public Transform playerTrans;
    public Transform bubbleTrans;
    public Vector3 firstPosition;
    public Vector3 menuPosition;
    public Vector3 bubblePosition;
    
    [Header("Event Monitor")]
    public SceneLoadEventSO loadEventSo;

    [Header("Event")] 
    public VoidEventSO afterLoadedScene;
    
    [Header("Scene")]
    public GameSceneSO firstLoadScene;
    public GameSceneSO menuScene;
    private GameSceneSO currentLoadedScene;
    private GameSceneSO sceneToLoad;
    private Vector3 positionToGo;
    private Vector3 bubbleToGo;
    private bool fadeScreen;
    private bool isLoading;
    
    public float fadeDuration;
    /*private void Awake()
    {
        //Addressables.LoadSceneAsync(firstLoadScene.sceneReference, LoadSceneMode.Additive);
        currentLoadedScene = firstLoadScene;
        currentLoadedScene.sceneReference.LoadSceneAsync(LoadSceneMode.Additive);
    }*/

    private void Start()
    {
        NewGame();
    }

    private void OnEnable()
    {
        loadEventSo.LoadRequestEvent += OnLoadRequestEvent;
    }

    private void OnDisable()
    {
        loadEventSo.LoadRequestEvent -= OnLoadRequestEvent;
    }

    private void NewGame()
    {
        sceneToLoad = firstLoadScene;
        OnLoadRequestEvent(sceneToLoad, firstPosition, bubblePosition,true);
    }
    
/// <summary>
/// request of scene loading Event
/// </summary>
/// <param name="loacationToLoad"></param>
/// <param name="posToGo"></param>
/// <param name="fadeScreen"></param>
    private void OnLoadRequestEvent(GameSceneSO loacationToLoad, Vector3 posToGo, Vector3 bubToGo, bool fadeScreen)
    {
        if(isLoading)
            return;
        
        isLoading = true;
        sceneToLoad = loacationToLoad;
        positionToGo = posToGo;
        bubbleToGo = bubToGo;
        this.fadeScreen = fadeScreen;

        if (currentLoadedScene != null)
        {
            StartCoroutine(UnLoadCurrentScene());
        }
        else
        {
            LoadNewScene();
        }
    }

    private IEnumerator UnLoadCurrentScene()
    {
        if (fadeScreen)
        {
            
        }
        
        yield return new WaitForSeconds(fadeDuration);

        yield return currentLoadedScene.sceneReference.UnLoadScene();
        
        //close player
        playerTrans.gameObject.SetActive(false);
        
        LoadNewScene();
    }

    private void LoadNewScene()
    {
        var loadingOption = sceneToLoad.sceneReference.LoadSceneAsync(LoadSceneMode.Additive, true);
        loadingOption.Completed += OnLoadCompleted;
    }
/// <summary>
/// after scene loaded
/// </summary>
/// <param name="obj"></param>
    private void OnLoadCompleted(AsyncOperationHandle<SceneInstance> obj)
    {
        currentLoadedScene = sceneToLoad;

        playerTrans.position = positionToGo;
        
        bubbleTrans.position = bubbleToGo;
        
        playerTrans.gameObject.SetActive(true);
        if (fadeScreen)
        {
            
        }
        
        isLoading = false;
        
        //event after scene loaded
        afterLoadedScene.RaiseEvent();
    }
}
