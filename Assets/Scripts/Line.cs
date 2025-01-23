using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Line : MonoBehaviour
{
    //each instance of this class has a shared material, create a variable to store each instance's different alpha value, and apply the alpha value to the material's color property using materialpropertyblock
    public LineRenderer lineRenderer;
    
    Material material;
    MaterialPropertyBlock materialPropertyBlock;
    float alpha;
    void OnValidate(){
        if(lineRenderer==null)
            lineRenderer=GetComponent<LineRenderer>();
        if(materialPropertyBlock==null)
            materialPropertyBlock=new MaterialPropertyBlock();
        if(material==null)
            material=lineRenderer.sharedMaterial;
    }
    void Start()
    {
        if(lineRenderer==null)
            lineRenderer=GetComponent<LineRenderer>();
        material=lineRenderer.sharedMaterial;
        materialPropertyBlock = new MaterialPropertyBlock();
    }
    public float Alpha{
        get=>alpha;
        set{
            #if UNITY_EDITOR
                if(lineRenderer==null || materialPropertyBlock==null || material==null)
                    OnValidate();
            #endif
            alpha=value;
            lineRenderer.GetPropertyBlock(materialPropertyBlock);
            materialPropertyBlock.SetFloat("_alpha", alpha);
            lineRenderer.SetPropertyBlock(materialPropertyBlock);
        }
    }
}
