using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using Cinemachine;
using Cinemachine.PostFX;

public class PostProcessingHandler : MonoBehaviour
{
    public CinemachinePostProcessing c;
    List<PostProcessEffectSettings> settings;
    ColorGrading colorGrading;


    private void Start()
    {
        settings = c.m_Profile.settings;
        colorGrading = c.m_Profile.GetSetting<ColorGrading>();

        SetSaturation(-12.75f);
    }

    private void OnApplicationQuit()
    {
        StopAllCoroutines();
        SetSaturation(-12.75f);
    }

    /// <summary>
    /// Don't use this when pausing the game because the coroutine gets paused because Time.timeScale gets set to 0
    /// </summary>
    /// <param name="speed"></param>
    /// <param name="fadeIn"></param>
    public void FadeSaturation(float speed, bool fadeIn = false)
    {
        StopAllCoroutines();

        if(fadeIn)
            StartCoroutine(FadeInSaturation(speed));
        else
            StartCoroutine(FadeOutSaturation(speed));
    }

    private IEnumerator FadeInSaturation(float speed)
    {
        float saturation;
        while(colorGrading.saturation < -12.75f)
        {
            saturation = colorGrading.saturation.value;
            saturation += speed * Time.deltaTime;
            SetSaturation(saturation);

            yield return new WaitForFixedUpdate();
        }

        SetSaturation(-12.75f); //Ensure it reaches the target in case of small errors

        yield return null;
    }

    private IEnumerator FadeOutSaturation(float speed)
    {
        float saturation;
        while(colorGrading.saturation > -100)
        {
            saturation = colorGrading.saturation.value;
            saturation -= speed * Time.deltaTime;
            SetSaturation(saturation);

            yield return new WaitForFixedUpdate();
        }

        SetSaturation(-100f); //Ensure it reaches the target in case of small errors

        yield return null;
    }

    public void SetSaturation(float saturation)
    {
        colorGrading.saturation.Override(saturation);
    }
}
