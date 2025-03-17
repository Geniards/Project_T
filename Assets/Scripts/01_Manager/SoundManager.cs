using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("����� �ҽ�")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("���� ����")]
    [Range(0f, 1f)] 
    public float bgmVolume = 1f;
    [Range(0f, 1f)] 
    public float sfxVolume = 1f;

    [Header("����� Ŭ��")]
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
    /// BGM ��� (���� BGM�� �ٸ� ��츸 ����)
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
    /// ȿ���� ���
    /// </summary>
    public void PlaySFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    /// <summary>
    /// ���� ���� (UI���� ȣ�� ����)
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
