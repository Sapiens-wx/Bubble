using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargeState : StateMachineBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        AudioManager.instance.SetEventEmitter(FMODEvents.instance.fishCharging, AudioManager.instance.fishChannel2);
        base.OnStateEnter(animator, stateInfo, layerIndex);
    }
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateExit(animator, stateInfo, layerIndex);
    }
}
