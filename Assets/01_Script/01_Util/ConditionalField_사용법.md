# ConditionalFieldAttribute 다중 필드 조건 사용법

## 개요
`ConditionalFieldAttribute`는 Unity 인스펙터에서 특정 조건에 따라 필드를 동적으로 표시하거나 숨길 수 있는 속성입니다. 이제 여러 필드를 조건으로 사용하고 AND/OR 연산을 지원합니다.

## 기능
- ✅ 단일 필드 조건 (기존 기능)
- ✅ 다중 필드 조건 (새로운 기능)
- ✅ AND/OR 연산 지원
- ✅ 배열 필드 지원
- ✅ 역조건 지원 (조건이 거짓일 때 표시)
- ✅ 다양한 데이터 타입 지원 (bool, int, float, string, enum)

## 단일 필드 조건 사용법

### 기본 사용법
```csharp
public bool _enableFeature = false;

[ConditionalField("_enableFeature", true)]
public float _featureValue = 100f;
```

### 다중 값 조건
```csharp
public int _mode = 0;

// mode가 1 또는 2일 때 표시
[ConditionalField("_mode", true, 1, 2)]
public string _modeSpecificSetting = "설정값";
```

### 역조건 (거짓일 때 표시)
```csharp
public bool _isAdvanced = false;

// isAdvanced가 false일 때만 표시
[ConditionalField("_isAdvanced", false, false)]
public string _beginnerHelp = "초보자용 도움말";
```

## 다중 필드 조건 사용법

### AND 조건 (모든 조건이 참이어야 함)
```csharp
public bool _enableSpecial = false;
public int _level = 1;

// enableSpecial이 true이고 level이 5일 때만 표시
[ConditionalField(true, "_enableSpecial", true, "_level", 5)]
public string _specialAbility = "특수 능력";
```

### OR 조건 (조건 중 하나라도 참이면 됨)
```csharp
public int _weaponType = 0;
public bool _isMagic = false;

// weaponType이 1이거나 isMagic이 true일 때 표시
[ConditionalField(ConditionalFieldAttribute.ConditionOperator.Or, true, 
    "_weaponType", 1, "_isMagic", true)]
public float _damageBonus = 1.5f;
```

### 3개 필드 조건
```csharp
public bool _isActive = false;
public string _class = "Warrior";
public int _weaponType = 0;

// 모든 조건이 만족될 때만 표시 (AND)
[ConditionalField(true, "_isActive", true, "_class", "Mage", "_weaponType", 3)]
public int _manaPoints = 200;

// 조건 중 하나라도 만족되면 표시 (OR)
[ConditionalField(ConditionalFieldAttribute.ConditionOperator.Or, true,
    "_class", "Warrior", "_class", "Paladin", "_class", "Knight")]
public int _strength = 100;
```

## 생성자 패턴

### 단일 필드
```csharp
// 단일 값
ConditionalField(string field, object value, bool showWhenTrue = true)

// 다중 값 (OR 조건)
ConditionalField(string field, bool showWhenTrue, params object[] values)
```

### 다중 필드 (2개)
```csharp
// AND 조건 (기본)
ConditionalField(bool showWhenTrue, string field1, object value1, string field2, object value2)

// 사용자 지정 연산자
ConditionalField(ConditionOperator op, bool showWhenTrue, string field1, object value1, string field2, object value2)
```

### 다중 필드 (3개)
```csharp
// AND 조건 (기본)
ConditionalField(bool showWhenTrue, string field1, object value1, string field2, object value2, string field3, object value3)

// 사용자 지정 연산자
ConditionalField(ConditionOperator op, bool showWhenTrue, string field1, object value1, string field2, object value2, string field3, object value3)
```

## 실용적인 사용 예시

### 게임 설정 UI
```csharp
public bool _enableAdvancedGraphics = false;
public int _qualityLevel = 1;

[ConditionalField("_enableAdvancedGraphics", true)]
public bool _enableRayTracing = false;

[ConditionalField(true, "_enableAdvancedGraphics", true, "_qualityLevel", 3)]
public int _shadowCascades = 4;
```

### 캐릭터 스킬 시스템
```csharp
public enum CharacterClass { Warrior, Mage, Archer }
public CharacterClass _class = CharacterClass.Warrior;
public int _level = 1;

[ConditionalField("_class", (int)CharacterClass.Mage)]
public int _mana = 100;

[ConditionalField(true, "_class", (int)CharacterClass.Warrior, "_level", 10)]
public string _ultimateSkill = "베르세르크";
```

### 무기 시스템
```csharp
public enum WeaponType { None, Sword, Bow, Staff }
public WeaponType _weaponType = WeaponType.None;
public bool _isEnchanted = false;

// 근접 무기일 때만 표시
[ConditionalField(ConditionalFieldAttribute.ConditionOperator.Or, true,
    "_weaponType", (int)WeaponType.Sword)]
public float _meleeRange = 2f;

// 인챈트된 스태프일 때만 표시
[ConditionalField(true, "_weaponType", (int)WeaponType.Staff, "_isEnchanted", true)]
public float _magicPower = 150f;
```

## 주의사항

1. **필드명 정확성**: 조건 필드명은 정확해야 하며, 대소문자를 구분합니다.
2. **타입 일치**: 조건 값의 타입이 필드 타입과 일치해야 합니다.
3. **Enum 처리**: Enum 값은 `(int)EnumValue` 형태로 캐스팅하여 사용합니다.
4. **배열 요소**: 배열 요소에서도 정상적으로 작동하지만, 조건 필드는 배열 밖에 있어야 합니다.

## 디버깅

개발 중 조건이 제대로 작동하지 않을 때는 다음을 확인하세요:

1. 스크립트 상단에 `#define DEBUG_CONDITIONAL_FIELD` 추가
2. 인스펙터에서 디버그 정보 확인
3. 조건 필드명과 값이 올바른지 확인

```csharp
#define DEBUG_CONDITIONAL_FIELD
using UnityEngine;

public class TestScript : MonoBehaviour
{
    // 디버그 정보가 인스펙터에 표시됩니다
}
```

이제 복잡한 조건부 UI를 쉽게 구현할 수 있습니다! 