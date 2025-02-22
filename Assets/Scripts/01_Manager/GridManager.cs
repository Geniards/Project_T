using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    public GameObject tilePrefab;   // Tile 프리펩

    // 해당 좌표의 값에 따른 타일 검색
    public Dictionary<Vector2Int, Tile> tileDictionary = new Dictionary<Vector2Int, Tile>();

    private Tile prevHighlightedTile;

    private void Awake()
    {
        if(!Instance)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Json기반의 데이터로 그리드 생성.
    /// </summary>
    public void LoadGrid(StageData stageData)
    {
        ResetGrid();
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

                    // 오브젝트 풀에서 타일 가져오기
                    Tile tile = GameManager.Instance.GetTile();
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

    /// <summary>
    /// 스테이지 변경 시 기존 타일 초기화(오브젝트 풀로 반환)
    /// </summary>
    public void ResetGrid()
    {
        foreach(var tile in tileDictionary.Values)
        {
            GameManager.Instance.ReturnTile(tile);
        }
        tileDictionary.Clear();
    }

    /// <summary>
    /// 마우스 오버 시 하이라이트
    /// </summary>
    public void HandleMouseOver(Vector2 worldPos)
    {
        Vector2Int gridPos = new Vector2Int(Mathf.RoundToInt(worldPos.x), Mathf.RoundToInt(worldPos.y));
        Tile hoveredTile = GetTile(gridPos);

        // 이전 하이라이트 제거 (단, 선택된 유닛 타일은 유지)
        if (prevHighlightedTile && prevHighlightedTile != GameManager.Instance.GetSelectedUnitTile())
        {
            prevHighlightedTile.ClearHighlight();
        }

        if (hoveredTile)
        {
            // 선택된 유닛이 있는 타일이면 하이라이트 덮어씌우지 않음
            if (hoveredTile != GameManager.Instance.GetSelectedUnitTile())
            {
                hoveredTile.HighlightTile(new Color(1f, 1f, 1f, 0.1f)); // 연한 흰색 하이라이트
            }
            prevHighlightedTile = hoveredTile;
        }
    }
}
