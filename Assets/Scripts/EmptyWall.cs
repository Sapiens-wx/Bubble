using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmptyWall : MonoBehaviour
{
    public GameObject emptyWall;
    public bool canPass;
    public GameObject subtitleUI;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = emptyWall.GetComponent<SpriteRenderer>();
    }
    
    private void Start()
    {
        if (Bubble.inst.insideBubble)
        {
            canPass = true;
        }
        
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }
        
        if (subtitleUI != null)
        {
            subtitleUI.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !Bubble.inst.insideBubble)
        {
            canPass = false;
            
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = true; 
            }

            if (subtitleUI != null)
            {
                subtitleUI.SetActive(true); 
            }
        }
        else
        {
            canPass = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = false;
            }

            if (subtitleUI != null)
            {
                subtitleUI.SetActive(false);
            }
        }
    }
}
