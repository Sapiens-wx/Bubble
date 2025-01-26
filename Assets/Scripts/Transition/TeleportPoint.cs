using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportPoint : MonoBehaviour
{
   public static TeleportPoint inst;
   public SceneLoadEventSO loadEventSo;
   
   public GameSceneSO sceneToGo;
   
   public Vector3 positionToGo;

   public Vector3 bubbleToGo;
   public bool isLastScene;
   public event System.Action onLevelComplete;
   void Awake(){
      inst=this;
   }
   public void TriggerAction()
   {
      if(isLastScene)
         onLevelComplete?.Invoke();
      loadEventSo.RaiseLoadRequestEvent(sceneToGo, positionToGo, bubbleToGo, true);
      
   }
}
