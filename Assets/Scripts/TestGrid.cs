using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestGrid : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("�׸��� �ٽ� �ε�");
            GridManager.Instance.LoadGrid(GameManager.Instance.currentStageData);
        }
    }
}
