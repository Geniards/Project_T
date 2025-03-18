using System.Collections.Generic;

/// <summary>
/// Json 구조
/// </summary>
[System.Serializable]
public class StageData
{
    public int stageId;
    public string stageName;
    public int width;
    public int height;
    public List<TileLegendEntry> tileLegendList;
    public string[] tileMap;
    public string imagePath;
    public string bgmFileName;

    // 유닛 배치 정보
    public List<UnitSpawnData> units;

    public Dictionary<string, TileInfo> tileLegend;

    public void ConvertTileLegend()
    {
        tileLegend = new Dictionary<string, TileInfo>();
        foreach (var entry in tileLegendList)
        {
            tileLegend[entry.key] = entry.value;
        }
    }

}

[System.Serializable]
public class TileLegendEntry
{
    public string key;
    public TileInfo value;
}