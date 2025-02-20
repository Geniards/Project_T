using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    // ���� �⺻ ����
    public UnitData unitData; // ���� �⺻ ����
    public Tile currentTile; // ������ ���� ��ġ�� Ÿ�� ��ġ

    // ���� �� ����
    public E_UnitTeam unitTeam;
    // ���� �ൿ ���� ����
    public E_UnitState unitState;


    /// <summary>
    /// ���� �ʱ�ȭ
    /// </summary>
    /// <param name="spawnData">���� �ʱ�ȭ ����</param>
    public void Initialize(UnitSpawnData spawnData)
    {
        unitTeam = (E_UnitTeam)spawnData.unitTeam;
        unitState = E_UnitState.Idle;

        // UnitDatabase���� unitId�� ������� �����͸� ������
        unitData = UnitDatabase.Instance.GetUnitDataById(spawnData.unitTypeId);

        if (unitData == null)
        {
            Debug.LogError($"UnitData�� ã�� �� �����ϴ�! unitTypeId: {spawnData.unitTypeId}");
        }
    }

    public void SetCurrentTile(Tile tile)
    {
        if(currentTile)
        {
            // ���� Ÿ�Ͽ� ĳ���� ����
            currentTile.isOccupied = false;
        }

        currentTile = tile;
        currentTile.isOccupied = true; // �� Ÿ�Ͽ� ĳ���� �ö��
        transform.position = tile.transform.position; // ���� ��ġ ������Ʈ
    }

    // TODO: �����̵�(A*)

}
