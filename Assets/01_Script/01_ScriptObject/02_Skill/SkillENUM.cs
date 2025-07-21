public enum EUSETYPE
{
    NOW,//즉시 발동
    OBJECT_ENTER,//오브젝트에 닿으면 발동
}

public enum ESKILLAREA
{
    ONE,//한마리
    ALL,//전체
    CIRCLE,//동그란 범위
    BOX,//네모난 범위
}

public enum ETARGETFILTERTYPE
{
    NONE,//필터 없음

    ME,//자기 자신

    POS_NEAR_HERO,//가장 가까운
    POS_FAR_HERO,//가장 먼
    MOST_CURRENT_HP_HERO,//현재 체력이 가장 많은
    MOST_SMALL_CURRENT_HP_HERO,//현재 체력이 가장 작은
    MOST_MAXHP_HERO,//최대 체력이 가장 큰
    MOST_SMALL_MAXHP_HERO,//최대 체력이 가장 작은
    MOST_POWER_HERO,//공격력이 가장 센

    POS_NEAR_MONSTER,//가장 가까운
    POS_FAR_MONSTER,//가장 먼
    MOST_CURRENT_HP_MONSTER,//현재 체력이 가장 많은
    MOST_SMALL_CURRENT_HP_MONSTER,//현재 체력이 가장 작은
    MOST_MAXHP_MONSTER,//최대 체력이 가장 큰
    MOST_SMALL_MAXHP_MONSTER,//최대 체력이 가장 작은
    MOST_POWER_MONSTER,//공격력이 가장 센
}

public enum ETARGETKIND
{
    ME,//자기 자신
    HERO,//영웅만
    MONSTER,//몬스터만
    BOSS,//보스만
    PROTECT,//보호 오브젝트만
}

public enum ESKILLKIND
{
    NONE,
    ATTACK,
    BUFF
}