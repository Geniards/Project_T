using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveLoadManager : MonoBehaviour
{
    public static SaveLoadManager Instance { get; private set; }
    private string saveDirectory;
    private SaveData pendingLoadData;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        saveDirectory = Path.Combine(Application.persistentDataPath, "Saves");
        if (!Directory.Exists(saveDirectory))
        {
            Directory.CreateDirectory(saveDirectory);
        }
    }

    /// <summary>
    /// Ư�� ���Կ� ���� ����
    /// </summary>
    /// <param name="slotIndex"></param>
    public void SaveGame(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= 10)
        {
            Debug.LogError($"�߸��� ���� �ε���: {slotIndex}");
            return;
        }

        string saveFilePath = Path.Combine(saveDirectory, $"save_slot_{slotIndex}.json");

        // ���� ������ ����
        SaveData saveData = new SaveData
        {
            currentStageIndex = GameManager.Instance.GetCurrentStage(),
            turnCount = TurnManager.Instance.GetTurnCount(),
            isPlayerTurn = TurnManager.Instance.IsPlayerTurn(),
            units = new List<UnitSaveData>(),
            saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };

        // ���� ���� ����
        foreach (Unit unit in UnitManager.Instance.GetAllUnits())
        {
            bool hasActed = (unit.unitState == E_UnitState.Complete);

            saveData.units.Add(new UnitSaveData
            {
                unitId = unit.unitData.unitId,
                team = (int)unit.unitData.unitTeam,

                level = unit.unitData.level,
                exp = unit.unitData.exp,
                maxExp = unit.unitData.maxExp,

                hp = unit.unitData.hP,
                maxHp = unit.unitData.maxHP,
                mana = unit.unitData.mana,
                maxMana = unit.unitData.maxMana,

                attackPower = unit.unitData.attackPower,
                attackRange = unit.unitData.attackRange,
                moveRange = unit.unitData.moveRange,

                x = unit.currentTile.vec2IntPos.x,
                y = unit.currentTile.vec2IntPos.y,
                hasActed = (unit.unitState == E_UnitState.Complete)
            });
        }

        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(saveFilePath, json);
        Debug.Log($"���� ���� �Ϸ� (���� {slotIndex}): {saveFilePath}");
    }

    /// <summary>
    /// Ư�� ���Կ��� ���� �ҷ�����
    /// </summary>
    /// <param name="slotIndex"></param>
    public void LoadGame(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= 10)
        {
            Debug.LogError($"�߸��� ���� �ε���: {slotIndex}");
            return;
        }

        string saveFilePath = Path.Combine(saveDirectory, $"save_slot_{slotIndex}.json");

        if (!File.Exists(saveFilePath))
        {
            Debug.LogWarning($"����� �����Ͱ� �����ϴ�. (���� {slotIndex})");
            return;
        }

        // JSON �����͸� ���� �����صΰ� ���� ��ȯ
        string json = File.ReadAllText(saveFilePath);
        pendingLoadData = JsonUtility.FromJson<SaveData>(json);

        SaveLoadUIManager.Instance.OffPanel();

        // �� ��ȯ
        SceneManager.LoadScene("InGame");
        Debug.Log("�ҷ����� �� ��ȯ");
    }

    /// <summary>
    /// Ư�� ������ ���� ���� ��� ��ȯ
    /// </summary>
    /// <param name="slotIndex"></param>
    /// <returns></returns>
    private string GetSaveFilePath(int slotIndex)
    {
        return Path.Combine(saveDirectory, $"save_slot_{slotIndex}.json");
    }

    /// <summary>
    /// Ư�� ���Կ� ����� �����Ͱ� �ִ��� Ȯ��
    /// </summary>
    /// <param name="slotIndex"></param>
    /// <returns></returns>
    public bool IsSlotOccupied(int slotIndex)
    {
        return File.Exists(GetSaveFilePath(slotIndex));
    }

    /// <summary>
    /// ����� �����͸� LoadStage ���� ����
    /// </summary>
    public void ApplyLoadedData()
    {
        if (pendingLoadData == null)
        {
            Debug.LogWarning("������ ���� �����Ͱ� �����ϴ�.");
            return;
        }

        // LoadStage ���Ŀ� ���� �����͸� ����
        GameManager.Instance.LoadStage(pendingLoadData.currentStageIndex);
        TurnManager.Instance.SetTurnCount(pendingLoadData.turnCount);
        TurnManager.Instance.SetPlayerTurn(pendingLoadData.isPlayerTurn);

        foreach (UnitSaveData unitData in pendingLoadData.units)
        {
            E_UnitTeam team = (E_UnitTeam)unitData.team;
            Vector2Int savedPosition = new Vector2Int(unitData.x, unitData.y);

            // ����� ���� ��ġ�� ���� ����� ������ ã��
            Unit unit = UnitManager.Instance.GetUnitById(unitData.unitId, team, savedPosition);

            if (unit != null)
            {
                unit.unitData.level = unitData.level;
                unit.unitData.exp = unitData.exp;
                unit.unitData.maxExp = unitData.maxExp;

                unit.unitData.hP = unitData.hp;
                unit.unitData.maxHP = unitData.maxHp;
                unit.unitData.mana = unitData.mana;
                unit.unitData.maxMana = unitData.maxMana;

                unit.unitData.attackPower = unitData.attackPower;
                unit.unitData.attackRange = unitData.attackRange;
                unit.unitData.moveRange = unitData.moveRange;

                Tile tile = GridManager.Instance.GetTile(savedPosition);
                unit.SetCurrentTile(tile);
                unit.unitState = unitData.hasActed ? E_UnitState.Complete : E_UnitState.Idle;
            }
        }

        Debug.Log($"���� �ҷ����� �Ϸ� (�������� {pendingLoadData.currentStageIndex})");

        pendingLoadData = null;
    }

    /// <summary>
    /// �ε��� �����Ͱ� �ִ��� Ȯ��
    /// </summary>
    public bool HasPendingLoadData()
    {
        return pendingLoadData != null;
    }
}
