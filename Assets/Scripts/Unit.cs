using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    // ���� �⺻ ����
    public UnitData unitData; // ���� �⺻ ����
    public Tile currentTile; // ������ ���� ��ġ�� Ÿ�� ��ġ

    // ���� �ൿ ���� ����
    public E_UnitState unitState;
    private bool isSelected = false;

    /// <summary>
    /// ���� �ʱ�ȭ
    /// </summary>
    /// <param name="spawnData">���� �ʱ�ȭ ����</param>
    public void Initialize(UnitSpawnData spawnData)
    {
        unitState = E_UnitState.Idle;

        // UnitDatabase���� unitId�� ������� �����͸� ������
        unitData = UnitDatabase.Instance.GetUnitDataById(spawnData.unitTypeId);
        unitData.unitTeam = (E_UnitTeam)spawnData.unitTeam;

        if (unitData == null)
        {
            Debug.LogError($"UnitData�� ã�� �� �����ϴ�! unitTypeId: {spawnData.unitTypeId}");
        }
    }

    /// <summary>
    /// ���� ����
    /// </summary>
    public void Select()
    {
        // �̹� ���õ� ��� �ߺ� ó�� ����
        if (isSelected) return;

        isSelected = true;
        currentTile.HighlightTile(new Color(1f, 1f, 0f, 0.3f)); // ����� ���̶���Ʈ
        GridManager.Instance.FindWalkableTiles(this);
    }

    /// <summary>
    /// ���� ���� ����
    /// </summary>
    public void Deselect()
    {
        // ���õ��� ���� ��� ����
        if (!isSelected) return;
        isSelected = false;
        currentTile.ClearHighlight();
        GridManager.Instance.ClearWalkableTiles();
        //GridManager.Instance.ClearAttackableTiles();
    }

    /// <summary>
    /// ���� �̵�
    /// </summary>
    /// <param name="targetTile"></param>
    public void MoveTo(Tile targetTile)
    {
        Tile moveTile = GridManager.Instance.FindNearestReachableTile(this, targetTile);
        List<Tile> path = GridManager.Instance.FindPathAStar(currentTile, moveTile);

        if (path.Count > 0)
        {
            StartCoroutine(MoveAlongPath(path));
        }
    }

    /// <summary>
    /// AI ���� �̵�
    /// </summary>
    /// <param name="targetTile"></param>
    /// <returns></returns>
    public IEnumerator MoveToAI(Tile targetTile)
    {
        Tile moveTile = GridManager.Instance.FindNearestReachableTile(this, targetTile);
        List<Tile> path = GridManager.Instance.FindPathAStar(currentTile, moveTile);

        if (path.Count > 0)
        {
            yield return StartCoroutine(MoveAlongPath(path));
        }
    }

    /// <summary>
    /// A* ��θ� ���� �̵�
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private IEnumerator MoveAlongPath(List<Tile> path)
    {
        foreach (Tile tile in path)
        {
            transform.position = tile.transform.position;
            yield return new WaitForSeconds(0.2f);
        }

        currentTile.isOccupied = false;
        SetCurrentTile(path[path.Count - 1]);
        currentTile.isOccupied = true;
        GridManager.Instance.FindAttackableTiles(this);
        Deselect();
    }

    /// <summary>
    /// ������ ���� ��ġ�� Ÿ���� ����
    /// </summary>
    /// <param name="tile">������ �̵��� Ÿ��</param>
    public void SetCurrentTile(Tile tile)
    {
        if (currentTile)
        {
            // ���� Ÿ�Ͽ� ĳ���� ����
            currentTile.isOccupied = false;
        }

        currentTile = tile;
        currentTile.isOccupied = true; // �� Ÿ�Ͽ� ĳ���� �ö��
        transform.position = tile.transform.position; // ���� ��ġ ������Ʈ
    }

    /// <summary>
    /// ������ �ڽ��� ���� ����
    /// </summary>
    /// <returns></returns>
    public IEnumerator TakeTurn()
    {
#if UNITY_EDITOR
        Debug.Log($"���� {unitData.unitId} �� ����");
#endif
        // 1. ���� ������ ������ ���̶���Ʈ
        GridManager.Instance.FindAttackableTiles(this);
        yield return new WaitForSeconds(0.5f);

        // 2. ��� ���� ������ ������ �ִ��� Ȯ��
        Unit target = FindAttackableUnit();
        if (target)
        {
            GridManager.Instance.ShowHiggLight();
            yield return new WaitForSeconds(0.5f);
            Attack(target);
            TurnManager.Instance.OnUnitTurnCompleted();
            yield break;
        }

        // 3. ������ ������ ���ٸ� �̵� ���� Ÿ�� Ž��
        GridManager.Instance.FindWalkableTiles(this);
        yield return new WaitForSeconds(0.5f);

        // 4. ���� ����� �� Ž��
        Tile targetTile = UnitManager.Instance.FindNearestUnit(this)?.currentTile;
        if (targetTile)
        {
            yield return MoveToAI(targetTile);
            yield return new WaitForSeconds(0.5f);

            // 5. �̵� �� �ٽ� ���� ���� ���� Ȯ��
            target = FindAttackableUnit();
            if (target)
            {
                Debug.Log("�����մϴ�~~~");
                GridManager.Instance.ShowHiggLight();
                yield return new WaitForSeconds(0.5f);
                Attack(target);
            }
        }

        // 6. �� ����
        unitState = E_UnitState.Complete;
        GridManager.Instance.ClearWalkableTiles();
        yield return new WaitForSeconds(0.3f);
        GridManager.Instance.ClearAttackableTiles();
        TurnManager.Instance.OnUnitTurnCompleted();
        yield return new WaitForSeconds(0.3f);
    }

    /// <summary>
    /// ���� ������ ���� Ž��
    /// </summary>
    /// <returns></returns>
    public Unit FindAttackableUnit()
    {
        foreach (Tile tile in GridManager.Instance.attackableTiles)
        {
            Unit target = UnitManager.Instance.GetUnitAtPosition(tile.vec2IntPos);
            if (target && target.unitData.unitTeam != unitData.unitTeam)
            {
                return target;
            }
        }
#if UNITY_EDITOR
        Debug.Log($"{unitData.unitId} ���� ������ ���� ����");
#endif
        return null;
    }

    /// <summary>
    /// ���� ����
    /// </summary>
    /// <param name="target"></param>
    public void Attack(Unit target)
    {
        if (!target) return;
#if UNITY_EDITOR
        Debug.Log($"{unitData.unitId}�� {target.unitData.unitId}�� ����!");
#endif
        if (unitState == E_UnitState.Complete) return;

        // ������ ��� �� ���� ����
        target.TakeDamage(unitData.attackPower);
        unitState = E_UnitState.Complete;
        Deselect();
    }

    /// <summary>
    /// ������ �Ծ�����
    /// </summary>
    /// <param name="damage"></param>
    public void TakeDamage(int damage)
    {
        unitData.hP -= damage;
#if UNITY_EDITOR
        Debug.Log($"{unitData.unitId}�� {damage}�� ���ظ� ����! ���� ü��: {unitData.hP}");
#endif
        if (unitData.hP <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// ���� ��� ó��
    /// </summary>
    private void Die()
    {
        Debug.Log($"{unitData.unitId} ���!");
        currentTile.isOccupied = false;
        UnitManager.Instance.RemoveUnit(this);
    }
}
