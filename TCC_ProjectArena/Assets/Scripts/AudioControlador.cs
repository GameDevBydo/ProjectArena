using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using System.IO;

public class AudioControlador : MonoBehaviour
{
    #region Audio Mixer e Sliders
    public static AudioControlador instance;
    public AudioMixer mixer;

    void Start()
    {
        instance = this;
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.K)) FadeInChant();
        if(Input.GetKeyDown(KeyCode.L)) FadeOutChant();
        if(Input.GetKeyDown(KeyCode.J)) PlayCheer();
    }

    public void AlterarVolumeMaster(float valor)
    {
        mixer.SetFloat("MasterVol", Mathf.Log10(valor) * 20);
    }
    public void AlterarVolumeSFX(float valor)
    {
        mixer.SetFloat("SFXVol", Mathf.Log10(valor) * 20);
    }
    public void AlterarVolumeBGM(float valor)
    {
        mixer.SetFloat("BGMVol", Mathf.Log10(valor) * 20);
    }

    #endregion

    /*
    #region Jukebox / Musicas

    public AudioClip[] songs;

    public AudioSource musicSpeaker, effectSpeaker;

    public void StopSong()
    {
        musicSpeaker.Stop();
    }

    public void PlaySong(int id)
    {
        if(id>=songs.Length)
        {
            id = 0;
            Debug.Log("<color=red>Index de música fora do Array.</color>");
        } 
        StopSong();
        musicSpeaker.clip = songs[id];
        musicSpeaker.Play();
    }

    public void PauseSong()
    {
        musicSpeaker.Pause();
    }
    public void UnpauseSong()
    {
        musicSpeaker.UnPause();
    }

    public void PlayEffect(AudioClip clip)
    {
        effectSpeaker.clip = clip;
        effectSpeaker.Play();
    }
    #endregion
    */

    #region Crowd Effects

    public AudioSource crowdCheer, crowdChant;

    AudioSource lerpSource;
    float lerpTime;

    public void PlayCheer()
    {
        crowdCheer.Play();
        Invoke(nameof(FadeOutChant),3.5f);
    }

    public void FadeInChant()
    {
        if(crowdChant.volume <= 0.1f)
        {
            lerpSource = crowdChant;
            lerpTime = 0.5f;
            StartCoroutine(nameof(VolumeUpLerp));
        }
    }

    public void FadeOutChant()
    {   
        if(crowdChant.volume > 0.1f)
        {
            lerpSource = crowdChant;
            lerpTime = 0.5f;
            StartCoroutine(nameof(VolumeDownLerp));
        }
    }

    IEnumerator VolumeUpLerp()
    {
        float currentTime = 0;
        while(currentTime<=lerpTime)
        {
            lerpSource.volume = Mathf.Lerp(0, 0.8f, currentTime/lerpTime);
            currentTime += Time.deltaTime;
            yield return new WaitForSeconds(Time.deltaTime);
        }
    }

    IEnumerator VolumeDownLerp()
    {
        float currentTime = 0;
        while(currentTime<=lerpTime)
        {
            lerpSource.volume = 0.8f-Mathf.Lerp(0, 0.8f, currentTime/lerpTime);
            currentTime += Time.deltaTime;
            yield return new WaitForSeconds(Time.deltaTime);
        }
    }

    #endregion  

}