using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class FMODEvents : MonoBehaviour
{
    public static FMODEvents instance;

    [field: Header("Fish Object")]
    [field: SerializeField] public EventReference fishCharging { get; private set; }
    [field: SerializeField] public EventReference fishSprint { get; private set; }
    [field: SerializeField] public EventReference fishIdle { get; private set; }

    [field: Header("Bubble Object")]
    [field: SerializeField] public EventReference callBackPt1 { get; private set; }
    [field: SerializeField] public EventReference callBackPt2 { get; private set; }

    [field: Header("Fully Charged Indicator")]
    [field: SerializeField] public EventReference fullyCharged { get; private set; }

    [field: Header("Glitch Sound")]
    [field: SerializeField] public EventReference glitch { get; private set; }

    [field: Header("Music Events")]
    [field: SerializeField] public EventReference musicInBubble { get; private set; }
    [field: SerializeField] public EventReference musicOutBubbleMelody { get; private set; }
    [field: SerializeField] public EventReference musicOutBubbleBassAndDrum { get; private set; }

    

    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
    }
}
