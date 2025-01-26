using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class FullyChargedEffect : MonoBehaviour
{
    public SpriteRenderer spr;
    public float fromScale, toScale;
    public float duration;

    public static FullyChargedEffect inst;
    Sequence sequence;
    void Awake(){
        inst=this;
    }
    void Start(){
        transform.localScale=new Vector3(fromScale, fromScale, fromScale);
        sequence=DOTween.Sequence();
        sequence.AppendCallback(()=>{transform.localScale=new Vector3(fromScale, fromScale, fromScale);});
        sequence.Append(transform.DOScale(toScale, duration));
        sequence.Join(spr.DOFade(0, duration));
        sequence.AppendCallback(()=>{
            transform.localScale=new Vector3(fromScale, fromScale, fromScale);
            spr.color=new Color(spr.color.r, spr.color.g, spr.color.b, 1);
        });
        sequence.SetLoops(-1);
        Stop();
    }
    public void Play(){
        AudioManager.instance.SetEventEmitter(FMODEvents.instance.fullyCharged, AudioManager.instance.orbChannel1);
        gameObject.SetActive(true);
        sequence.Restart();
    }
    public void Stop(){
        AudioManager.instance.StopEventEmitter(AudioManager.instance.orbChannel1);
        gameObject.SetActive(false);
        sequence.Pause();
    }
}
