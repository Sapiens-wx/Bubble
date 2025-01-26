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
    private IInteractable targetItem;
    private void Awake()
    {
        spriteRenderer = emptyWall.GetComponent<SpriteRenderer>();
        targetItem = GetComponent<IInteractable>();
    }
    
    private void Start()
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

    private void Update()
    {
        //Debug.Log(Bubble.inst.insideBubble);
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log(other.gameObject.tag);
        
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
            Debug.Log("传送");
            targetItem.TriggerAction();
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
