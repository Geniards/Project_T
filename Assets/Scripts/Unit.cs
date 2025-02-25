using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    // 유닛 기본 정보
    public UnitData unitData; // 유닛 기본 정보
    public Tile currentTile; // 유닛이 현재 위치한 타일 위치

    // 유닛 행동 상태 정보
    public E_UnitState unitState;
    private bool isSelected = false;

    /// <summary>
    /// 유닛 초기화
    /// </summary>
    /// <param name="spawnData">유닛 초기화 정보</param>
    public void Initialize(UnitSpawnData spawnData)
    {
        unitState = E_UnitState.Idle;

        // UnitDatabase에서 unitId를 기반으로 데이터를 가져옴
        unitData = UnitDatabase.Instance.GetUnitDataById(spawnData.unitTypeId);
        unitData.unitTeam = (E_UnitTeam)spawnData.unitTeam;

        if (unitData == null)
        {
            Debug.LogError($"UnitData를 찾을 수 없습니다! unitTypeId: {spawnData.unitTypeId}");
        }
    }

    /// <summary>
    /// 유닛 선택
    /// </summary>
    public void Select()
    {
        // 이미 선택된 경우 중복 처리 방지
        if (isSelected) return;

        isSelected = true;
        currentTile.HighlightTile(new Color(1f, 1f, 0f, 0.3f)); // 노란색 하이라이트
        GridManager.Instance.FindWalkableTiles(this);
    }

    /// <summary>
    /// 유닛 선택 해제
    /// </summary>
    public void Deselect()
    {
        // 선택되지 않은 경우 무시
        if (!isSelected) return;
        isSelected = false;
        currentTile.ClearHighlight();
        GridManager.Instance.ClearWalkableTiles();
        //GridManager.Instance.ClearAttackableTiles();
    }

    /// <summary>
    /// 유닛 이동
    /// </summary>
    /// <param name="targetTile"></param>
    public void MoveTo(Tile targetTile)
    {
        Tile moveTile = GridManager.Instance.FindNearestReachableTile(this, targetTile);
        List<Tile> path = GridManager.Instance.FindPathAStar(currentTile, moveTile);

        if (path.Count > 0)
        {
            StartCoroutine(MoveAlongPath(path));
        }
    }

    /// <summary>
    /// AI 유닛 이동
    /// </summary>
    /// <param name="targetTile"></param>
    /// <returns></returns>
    public IEnumerator MoveToAI(Tile targetTile)
    {
        Tile moveTile = GridManager.Instance.FindNearestReachableTile(this, targetTile);
        List<Tile> path = GridManager.Instance.FindPathAStar(currentTile, moveTile);

        if (path.Count > 0)
        {
            yield return StartCoroutine(MoveAlongPath(path));
        }
    }

    /// <summary>
    /// A* 경로를 따라 이동
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private IEnumerator MoveAlongPath(List<Tile> path)
    {
        foreach (Tile tile in path)
        {
            transform.position = tile.transform.position;
            yield return new WaitForSeconds(0.2f);
        }

        currentTile.isOccupied = false;
        SetCurrentTile(path[path.Count - 1]);
        currentTile.isOccupied = true;
        GridManager.Instance.FindAttackableTiles(this);
        Deselect();
    }

    /// <summary>
    /// 유닛이 현재 위치한 타일을 설정
    /// </summary>
    /// <param name="tile">유닛이 이동할 타일</param>
    public void SetCurrentTile(Tile tile)
    {
        if (currentTile)
        {
            // 이전 타일에 캐릭터 해제
            currentTile.isOccupied = false;
        }

        currentTile = tile;
        currentTile.isOccupied = true; // 새 타일에 캐릭터 올라옴
        transform.position = tile.transform.position; // 유닛 위치 업데이트
    }

    /// <summary>
    /// 유닛이 자신의 턴을 수행
    /// </summary>
    /// <returns></returns>
    public IEnumerator TakeTurn()
    {
#if UNITY_EDITOR
        Debug.Log($"유닛 {unitData.unitId} 턴 시작");
#endif
        // 1. 공격 가능한 범위를 하이라이트
        GridManager.Instance.FindAttackableTiles(this);
        yield return new WaitForSeconds(0.5f);

        // 2. 즉시 공격 가능한 유닛이 있는지 확인
        Unit target = FindAttackableUnit();
        if (target)
        {
            GridManager.Instance.ShowHiggLight();
            yield return new WaitForSeconds(0.5f);
            Attack(target);
            TurnManager.Instance.OnUnitTurnCompleted();
            yield break;
        }

        // 3. 공격할 유닛이 없다면 이동 가능 타일 탐색
        GridManager.Instance.FindWalkableTiles(this);
        yield return new WaitForSeconds(0.5f);

        // 4. 가장 가까운 적 탐색
        Tile targetTile = UnitManager.Instance.FindNearestUnit(this)?.currentTile;
        if (targetTile)
        {
            yield return MoveToAI(targetTile);
            yield return new WaitForSeconds(0.5f);

            // 5. 이동 후 다시 공격 가능 여부 확인
            target = FindAttackableUnit();
            if (target)
            {
                Debug.Log("공격합니다~~~");
                GridManager.Instance.ShowHiggLight();
                yield return new WaitForSeconds(0.5f);
                Attack(target);
            }
        }

        // 6. 턴 종료
        unitState = E_UnitState.Complete;
        GridManager.Instance.ClearWalkableTiles();
        yield return new WaitForSeconds(0.3f);
        GridManager.Instance.ClearAttackableTiles();
        TurnManager.Instance.OnUnitTurnCompleted();
        yield return new WaitForSeconds(0.3f);
    }

    /// <summary>
    /// 공격 가능한 유닛 탐색
    /// </summary>
    /// <returns></returns>
    public Unit FindAttackableUnit()
    {
        foreach (Tile tile in GridManager.Instance.attackableTiles)
        {
            Unit target = UnitManager.Instance.GetUnitAtPosition(tile.vec2IntPos);
            if (target && target.unitData.unitTeam != unitData.unitTeam)
            {
                return target;
            }
        }
#if UNITY_EDITOR
        Debug.Log($"{unitData.unitId} 공격 가능한 유닛 없음");
#endif
        return null;
    }

    /// <summary>
    /// 유닛 공격
    /// </summary>
    /// <param name="target"></param>
    public void Attack(Unit target)
    {
        if (!target) return;
#if UNITY_EDITOR
        Debug.Log($"{unitData.unitId}가 {target.unitData.unitId}를 공격!");
#endif
        if (unitState == E_UnitState.Complete) return;

        // 데미지 계산 및 공격 실행
        target.TakeDamage(unitData.attackPower);
        unitState = E_UnitState.Complete;
        Deselect();
    }

    /// <summary>
    /// 데미지 입었을때
    /// </summary>
    /// <param name="damage"></param>
    public void TakeDamage(int damage)
    {
        unitData.hP -= damage;
#if UNITY_EDITOR
        Debug.Log($"{unitData.unitId}가 {damage}의 피해를 입음! 남은 체력: {unitData.hP}");
#endif
        if (unitData.hP <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// 유닛 사망 처리
    /// </summary>
    private void Die()
    {
        Debug.Log($"{unitData.unitId} 사망!");
        currentTile.isOccupied = false;
        UnitManager.Instance.RemoveUnit(this);
    }
}
