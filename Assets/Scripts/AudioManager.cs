using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioSource soundSource;
    [SerializeField] private AudioClipRefsSO audioClipRefsSO;

    private float soundEffectVolumeMultiplier = 0.5f;
    private float musicVolumeMultiplier = 0.5f;
    
    private void Awake()
    {
        Instance = this;
    }

    public enum BGMType
    {
        None,
        Normal,
        Normal2,
        Welcome
    }

    public void Play(AudioClip[] clipArray, float volume = 1f)
    {
        Play(clipArray[Random.Range(0, clipArray.Length)], volume * soundEffectVolumeMultiplier);
    }
    
    public void Play(AudioClip clip, float volume = 1f)
    {
        soundSource.volume = volume * soundEffectVolumeMultiplier;
        soundSource.clip = clip;
        soundSource.Play();
    }

    public void Play_BGM(BGMType bgmType)
    {
        AudioClip clip = null;
        switch (bgmType)
        {
            case BGMType.None:
                break;
            case BGMType.Normal:
                clip = Resources.Load<AudioClip>(ResourcesPath.BGM_Normal);
                break;
            case BGMType.Normal2:
                clip = Resources.Load<AudioClip>(ResourcesPath.BGM_Normal2);
                break;
            case BGMType.Welcome:
                clip = Resources.Load<AudioClip>(ResourcesPath.BGM_Welcome);
                break;
        }
        audioSource.loop = true;
        audioSource.clip = clip;
        audioSource.Play();
    }
    
    public void PlaySound_Call()
    {
        Play(audioClipRefsSO.Call);
    }
    
    public void PlaySound_NotCall()
    {
        Play(audioClipRefsSO.NotCall);
    }
    
    public void PlaySound_Pass()
    {
        Play(audioClipRefsSO.Pass);
    }
    
    public void PlaySound_Rob()
    {
        Play(audioClipRefsSO.Rob);
    }
    
    public void PlaySound_PlayCard()
    {
        Play(audioClipRefsSO.PlayCard);
    }
    
    public void PlayBGM_PokerResult(bool isWin)
    {
        audioSource.loop = false;
        AudioClip clip = Resources.Load<AudioClip>(isWin ? ResourcesPath.BGM_Win : ResourcesPath.BGM_Lose);
        audioSource.clip = clip;
        audioSource.Play();
    }

    public void SetSoundEffectVolumeMultiplier(float newValue)
    {
        soundEffectVolumeMultiplier = newValue;
    }
    
    public void SetMusicVolumeMultiplier(float newValue)
    {
        audioSource.volume = newValue;
    }
}
