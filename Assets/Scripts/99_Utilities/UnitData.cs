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
    public float hP;
    public float maxHP;
    public float mana;
    public float maxMana;
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
