using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public int currentStageIndex;   // ���� �������� ��ȣ
    public int turnCount;           // ���� �� ��ȣ
    public bool isPlayerTurn;       // �÷��̾� �� ����
    public string saveTime;
    public List<UnitSaveData> units;// ���� ������ ����Ʈ
    public List<int> unitActions;   // �ൿ�� ���� ���� ���
}

[Serializable]
public class UnitSaveData
{
    public int unitId;        // ���� ���� ID
    public int team;          // ���� �� (�Ʊ� / ����)

    public int level;         // ���� ����
    public float exp;           // ���� ����ġ
    public float maxExp;        // ������ �ʿ� ����ġ

    public float hp;            // ���� ü��
    public float maxHp;         // �ִ� ü��
    public float mana;          // ���� ����
    public float maxMana;       // �ִ� ����

    public int attackPower;   // ���ݷ�
    public int attackRange;   // ���� ����
    public int moveRange;     // �̵� �Ÿ�

    public int x;             // ���� X ��ǥ
    public int y;             // ���� Y ��ǥ
    public bool hasActed;     // �ൿ �Ϸ� ����
}
