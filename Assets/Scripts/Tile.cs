using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    // Ÿ�� ��ǥ
    public Vector2Int vec2IntPos;                   // Ÿ�� ��ǥ

    // Ÿ�� �Ӽ�
    public bool isWalkable = true;                  // �̵� ���� ����
    public bool isOccupied = false;                 // ���� ���� ����
    public E_TileType tileType = E_TileType.Plane;  // Ÿ�� Ÿ��

    // Ÿ�� ���̶���Ʈ
    private SpriteRenderer spriteRenderer;
    private Color defaultColor = new Color(1, 1, 1, 0.5f); // �����


    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        ClearHighlight();
    }

    /// <summary>
    /// Ÿ�� ��ġ ����
    /// </summary>
    /// <param name="pos">������ ��ġ</param>
    public void SetTilePosition(Vector2Int pos)
    {
        vec2IntPos = pos;
        transform.position = new Vector3(pos.x, pos.y, 0);
    }

    /// <summary>
    /// ���̶���Ʈ ǥ��
    /// </summary>
    /// <param name="color">���̶���Ʈ�� ���� ����</param>
    public void HighlightTile(Color color)
    {
        spriteRenderer.color = color;
    }

    /// <summary>
    /// ���̶���Ʈ ����
    /// </summary>
    public void ClearHighlight()
    {
        spriteRenderer.color = defaultColor;
    }

    /// <summary>
    /// Ÿ�� �Ӽ� ������Ʈ
    /// </summary>
    /// <param name="walkable">Ÿ���� ������ �̵����� ����</param>
    /// <param name="type">Ÿ���� ������ Ÿ��</param>
    public void SetTileProperties(bool walkable, E_TileType type)
    {
        isWalkable = walkable;
        tileType = type;
    }

    /// <summary>
    /// Ÿ���� ��� Ž���� ��� �������� ���� Ȯ��
    /// </summary>
    /// <param name="isDest">������ ������ ����</param>
    /// <returns></returns>
    public bool CanMoveThrough(bool isDest)
    {
        // �������� ������ ������ ���� �� �� ����.
        if (isDest)
        {
            return isWalkable && !isOccupied;
        }

        // �������� ������ �ֵ� ���� �������.
        return isWalkable;
    }
}
