using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    // 유닛 기본 정보
    public UnitData unitData; // 유닛 기본 정보
    public Tile currentTile; // 유닛이 현재 위치한 타일 위치

    // 유닛 팀 정보
    public E_UnitTeam unitTeam;
    // 유닛 행동 상태 정보
    public E_UnitState unitState;


    /// <summary>
    /// 유닛 초기화
    /// </summary>
    /// <param name="spawnData">유닛 초기화 정보</param>
    public void Initialize(UnitSpawnData spawnData)
    {
        unitTeam = (E_UnitTeam)spawnData.unitTeam;
        unitState = E_UnitState.Idle;

        // UnitDatabase에서 unitId를 기반으로 데이터를 가져옴
        unitData = UnitDatabase.Instance.GetUnitDataById(spawnData.unitTypeId);

        if (unitData == null)
        {
            Debug.LogError($"UnitData를 찾을 수 없습니다! unitTypeId: {spawnData.unitTypeId}");
        }
    }

    public void SetCurrentTile(Tile tile)
    {
        if(currentTile)
        {
            // 이전 타일에 캐릭터 해제
            currentTile.isOccupied = false;
        }

        currentTile = tile;
        currentTile.isOccupied = true; // 새 타일에 캐릭터 올라옴
        transform.position = tile.transform.position; // 유닛 위치 업데이트
    }

    // TODO: 유닛이동(A*)

}
