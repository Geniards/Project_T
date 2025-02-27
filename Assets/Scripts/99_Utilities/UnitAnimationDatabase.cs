using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "UnitAnimationDatabase", menuName = "Unit/AnimationDatabase")]
public class UnitAnimationDatabase : ScriptableObject
{
    public List<UnitAnimationData> unitAnimations = new List<UnitAnimationData>();

    /// <summary>
    /// ���� ID�� �´� �ִϸ��̼� �����͸� ��ȯ
    /// </summary>
    /// <param name="unitTypeId"></param>
    /// <returns></returns>
    public UnitAnimationData GetAnimationData(int unitTypeId)
    {
        foreach (var animData in unitAnimations)
        {
            if (animData.unitTypeId == unitTypeId)
                return animData;
        }
        // �ش� ���� ID�� ������ �⺻�� ó�� �ʿ�
        return null;
    }
}