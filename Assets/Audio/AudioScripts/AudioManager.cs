using FMOD.Studio;
using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("FMOD Clean Up Lists")]
    private List<EventInstance> eventInstances;
    private List<StudioEventEmitter> eventEmitters;

    [Header("Player GameObjects")]
    [SerializeField] private GameObject fishObj;
    [SerializeField] private GameObject beepingOrbObj;
    [SerializeField] private GameObject bubbleObj;
    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
        eventInstances = new List<EventInstance>();
        eventEmitters = new List<StudioEventEmitter>();
        
    }

    public EventInstance CreateEventInstance(EventReference eventReference)
    {
        EventInstance eventInstance = RuntimeManager.CreateInstance(eventReference);
        eventInstances.Add(eventInstance);
        return eventInstance;
    }

    public StudioEventEmitter SetEventEmitter(EventReference eventReference, GameObject emitterGameObject)
    {
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
}
