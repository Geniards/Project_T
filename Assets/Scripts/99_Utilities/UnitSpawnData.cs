/// <summary>
/// 스테이지 유닛 스폰 위치 정보구조
/// </summary>
[System.Serializable]
public class UnitSpawnData
{
    public int unitTypeId; // 스폰 유닛 ID
    public int startX;
    public int startY;
    public int unitTeam;
}
