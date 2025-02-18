using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public GameObject tilePrefab;   // Tile ������
    public TextAsset jsonFile;      // JSON ����

    // �ش� ��ǥ�� ���� ���� Ÿ�� �˻�
    public Dictionary<Vector2Int, Tile> tileDictionary = new Dictionary<Vector2Int, Tile>();

    /// <summary>
    /// Json����� �����ͷ� �׸��� ����.
    /// </summary>
    public void LoadGridFromJson()
    {
        // Json �Ľ�
        StageData stageData = JsonUtility.FromJson<StageData>(jsonFile.text);

        // �׸��� ����
        GenerateGridFromTileMap(stageData);
    }

    private void GenerateGridFromTileMap(StageData stageData)
    {
        for(int y = 0; y < stageData.height; y++)
        {
            for (int x = 0; x < stageData.width; x++) 
            {
                char tileCode = stageData.tileMap[y][x];

                if(stageData.tileLegend.TryGetValue(tileCode.ToString(), out TileInfo tileInfo))
                {
                    Vector2Int position = new Vector2Int(x, stageData.height - y - 1);
                    GameObject tileObj = Instantiate(tilePrefab, new Vector3(x,-y,0), Quaternion.identity);
                    Tile tile = tileObj.GetComponent<Tile>();

                    tile.SetTilePosition(position);
                    tile.SetTileProperties(tileInfo.walkable, Enum.Parse<E_TileType>(tileInfo.type));
                    tileDictionary[position] = tile;
                }
            }
        }
    }

    /// <summary>
    /// Ư�� ��ǥ�� Ÿ���� ��ȯ
    /// </summary>
    /// <param name="position">Ư�� Ÿ���� ��ġ</param>
    /// <returns></returns>
    public Tile GetTile(Vector2Int position)
    {
        tileDictionary.TryGetValue(position, out Tile tile);
        return tile;
    }

}
