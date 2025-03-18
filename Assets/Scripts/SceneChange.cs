using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SceneChange : MonoBehaviour
{
    [Header("��� �̹��� ����")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Sprite[] backgroundSprites;

    private int currentImageIndex = 0;

    private void Start()
    {
       StartCoroutine(ChangeBackgroundImage());
    }

    /// <summary>
    /// ���� ���� ��ư�� ������ �� ȣ��
    /// </summary>
    public void StartGame()
    {
        SceneManager.LoadScene("InGame"); // �ΰ��� ������ �̵�
    }

    /// <summary>
    /// 1�ʸ��� ��� �̹����� �����ϴ� �ڷ�ƾ
    /// </summary>
    private IEnumerator ChangeBackgroundImage()
    {
        while (true)
        {
            if (backgroundSprites.Length > 0)
            {
                backgroundImage.sprite = backgroundSprites[currentImageIndex]; // �̹��� ����
                currentImageIndex = (currentImageIndex + 1) % backgroundSprites.Length; // �ε��� ��ȯ
            }
            yield return new WaitForSeconds(1f);
        }
    }
}
