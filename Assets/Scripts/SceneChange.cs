using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class SceneChange : MonoBehaviour
{
    /// <summary>
    /// ���� ���� ��ư�� ������ �� ȣ��
    /// </summary>
    public void StartGame()
    {
        SceneManager.LoadScene("InGame"); // �ΰ��� ������ �̵�
    }


}
