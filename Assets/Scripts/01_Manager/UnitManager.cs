using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    public static UnitManager Instance { get; private set; }

    [Header("애니메이션 데이터베이스")]
    public UnitAnimationDatabase animationDatabase;

    // unitId를 가진 유닛이 여러 개 존재할 수 있기 때문에 같은 ID로 List로 묶어둔다.
    private Dictionary<int, List<Unit>> unitDictionary = new Dictionary<int, List<Unit>>();

    // 유닛 리스트.
    private List<Unit> playerUnit = new List<Unit>();
    private List<Unit> enemyUnit = new List<Unit>();

    // 유닛 전체 수.
    private int allUnitCount;

    private void Awake()
    {
        if (!Instance)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        allUnitCount = 0;
    }

    /// <summary>
    /// 스테이지에서 유닛 배치
    /// </summary>
    /// <param name="stageData"></param>
    public void LoadUnit(StageData stageData)
    {
        foreach(UnitSpawnData spawnData in stageData.units)
        {
            if(UnitDatabase.Instance.HasUnitData(spawnData.unitTypeId))
            {
                Tile tile = GridManager.Instance.GetTile(new Vector2Int(spawnData.startX, spawnData.startY));
                
                if(tile)
                {
                    RegisterUnit(spawnData, tile);
                }
            }
        }
        Debug.Log($"UnitManager All count : {allUnitCount}");

    }

    /// <summary>
    /// 유닛 배치되는 메서드
    /// </summary>
    /// <param name="spawnData">배치될 데이터</param>
    /// <param name="tile">배치할 타일위치</param>
    public void RegisterUnit(UnitSpawnData spawnData, Tile tile)
    {
        // 오브젝트 풀에서 유닛 가져오기
        Unit unit = GameManager.Instance.GetUnit();
        Debug.Log($"Spawndata는 무엇일까여? {spawnData}");
        unit.Initialize(spawnData, animationDatabase.GetAnimationData(spawnData.unitTypeId));
        unit.transform.position = tile.transform.position;

        // 현재 타일에 유닛 등록
        unit.currentTile = tile;
        tile.isOccupied = true;

        // 같은 unitID를 가진 유닛들을 리스트로 저장
        if(!unitDictionary.ContainsKey(spawnData.unitTypeId))
        {
            unitDictionary[spawnData.unitTypeId] = new List<Unit>();
        }
        unitDictionary[spawnData.unitTypeId].Add(unit);
        Debug.Log($"UnitManager {spawnData.unitTypeId} count : {unitDictionary[spawnData.unitTypeId].Count}");

        // 팀에 따른 유닛리스트 추가
        if(unit.unitData.unitTeam == E_UnitTeam.Ally)
        {
            playerUnit.Add(unit);
        }
        else if(unit.unitData.unitTeam == E_UnitTeam.Enemy)
        {
            enemyUnit.Add(unit);
        }

        // 전체 유닛 수 카운트
        allUnitCount++;
    }

    /// <summary>
    /// unitId에 해당하는 모든 유닛을 가져오기
    /// </summary>
    /// <param name="unitId">가져올 유닛 정보의 ID</param>
    /// <returns></returns>
    public List<Unit> GetUnitsByType(int unitId)
    {
        return unitDictionary.ContainsKey(unitId) ? unitDictionary[unitId] : new List<Unit>();
    }

    /// <summary>
    /// 특정 유닛 삭제
    /// </summary>
    /// <param name="unit">삭제시킬 유닛</param>
    public void RemoveUnit(Unit unit)
    {
        if(unitDictionary.ContainsKey(unit.unitData.unitId))
        {
            unitDictionary[unit.unitData.unitId].Remove(unit);
        }

        if (unit.unitData.unitTeam == E_UnitTeam.Ally)
        {
            playerUnit.Remove(unit);
        }
        else if (unit.unitData.unitTeam == E_UnitTeam.Enemy)
        {
            enemyUnit.Remove(unit); 
        }

        // 유닛 반환 (풀링 활용)
        GameManager.Instance.ReturnUnit(unit);
    }

    /// <summary>
    /// 아군 리스트 반환
    /// </summary>
    /// <returns></returns>
    public List<Unit> GetPlayerUnits()
    {
        return playerUnit;
    }

    /// <summary>
    /// 적군 리스트 반환
    /// </summary>
    /// <returns></returns>
    public List<Unit> GetEnemyUnits()
    {
        return enemyUnit;
    }

    /// <summary>
    /// 현재 필드에 존재하는 모든 유닛을 가져옴.
    /// </summary>
    /// <returns></returns>
    public List<Unit> GetAllUnits()
    {
        List<Unit> allUnits = new List<Unit>();
        foreach (var unitList in unitDictionary.Values)
        {
            allUnits.AddRange(unitList);
        }
        return allUnits;
    }

    /// <summary>
    /// 스테이지 변경시 기존 유닛 초기화(오브젝트 풀로 반환)
    /// </summary>
    public void ResetUnits()
    {
        foreach(var unitList in unitDictionary.Values)
        {
            foreach(var unit in unitList)
            {
                GameManager.Instance.ReturnUnit(unit);
            }
        }
        unitDictionary.Clear();
        playerUnit.Clear();
        enemyUnit.Clear();
    }

    /// <summary>
    /// 특정 위치에 존재하는 유닛 반환
    /// </summary>
    /// <param name="position">유닛 존재 여부를 반환할 타일 위치</param>
    /// <returns></returns>
    public Unit GetUnitAtPosition(Vector2Int position)
    {
        // 해당 위치의 유닛 반환
        foreach (var unitList in unitDictionary.Values)
        {
            foreach (var unit in unitList)
            {
                if (unit.currentTile && unit.currentTile.vec2IntPos == position)
                {
                    return unit;
                }
            }
        }
        // 유닛이 없으면 null 반환
        return null;
    }

    // 주변 유닛 탐색
    public Unit FindNearestUnit(Unit unitList)
    {
        Unit nearestUnit = null;
        int minDistance = int.MaxValue;

        if (unitList.unitData.unitTeam != E_UnitTeam.Ally)
        {
            foreach (Unit playerUnit in playerUnit)
            {
                int distance = (int)Vector2Int.Distance(unitList.currentTile.vec2IntPos, playerUnit.currentTile.vec2IntPos);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestUnit = playerUnit;
                }
            }
        }
        else
        {
            foreach (Unit enemyUnit in enemyUnit)
            {
                int distance = (int)Vector2Int.Distance(unitList.currentTile.vec2IntPos, enemyUnit.currentTile.vec2IntPos);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestUnit = enemyUnit;
                }
            }
        }
        return nearestUnit;
    }

    /// <summary>
    /// 특정 unitId를 가진 유닛 반환
    /// 동일한 ID가 여러 개 있을 경우, 가장 가까운 유닛을 반환.
    /// </summary>
    /// <param name="unitId"></param>
    /// <param name="team"></param>
    /// <param name="position"></param>
    /// <returns></returns>
    public Unit GetUnitById(int unitId, E_UnitTeam team, Vector2Int position)
    {
        // 특정 팀에서 unitId를 가진 유닛 리스트 가져오기
        List<Unit> unitList = GetUnitsByType(unitId).Where(u => u.unitData.unitTeam == team).ToList();

        // 찾는 유닛이 없음
        if (unitList.Count == 0)
        {
            return null;
        }
        // 하나뿐이라면 바로 반환
        else if (unitList.Count == 1)
        {
            return unitList[0];
        }

        // 같은 ID를 가진 유닛이 여러 개 있을 경우, 가장 가까운 유닛 반환
        return unitList.OrderBy(u => Vector2Int.Distance(position, u.currentTile.vec2IntPos)).First();
    }
}
