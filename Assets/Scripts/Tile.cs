using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    // 타일 좌표
    public Vector2Int vec2IntPos;                   // 타일 좌표

    // 타일 속성
    public bool isWalkable = true;                  // 이동 가능 여부
    public bool isOccupied = false;                 // 유닛 존재 여부
    public E_TileType tileType = E_TileType.Plane;  // 타일 타입

    // 타일 하이라이트
    private SpriteRenderer spriteRenderer;
    private Color defaultColor = new Color(1, 1, 1, 0.5f); // 투명색


    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        ClearHighlight();
    }

    /// <summary>
    /// 타일 위치 설정
    /// </summary>
    /// <param name="pos">변경할 위치</param>
    public void SetTilePosition(Vector2Int pos)
    {
        vec2IntPos = pos;
        transform.position = new Vector3(pos.x, pos.y, 0);
    }

    /// <summary>
    /// 하이라이트 표시
    /// </summary>
    /// <param name="color">하이라이트에 넣을 색상</param>
    public void HighlightTile(Color color)
    {
        spriteRenderer.color = color;
    }

    /// <summary>
    /// 하이라이트 제거
    /// </summary>
    public void ClearHighlight()
    {
        spriteRenderer.color = defaultColor;
    }

    /// <summary>
    /// 타일 속성 업데이트
    /// </summary>
    /// <param name="walkable">타일의 변경한 이동가능 여부</param>
    /// <param name="type">타일의 변경한 타입</param>
    public void SetTileProperties(bool walkable, E_TileType type)
    {
        isWalkable = walkable;
        tileType = type;
    }

    /// <summary>
    /// 타일이 경로 탐색시 통과 가능한지 여부 확인
    /// </summary>
    /// <param name="isDest">도착할 목적지 여부</param>
    /// <returns></returns>
    public bool CanMoveThrough(bool isDest)
    {
        // 도착지는 유닛이 있으면 도착 할 수 없다.
        if (isDest)
        {
            return isWalkable && !isOccupied;
        }

        // 경유지는 유닛이 있든 없든 상관없다.
        return isWalkable;
    }
}
