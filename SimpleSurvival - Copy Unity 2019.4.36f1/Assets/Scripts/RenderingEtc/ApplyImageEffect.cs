using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplyImageEffect : MonoBehaviour
{
    public Material imageEffectMat;


    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if(imageEffectMat)
            Graphics.Blit(source, destination, imageEffectMat);
    }
}
