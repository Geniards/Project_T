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
        if(Input.GetKeyDown(KeyCode.S))
        {
            CheckPlayerTurnEnd();
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            GridManager.Instance.ShowHighLight();
        }
    }

    /// <summary>
    /// �� ����(�Ʊ� / ����)
    /// </summary>
    public void StartTurn()
    {
        GridManager.Instance.ClearAttackableTiles();
        if (isPlayerTurn)
        {
            Debug.Log("�Ʊ� �� ����!");
            StartPlayerTurn();
        }
        else
        {
            Debug.Log("���� �� ����!");
            StartCoroutine(EnemyTurnRoutine());
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
}
