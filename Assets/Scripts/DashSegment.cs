using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashSegment : MonoBehaviour
{
    public int index; 
    private DashedlineRenderer lineManager;
    public GameObject breakEffectPrefab;

    public void Initialize(int index, DashedlineRenderer lineManager)
    {
        this.index = index;
        this.lineManager = lineManager;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.name);
        if (other.CompareTag("Player")) 
        {
            Debug.Log(other.name);
            if (breakEffectPrefab != null)
            {
                Debug.Log("Effect");
                Instantiate(breakEffectPrefab, transform.position, Quaternion.identity);
            }
            
            lineManager.BreakDash(index); 
        }
    }
}
