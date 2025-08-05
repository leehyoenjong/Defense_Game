using UnityEngine;

public class SkillChoseManager : MonoBehaviour
{
    [SerializeField] GameObject _skillchoseobject;
    public const int CREATESKILLCHOSESTAGE = 2;//2스테이지 마다 스킬 선택지 등장

    int _currentstageclear = 0;

    void Start()
    {
        PlayManager._ongamestatechanged += CreateSkillChose;
    }

    void OnDisable()
    {
        PlayManager._ongamestatechanged -= CreateSkillChose;
    }

    void CreateSkillChose(GameStateData gamestate)
    {
        if (gamestate._state != EPLAYSTATE.STAGE_NEXT)
        {
            return;
        }

        _currentstageclear++;
        if (_currentstageclear % CREATESKILLCHOSESTAGE != 0)
        {
            return;
        }

        _skillchoseobject.SetActive(true);
    }
}