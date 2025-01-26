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
        spriteRenderer = GetComponent<SpriteRenderer>();
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

    void LoadNextLevel(){
        SceneLoader.inst.LoadNextLevel();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (GameManager.IsLayer(GameManager.inst.playerLayer, other.gameObject.layer))
        {
            if(Bubble.inst.insideBubble){
                canPass=true;
                LoadNextLevel();
            }
            else{
                canPass = false;
                if (spriteRenderer != null)
                    spriteRenderer.enabled = true; 
                if (subtitleUI != null)
                    subtitleUI.SetActive(true); 
            }
            
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (GameManager.IsLayer(GameManager.inst.playerLayer, other.gameObject.layer))
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
