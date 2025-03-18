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
    
    [Header("����� �ͼ�")]
    [SerializeField] private AudioMixer audioMixer;
    
    [Header("����� �ҽ�")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("����� �����̴�")]
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("����� Ŭ��")]
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
        // �����̴� �� ���� �� ���� ���� ����(delegate�� ���)
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
    /// BGM ��� (���� BGM�� �ٸ� ��츸 ����)
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
    /// ȿ���� ���
    /// </summary>
    public void PlaySFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }

    /// <summary>
    /// ����� �ͼ��� ����Ͽ� ���� ����
    /// </summary>
    public void AudioControl(string parameter, float value)
    {
        float sound = value;

        // �ּ� ���� ���� (���� ���� ó��)
        if (value <= 0.01f)
        {
            audioMixer.SetFloat(parameter, -80f);
        }
        else
        {
            // 0~1 ���� -80dB ~ 0dB�� ��ȯ�Ͽ� ����
            float volume = Mathf.Log10(value) * 20;
            audioMixer.SetFloat(parameter, volume);
        }
    }

    /// <summary>
    /// ��ü ���� ON/OFF
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
