using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : StateMachineBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        AudioManager.instance.SetEventEmitter(FMODEvents.instance.fishIdle, AudioManager.instance.fishChannel1);
        base.OnStateEnter(animator, stateInfo, layerIndex);
    }
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        AudioManager.instance.StopEventEmitter(AudioManager.instance.fishChannel1);
        base.OnStateExit(animator, stateInfo, layerIndex);
    }
}
