using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MysteryBox : MonoBehaviour
{
    protected bool m_available = true;
    public bool available { get => m_available; }
    private AudioSource audioSource;
    public AudioClip audioClip;
    public Transform gunHolder;
    //public float verticalDistance = 1f;
    public Animator anim;
    private int gunIdx;


    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void Open()
    {
        if(m_available)
        {
            StartCoroutine(OpenCoroutine());

            m_available = false;
        }
    }

    public void SwitchGun()
    {

        gunHolder.GetChild(gunIdx).gameObject.SetActive(false);

        int lastGunIdx = gunIdx;
        do { gunIdx = Random.Range(0, gunHolder.childCount); }
        while(gunIdx == lastGunIdx);

        gunHolder.GetChild(gunIdx).gameObject.SetActive(true);
    }

    private IEnumerator OpenCoroutine()
    {
        if(m_available)
        {
            float stopTime = 8.7f;
            int numOfGuns = 10;
            float interval = stopTime / numOfGuns;
            int childIndex = 0;

            audioSource.Play();
            anim.Play("Open");

            gunHolder.GetChild(childIndex).gameObject.SetActive(true); //Activate the first one

            while(audioSource.time <= stopTime)
            {
                if(audioSource.time % interval <= 0.01f)
                {
                    gunHolder.GetChild(childIndex++).gameObject.SetActive(false);

                    childIndex = (int)Mathf.Repeat(childIndex, gunHolder.childCount - 1);
                }

                yield return new WaitForEndOfFrame();
            }

            //Finished
            audioSource.PlayOneShot(audioClip);
            m_available = true;
        }

        yield return null;
    }
}
