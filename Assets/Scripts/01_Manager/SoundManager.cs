using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("오디오 소스")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("볼륨 조절")]
    [Range(0f, 1f)] 
    public float bgmVolume = 1f;
    [Range(0f, 1f)] 
    public float sfxVolume = 1f;

    [Header("오디오 클립")]
    public AudioClip battleBGM;
    public AudioClip moveSFX;
    public AudioClip attackSFX;
    public AudioClip dialogueSFX;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// BGM 재생 (이전 BGM과 다를 경우만 변경)
    /// </summary>
    public void PlayBGM(AudioClip clip)
    {
        if (bgmSource.clip == clip) return;

        bgmSource.Stop();
        bgmSource.clip = clip;
        bgmSource.volume = bgmVolume;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    /// <summary>
    /// 효과음 재생
    /// </summary>
    public void PlaySFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    /// <summary>
    /// 볼륨 조절 (UI에서 호출 가능)
    /// </summary>
    public void SetBGMVolume(float volume)
    {
        bgmVolume = volume;
        bgmSource.volume = bgmVolume;
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = volume;
        sfxSource.volume = sfxVolume;
    }
}
