using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportPoint : MonoBehaviour,IInteractable
{
   public SceneLoadEventSO loadEventSo;
   
   public GameSceneSO sceneToGo;
   
   public Vector3 positionToGo;

   public Vector3 bubbleToGo;
   
   public int tag;
   public void TriggerAction()
   {
      Debug.Log("Where are you?");
      loadEventSo.RaiseLoadRequestEvent(sceneToGo, positionToGo, bubbleToGo, true);
   }
}
