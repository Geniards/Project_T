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
        Debug.Log($"���콺 ��ġ [{gridPos}]");
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
                if (nextTile && nextTile.isWalkable /*&& !nextTile.isOccupied*/ && !distanceMap.ContainsKey(nextTile))
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

   
}
