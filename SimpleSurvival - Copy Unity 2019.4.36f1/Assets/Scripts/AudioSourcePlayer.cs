using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSourcePlayer : MonoBehaviour
{
    [System.Serializable]
    public struct AudioClipNamePair
    {
        public string name;
        public AudioClip audioClip;
    }

    public AudioSource source;
    public AudioClipNamePair[] audioClips;


    private void Start()
    {
        if(!source)
            source = GetComponent<AudioSource>();
    }

    public void Play(string clipName)
    {
        foreach(AudioClipNamePair a in audioClips)
        {
            if(a.name.Equals(clipName))
            {
                if(a.audioClip)
                    source.PlayOneShot(a.audioClip);


                break;
            }
        }
    }
}
