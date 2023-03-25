using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Target : MonoBehaviour
{
    //public AudioClip hitSound;
    private AudioSource audioSource;
    public ulong soundDelay = 1000;


    public virtual void Init(TargetSet parent)
    {
        gameObject.layer = LayerMask.NameToLayer("Target");
        audioSource = GetComponent<AudioSource>();
    }

    public virtual void Hit(RaycastHit hit)
    {
        audioSource.Play(soundDelay);
    }

    public virtual void ResetTarget()
    {

    }
}
