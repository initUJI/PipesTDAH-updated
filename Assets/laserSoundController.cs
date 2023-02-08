using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class laserSoundController : MonoBehaviour
{
    [SerializeField] AudioSource laserAudio;


    public void playSound()
    {
        laserAudio.Play();
    }

    public void stopSound()
    {
        laserAudio.Stop();
    }
}
