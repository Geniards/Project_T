using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "UnitAnimationDatabase", menuName = "Unit/AnimationDatabase")]
public class UnitAnimationDatabase : ScriptableObject
{
    public List<UnitAnimationData> unitAnimations = new List<UnitAnimationData>();

    /// <summary>
    /// 유닛 ID에 맞는 애니메이션 데이터를 반환
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
        // 해당 유닛 ID가 없으면 기본값 처리 필요
        return null;
    }
}