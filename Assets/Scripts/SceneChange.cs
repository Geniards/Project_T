using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SceneChange : MonoBehaviour
{
    [Header("배경 이미지 설정")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Sprite[] backgroundSprites;

    private int currentImageIndex = 0;

    private void Start()
    {
       StartCoroutine(ChangeBackgroundImage());
    }

    /// <summary>
    /// 게임 시작 버튼을 눌렀을 때 호출
    /// </summary>
    public void StartGame()
    {
        SceneManager.LoadScene("InGame"); // 인게임 씬으로 이동
    }

    /// <summary>
    /// 1초마다 배경 이미지를 변경하는 코루틴
    /// </summary>
    private IEnumerator ChangeBackgroundImage()
    {
        while (true)
        {
            if (backgroundSprites.Length > 0)
            {
                backgroundImage.sprite = backgroundSprites[currentImageIndex]; // 이미지 변경
                currentImageIndex = (currentImageIndex + 1) % backgroundSprites.Length; // 인덱스 순환
            }
            yield return new WaitForSeconds(1f);
        }
    }
}
