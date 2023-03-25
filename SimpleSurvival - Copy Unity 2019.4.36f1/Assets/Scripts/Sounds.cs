using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sounds : MonoBehaviour
{
    private AudioSource audioSource;
    public string folder = "/";
    private AudioClip footstep;
    private List<AudioClip> vocalization;
    private AudioClip jump;
    private AudioClip land;
    private AudioClip attack;
    private AudioClip idle;
    private List<AudioClip> breatheIn;
    private List<AudioClip> breatheOut;


    private UnityEngine.Random random = new UnityEngine.Random();


    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if(!audioSource)
            audioSource = gameObject.AddComponent<AudioSource>();

        footstep = Load("Footstep");
        jump = Load("Jump");
        land = Load("Land");
        attack = Load("Attack");
        idle = Load("Idle");

        breatheIn = PopulateList("BreatheIn");
        breatheOut = PopulateList("BreatheOut");
        vocalization = PopulateList("Vocalization");
    }

    public void FootStep()
    {
        audioSource.PlayOneShot(footstep);
    }

    public void Vocalize()
    {
        audioSource.PlayOneShot(vocalization[Rand(vocalization)]);
    }

    public void Attack()
    {
        audioSource.PlayOneShot(attack);
    }

    public void Idle(AudioClip a)
    {
        audioSource.PlayOneShot(idle);
    }

    public void BreatheIn()
    {
        audioSource.PlayOneShot(breatheIn[Rand(breatheIn)]);
    }

    public void BreatheOut()
    {
        audioSource.PlayOneShot(breatheOut[Rand(breatheOut)]);
    }

    private AudioClip Load(string fileName)
    {
        return Resources.Load<AudioClip>(folder + fileName);
    }

    private List<AudioClip> PopulateList(string name)
    {
        int i = 0;
        AudioClip currentSound = Load(name + i);
        List<AudioClip> sounds = new List<AudioClip>();

        while(currentSound)
        {
            sounds.Add(currentSound);
            i++;
            currentSound = Load(name + i);
        }

        return sounds;
    }

    private int Rand(List<AudioClip> sounds)
    {
        return Mathf.RoundToInt(UnityEngine.Random.Range(0, sounds.Count));
    }
}
