using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ���� ���� ����
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

    // TODO : ��ųʸ� Json ��밡���ϰ� �Ľ��ؼ� ����.
    public Dictionary<E_UnitType, UnitInfo> rangeInfo;
}
