using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestGrid : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("그리드 다시 로드");
            GridManager.Instance.LoadGrid(GameManager.Instance.currentStageData);
        }
    }
}
