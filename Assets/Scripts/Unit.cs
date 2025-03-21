using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.UI.CanvasScaler;

public class Unit : MonoBehaviour
{
    public event Action OnMoveComplete; // 이동 완료 이벤트

    // 유닛 기본 정보
    public UnitData unitData; // 유닛 기본 정보
    public Tile currentTile; // 유닛이 현재 위치한 타일 위치

    public Vector2Int previousPosition; // 이동 전 위치 저장

    // 유닛 행동 상태 정보
    public E_UnitState unitState;
    public bool isSelected = false;

    // 애니메이션 컨트롤러
    private UnitAnimator unitAnimator;

    // 유닛의 색상 변경을 위한 렌더러
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        unitAnimator = GetComponent<UnitAnimator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }


    /// <summary>
    /// 유닛 초기화
    /// </summary>
    /// <param name="spawnData">유닛 초기화 정보</param>
    public void Initialize(UnitSpawnData spawnData, UnitAnimationData animData)
    {
        unitState = E_UnitState.Idle;

        // UnitDatabase에서 unitId를 기반으로 데이터를 가져옴
        UnitData unitDatas = UnitDatabase.Instance.GetUnitDataById(spawnData.unitTypeId);

        if (unitData == null)
        {
            Debug.LogError($"UnitData를 찾을 수 없습니다! unitTypeId: {spawnData.unitTypeId}");
        }

        // 새로운 인스턴스 생성 후 값 복사 (깊은 복사)
        unitData = new UnitData
        {
            unitId = unitDatas.unitId,
            unitName = unitDatas.unitName,
            unitType = unitDatas.unitType,
            unitTeam = (E_UnitTeam)spawnData.unitTeam,
            hP = unitDatas.hP,
            maxHP = unitDatas.maxHP,
            mana = unitDatas.mana,
            maxMana = unitDatas.maxMana,
            exp = unitDatas.exp,
            maxExp = unitDatas.maxExp,
            level = unitDatas.level,
            attackPower = unitDatas.attackPower,
            attackRange = unitDatas.attackRange,
            moveRange = unitDatas.moveRange
        };

        unitAnimator.SetOverrideAnimations(animData);
    }

    /// <summary>
    /// 유닛 상태 변경 시 색상 업데이트
    /// </summary>
    public void UpdateUnitAppearance()
    {
        if (unitState == E_UnitState.Complete)
        {
            // 행동 완료된 유닛
            spriteRenderer.color = new Color(0.6f, 0.6f, 0.6f, 1f);
        }
        else
        {
            // 행동 가능한 유닛
            spriteRenderer.color = Color.white;
        }
    }


    /// <summary>
    /// 유닛 선택
    /// </summary>
    public void Select()
    {
        // 이미 선택된 경우 중복 처리 방지
        if (isSelected) return;

        isSelected = true;
        currentTile.HighlightTile(new Color(1f, 1f, 0f, 0.3f)); // 노란색 하이라이트

        // 이동 범위 탐색
        GridManager.Instance.FindWalkableTiles(this);
    }

    /// <summary>
    /// 유닛 선택 해제
    /// </summary>
    public void Deselect()
    {
        // 선택되지 않은 경우 무시
        if (!isSelected) return;
        isSelected = false;
        currentTile.ClearHighlight();
        GridManager.Instance.ClearWalkableTiles();
        GridManager.Instance.GetTile(this.previousPosition).ClearHighlight();
    }

    /// <summary>
    /// 유닛이 행동을 완료했을 때 호출
    /// </summary>
    public void CompleteAction()
    {
        unitState = E_UnitState.Complete;
        UpdateUnitAppearance();
    }

    /// <summary>
    /// 턴이 시작될 때 유닛 상태 초기화
    /// </summary>
    public void ResetTurn()
    {
        unitState = E_UnitState.Idle;
        UpdateUnitAppearance();
    }

    /// <summary>
    /// 유닛 이동
    /// </summary>
    /// <param name="targetTile"></param>
    public void MoveTo(Tile targetTile)
    {
        // 현재 위치를 이동 전 위치로 저장
        previousPosition = currentTile.vec2IntPos;

        Tile moveTile = GridManager.Instance.FindNearestReachableTile(this, targetTile);
        List<Tile> path = GridManager.Instance.FindPathAStar(currentTile, moveTile);

        if (path.Count > 0)
        {
            StartCoroutine(MoveAlongPath(path));
        }
    }

    public IEnumerator MoveToCoroutine(Tile targetTile)
    {
        if (targetTile == null) yield break;

        List<Tile> path = GridManager.Instance.FindPathAStar(currentTile, targetTile);

        // 경로의 각 타일로 한 칸씩 이동
        foreach (Tile tile in path)
        {
            // 이동 방향 계산
            Vector2Int direction = tile.vec2IntPos - currentTile.vec2IntPos;
            unitAnimator.PlayMoveAnimation(direction); // 방향에 맞는 이동 애니메이션 실행

            // 타일 이동(부드럽게 이동)
            Vector3 startPos = transform.position;
            Vector3 endPos = tile.transform.position;
            float elapsedTime = 0f;
            float moveDuration = 0.3f; // 이동 속도 조절

            while (elapsedTime < moveDuration)
            {
                transform.position = Vector3.Lerp(startPos, endPos, (elapsedTime / moveDuration));
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            SoundManager.Instance.PlaySFX(SoundManager.Instance.moveSFX);

            transform.position = endPos;
            currentTile = tile;
            currentTile.isOccupied = false;

            // 한 칸 이동 후 멈춤 효과
            yield return new WaitForSeconds(0.2f);
        }

        GridManager.Instance.GetTile(this.previousPosition).ClearHighlight();
    }

    /// <summary>
    /// AI 유닛 이동
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
    /// A* 경로를 따라 이동
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private IEnumerator MoveAlongPath(List<Tile> path)
    {
        foreach (Tile tile in path)
        {
            SoundManager.Instance.PlaySFX(SoundManager.Instance.moveSFX);

            // 이동 방향 계산
            Vector2Int direction = tile.vec2IntPos - currentTile.vec2IntPos;
            unitAnimator.PlayMoveAnimation(direction); // 방향에 맞는 이동 애니메이션 실행

            // 타일 이동(부드럽게 이동)
            Vector3 startPos = transform.position;
            Vector3 endPos = tile.transform.position;
            float elapsedTime = 0f;
            float moveDuration = 0.3f; // 이동 속도 조절

            while (elapsedTime < moveDuration)
            {
                transform.position = Vector3.Lerp(startPos, endPos, (elapsedTime / moveDuration));
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            transform.position = endPos;
            currentTile = tile;
            currentTile.isOccupied = false;

            // 한 칸 이동 후 멈춤 효과
            yield return new WaitForSeconds(0.2f);
        }

        // 이동 완료 후 상태 변경
        unitState = E_UnitState.Move;

        // 이동 완료 후 타일 정보 업데이트
        currentTile.isOccupied = false;
        SetCurrentTile(path[path.Count - 1]);
        currentTile.isOccupied = true;

        CheckIdleState();
        // 이동완료 후 메뉴 UI 갱신
        if (unitData.unitTeam == E_UnitTeam.Ally)
        {
            UIManager.Instance.selectedUnit = this;
            UIManager.Instance.ShowActionMenu();
        }
        // 이동 완료 후 이벤트 실행
        OnMoveComplete?.Invoke();

        Deselect();
    }

    /// <summary>
    /// 유닛이 현재 위치한 타일을 설정
    /// </summary>
    /// <param name="tile">유닛이 이동할 타일</param>
    public void SetCurrentTile(Tile tile)
    {
        if (currentTile)
        {
            // 이전 타일에 캐릭터 해제
            currentTile.isOccupied = false;
        }

        currentTile = tile;
        currentTile.isOccupied = true; // 새 타일에 캐릭터 올라옴
        transform.position = tile.transform.position; // 유닛 위치 업데이트
    }

    /// <summary>
    /// 유닛이 자신의 턴을 수행
    /// </summary>
    /// <returns></returns>
    public IEnumerator TakeTurn()
    {
#if UNITY_EDITOR
        Debug.Log($"유닛 {unitData.unitId} 턴 시작");
#endif
        // 1. 공격 가능한 범위를 하이라이트
        GridManager.Instance.FindAttackableTiles(this);
        yield return new WaitForSeconds(0.5f);

        // 2. 즉시 공격 가능한 유닛이 있는지 확인
        Unit target = FindAttackableUnit();
        if (target)
        {
            Debug.Log("제자리에서 공격합니다~~~");
            GridManager.Instance.ShowHighLight();
            yield return new WaitForSeconds(0.5f);
            StartCoroutine(Attack(target));
            TurnManager.Instance.OnUnitTurnCompleted();
            yield break;
        }

        // 3. 공격할 유닛이 없다면 이동 가능 타일 탐색
        GridManager.Instance.FindWalkableTiles(this);
        yield return new WaitForSeconds(0.5f);

        // 4. 가장 가까운 적 탐색
        Tile targetTile = UnitManager.Instance.FindNearestUnit(this)?.currentTile;
        if (targetTile)
        {
            yield return MoveToAI(targetTile);

            // 5. 이동 후 다시 공격 가능 여부 확인
            GridManager.Instance.FindAttackableTiles(this);
            target = FindAttackableUnit();
            if (target)
            {
                Debug.Log("이동 후 공격합니다~~~");
                GridManager.Instance.ShowHighLight();
                yield return new WaitForSeconds(0.5f);
                StartCoroutine(Attack(target));
            }
        }

        // 6. 턴 종료
        unitState = E_UnitState.Complete;
        GridManager.Instance.ClearWalkableTiles();
        yield return new WaitForSeconds(0.3f);
        GridManager.Instance.ClearAttackableTiles();
        TurnManager.Instance.OnUnitTurnCompleted();
    }

    /// <summary>
    /// 공격 가능한 유닛 탐색
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
        Debug.Log($"{unitData.unitId} 공격 가능한 유닛 없음");
#endif
        return null;
    }

    /// <summary>
    /// 유닛 공격
    /// </summary>
    /// <param name="target"></param>
    public IEnumerator Attack(Unit target)
    {
        if (!target) yield break;
#if UNITY_EDITOR
        Debug.Log($"{unitData.unitId}가 {target.unitData.unitId}를 공격!");
#endif
        if (unitState == E_UnitState.Complete) yield break;

        // 공격애니메이션 실행
        SoundManager.Instance.PlaySFX(SoundManager.Instance.attackSFX);
        Vector2Int direction = target.currentTile.vec2IntPos - currentTile.vec2IntPos;
        unitAnimator.PlayAttackAnimation(direction);

        // 데미지 계산 및 공격 실행
        StartCoroutine( target.TakeDamage(unitData.attackPower));
        unitState = E_UnitState.Complete;
        yield return new WaitForSeconds(0.3f);
        unitAnimator.PlayIdleAnimation();
        GridManager.Instance.ClearAttackableTiles();

        // UI 선택창 다시 On
        if (unitData.unitTeam == E_UnitTeam.Ally)
        {
            UIManager.Instance.selectedUnit = this;
            UIManager.Instance.ShowActionMenu();
        }
        Deselect();
    }

    /// <summary>
    /// 데미지 입었을때
    /// </summary>
    /// <param name="damage"></param>
    public IEnumerator TakeDamage(int damage)
    {
        // 피격 애니메이션 실행
        unitAnimator.PlayHitAnimation();
        SoundManager.Instance.PlaySFX(SoundManager.Instance.hitSFX);
        unitData.hP -= damage;
        UIManager.Instance.CheckHp(this);
#if UNITY_EDITOR
        Debug.Log($"{unitData.unitId}가 {damage}의 피해를 입음! 남은 체력: {unitData.hP}");
#endif
        yield return new WaitForSeconds(0.5f);
        if (unitData.hP <= 0)
        {
            Die();
        }
        CheckIdleState();
    }

    /// <summary>
    /// 유닛 사망 처리
    /// </summary>
    private void Die()
    {
        // 사망 애니메이션 실행
        unitAnimator.PlayDeathAnimation();
        Debug.Log($"{unitData.unitId} 사망!");
        currentTile.isOccupied = false;
        UnitManager.Instance.RemoveUnit(this);

        // 즉시 게임 상태 확인 (승리/패배 판정)
        GameManager.Instance.CheckGameState();
    }

    /// <summary>
    /// 현재 체력에 따라 적절한 Idle 상태 적용
    /// </summary>
    private void CheckIdleState()
    {
        bool isDamaged = unitData.hP <= (unitData.maxHP * 0.3f); // 체력이 30% 이하이면 부상 상태
        unitAnimator.PlayIdleAnimation();
    }

    /// <summary>
    /// Undo행동
    /// </summary>
    public void UndoMove()
    {
        if (unitState != E_UnitState.Move) return; // 이동한 상태가 아닐 경우 실행 X

        Tile previousTile = GridManager.Instance.GetTile(previousPosition);
        if (previousTile == null) return;

        // 현재 타일 비우기
        currentTile.isOccupied = false;

        // 이전 타일로 복귀
        SetCurrentTile(previousTile);
        transform.position = previousTile.transform.position;

        // 상태를 Idle로 되돌려서 다시 행동 가능하게 설정
        unitState = E_UnitState.Idle;

        // UI 업데이트
        UIManager.Instance.HideActionMenu();
    }
}
