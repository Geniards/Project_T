using System;
using System.Collections.Generic;

[Serializable]
public class DialogueData
{
    public List<DialogueEntry> dialogues;
}

[Serializable]
public class DialogueEntry
{
    public string name;      // ��ȭ�ϴ� ĳ���� �̸�
    public string portrait;  // ĳ���� �ʻ�ȭ (Resources���� �ҷ���)
    public string BG;        // ��׶��� ����.
    public string text;      // ��ȭ ����

    public int unitId;           // ������ ���� ID (������ -1 �Ǵ� 0)
    public string animationType; // "Attack", "Idle", "Victory" ��
    public int repeatCount;      // �� �� �ݺ����� (1=�� ��, 0=���ѷ���, n= n��)
}
