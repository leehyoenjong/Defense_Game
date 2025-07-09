using System;
using System.Collections.Generic;


[Serializable]
public struct St_UserEquitHero
{
    public List<int> _equipheroid;
    public List<int> GetEquipHeroList() => _equipheroid;

    public void EquipHero(int heroitemid, int idx)
    {
        UnequipHero(heroitemid);
        _equipheroid[idx] = heroitemid;
    }

    public void UnequipHero(int heroitemid)
    {
        var idx = _equipheroid.FindIndex(x => x == heroitemid);
        if (idx == -1)
        {
            return;
        }
        _equipheroid[idx] = 0;
    }
}