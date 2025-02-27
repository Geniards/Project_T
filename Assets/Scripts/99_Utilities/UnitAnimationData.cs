using UnityEngine;

[CreateAssetMenu(fileName = "UnitAnimationData", menuName = "Unit/AnimationData")]
public class UnitAnimationData : ScriptableObject
{
    public int unitTypeId;  // ���� ID (JSON �����Ϳ� ��Ī)
    public AnimatorOverrideController overrideController;

    [Header("���⺰ �ִϸ��̼� �ؽ�")]
    public int IdleHash = Animator.StringToHash("Idle");
    public int InjuredIdleHash = Animator.StringToHash("InjuredIdle");

    // 4���� �̵� �ִϸ��̼�
    public int MoveUpHash = Animator.StringToHash("Move_Up");
    public int MoveDownHash = Animator.StringToHash("Move_Down");
    public int MoveLeftHash = Animator.StringToHash("Move_Left");
    public int MoveRightHash = Animator.StringToHash("Move_Right");

    // 4���� ���� �ִϸ��̼�
    public int AttackUpHash = Animator.StringToHash("Attack_Up");
    public int AttackDownHash = Animator.StringToHash("Attack_Down");
    public int AttackLeftHash = Animator.StringToHash("Attack_Left");
    public int AttackRightHash = Animator.StringToHash("Attack_Right");

    // ���� �ִϸ��̼�
    public int HitHash = Animator.StringToHash("Hit");
    public int DieHash = Animator.StringToHash("Die");
}
