using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public int currentStageIndex;   // 현재 스테이지 번호
    public int turnCount;           // 현재 턴 번호
    public bool isPlayerTurn;       // 플레이어 턴 여부
    public string saveTime;
    public List<UnitSaveData> units;// 유닛 데이터 리스트
    public List<int> unitActions;   // 행동이 끝난 유닛 목록
}

[Serializable]
public class UnitSaveData
{
    public int unitId;        // 유닛 고유 ID
    public int team;          // 유닛 팀 (아군 / 적군)

    public int level;         // 유닛 레벨
    public float exp;           // 현재 경험치
    public float maxExp;        // 레벨업 필요 경험치

    public float hp;            // 현재 체력
    public float maxHp;         // 최대 체력
    public float mana;          // 현재 마나
    public float maxMana;       // 최대 마나

    public int attackPower;   // 공격력
    public int attackRange;   // 공격 범위
    public int moveRange;     // 이동 거리

    public int x;             // 유닛 X 좌표
    public int y;             // 유닛 Y 좌표
    public bool hasActed;     // 행동 완료 여부
}
