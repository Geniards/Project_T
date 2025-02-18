using System.Collections.Generic;

/// <summary>
/// Json ����
/// </summary>
[System.Serializable]
public class StageData
{
    public int stageId;
    public string stageName;
    public int width;
    public int height;
    public Dictionary<string, TileInfo> tileLegend;
    public string[] tileMap;
}
