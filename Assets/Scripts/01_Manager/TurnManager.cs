using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance {  get; private set; }
    
    // 아군 - 리스트 / 유저가 직접 선택
    // 적군 - 큐 / FIFO를 이용한 자동선택
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
    /// 턴 시작(아군 / 적군)
    /// </summary>
    public void StartTurn()
    {
        StartCoroutine(StartTurnSequence());
    }

    private IEnumerator StartTurnSequence()
    {
        // 턴 UI 표시
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
    /// 플레이어 턴 시작
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
    /// 아군 유닛 행동 완료 확인.
    /// </summary>
    public void CheckPlayerTurnEnd()
    {
        foreach (Unit unit in playerUnits)
        {
            // 아직 유닛의 행동이 끝나지 않았다면 종료되지 않음.
            if (unit.unitState != E_UnitState.Complete)
            {
                Debug.Log("아군 턴 진행 중");
                return;
            }
        }

        Debug.Log($"{playerUnits.Count}");
        Debug.Log("아군 턴 종료!");
        EndTurn();
    }

    /// <summary>
    /// 적군 자동 진행.
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

            Debug.Log("적유닛 행동완료.");
        }
    }

    private void EndTurn()
    {
        // 턴 변경
        isPlayerTurn = !isPlayerTurn;
        if (isPlayerTurn)
        {
            turnCount++;
        }

        // 승리/ 패배 체크
        GameManager.Instance.CheckGameState();

        StartTurn();
    }

    /// <summary>
    /// 적군 유닛이 자신의 턴을 완료했을 때 호출
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
    /// 적군 행동 순서를 랜덤하게 변경
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
