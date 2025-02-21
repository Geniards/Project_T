using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitDatabase : MonoBehaviour
{
    public static UnitDatabase Instance { get; private set; }

    public TextAsset unitJsonFile; // JSON ���� (���� ����)
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
    /// JSON���� ���� ������ �ε�
    /// </summary>
    private void LoadUnitData()
    {
        UnitDataCollection dataCollection = JsonUtility.FromJson<UnitDataCollection>(unitJsonFile.text);
        
        foreach (var unitData in dataCollection.units)
        {
            unitDataDictionary[unitData.unitId] = unitData;
            unitDataDictionary[unitData.unitId].IntToUnitType();
        }
        Debug.Log($"[UnitDatabase] ��� ���� ������ �ε� �Ϸ� - �� {unitDataDictionary.Count}��");
    }

    /// <summary>
    /// Ư�� unitId�� �ش��ϴ� ���� �����͸� ��ȯ
    /// </summary>
    /// <param name="unitId">�����͸� ��ȯ�� ����ID</param>
    /// <returns></returns>
    public UnitData GetUnitDataById(int unitId)
    {
        return unitDataDictionary.ContainsKey(unitId) ? unitDataDictionary[unitId] : null;
    }

    /// <summary>
    /// Ư�� unitId�� �ش��ϴ� ���� ������ ���� ���� ��ȯ
    /// </summary>
    /// <param name="unitId"></param>
    /// <returns></returns>
    public bool HasUnitData(int unitId)
    {
        return unitDataDictionary.ContainsKey(unitId);
    }
}

/// <summary>
/// JSON���� ���� ���� ���� �����͸� �ε��� �� ���
/// </summary>
[System.Serializable]
public class UnitDataCollection
{
    public List<UnitData> units;
}