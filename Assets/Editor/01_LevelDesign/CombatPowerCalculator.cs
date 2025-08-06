using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class CombatPowerCalculator
{
    private static SO_Status_Table _statusTable;
    private static SO_MonsterTable _monsterTable;

    public static float Calculate(SO_StageData stageData)
    {
        if (stageData == null) return 0;

        // 테이블 데이터 로드 (캐싱)
        if (_statusTable == null)
        {
            string[] statusTableGuid = AssetDatabase.FindAssets("t:SO_Status_Table");
            if (statusTableGuid.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(statusTableGuid[0]);
                _statusTable = AssetDatabase.LoadAssetAtPath<SO_Status_Table>(path);
            }
            else
            {
                Debug.LogError("SO_Status_Table 에셋을 찾을 수 없습니다.");
                return 0;
            }
        }

        if (_monsterTable == null)
        {
            string[] monsterTableGuid = AssetDatabase.FindAssets("t:SO_MonsterTable");
            if (monsterTableGuid.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(monsterTableGuid[0]);
                _monsterTable = AssetDatabase.LoadAssetAtPath<SO_MonsterTable>(path);
            }
            else
            {
                Debug.LogError("SO_MonsterTable 에셋을 찾을 수 없습니다.");
                return 0;
            }
        }


        float totalCombatPower = 0;
        foreach (var wave in stageData._monsterlist)
        {
            var monsterInfo = _monsterTable.GetMonsterInfo(wave._monsterid);
            if (monsterInfo._npc == null)
            {
                Debug.LogWarning($"몬스터 ID {wave._monsterid}를 SO_MonsterTable에서 찾을 수 없습니다.");
                continue;
            }

            // 몬스터의 기본 스탯 가져오기
            var statusList = _statusTable.GetStatusData(monsterInfo._npc._statusid);
            if (statusList == null || statusList.Count == 0)
            {
                Debug.LogWarning($"몬스터 ID {wave._monsterid}의 스탯 정보를 SO_Status_Table에서 찾을 수 없습니다. (Status ID: {monsterInfo._npc._statusid})");
                continue;
            }

            // 1레벨 기준 스탯으로 계산 (혹은 필요에 따라 레벨 지정)
            St_Status status = statusList[0];

            float attackPower = status._damge * (1 + status._critical * status._critical_damage);
            float defensePower = status._hp * (1 + status._armor / 100f);

            float singleMonsterPower = attackPower + defensePower;
            totalCombatPower += singleMonsterPower * wave._count;
        }

        return totalCombatPower;
    }
}
