using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TypewriterEffect : MonoBehaviour
{
    public AudioSourcePlayer audioSourcePlayer;
    public bool setTextOnUpdate;
    public Text text;
    public string message;
    public int toIndex; //The index indicating how much of the message string should be revealed


    private void Update()
    {
        if(setTextOnUpdate)
            SetText();
    }

    public void DeathSound()
    {
        audioSourcePlayer.Play("DeathSound");
    }

    public void OneTap1()
    {
        audioSourcePlayer.Play("OneTap1");
    }

    public void OneTap2()
    {
        audioSourcePlayer.Play("OneTap2");
    }

    public void OneTap3()
    {
        audioSourcePlayer.Play("OneTap3");
    }

    public void TwoTaps()
    {
        audioSourcePlayer.Play("TwoTaps");
    }

    public void ThreeTaps()
    {
        audioSourcePlayer.Play("ThreeTaps");
    }

    public void TipSound()
    {
        audioSourcePlayer.Play("TipSound");
    }

    public void SetText()
    {
        text.text = message.Substring(0, toIndex);
    }

    public void SetTextOnUpdate()
    {
        setTextOnUpdate = true;
    }

    public void DontSetTextOnUpdate()
    {
        setTextOnUpdate = false;
    }
}
