using System;
using System.Collections.Generic;


[Serializable]
public struct St_UserEquitHero
{
    public List<int> _equipheroid;
    public List<int> GetEquipHeroList() => _equipheroid;

    public void EquipHero(int heroid)
    {
        _equipheroid.Add(heroid);
    }

    public void UnequipHero(int heroid)
    {
        _equipheroid.Remove(heroid);
    }
}