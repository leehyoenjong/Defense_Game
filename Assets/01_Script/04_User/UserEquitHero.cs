using System;
using System.Collections.Generic;
using BackEnd;


[Serializable]
public struct St_UserEquitHero
{
    public List<int> _equipheroid;
    public List<int> GetEquipHeroList() => _equipheroid;

    public Param Get_UserData()
    {
        var param = new Param();
        param.Add("_equipheroid", _equipheroid);
        return param;
    }

    public bool Load_UserData(BackendReturnObject loadresult)
    {
        if (loadresult.IsSuccess() == false)
        {
            return false;
        }

        var userdatajson = loadresult.FlattenRows()[0];

        // 장착 영웅 ID 리스트 로드
        if (userdatajson.ContainsKey("_equipheroid"))
        {
            var equipheroid = userdatajson["_equipheroid"];
            var maxcount = equipheroid.Count;
            for (int i = 0; i < maxcount; i++)
            {
                if (int.TryParse(equipheroid[i].ToString(), out var heroId))
                {
                    _equipheroid.Add(heroId);
                }
            }
        }

        return true;
    }

    public void EquipHero(int heroitemid, int idx)
    {
        UnequipHero(heroitemid);
        _equipheroid[idx] = heroitemid;
        BackEndLog.WriteLog(LogType.EQUIP, $"{idx} 번호에 {heroitemid}번 영웅 장착");
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