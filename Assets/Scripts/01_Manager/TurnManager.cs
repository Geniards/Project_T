using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance {  get; private set; }
    
    // �Ʊ� - ����Ʈ / ������ ���� ����
    // ���� - ť / FIFO�� �̿��� �ڵ�����
    private List<Unit> playerUnits = new List<Unit>();
    private Queue<Unit> enemyUnits = new Queue<Unit>();

    private bool isPlayerTurn = true;
    private int turnCount = 1;

    private void Awake()
    {
        if(!Instance)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        //if(Input.GetKeyDown(KeyCode.S))
        //{
        //    CheckPlayerTurnEnd();
        //}
    }

    /// <summary>
    /// �� ����(�Ʊ� / ����)
    /// </summary>
    public void StartTurn()
    {
        StartCoroutine(StartTurnSequence());
    }

    private IEnumerator StartTurnSequence()
    {
        // �� UI ǥ��
        yield return UIManager.Instance.ShowTurnUI(isPlayerTurn, turnCount);

        GridManager.Instance.ClearAttackableTiles();
        if (isPlayerTurn)
        {
            StartPlayerTurn();
        }
        else
        {
            StartCoroutine(EnemyTurnRoutine());
        }

        foreach (Unit unit in UnitManager.Instance.GetAllUnits())
        {
            unit.ResetTurn();
        }
    }

    /// <summary>
    /// �÷��̾� �� ����
    /// </summary>
    private void StartPlayerTurn()
    {
        playerUnits = UnitManager.Instance.GetPlayerUnits();

        foreach (Unit unit in playerUnits)
        {
            unit.unitState = E_UnitState.Idle;
        }
    }

    /// <summary>
    /// �Ʊ� ���� �ൿ �Ϸ� Ȯ��.
    /// </summary>
    public void CheckPlayerTurnEnd()
    {
        foreach (Unit unit in playerUnits)
        {
            // ���� ������ �ൿ�� ������ �ʾҴٸ� ������� ����.
            if (unit.unitState != E_UnitState.Complete)
            {
                Debug.Log("�Ʊ� �� ���� ��");
                return;
            }
        }

        Debug.Log($"{playerUnits.Count}");
        Debug.Log("�Ʊ� �� ����!");
        EndTurn();
    }

    /// <summary>
    /// ���� �ڵ� ����.
    /// </summary>
    /// <returns></returns>
    private IEnumerator EnemyTurnRoutine()
    {
        enemyUnits.Clear();

        foreach (Unit unit in UnitManager.Instance.GetEnemyUnits())
        {
            unit.unitState = E_UnitState.Idle;
            enemyUnits.Enqueue(unit);
        }

        while (enemyUnits.Count > 0)
        {
            Unit enemy = enemyUnits.Dequeue();
            yield return StartCoroutine(enemy.TakeTurn());

            Debug.Log("������ �ൿ�Ϸ�.");
        }
    }

    private void EndTurn()
    {
        // �� ����
        isPlayerTurn = !isPlayerTurn;
        if (isPlayerTurn)
        {
            turnCount++;
        }

        // �¸�/ �й� üũ
        GameManager.Instance.CheckGameState();

        StartTurn();
    }

    /// <summary>
    /// ���� ������ �ڽ��� ���� �Ϸ����� �� ȣ��
    /// </summary>
    public void OnUnitTurnCompleted()
    {
        if (!isPlayerTurn && enemyUnits.Count == 0)
        {
            EndTurn();
        }
    }

    public int GetTurnCount()
    {
        return turnCount;
    }


    /// <summary>
    /// ���� �ൿ ������ �����ϰ� ����
    /// </summary>
    /// <param name="enemyQueue"></param>
    private void ShuffleEnemyQueue(Queue<Unit> enemyQueue)
    {
        List<Unit> enemyList = new List<Unit>(enemyQueue);

        // Fisher-Yates Shuffle
        for (int i = enemyList.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (enemyList[i], enemyList[j]) = (enemyList[j], enemyList[i]); // Swap
        }

        enemyQueue.Clear();
        foreach (Unit unit in enemyList)
        {
            enemyQueue.Enqueue(unit);
        }
    }

    public void ResetTurn()
    {
        turnCount = 1;
        isPlayerTurn = true;
    }

    public bool IsPlayerTurn()
    {
        return isPlayerTurn;
    }

    public void SetTurnCount(int count)
    {
        turnCount = count;
    }

    public void SetPlayerTurn(bool isTurn)
    {
        isPlayerTurn = isTurn;
        playerUnits = UnitManager.Instance.GetPlayerUnits();
    }
}
