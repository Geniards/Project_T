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
    /// 특정 슬롯에 게임 저장
    /// </summary>
    /// <param name="slotIndex"></param>
    public void SaveGame(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= 10)
        {
            Debug.LogError($"잘못된 슬롯 인덱스: {slotIndex}");
            return;
        }

        string saveFilePath = Path.Combine(saveDirectory, $"save_slot_{slotIndex}.json");

        // 게임 데이터 저장
        SaveData saveData = new SaveData
        {
            currentStageIndex = GameManager.Instance.GetCurrentStage(),
            turnCount = TurnManager.Instance.GetTurnCount(),
            isPlayerTurn = TurnManager.Instance.IsPlayerTurn(),
            units = new List<UnitSaveData>(),
            saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };

        // 유닛 정보 저장
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
        Debug.Log($"게임 저장 완료 (슬롯 {slotIndex}): {saveFilePath}");
    }

    /// <summary>
    /// 특정 슬롯에서 게임 불러오기
    /// </summary>
    /// <param name="slotIndex"></param>
    public void LoadGame(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= 10)
        {
            Debug.LogError($"잘못된 슬롯 인덱스: {slotIndex}");
            return;
        }

        string saveFilePath = Path.Combine(saveDirectory, $"save_slot_{slotIndex}.json");

        if (!File.Exists(saveFilePath))
        {
            Debug.LogWarning($"저장된 데이터가 없습니다. (슬롯 {slotIndex})");
            return;
        }

        // JSON 데이터를 먼저 저장해두고 씬을 전환
        string json = File.ReadAllText(saveFilePath);
        pendingLoadData = JsonUtility.FromJson<SaveData>(json);

        SaveLoadUIManager.Instance.OffPanel();

        // 씬 전환
        SceneManager.LoadScene("InGame");
        Debug.Log("불러오기 씬 전환");
    }

    /// <summary>
    /// 특정 슬롯의 저장 파일 경로 반환
    /// </summary>
    /// <param name="slotIndex"></param>
    /// <returns></returns>
    private string GetSaveFilePath(int slotIndex)
    {
        return Path.Combine(saveDirectory, $"save_slot_{slotIndex}.json");
    }

    /// <summary>
    /// 특정 슬롯에 저장된 데이터가 있는지 확인
    /// </summary>
    /// <param name="slotIndex"></param>
    /// <returns></returns>
    public bool IsSlotOccupied(int slotIndex)
    {
        return File.Exists(GetSaveFilePath(slotIndex));
    }

    /// <summary>
    /// 저장된 데이터를 LoadStage 이후 적용
    /// </summary>
    public void ApplyLoadedData()
    {
        if (pendingLoadData == null)
        {
            Debug.LogWarning("적용할 저장 데이터가 없습니다.");
            return;
        }

        // LoadStage 이후에 게임 데이터를 적용
        GameManager.Instance.LoadStage(pendingLoadData.currentStageIndex);
        TurnManager.Instance.SetTurnCount(pendingLoadData.turnCount);
        TurnManager.Instance.SetPlayerTurn(pendingLoadData.isPlayerTurn);

        foreach (UnitSaveData unitData in pendingLoadData.units)
        {
            E_UnitTeam team = (E_UnitTeam)unitData.team;
            Vector2Int savedPosition = new Vector2Int(unitData.x, unitData.y);

            // 저장된 유닛 위치와 가장 가까운 유닛을 찾음
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

        Debug.Log($"게임 불러오기 완료 (스테이지 {pendingLoadData.currentStageIndex})");

        pendingLoadData = null;
    }

    /// <summary>
    /// 로드할 데이터가 있는지 확인
    /// </summary>
    public bool HasPendingLoadData()
    {
        return pendingLoadData != null;
    }
}
