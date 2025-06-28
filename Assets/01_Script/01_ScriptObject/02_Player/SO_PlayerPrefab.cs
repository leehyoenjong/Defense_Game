using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SO_PlayerPrefab", menuName = "SO_PlayerPrefab", order = 0)]
public class SO_PlayerPrefab : ScriptableObject
{
    [SerializeField] List<St_PlayerList> _playerlist;

    public St_PlayerList GetHeroList(int heroid)
    {
        var playerdata = _playerlist.Find(x => x._player_id == heroid);
        return playerdata;
    }
}

[Serializable]
public struct St_PlayerList
{
    public int _player_id;
    public string _name;
    public Sprite _icon;
    public GameObject _playerobject;
}