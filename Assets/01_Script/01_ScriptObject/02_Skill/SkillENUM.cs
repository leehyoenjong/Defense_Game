public enum EATTACKTYPE
{
    NOW,//즉시 공격
    OBJECT_ENTER,//오브젝트에 닿으면 공격
}

public enum EATTACKAREA
{
    ONE,//한마리
    ALL,//전체
    CIRCLE,//동그란 범위
    BOX,//네모난 범위
}

public enum EATTACKTARGETKIND
{
    POS_NEAR,//가장 가까운
    POS_FAR,//가장 먼
    MOST_CURRENT_HP,//현재 체력이 가장 많은
    MOST_SMALL_CURRENT_HP,//현재 체력이 가장 작은
    MOST_MAXHP,//최대 체력이 가장 큰
    MOST_SMALL_MAXHP,//최대 체력이 가장 작은
    MOST_POWER,//공격력이 가장 센
    BOSS,//보스만
    PROTECT,//보호 오브젝트만
}