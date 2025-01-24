using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public CinemachineConfiner2D confiner2D;

    private void Awake()
    {
        confiner2D = GetComponent<CinemachineConfiner2D>();
    }
    void Start()
    {
        GetNewCameraBounds();
    }

    private void GetNewCameraBounds()
    {
        var obj = GameObject.FindGameObjectWithTag("Bounds");
        if (obj == null)
            return;
        
        confiner2D.m_BoundingShape2D = obj.GetComponent<Collider2D>();
        
        confiner2D.InvalidateCache();
    }
}
