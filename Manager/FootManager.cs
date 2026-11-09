using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FootManager : MonoBehaviour
{
    public float footDistance;
    public Vector3 lastTransform;
    public Transform cenrrentTransfom;
    public AudioSource footAudio;
    public float timer;
   /* private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= 1)
        {
            timer = 0;
            footDistance += Vector3.Distance(cenrrentTransfom.position, lastTransform);
            if (footDistance > 0.1 && !footAudio.isPlaying)
            {
                footAudio.Play();
                footDistance = 0;
            }
            else
            {
                footAudio.Stop();
            }
            lastTransform = cenrrentTransfom.position;
        }
    }*/
   public void WalkVolume()
    {
        footAudio.volume = 0.3f;
    }public void RunVolume()
    {
        footAudio.volume = 0.5f;
    }public void SprRunVolume()
    {
        footAudio.volume = 0.7f;
    }
    public void FootSoundPlay()
    {
        footAudio.Play();
    }
    public void FootSoundStop()
    {
        footAudio.Stop();
    }
}
