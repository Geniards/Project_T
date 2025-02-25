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

    // 공격가능한 타일리스트
    public List<Tile> attackableTiles = new List<Tile>();

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
        //Debug.Log($"마우스 위치 [{gridPos}]");
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
                if (nextTile && nextTile.isWalkable && !distanceMap.ContainsKey(nextTile))
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

    private const int INT_MAX = int.MaxValue;

    // A* 알고리즘을 이용한 경로 탐색
    public List<Tile> FindPathAStar(Tile startTile, Tile targetTile)
    {
        // 1. 시작 타일을 openSet에 넣음.
        List<Tile> openSet = new List<Tile>();
        HashSet<Tile> closedSet = new HashSet<Tile>(); // 리스트보다 빠른 탐색을 위해 HashSet 사용

        // 2. 시작 타일의 gScore, fScore 설정(리스트 O(n), Dictionary O(1))
        Dictionary<Tile, Tile> cameFrom = new Dictionary<Tile, Tile>();
        Dictionary<Tile, int> gScore = new Dictionary<Tile, int>();
        Dictionary<Tile, int> fScore = new Dictionary<Tile, int>();

        // 3. gScore, fScore 초기화
        foreach (var tile in tileDictionary.Values)
        {
            gScore[tile] = INT_MAX;
            fScore[tile] = INT_MAX;
        }

        // 4. 시작 타일의 gScore, fScore 설정
        gScore[startTile] = 0;
        fScore[startTile] = Heuristic(startTile, targetTile);
        openSet.Add(startTile);

        // 5. openSet이 빌 때까지 반복
        while (openSet.Count > 0)
        {
            // 6. openSet에서 fScore가 가장 작은 타일을 currentTile로 설정
            Tile currentTile = openSet.OrderBy(t => fScore[t]).First();

            // 7. currentTile이 목표 타일이면 경로 재구성
            if (currentTile == targetTile)
            {
                return ReconstructPath(cameFrom, currentTile);
            }

            // 8. currentTile을 openSet에서 제거하고 closedSet에 추가
            openSet.Remove(currentTile);
            closedSet.Add(currentTile);


            foreach (Vector2Int direction in new Vector2Int[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
            {
                // 9. 상하좌우 타일을 검사하여 이동 가능한 타일을 찾음
                Vector2Int neighborPos = currentTile.vec2IntPos + direction;
                // 이동 가능한 타일이 아니거나 이미 방문한 타일이면 넘어감
                if (!tileDictionary.ContainsKey(neighborPos)) continue;

                Tile neighborTile = tileDictionary[neighborPos];
                if (closedSet.Contains(neighborTile) || !neighborTile.isWalkable || neighborTile.isOccupied) continue;

                // 10. 이동 가능한 타일의 gScore를 계산하고, 이전 gScore보다 작으면 갱신
                int tentativeGScore = gScore[currentTile] + 1;
                if (tentativeGScore < gScore[neighborTile])
                {
                    cameFrom[neighborTile] = currentTile;
                    gScore[neighborTile] = tentativeGScore;
                    fScore[neighborTile] = gScore[neighborTile] + Heuristic(neighborTile, targetTile);

                    // 11. 이동 가능한 타일이 openSet에 없으면 추가
                    if (!openSet.Contains(neighborTile))
                        openSet.Add(neighborTile);
                }
            }
        }
        return new List<Tile>(); // 경로 없음
    }

    /// <summary>
    /// 맨하튼 방식으로 휴리스틱 계산(상하좌우)
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    private int Heuristic(Tile a, Tile b)
    {
        return Mathf.Abs(a.vec2IntPos.x - b.vec2IntPos.x) + Mathf.Abs(a.vec2IntPos.y - b.vec2IntPos.y);
    }

    /// <summary>
    /// 경로 재구성
    /// </summary>
    /// <param name="cameFrom">지나갔던 경로</param>
    /// <param name="current">현재 타일</param>
    /// <returns></returns>
    private List<Tile> ReconstructPath(Dictionary<Tile, Tile> cameFrom, Tile current)
    {
        List<Tile> path = new List<Tile> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Add(current);
        }
        path.Reverse();
        return path;
    }

    /// <summary>
    /// 이동 가능 범위 내에서 목표 타일과 가장 가까운 타일을 찾는 함수
    /// </summary>
    public Tile FindNearestReachableTile(Unit unit, Tile targetTile)
    {
        Tile bestTile = null;
        int minDistance = int.MaxValue;

        // BFS를 통해 이동 가능한 타일들을 가져옴
        List<Tile> reachableTiles = FindWalkableTilesWithoutHighlight(unit);

        foreach (Tile tile in reachableTiles)
        {
            int distance = Heuristic(tile, targetTile);
            if (distance < minDistance)
            {
                minDistance = distance;
                bestTile = tile;
            }
        }

        return bestTile ?? unit.currentTile; // 이동 가능한 타일이 없으면 제자리 유지
    }

    /// <summary>
    /// 이동 가능한 타일을 반환 (하이라이트 X, 내부 처리용)
    /// </summary>
    public List<Tile> FindWalkableTilesWithoutHighlight(Unit selectedUnit)
    {
        List<Tile> walkableTiles = new List<Tile>();

        Queue<Tile> queue = new Queue<Tile>();
        Dictionary<Tile, int> distanceMap = new Dictionary<Tile, int>();

        Tile startTile = selectedUnit.currentTile;
        queue.Enqueue(startTile);
        distanceMap[startTile] = 0;

        while (queue.Count > 0)
        {
            Tile currentTile = queue.Dequeue();
            int currentDistance = distanceMap[currentTile];

            if (currentDistance >= selectedUnit.unitData.moveRange) continue;

            foreach (Vector2Int direction in new Vector2Int[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
            {
                Vector2Int neighborPos = currentTile.vec2IntPos + direction;
                Tile nextTile = GetTile(neighborPos);

                if (nextTile && nextTile.isWalkable && !distanceMap.ContainsKey(nextTile))
                {
                    queue.Enqueue(nextTile);
                    distanceMap[nextTile] = currentDistance + 1;
                    walkableTiles.Add(nextTile);
                }
            }
        }

        return walkableTiles;
    }

    /// <summary>
    /// 공격 가능한 타일을 찾고 하이라이트 표시
    /// </summary>
    /// <param name="unit"></param>
    public void FindAttackableTiles(Unit unit, bool isHighlight = false)
    {
        attackableTiles.Clear();

        foreach (Vector2Int direction in new Vector2Int[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
        {
            Vector2Int attackTilePos = unit.currentTile.vec2IntPos + direction;
            Tile attackTile = GetTile(attackTilePos);

            if (attackTile != null)
            {
                attackableTiles.Add(attackTile);
                
                if (isHighlight)
                    attackTile.HighlightTile(new Color(1f, 0f, 0f, 0.3f)); // 빨간색 하이라이트
#if UNITY_EDITOR
                //Debug.Log($"[공격 가능] {unit.unitData.unitId} -> {attackTile.vec2IntPos}");
#endif
            }
        }
    }

    // 공격범위 하이라이트
    public void ShowHiggLight()
    {
        if (attackableTiles.Count == 0)
            Debug.Log("공격범위 없음");

        foreach (Tile tile in attackableTiles)
        {
            tile.HighlightTile(new Color(1f, 0f, 0f, 0.3f));
        }


    }

    /// <summary>
    /// 공격 가능한 타일 하이라이트 초기화
    /// </summary>
    public void ClearAttackableTiles()
    {
        foreach (Tile tile in attackableTiles)
        {
            tile.ClearHighlight();
        }
        attackableTiles.Clear();
    }
}
