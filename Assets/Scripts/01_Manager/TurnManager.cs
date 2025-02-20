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

        if (Input.GetKeyDown(KeyCode.F))
        {
            foreach (Unit unit in playerUnits)
            {
                unit.unitState = E_UnitState.Complete;
                Debug.Log("�Ʊ� �� �Ϸ�");
            }
        }
    }

    /// <summary>
    /// �� ����(�Ʊ� / ����)
    /// </summary>
    public void StartTurn()
    {
        if (isPlayerTurn)
        {
            Debug.Log("�Ʊ� �� ����.");
            playerUnits = UnitManager.Instance.GetPlayerUnits();
            foreach (Unit unit in playerUnits)
            {
                unit.unitState = E_UnitState.Idle;
            }
        }
        else
        {
            Debug.Log("���� �� ����.");
            // ť �ʱ�ȭ �� ����.
            enemyUnits.Clear();
            foreach (Unit unit in UnitManager.Instance.GetEnemyUnits())
            {
                unit.unitState = E_UnitState.Idle;
                enemyUnits.Enqueue(unit);
            }
            StartCoroutine(EnemyTurnRoutine());
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
        while(enemyUnits.Count > 0)
        {
            Unit enemy = enemyUnits.Dequeue();
            yield return new WaitForSeconds(1f);
            enemy.unitState = E_UnitState.Complete;
            Debug.Log("������ �ൿ�Ϸ�.");
        }

        EndTurn();
    }

    private void EndTurn()
    {
        // �� ����
        isPlayerTurn = !isPlayerTurn;
        StartTurn();
    }
}
