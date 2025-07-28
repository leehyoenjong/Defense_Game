using UnityEditor;
using UnityEngine;

/// <summary>
/// ConditionalFieldAttribute용 PropertyDrawer
/// 조건에 따라 필드를 표시하거나 숨깁니다.
/// 배열 필드와 다중 필드 조건도 지원합니다.
/// </summary>
[CustomPropertyDrawer(typeof(ConditionalFieldAttribute))]
public class ConditionalFieldPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ConditionalFieldAttribute conditionalAttribute = (ConditionalFieldAttribute)attribute;

        // 조건 체크
        bool shouldShow = ShouldShowProperty(property, conditionalAttribute);

        // 디버깅 정보 (개발 중에만 표시)
        #if UNITY_EDITOR && DEBUG_CONDITIONAL_FIELD
        if (shouldShow)
        {
            var debugRect = new Rect(position.x, position.y - 15, position.width, 15);
            string debugInfo = GetDebugInfo(property, conditionalAttribute);
            EditorGUI.LabelField(debugRect, debugInfo, EditorStyles.miniLabel);
        }
        #endif

        if (shouldShow)
        {
            EditorGUI.PropertyField(position, property, label, true);
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        ConditionalFieldAttribute conditionalAttribute = (ConditionalFieldAttribute)attribute;
        
        // 조건 체크
        bool shouldShow = ShouldShowProperty(property, conditionalAttribute);

        if (shouldShow)
        {
            float height = EditorGUI.GetPropertyHeight(property, label, true);
            #if UNITY_EDITOR && DEBUG_CONDITIONAL_FIELD
            height += 15; // 디버그 라인 추가
            #endif
            return height;
        }

        return -2f; // 완전히 숨김 (여백도 제거)
    }

    private bool ShouldShowProperty(SerializedProperty property, ConditionalFieldAttribute conditionalAttribute)
    {
        bool conditionMet;

        if (conditionalAttribute.IsMultiFieldCondition)
        {
            // 다중 필드 조건 처리
            conditionMet = EvaluateMultiFieldConditions(property, conditionalAttribute);
        }
        else
        {
            // 기존 단일 필드 조건 처리
            conditionMet = EvaluateSingleFieldCondition(property, conditionalAttribute);
        }

        return conditionalAttribute._showWhenTrue ? conditionMet : !conditionMet;
    }

    private bool EvaluateSingleFieldCondition(SerializedProperty property, ConditionalFieldAttribute conditionalAttribute)
    {
        // 조건 필드 찾기
        SerializedProperty conditionalProperty = FindConditionalProperty(property, conditionalAttribute._conditionalSourceField);
        
        if (conditionalProperty == null)
        {
            // 조건 필드를 찾을 수 없으면 기본적으로 표시
            return true;
        }

        // 조건 체크 (여러 값 중 하나라도 일치하면 조건 만족)
        return CheckConditions(conditionalProperty, conditionalAttribute._conditionalValues);
    }

    private bool EvaluateMultiFieldConditions(SerializedProperty property, ConditionalFieldAttribute conditionalAttribute)
    {
        var fieldConditions = conditionalAttribute._fieldConditions;
        bool[] conditionResults = new bool[fieldConditions.Length];

        // 각 필드 조건을 개별적으로 평가
        for (int i = 0; i < fieldConditions.Length; i++)
        {
            var fieldCondition = fieldConditions[i];
            SerializedProperty conditionalProperty = FindConditionalProperty(property, fieldCondition._fieldName);
            
            if (conditionalProperty == null)
            {
                // 조건 필드를 찾을 수 없으면 해당 조건은 거짓으로 처리
                conditionResults[i] = false;
            }
            else
            {
                conditionResults[i] = CheckConditions(conditionalProperty, fieldCondition._values);
            }
        }

        // AND/OR 연산 수행
        return conditionalAttribute._conditionOperator == ConditionalFieldAttribute.ConditionOperator.And
            ? EvaluateAndConditions(conditionResults)
            : EvaluateOrConditions(conditionResults);
    }

    private bool EvaluateAndConditions(bool[] conditionResults)
    {
        // 모든 조건이 참이어야 함
        foreach (bool result in conditionResults)
        {
            if (!result) return false;
        }
        return true;
    }

    private bool EvaluateOrConditions(bool[] conditionResults)
    {
        // 조건 중 하나라도 참이면 됨
        foreach (bool result in conditionResults)
        {
            if (result) return true;
        }
        return false;
    }

    private SerializedProperty FindConditionalProperty(SerializedProperty property, string conditionalSourceField)
    {
        // 배열 요소인 경우 부모 오브젝트에서 조건 필드를 찾아야 함
        string propertyPath = property.propertyPath;
        
        // 배열 요소의 경우 (예: "_enter_hit_object.Array.data[0]")
        if (propertyPath.Contains(".Array.data["))
        {
            // 배열 이름까지의 경로에서 조건 필드 찾기
            var parts = propertyPath.Split('.');
            string basePath = "";
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i] == "Array")
                    break;
                if (i > 0) basePath += ".";
                basePath += parts[i];
            }
            
            // 같은 레벨에서 조건 필드 찾기
            string[] pathSegments = basePath.Split('.');
            pathSegments[pathSegments.Length - 1] = conditionalSourceField;
            string conditionalPath = string.Join(".", pathSegments);
            
            return property.serializedObject.FindProperty(conditionalPath);
        }
        
        // 일반 필드의 경우
        return property.serializedObject.FindProperty(conditionalSourceField);
    }

    private bool CheckConditions(SerializedProperty conditionalProperty, object[] conditionalValues)
    {
        // 여러 조건 값 중 하나라도 일치하면 true 반환
        foreach (object conditionalValue in conditionalValues)
        {
            if (CheckCondition(conditionalProperty, conditionalValue))
            {
                return true;
            }
        }
        return false;
    }

    private bool CheckCondition(SerializedProperty conditionalProperty, object conditionalValue)
    {
        switch (conditionalProperty.propertyType)
        {
            case SerializedPropertyType.Boolean:
                return conditionalProperty.boolValue.Equals(conditionalValue);
            
            case SerializedPropertyType.Integer:
                return conditionalProperty.intValue.Equals(conditionalValue);
            
            case SerializedPropertyType.Float:
                return conditionalProperty.floatValue.Equals(conditionalValue);
            
            case SerializedPropertyType.String:
                return conditionalProperty.stringValue.Equals(conditionalValue);
            
            case SerializedPropertyType.Enum:
                // 열거형 비교 개선
                int enumValue = conditionalProperty.enumValueIndex;
                int targetValue = System.Convert.ToInt32(conditionalValue);
                return enumValue == targetValue;
            
            default:
                return false;
        }
    }

    private string GetDebugInfo(SerializedProperty property, ConditionalFieldAttribute conditionalAttribute)
    {
        if (conditionalAttribute.IsMultiFieldCondition)
        {
            // 다중 필드 디버그 정보
            string debugInfo = $"[DEBUG Multi] {conditionalAttribute._conditionOperator}: ";
            for (int i = 0; i < conditionalAttribute._fieldConditions.Length; i++)
            {
                var fieldCondition = conditionalAttribute._fieldConditions[i];
                var conditionalProperty = FindConditionalProperty(property, fieldCondition._fieldName);
                if (conditionalProperty != null)
                {
                    string currentValue = GetCurrentValue(conditionalProperty);
                    string valuesStr = string.Join(", ", fieldCondition._values);
                    debugInfo += $"{fieldCondition._fieldName}={currentValue} in [{valuesStr}]";
                    if (i < conditionalAttribute._fieldConditions.Length - 1)
                        debugInfo += " | ";
                }
            }
            return debugInfo;
        }
        else
        {
            // 단일 필드 디버그 정보
            var conditionalProperty = FindConditionalProperty(property, conditionalAttribute._conditionalSourceField);
            if (conditionalProperty != null)
            {
                string valuesStr = string.Join(", ", conditionalAttribute._conditionalValues);
                return $"[DEBUG] {conditionalAttribute._conditionalSourceField}: {GetCurrentValue(conditionalProperty)} in [{valuesStr}]";
            }
            return "[DEBUG] Property not found";
        }
    }

    private string GetCurrentValue(SerializedProperty property)
    {
        switch (property.propertyType)
        {
            case SerializedPropertyType.Boolean:
                return property.boolValue.ToString();
            case SerializedPropertyType.Integer:
                return property.intValue.ToString();
            case SerializedPropertyType.Float:
                return property.floatValue.ToString();
            case SerializedPropertyType.String:
                return property.stringValue;
            case SerializedPropertyType.Enum:
                return $"{property.enumNames[property.enumValueIndex]}({property.enumValueIndex})";
            default:
                return "Unknown";
        }
    }
} 