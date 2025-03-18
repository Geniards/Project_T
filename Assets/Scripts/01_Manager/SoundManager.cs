using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Content;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }
    
    [Header("오디오 믹서")]
    [SerializeField] private AudioMixer audioMixer;
    
    [Header("오디오 소스")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("오디오 슬라이더")]
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("오디오 클립")]
    public AudioClip introClip;
    public AudioClip moveSFX;
    public AudioClip attackSFX;
    public AudioClip hitSFX;
    public AudioClip dialogueSFX;
    public AudioClip buttomSFX;
    public AudioClip canselSFX;
    public AudioClip victorySFX;
    public AudioClip defeatSFX;

    [SerializeField] private GameObject UICanvas;

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

    private void Start()
    {
        // 슬라이더 값 변경 시 볼륨 조절 적용(delegate로 등록)
        bgmSlider.onValueChanged.AddListener(delegate { AudioControl("BGMParameters", bgmSlider.value); });
        sfxSlider.onValueChanged.AddListener(delegate { AudioControl("SFXParameters", sfxSlider.value); });

        UICanvas.gameObject.SetActive(false);
        SceneManager.sceneLoaded += OnSceneLoaded;

        if (SceneManager.GetActiveScene().name == "00_Start")
        {
            SoundManager.Instance.PlayBGM(SoundManager.Instance.introClip);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// BGM 재생 (이전 BGM과 다를 경우만 변경)
    /// </summary>
    public void PlayBGM(AudioClip clip)
    {
        if (bgmSource.clip == clip) return;

        bgmSource.Stop();
        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    /// <summary>
    /// 효과음 재생
    /// </summary>
    public void PlaySFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }

    /// <summary>
    /// 오디오 믹서를 사용하여 볼륨 조절
    /// </summary>
    public void AudioControl(string parameter, float value)
    {
        float sound = value;

        // 최소 볼륨 설정 (완전 무음 처리)
        if (value <= 0.01f)
        {
            audioMixer.SetFloat(parameter, -80f);
        }
        else
        {
            // 0~1 값을 -80dB ~ 0dB로 변환하여 적용
            float volume = Mathf.Log10(value) * 20;
            audioMixer.SetFloat(parameter, volume);
        }
    }

    /// <summary>
    /// 전체 사운드 ON/OFF
    /// </summary>
    public void ToggleAudioVolume()
    {
        AudioListener.volume = AudioListener.volume == 0 ? 1 : 0;
    }

    public void UION()
    {
        UICanvas.gameObject.SetActive(true);
    }

    public void UIOFF()
    {
        UICanvas.gameObject.SetActive(false);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        switch (scene.name)
        {
            case "00_Start":
                SoundManager.Instance.PlayBGM(SoundManager.Instance.introClip);
                break;
        }
    }
}
