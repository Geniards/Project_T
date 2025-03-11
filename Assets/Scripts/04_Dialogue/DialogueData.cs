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
    public string name;      // 대화하는 캐릭터 이름
    public string portrait;  // 캐릭터 초상화 (Resources에서 불러옴)
    public string BG;        // 백그라운드 내용.
    public string text;      // 대화 내용

    public int unitId;           // 연출할 유닛 ID (없으면 -1 또는 0)
    public string animationType; // "Attack", "Idle", "Victory" 등
    public int repeatCount;      // 몇 번 반복할지 (1=한 번, 0=무한루프, n= n번)
}
