using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 유닛 정보 구조
/// </summary>
[System.Serializable]
public class UnitData
{
    public int unitId;
    public E_UnitTeam unitTeam;
    public int hP;
    public int maxHP;
    public int mana;
    public int maxMana;
    public int level;
    public float exp;
    public float maxExp;

    // TODO : 딕셔너리 Json 사용가능하게 파싱해서 정리.
    public Dictionary<E_UnitType, UnitInfo> rangeInfo;
}
