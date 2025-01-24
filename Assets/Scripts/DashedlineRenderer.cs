using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashedlineRenderer : MonoBehaviour
{
    public Vector3 startPoint; 
    public Vector3 endPoint;   
    public float dashLength = 0.2f; 
    public float dashGap = 0.1f;    
    public float lineWidth = 0.05f; 
    public GameObject dashPrefab;   

    private GameObject[] dashes;

    private void Start()
    {
        GenerateDashedLine();
    }

    private void GenerateDashedLine()
    {
        float distance = Vector3.Distance(startPoint, endPoint); 
        Vector3 direction = (endPoint - startPoint).normalized; 
        int dashCount = Mathf.FloorToInt(distance / (dashLength + dashGap)); 

        dashes = new GameObject[dashCount]; 

        for (int i = 0; i < dashCount; i++)
        {
            Vector3 dashStart = startPoint + direction * i * (dashLength + dashGap);
            Vector3 dashEnd = dashStart + direction * dashLength;
            
            GameObject dash = Instantiate(dashPrefab, transform);
            dash.transform.position = (dashStart + dashEnd) / 2; 
            dash.transform.localScale = new Vector3(dashLength, lineWidth, 1); 
            dash.transform.rotation = Quaternion.LookRotation(Vector3.forward, direction); 
            
            DashSegment segment = dash.AddComponent<DashSegment>();
            segment.Initialize(i, this);
            segment.breakEffectPrefab = dashPrefab;
            
            dashes[i] = dash; 
        }
    }
    
    public void BreakDash(int index)
    {
        if (index >= 0 && index < dashes.Length && dashes[index] != null)
        {
            Destroy(dashes[index]); 
            dashes[index] = null;
        }
    }
}

