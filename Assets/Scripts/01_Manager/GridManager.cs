using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    public GameObject tilePrefab;   // Tile ������

    // �ش� ��ǥ�� ���� ���� Ÿ�� �˻�
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

        // ���� ���̶���Ʈ ���� (��, ���õ� ���� Ÿ���� ����)
        if (prevHighlightedTile && prevHighlightedTile != GameManager.Instance.GetSelectedUnitTile())
        {
            prevHighlightedTile.ClearHighlight();
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
}
