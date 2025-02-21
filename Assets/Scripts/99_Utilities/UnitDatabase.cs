using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitDatabase : MonoBehaviour
{
    public static UnitDatabase Instance { get; private set; }

    public TextAsset unitJsonFile; // JSON 파일 (유닛 정보)
    private Dictionary<int, UnitData> unitDataDictionary = new Dictionary<int, UnitData>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            LoadUnitData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// JSON에서 유닛 데이터 로드
    /// </summary>
    private void LoadUnitData()
    {
        UnitDataCollection dataCollection = JsonUtility.FromJson<UnitDataCollection>(unitJsonFile.text);
        
        foreach (var unitData in dataCollection.units)
        {
            unitDataDictionary[unitData.unitId] = unitData;
            unitDataDictionary[unitData.unitId].IntToUnitType();
        }
        Debug.Log($"[UnitDatabase] 모든 유닛 데이터 로드 완료 - 총 {unitDataDictionary.Count}개");
    }

    /// <summary>
    /// 특정 unitId에 해당하는 유닛 데이터를 반환
    /// </summary>
    /// <param name="unitId">데이터를 반환할 유닛ID</param>
    /// <returns></returns>
    public UnitData GetUnitDataById(int unitId)
    {
        return unitDataDictionary.ContainsKey(unitId) ? unitDataDictionary[unitId] : null;
    }

    /// <summary>
    /// 특정 unitId에 해당하는 유닛 데이터 존재 여부 반환
    /// </summary>
    /// <param name="unitId"></param>
    /// <returns></returns>
    public bool HasUnitData(int unitId)
    {
        return unitDataDictionary.ContainsKey(unitId);
    }
}

/// <summary>
/// JSON에서 여러 개의 유닛 데이터를 로드할 때 사용
/// </summary>
[System.Serializable]
public class UnitDataCollection
{
    public List<UnitData> units;
}