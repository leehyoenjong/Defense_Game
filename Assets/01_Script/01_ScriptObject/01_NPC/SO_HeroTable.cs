using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SO_PlayerPrefab", menuName = "SO_PlayerPrefab", order = 0)]
public class SO_HeroTable : ScriptableObject
{
    [SerializeField] List<St_HeroTable> _playerlist;
    public List<St_HeroTable> GetHeroList() => _playerlist;
    public St_HeroTable GetHeroList(int heroid)
    {
        var playerdata = _playerlist.Find(x => x._npc._mid == heroid);
        return playerdata;
    }
}

[Serializable]
public struct St_HeroTable
{
    [Header("플레이어 정보")]
    public SO_NPC _npc;
    [Header("애니메이션")]
    public AnimatorOverrideController _animator_with_ui;
}