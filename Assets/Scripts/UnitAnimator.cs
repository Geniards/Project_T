using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitAnimator : MonoBehaviour
{
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private UnitAnimationData animationData;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// 유닛의 애니메이션을 변경 (애니메이션 오버라이드 적용)
    /// </summary>
    /// <param name="newOverrideController"></param>
    public void SetOverrideAnimations(UnitAnimationData animData)
    {
        animationData = animData;
        if (animator && animationData.overrideController)
        {
            animator.runtimeAnimatorController = animationData.overrideController;
        }
    }

    /// <summary>
    /// 체력 상태에 따라 적절한 Idle 애니메이션 실행
    /// </summary>
    /// <param name="isDamaged"></param>
    public void PlayIdleAnimation(bool isDamaged = false)
    {
        if (animator)
        {
            int idleHash = isDamaged ? Animator.StringToHash("InjuredIdle") : animationData.IdleHash;
            animator.Play(idleHash);
        }
    }

    /// <summary>
    /// 이동 애니메이션 실행
    /// </summary>
    public void PlayMoveAnimation(Vector2Int direction)
    {
        // 왼쪽(-1,0) 또는 오른쪽(1,0) 방향일 경우 Flip 처리
        if (direction.x != 0)
        {
            spriteRenderer.flipX = direction.x > 0; // 오른쪽이면 flipX 적용
        }
        
        // 방향에 맞는 애니메이션 실행
        if (direction.y > 0)
            animator.Play(animationData.MoveUpHash);
        else if (direction.y < 0)
            animator.Play(animationData.MoveDownHash);
        else
            animator.Play(animationData.MoveLeftHash); // 왼쪽 애니메이션을 기본으로 사용

    }

    /// <summary>
    /// 공격 애니메이션 실행
    /// </summary>
    public void PlayAttackAnimation(Vector2Int direction)
    {
        // TODO : GetAnimationStateInfo()를 사용해서 현재 애니메이션 상태를 호출가능한데 이것을 이용할수 없을까?
        if (animator)
        {
            if (direction.x != 0)
            {
                spriteRenderer.flipX = direction.x > 0; // 오른쪽이면 flipX 적용
            }

            if (direction.y > 0)
                animator.Play(animationData.AttackUpHash);
            else if (direction.y < 0)
                animator.Play(animationData.AttackDownHash);
            else
                animator.Play(animationData.AttackLeftHash); // 왼쪽 애니메이션을 기본으로 사용
        }
    }

    /// <summary>
    /// 피격 애니메이션 실행
    /// </summary>
    public void PlayHitAnimation()
    {
        animator.Play(animationData.HitHash);
    }

    /// <summary>
    /// 사망 애니메이션 실행
    /// </summary>
    public void PlayDeathAnimation()
    {
        animator.Play(animationData.DieHash);
    }
}
