using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    public static UnitManager Instance { get; private set; }

    // unitId�� ���� ������ ���� �� ������ �� �ֱ� ������ ���� ID�� List�� ����д�.
    private Dictionary<int, List<Unit>> unitDictionary = new Dictionary<int, List<Unit>>();

    // ���� ����Ʈ.
    private List<Unit> playerUnit = new List<Unit>();
    private List<Unit> enemyUnit = new List<Unit>();

    // ���� ��ü ��.
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
    /// ������������ ���� ��ġ
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
    /// ���� ��ġ�Ǵ� �޼���
    /// </summary>
    /// <param name="spawnData">��ġ�� ������</param>
    /// <param name="tile">��ġ�� Ÿ����ġ</param>
    public void RegisterUnit(UnitSpawnData spawnData, Tile tile)
    {
        // ������Ʈ Ǯ���� ���� ��������
        Unit unit = GameManager.Instance.GetUnit();
        unit.Initialize(spawnData);
        unit.transform.position = tile.transform.position;

        // ���� Ÿ�Ͽ� ���� ���
        unit.currentTile = tile;
        tile.isOccupied = true;

        // ���� unitID�� ���� ���ֵ��� ����Ʈ�� ����
        if(!unitDictionary.ContainsKey(spawnData.unitTypeId))
        {
            unitDictionary[spawnData.unitTypeId] = new List<Unit>();
        }
        unitDictionary[spawnData.unitTypeId].Add(unit);
        Debug.Log($"UnitManager {spawnData.unitTypeId} count : {unitDictionary[spawnData.unitTypeId].Count}");

        // ���� ���� ���ָ���Ʈ �߰�
        if(unit.unitData.unitTeam == E_UnitTeam.Ally)
        {
            playerUnit.Add(unit);
        }
        else if(unit.unitData.unitTeam == E_UnitTeam.Enemy)
        {
            enemyUnit.Add(unit);
        }

        // ��ü ���� �� ī��Ʈ
        allUnitCount++;
    }

    /// <summary>
    /// unitId�� �ش��ϴ� ��� ������ ��������
    /// </summary>
    /// <param name="unitId">������ ���� ������ ID</param>
    /// <returns></returns>
    public List<Unit> GetUnitsByType(int unitId)
    {
        return unitDictionary.ContainsKey(unitId) ? unitDictionary[unitId] : new List<Unit>();
    }

    /// <summary>
    /// Ư�� ���� ����
    /// </summary>
    /// <param name="unit">������ų ����</param>
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

        // ���� ��ȯ (Ǯ�� Ȱ��)
        GameManager.Instance.ReturnUnit(unit);
    }

    /// <summary>
    /// �Ʊ� ����Ʈ ��ȯ
    /// </summary>
    /// <returns></returns>
    public List<Unit> GetPlayerUnits()
    {
        return playerUnit;
    }

    /// <summary>
    /// ���� ����Ʈ ��ȯ
    /// </summary>
    /// <returns></returns>
    public List<Unit> GetEnemyUnits()
    {
        return enemyUnit;
    }

    /// <summary>
    /// ���� �ʵ忡 �����ϴ� ��� ������ ������.
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
    /// �������� ����� ���� ���� �ʱ�ȭ(������Ʈ Ǯ�� ��ȯ)
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
    /// Ư�� ��ġ�� �����ϴ� ���� ��ȯ
    /// </summary>
    /// <param name="position">���� ���� ���θ� ��ȯ�� Ÿ�� ��ġ</param>
    /// <returns></returns>
    public Unit GetUnitAtPosition(Vector2Int position)
    {
        // �ش� ��ġ�� ���� ��ȯ
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
        // ������ ������ null ��ȯ
        return null;
    }

    // �ֺ� ���� Ž��
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
}
