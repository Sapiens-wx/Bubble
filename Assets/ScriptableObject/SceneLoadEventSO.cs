using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Event/SceneLoadEventSO")]
public class SceneLoadEventSO : ScriptableObject
{
    public UnityAction<GameSceneSO, Vector3, Vector3, bool> LoadRequestEvent;

    /// <summary>
    /// request of scene loading
    /// </summary>
    /// <param name="locationToLoad">to load scene</param>
    /// <param name="posToGo">player's destination position</param>
    /// <param name="bubToGo"></param>
    /// <param name="fadeScreen">if fade</param>
    public void RaiseLoadRequestEvent(GameSceneSO locationToLoad, Vector3 posToGo, Vector3 bubToGo, bool fadeScreen)
    {
        LoadRequestEvent?.Invoke(locationToLoad, posToGo, bubToGo, fadeScreen);
    }
}
