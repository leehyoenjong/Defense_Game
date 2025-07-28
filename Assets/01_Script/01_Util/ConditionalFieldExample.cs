using UnityEngine;

/// <summary>
/// ConditionalFieldAttribute의 다양한 사용 예시를 보여주는 테스트 클래스
/// 단일/다중 필드 조건과 AND/OR 연산 사용법을 확인할 수 있습니다.
/// </summary>
public class ConditionalFieldExample : MonoBehaviour
{
    [Header("기본 조건 설정")]
    public bool _enableSpecialMode = false;
    public int _weaponType = 0; // 0: None, 1: Sword, 2: Bow, 3: Magic
    public string _characterClass = "Warrior";

    [Header("단일 필드 조건 예시")]
    [ConditionalField("_enableSpecialMode", true)]
    public float _specialModeValue = 100f;

    [ConditionalField("_weaponType", 1)] // Sword일 때만 표시
    public int _swordDamage = 50;

    [ConditionalField("_weaponType", 2)] // Bow일 때만 표시
    public int _arrowCount = 30;

    [ConditionalField("_weaponType", true, 1, 2)] // Sword 또는 Bow일 때 표시
    public float _criticalChance = 0.1f;

    [Header("다중 필드 AND 조건 예시")]
    // enableSpecialMode가 true이고 weaponType이 1(Sword)일 때만 표시
    [ConditionalField(true, "_enableSpecialMode", true, "_weaponType", 1)]
    public string _specialSwordAbility = "Fire Slash";

    // enableSpecialMode가 true이고 characterClass가 "Mage"일 때만 표시
    [ConditionalField(true, "_enableSpecialMode", true, "_characterClass", "Mage")]
    public int _manaPoints = 200;

    // 3개 필드 AND 조건: 특수모드 + 활 + 전사 클래스
    [ConditionalField(true, "_enableSpecialMode", true, "_weaponType", 2, "_characterClass", "Warrior")]
    public string _archerWarriorSkill = "Power Shot";

    [Header("다중 필드 OR 조건 예시")]
    // enableSpecialMode가 true이거나 weaponType이 3(Magic)일 때 표시
    [ConditionalField(ConditionalFieldAttribute.ConditionOperator.Or, true, "_enableSpecialMode", true, "_weaponType", 3)]
    public float _magicPower = 150f;

    // weaponType이 1(Sword)이거나 2(Bow)일 때 표시 (OR 조건)
    [ConditionalField(ConditionalFieldAttribute.ConditionOperator.Or, true, "_weaponType", 1, "_weaponType", 2)]
    public float _physicalDamageBonus = 1.2f;

    // 3개 필드 OR 조건
    [ConditionalField(ConditionalFieldAttribute.ConditionOperator.Or, true, "_characterClass", "Warrior", "_characterClass", "Rogue", "_characterClass", "Paladin")]
    public int _stamina = 100;

    [Header("역조건 예시 (조건이 거짓일 때 표시)")]
    // enableSpecialMode가 false일 때만 표시 (명시적으로 단일 값 생성자 사용)
    [ConditionalField("_enableSpecialMode", (object)false, false)]
    public string _normalModeMessage = "일반 모드입니다";

    // weaponType이 0이 아닐 때 표시 (무기가 있을 때)
    [ConditionalField("_weaponType", 0, false)]
    public float _weaponWeight = 2.5f;

    // AND 조건의 역: enableSpecialMode가 false이거나 weaponType이 0이 아닐 때 숨김
    [ConditionalField(false, "_enableSpecialMode", false, "_weaponType", 0)]
    public string _hiddenWhenConditionsMet = "조건이 맞으면 숨겨집니다";

    [Header("배열 필드 조건 예시")]
    public int[] _skillLevels = new int[3];

    [ConditionalField("_enableSpecialMode", true)]
    public GameObject[] _specialEffects = new GameObject[0];

    [ConditionalField(true, "_enableSpecialMode", true, "_weaponType", 3)]
    public string[] _spellNames = { "Fireball", "Lightning", "Heal" };

    public enum CharacterState { Idle, Combat, Dead, Casting }

    [Header("복잡한 조건 조합 예시")]
    public CharacterState _currentState = CharacterState.Idle;

    // 특수모드이고 전투 상태이며 마법 무기를 사용할 때만 표시
    [ConditionalField(true, "_enableSpecialMode", true, "_currentState", (int)CharacterState.Combat, "_weaponType", 3)]
    public float _combatMagicBonus = 2.0f;

    // 죽지 않은 상태일 때 표시 (OR 조건 활용)
    [ConditionalField(ConditionalFieldAttribute.ConditionOperator.Or, true,
        "_currentState", (int)CharacterState.Idle,
        "_currentState", (int)CharacterState.Combat,
        "_currentState", (int)CharacterState.Casting)]
    public float _healthRegenRate = 5f;

    private void Start()
    {
        Debug.Log("ConditionalFieldExample 시작!");
        Debug.Log($"현재 설정: 특수모드={_enableSpecialMode}, 무기타입={_weaponType}, 클래스={_characterClass}");
    }

    [ContextMenu("특수 모드 토글")]
    private void ToggleSpecialMode()
    {
        _enableSpecialMode = !_enableSpecialMode;
        Debug.Log($"특수 모드: {_enableSpecialMode}");
    }

    [ContextMenu("무기 타입 변경")]
    private void ChangeWeaponType()
    {
        _weaponType = (_weaponType + 1) % 4;
        string[] weaponNames = { "없음", "검", "활", "마법" };
        Debug.Log($"무기 타입: {weaponNames[_weaponType]}");
    }
}