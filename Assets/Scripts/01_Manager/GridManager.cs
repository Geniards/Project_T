using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    public GameObject tilePrefab;   // Tile 프리펩

    // 해당 좌표의 값에 따른 타일 검색
    public Dictionary<Vector2Int, Tile> tileDictionary = new Dictionary<Vector2Int, Tile>();

    // 이동가능한 타일리스트
    private List<Tile> walkableTiles = new List<Tile>();
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
#if UNITY_EDITOR
        Debug.Log($"마우스 위치 [{gridPos}]");
#endif

        // 이전 하이라이트 제거 (단, 선택된 유닛 타일은 유지)
        if (prevHighlightedTile && prevHighlightedTile != GameManager.Instance.GetSelectedUnitTile())
        {
            // 이동 가능한 타일은 이동 가능 하이라이트 사용
            if (!IsWalkableTile(prevHighlightedTile))
                prevHighlightedTile.ClearHighlight();
            else 
                prevHighlightedTile.HighlightTile(new Color(0f, 0f, 1f, 0.3f));
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

    /// <summary>
    /// 선택된 유닛의 이동가능한 범위(BFS)
    /// </summary>
    /// <param name="selectedUnit"></param>
    public void FindWalkableTiles(Unit selectedUnit)
    {
        // 0. 이동 가능한 하이라이트 초기화
        ClearWalkableTiles();

        // 1. Queue 초기화
        Queue<Tile> queue = new Queue<Tile>();
        Dictionary<Tile, int> distanceMap = new Dictionary<Tile, int>();

        // 2. 시작 타일을 큐에 넣고 거리 0으로 설정
        Tile startTile = selectedUnit.currentTile;
        queue.Enqueue(startTile);
        distanceMap[startTile] = 0;

        // 3. 큐가 빌 때까지 반복
        while (queue.Count > 0)
        {
            Tile currentTile = queue.Dequeue();
            int currentDistance = distanceMap[currentTile];

            if (currentDistance >= selectedUnit.unitData.moveRange) continue;

            // 상하좌우 타일을 검사하여 이동 가능한 타일을 큐에 넣음
            foreach (Vector2Int direction in new Vector2Int[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
            {
                Vector2Int neighborPos = currentTile.vec2IntPos + direction;
                Tile nextTile = GetTile(neighborPos);

                // 이동 가능한 타일이고, 아직 방문하지 않은 타일이고 유닛이 해당 타일에 존재하지 않는다면 큐에 넣음
                if (nextTile && nextTile.isWalkable /*&& !nextTile.isOccupied*/ && !distanceMap.ContainsKey(nextTile))
                {
                    queue.Enqueue(nextTile);
                    distanceMap[nextTile] = currentDistance + 1;
                    walkableTiles.Add(nextTile);
                    nextTile.HighlightTile(new Color(0f, 0f, 1f, 0.3f)); // 이동 가능한 타일 하이라이트
                }
            }
        }
    }



    /// <summary>
    /// 이동 가능한 타일 하이라이트 초기화
    /// </summary>
    public void ClearWalkableTiles()
    {
        foreach (var tile in walkableTiles)
        {
            tile.ClearHighlight();
        }
        walkableTiles.Clear();
    }

    public bool IsWalkableTile(Tile tile)
    {
        return walkableTiles.Contains(tile);
    }

   
}
