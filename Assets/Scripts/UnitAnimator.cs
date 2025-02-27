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
    /// ������ �ִϸ��̼��� ���� (�ִϸ��̼� �������̵� ����)
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
    /// ü�� ���¿� ���� ������ Idle �ִϸ��̼� ����
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
    /// �̵� �ִϸ��̼� ����
    /// </summary>
    public void PlayMoveAnimation(Vector2Int direction)
    {
        // ����(-1,0) �Ǵ� ������(1,0) ������ ��� Flip ó��
        if (direction.x != 0)
        {
            spriteRenderer.flipX = direction.x > 0; // �������̸� flipX ����
        }
        
        // ���⿡ �´� �ִϸ��̼� ����
        if (direction.y > 0)
            animator.Play(animationData.MoveUpHash);
        else if (direction.y < 0)
            animator.Play(animationData.MoveDownHash);
        else
            animator.Play(animationData.MoveLeftHash); // ���� �ִϸ��̼��� �⺻���� ���

    }

    /// <summary>
    /// ���� �ִϸ��̼� ����
    /// </summary>
    public void PlayAttackAnimation(Vector2Int direction)
    {
        // TODO : GetAnimationStateInfo()�� ����ؼ� ���� �ִϸ��̼� ���¸� ȣ�Ⱑ���ѵ� �̰��� �̿��Ҽ� ������?
        if (animator)
        {
            if (direction.x != 0)
            {
                spriteRenderer.flipX = direction.x > 0; // �������̸� flipX ����
            }

            if (direction.y > 0)
                animator.Play(animationData.AttackUpHash);
            else if (direction.y < 0)
                animator.Play(animationData.AttackDownHash);
            else
                animator.Play(animationData.AttackLeftHash); // ���� �ִϸ��̼��� �⺻���� ���
        }
    }

    /// <summary>
    /// �ǰ� �ִϸ��̼� ����
    /// </summary>
    public void PlayHitAnimation()
    {
        animator.Play(animationData.HitHash);
    }

    /// <summary>
    /// ��� �ִϸ��̼� ����
    /// </summary>
    public void PlayDeathAnimation()
    {
        animator.Play(animationData.DieHash);
    }
}
