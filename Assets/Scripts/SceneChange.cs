using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class SceneChange : MonoBehaviour
{
    /// <summary>
    /// 게임 시작 버튼을 눌렀을 때 호출
    /// </summary>
    public void StartGame()
    {
        SceneManager.LoadScene("InGame"); // 인게임 씬으로 이동
    }


}
