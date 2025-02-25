using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    public GameObject tilePrefab;   // Tile ������

    // �ش� ��ǥ�� ���� ���� Ÿ�� �˻�
    public Dictionary<Vector2Int, Tile> tileDictionary = new Dictionary<Vector2Int, Tile>();

    // �̵������� Ÿ�ϸ���Ʈ
    private List<Tile> walkableTiles = new List<Tile>();
    private Tile prevHighlightedTile;

    // ���ݰ����� Ÿ�ϸ���Ʈ
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
    /// Json����� �����ͷ� �׸��� ����.
    /// </summary>
    public void LoadGrid(StageData stageData)
    {
        ResetGrid();
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

                    // ������Ʈ Ǯ���� Ÿ�� ��������
                    Tile tile = GameManager.Instance.GetTile();
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

    /// <summary>
    /// �������� ���� �� ���� Ÿ�� �ʱ�ȭ(������Ʈ Ǯ�� ��ȯ)
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
    /// ���콺 ���� �� ���̶���Ʈ
    /// </summary>
    public void HandleMouseOver(Vector2 worldPos)
    {
        Vector2Int gridPos = new Vector2Int(Mathf.RoundToInt(worldPos.x), Mathf.RoundToInt(worldPos.y));
        Tile hoveredTile = GetTile(gridPos);
#if UNITY_EDITOR
        //Debug.Log($"���콺 ��ġ [{gridPos}]");
#endif

        // ���� ���̶���Ʈ ���� (��, ���õ� ���� Ÿ���� ����)
        if (prevHighlightedTile && prevHighlightedTile != GameManager.Instance.GetSelectedUnitTile())
        {
            // �̵� ������ Ÿ���� �̵� ���� ���̶���Ʈ ���
            if (!IsWalkableTile(prevHighlightedTile))
                prevHighlightedTile.ClearHighlight();
            else 
                prevHighlightedTile.HighlightTile(new Color(0f, 0f, 1f, 0.3f));
        }

        if (hoveredTile)
        {
            // ���õ� ������ �ִ� Ÿ���̸� ���̶���Ʈ ������� ����
            if (hoveredTile != GameManager.Instance.GetSelectedUnitTile())
            {
                hoveredTile.HighlightTile(new Color(1f, 1f, 1f, 0.1f)); // ���� ��� ���̶���Ʈ
            }
            prevHighlightedTile = hoveredTile;
        }
    }

    /// <summary>
    /// ���õ� ������ �̵������� ����(BFS)
    /// </summary>
    /// <param name="selectedUnit"></param>
    public void FindWalkableTiles(Unit selectedUnit)
    {
        // 0. �̵� ������ ���̶���Ʈ �ʱ�ȭ
        ClearWalkableTiles();

        // 1. Queue �ʱ�ȭ
        Queue<Tile> queue = new Queue<Tile>();
        Dictionary<Tile, int> distanceMap = new Dictionary<Tile, int>();

        // 2. ���� Ÿ���� ť�� �ְ� �Ÿ� 0���� ����
        Tile startTile = selectedUnit.currentTile;
        queue.Enqueue(startTile);
        distanceMap[startTile] = 0;

        // 3. ť�� �� ������ �ݺ�
        while (queue.Count > 0)
        {
            Tile currentTile = queue.Dequeue();
            int currentDistance = distanceMap[currentTile];

            if (currentDistance >= selectedUnit.unitData.moveRange) continue;

            // �����¿� Ÿ���� �˻��Ͽ� �̵� ������ Ÿ���� ť�� ����
            foreach (Vector2Int direction in new Vector2Int[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
            {
                Vector2Int neighborPos = currentTile.vec2IntPos + direction;
                Tile nextTile = GetTile(neighborPos);

                // �̵� ������ Ÿ���̰�, ���� �湮���� ���� Ÿ���̰� ������ �ش� Ÿ�Ͽ� �������� �ʴ´ٸ� ť�� ����
                if (nextTile && nextTile.isWalkable && !distanceMap.ContainsKey(nextTile))
                {
                    queue.Enqueue(nextTile);
                    distanceMap[nextTile] = currentDistance + 1;
                    walkableTiles.Add(nextTile);
                    nextTile.HighlightTile(new Color(0f, 0f, 1f, 0.3f)); // �̵� ������ Ÿ�� ���̶���Ʈ
                }
            }
        }
    }



    /// <summary>
    /// �̵� ������ Ÿ�� ���̶���Ʈ �ʱ�ȭ
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

    // A* �˰����� �̿��� ��� Ž��
    public List<Tile> FindPathAStar(Tile startTile, Tile targetTile)
    {
        // 1. ���� Ÿ���� openSet�� ����.
        List<Tile> openSet = new List<Tile>();
        HashSet<Tile> closedSet = new HashSet<Tile>(); // ����Ʈ���� ���� Ž���� ���� HashSet ���

        // 2. ���� Ÿ���� gScore, fScore ����(����Ʈ O(n), Dictionary O(1))
        Dictionary<Tile, Tile> cameFrom = new Dictionary<Tile, Tile>();
        Dictionary<Tile, int> gScore = new Dictionary<Tile, int>();
        Dictionary<Tile, int> fScore = new Dictionary<Tile, int>();

        // 3. gScore, fScore �ʱ�ȭ
        foreach (var tile in tileDictionary.Values)
        {
            gScore[tile] = INT_MAX;
            fScore[tile] = INT_MAX;
        }

        // 4. ���� Ÿ���� gScore, fScore ����
        gScore[startTile] = 0;
        fScore[startTile] = Heuristic(startTile, targetTile);
        openSet.Add(startTile);

        // 5. openSet�� �� ������ �ݺ�
        while (openSet.Count > 0)
        {
            // 6. openSet���� fScore�� ���� ���� Ÿ���� currentTile�� ����
            Tile currentTile = openSet.OrderBy(t => fScore[t]).First();

            // 7. currentTile�� ��ǥ Ÿ���̸� ��� �籸��
            if (currentTile == targetTile)
            {
                return ReconstructPath(cameFrom, currentTile);
            }

            // 8. currentTile�� openSet���� �����ϰ� closedSet�� �߰�
            openSet.Remove(currentTile);
            closedSet.Add(currentTile);


            foreach (Vector2Int direction in new Vector2Int[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
            {
                // 9. �����¿� Ÿ���� �˻��Ͽ� �̵� ������ Ÿ���� ã��
                Vector2Int neighborPos = currentTile.vec2IntPos + direction;
                // �̵� ������ Ÿ���� �ƴϰų� �̹� �湮�� Ÿ���̸� �Ѿ
                if (!tileDictionary.ContainsKey(neighborPos)) continue;

                Tile neighborTile = tileDictionary[neighborPos];
                if (closedSet.Contains(neighborTile) || !neighborTile.isWalkable || neighborTile.isOccupied) continue;

                // 10. �̵� ������ Ÿ���� gScore�� ����ϰ�, ���� gScore���� ������ ����
                int tentativeGScore = gScore[currentTile] + 1;
                if (tentativeGScore < gScore[neighborTile])
                {
                    cameFrom[neighborTile] = currentTile;
                    gScore[neighborTile] = tentativeGScore;
                    fScore[neighborTile] = gScore[neighborTile] + Heuristic(neighborTile, targetTile);

                    // 11. �̵� ������ Ÿ���� openSet�� ������ �߰�
                    if (!openSet.Contains(neighborTile))
                        openSet.Add(neighborTile);
                }
            }
        }
        return new List<Tile>(); // ��� ����
    }

    /// <summary>
    /// ����ư ������� �޸���ƽ ���(�����¿�)
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    private int Heuristic(Tile a, Tile b)
    {
        return Mathf.Abs(a.vec2IntPos.x - b.vec2IntPos.x) + Mathf.Abs(a.vec2IntPos.y - b.vec2IntPos.y);
    }

    /// <summary>
    /// ��� �籸��
    /// </summary>
    /// <param name="cameFrom">�������� ���</param>
    /// <param name="current">���� Ÿ��</param>
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
    /// �̵� ���� ���� ������ ��ǥ Ÿ�ϰ� ���� ����� Ÿ���� ã�� �Լ�
    /// </summary>
    public Tile FindNearestReachableTile(Unit unit, Tile targetTile)
    {
        Tile bestTile = null;
        int minDistance = int.MaxValue;

        // BFS�� ���� �̵� ������ Ÿ�ϵ��� ������
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

        return bestTile ?? unit.currentTile; // �̵� ������ Ÿ���� ������ ���ڸ� ����
    }

    /// <summary>
    /// �̵� ������ Ÿ���� ��ȯ (���̶���Ʈ X, ���� ó����)
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
    /// ���� ������ Ÿ���� ã�� ���̶���Ʈ ǥ��
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
                    attackTile.HighlightTile(new Color(1f, 0f, 0f, 0.3f)); // ������ ���̶���Ʈ
#if UNITY_EDITOR
                //Debug.Log($"[���� ����] {unit.unitData.unitId} -> {attackTile.vec2IntPos}");
#endif
            }
        }
    }

    // ���ݹ��� ���̶���Ʈ
    public void ShowHiggLight()
    {
        if (attackableTiles.Count == 0)
            Debug.Log("���ݹ��� ����");

        foreach (Tile tile in attackableTiles)
        {
            tile.HighlightTile(new Color(1f, 0f, 0f, 0.3f));
        }


    }

    /// <summary>
    /// ���� ������ Ÿ�� ���̶���Ʈ �ʱ�ȭ
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
