using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public GameObject tilePrefab;   // Tile 프리펩
    public TextAsset jsonFile;      // JSON 파일

    // 해당 좌표의 값에 따른 타일 검색
    public Dictionary<Vector2Int, Tile> tileDictionary = new Dictionary<Vector2Int, Tile>();

    /// <summary>
    /// Json기반의 데이터로 그리드 생성.
    /// </summary>
    public void LoadGridFromJson()
    {
        // Json 파싱
        StageData stageData = JsonUtility.FromJson<StageData>(jsonFile.text);

        // 그리드 생성
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
    /// 특정 좌표의 타일을 반환
    /// </summary>
    /// <param name="position">특정 타일의 위치</param>
    /// <returns></returns>
    public Tile GetTile(Vector2Int position)
    {
        tileDictionary.TryGetValue(position, out Tile tile);
        return tile;
    }

}
