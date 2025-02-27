using UnityEngine;

[CreateAssetMenu(fileName = "UnitAnimationData", menuName = "Unit/AnimationData")]
public class UnitAnimationData : ScriptableObject
{
    public int unitTypeId;  // 유닛 ID (JSON 데이터와 매칭)
    public AnimatorOverrideController overrideController;

    [Header("방향별 애니메이션 해시")]
    public int IdleHash = Animator.StringToHash("Idle");
    public int InjuredIdleHash = Animator.StringToHash("InjuredIdle");

    // 4방향 이동 애니메이션
    public int MoveUpHash = Animator.StringToHash("Move_Up");
    public int MoveDownHash = Animator.StringToHash("Move_Down");
    public int MoveLeftHash = Animator.StringToHash("Move_Left");
    public int MoveRightHash = Animator.StringToHash("Move_Right");

    // 4방향 공격 애니메이션
    public int AttackUpHash = Animator.StringToHash("Attack_Up");
    public int AttackDownHash = Animator.StringToHash("Attack_Down");
    public int AttackLeftHash = Animator.StringToHash("Attack_Left");
    public int AttackRightHash = Animator.StringToHash("Attack_Right");

    // 공통 애니메이션
    public int HitHash = Animator.StringToHash("Hit");
    public int DieHash = Animator.StringToHash("Die");
}
