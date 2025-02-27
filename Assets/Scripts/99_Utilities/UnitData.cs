using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 유닛 정보 구조
/// </summary>
[System.Serializable]
public class UnitData
{
    public int unitId;
    public string unitName;
    public E_UnitTeam unitTeam;
    public int unitTeamNum;
    public int hP;
    public int maxHP;
    public int mana;
    public int maxMana;
    public int level;
    public float exp;
    public float maxExp;

    public int attackPower;

    // 유닛 클래스 정보
    public E_UnitType unitType;
    public int unitTypeNum;
    public int attackRange;
    public int moveRange;

    public void IntToUnitType()
    {
        unitType = (E_UnitType)unitTypeNum;
    }

    public void IntToUnitTeam()
    {
        unitTeam = (E_UnitTeam)unitTeamNum;
    }
}
