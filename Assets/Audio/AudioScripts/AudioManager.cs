using FMOD.Studio;
using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using static System.TimeZoneInfo;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Scene Management")]
    [SerializeField] private bool isLevelSelect;

    [Header("FMOD Clean Up Lists")]
    private List<EventInstance> eventInstances;
    private List<StudioEventEmitter> eventEmitters;

    [Header("Orb Obj Channels")]
    [SerializeField] public GameObject orbChannel1;

    [Header("Fish Obj Channels")]
    [SerializeField] public GameObject fishChannel1;
    [SerializeField] public GameObject fishChannel2;
    [SerializeField] public GameObject fishChannel3;
    [SerializeField] public GameObject fishChannel4;
    [SerializeField] public GameObject fishChannel5;

    [Header("Bubble Object Channels")]
    [SerializeField] public GameObject bubbleChannel1;
    [SerializeField] public GameObject bubbleChannel2;

    [field: Header("Channel Buses")]
    [field: SerializeField] public Bus busOutBubbleMusic;
    [field: SerializeField] public Bus busInBubbleMusic;
    [field: SerializeField] public Bus busLevelSelectMusic;
    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
        eventInstances = new List<EventInstance>();
        eventEmitters = new List<StudioEventEmitter>();

        busInBubbleMusic = RuntimeManager.GetBus("bus:/Music/MusicInBubble");
        busOutBubbleMusic = RuntimeManager.GetBus("bus:/Music/MusicOutBubble");
        busLevelSelectMusic = RuntimeManager.GetBus("bus:/Music/MusicLevelSelect");

        SetEventInstance(FMODEvents.instance.musicInBubble);
        SetEventInstance(FMODEvents.instance.musicOutBubbleMelody);
        SetEventInstance(FMODEvents.instance.musicOutBubbleBassAndDrum);
        SetEventInstance(FMODEvents.instance.musicLevelSelect);

        if (isLevelSelect)
        {
            LevelSelectMusicPlay();
        }
        else
        {
            StartCoroutine(TransitionInBubble(0.01f));
        }
        
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.E))
        {
            AudioManager.instance.SetEventEmitter(FMODEvents.instance.callBackPt2, AudioManager.instance.bubbleChannel2);
        }
    }

    public EventInstance SetEventInstance(EventReference eventReference)
    {
        EventInstance eventInstance = RuntimeManager.CreateInstance(eventReference);
        eventInstances.Add(eventInstance);
        eventInstance.start();
        return eventInstance;
    }

    public StudioEventEmitter SetEventEmitter(EventReference eventReference, GameObject emitterGameObject)
    {
        Debug.Log("play emitter sound | " + eventReference + " | " + emitterGameObject.name);
        StudioEventEmitter emitter = emitterGameObject.GetComponent<StudioEventEmitter>();
        emitter.EventReference = eventReference;
        emitter.Play();
        eventEmitters.Add(emitter);
        return emitter;
    }

    public StudioEventEmitter StopEventEmitter(GameObject emitterGameObject)
    {
        StudioEventEmitter emitter = emitterGameObject.GetComponent<StudioEventEmitter>();
        emitter.Stop();
        eventEmitters.Remove(emitter);
        return emitter;
    }

    public void InBubbleMusicPlay()
    {
        busInBubbleMusic.setVolume(1);
        StartCoroutine(TransitionInBubble(0.5f));
    }
    public void OutBubbleMusicPlay()
    {
        StartCoroutine(TransitionOutBubble(0.2f));
    }

    public void LevelSelectMusicPlay()
    {
        StartCoroutine(TransitionLevelSelect(0.05f));
    }

    private IEnumerator TransitionInBubble(float transitionTime)
    {
        busLevelSelectMusic.setVolume(0);

        float timer = 0.0f;
        while (timer < transitionTime)
        {
            timer += Time.deltaTime;
            busOutBubbleMusic.setVolume(1 - (timer / transitionTime));

            yield return new WaitForFixedUpdate();
        }

        busOutBubbleMusic.setVolume(0);
        yield return null;
    }
    private IEnumerator TransitionOutBubble(float transitionTime)
    {
        busLevelSelectMusic.setVolume(0);

        SetEventEmitter(FMODEvents.instance.glitch, fishChannel4);
        busInBubbleMusic.setVolume(0);
        busOutBubbleMusic.setVolume(0);

        yield return new WaitForSeconds(0.214f);

        float timer = 0.0f;
        while (timer < transitionTime)
        {
            timer += Time.deltaTime;
            busOutBubbleMusic.setVolume((timer / transitionTime));
            busInBubbleMusic.setVolume((timer / transitionTime));

            yield return new WaitForFixedUpdate();
        }

        busInBubbleMusic.setVolume(1);
        busOutBubbleMusic.setVolume(1);

        yield return null;
    }

    private IEnumerator TransitionLevelSelect(float transitionTime)
    {
        float timer = 0.0f;
        while (timer < transitionTime)
        {
            timer += Time.deltaTime;
            busLevelSelectMusic.setVolume((timer / transitionTime));
            busOutBubbleMusic.setVolume(1 - (timer / transitionTime));
            busInBubbleMusic.setVolume(1 - (timer / transitionTime));

            yield return new WaitForFixedUpdate();
        }

        busLevelSelectMusic.setVolume(1);
        busOutBubbleMusic.setVolume(0);
        busInBubbleMusic.setVolume(0);

        yield return null;
    }
    private void CleanUp()
    {
        foreach(EventInstance eventInstance in eventInstances)
        {
            eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            eventInstance.release();
        }

        foreach(StudioEventEmitter eventEmitter in eventEmitters)
        {
            eventEmitter.Stop();
        }
    }

    private void OnDestroy()
    {
        CleanUp();
    }
}
