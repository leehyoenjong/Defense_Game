using System;
using UnityEngine;

/// <summary>
/// 조건부 필드를 표시하기 위한 PropertyAttribute
/// 지정된 필드의 값이 특정 조건과 일치할 때만 인스펙터에서 표시됩니다.
/// 단일/다중 필드 조건과 AND/OR 연산을 지원합니다.
/// </summary>
public class ConditionalFieldAttribute : PropertyAttribute
{
    /// <summary>
    /// 다중 조건에서 사용할 연산자
    /// </summary>
    public enum ConditionOperator
    {
        /// <summary>모든 조건이 참이어야 함</summary>
        And,
        /// <summary>조건 중 하나라도 참이면 됨</summary>
        Or
    }

    /// <summary>
    /// 개별 필드 조건 정보
    /// </summary>
    [System.Serializable]
    public class FieldCondition
    {
        public string _fieldName;
        public object[] _values;

        public FieldCondition(string fieldName, params object[] values)
        {
            _fieldName = fieldName;
            _values = values;
        }
    }

    // 기존 단일 필드 지원을 위한 속성들
    public string _conditionalSourceField;
    public object[] _conditionalValues;
    
    // 다중 필드 지원을 위한 속성들
    public FieldCondition[] _fieldConditions;
    public ConditionOperator _conditionOperator;
    
    public bool _showWhenTrue;

    /// <summary>
    /// 조건부 필드 생성자 (단일 값)
    /// </summary>
    /// <param name="conditionalSourceField">조건을 체크할 필드명</param>
    /// <param name="conditionalValue">조건 값</param>
    /// <param name="showWhenTrue">조건이 참일 때 표시할지 여부</param>
    public ConditionalFieldAttribute(string conditionalSourceField, object conditionalValue, bool showWhenTrue = true)
    {
        _conditionalSourceField = conditionalSourceField;
        _conditionalValues = new object[] { conditionalValue };
        _fieldConditions = null;
        _showWhenTrue = showWhenTrue;
        _conditionOperator = ConditionOperator.And;
    }

    /// <summary>
    /// 조건부 필드 생성자 (단일 필드, 다중 값)
    /// </summary>
    /// <param name="conditionalSourceField">조건을 체크할 필드명</param>
    /// <param name="showWhenTrue">조건이 참일 때 표시할지 여부</param>
    /// <param name="conditionalValues">조건 값들 (여러 개 중 하나라도 일치하면 조건 만족)</param>
    public ConditionalFieldAttribute(string conditionalSourceField, bool showWhenTrue, params object[] conditionalValues)
    {
        _conditionalSourceField = conditionalSourceField;
        _conditionalValues = conditionalValues;
        _fieldConditions = null;
        _showWhenTrue = showWhenTrue;
        _conditionOperator = ConditionOperator.And;
    }

    /// <summary>
    /// 조건부 필드 생성자 (다중 필드, AND 연산)
    /// 모든 필드가 지정된 조건을 만족해야 표시됩니다.
    /// </summary>
    /// <param name="showWhenTrue">조건이 참일 때 표시할지 여부</param>
    /// <param name="field1">첫 번째 필드명</param>
    /// <param name="value1">첫 번째 필드 조건값</param>
    /// <param name="field2">두 번째 필드명</param>
    /// <param name="value2">두 번째 필드 조건값</param>
    public ConditionalFieldAttribute(bool showWhenTrue, string field1, object value1, string field2, object value2)
    {
        _conditionalSourceField = null;
        _conditionalValues = null;
        _fieldConditions = new FieldCondition[]
        {
            new FieldCondition(field1, value1),
            new FieldCondition(field2, value2)
        };
        _showWhenTrue = showWhenTrue;
        _conditionOperator = ConditionOperator.And;
    }

    /// <summary>
    /// 조건부 필드 생성자 (다중 필드, 사용자 지정 연산자)
    /// </summary>
    /// <param name="conditionOperator">조건 간 연산자 (And/Or)</param>
    /// <param name="showWhenTrue">조건이 참일 때 표시할지 여부</param>
    /// <param name="field1">첫 번째 필드명</param>
    /// <param name="value1">첫 번째 필드 조건값</param>
    /// <param name="field2">두 번째 필드명</param>
    /// <param name="value2">두 번째 필드 조건값</param>
    public ConditionalFieldAttribute(ConditionOperator conditionOperator, bool showWhenTrue, string field1, object value1, string field2, object value2)
    {
        _conditionalSourceField = null;
        _conditionalValues = null;
        _fieldConditions = new FieldCondition[]
        {
            new FieldCondition(field1, value1),
            new FieldCondition(field2, value2)
        };
        _showWhenTrue = showWhenTrue;
        _conditionOperator = conditionOperator;
    }

    /// <summary>
    /// 조건부 필드 생성자 (3개 필드, AND 연산)
    /// </summary>
    /// <param name="showWhenTrue">조건이 참일 때 표시할지 여부</param>
    /// <param name="field1">첫 번째 필드명</param>
    /// <param name="value1">첫 번째 필드 조건값</param>
    /// <param name="field2">두 번째 필드명</param>
    /// <param name="value2">두 번째 필드 조건값</param>
    /// <param name="field3">세 번째 필드명</param>
    /// <param name="value3">세 번째 필드 조건값</param>
    public ConditionalFieldAttribute(bool showWhenTrue, string field1, object value1, string field2, object value2, string field3, object value3)
    {
        _conditionalSourceField = null;
        _conditionalValues = null;
        _fieldConditions = new FieldCondition[]
        {
            new FieldCondition(field1, value1),
            new FieldCondition(field2, value2),
            new FieldCondition(field3, value3)
        };
        _showWhenTrue = showWhenTrue;
        _conditionOperator = ConditionOperator.And;
    }

    /// <summary>
    /// 조건부 필드 생성자 (3개 필드, 사용자 지정 연산자)
    /// </summary>
    /// <param name="conditionOperator">조건 간 연산자 (And/Or)</param>
    /// <param name="showWhenTrue">조건이 참일 때 표시할지 여부</param>
    /// <param name="field1">첫 번째 필드명</param>
    /// <param name="value1">첫 번째 필드 조건값</param>
    /// <param name="field2">두 번째 필드명</param>
    /// <param name="value2">두 번째 필드 조건값</param>
    /// <param name="field3">세 번째 필드명</param>
    /// <param name="value3">세 번째 필드 조건값</param>
    public ConditionalFieldAttribute(ConditionOperator conditionOperator, bool showWhenTrue, string field1, object value1, string field2, object value2, string field3, object value3)
    {
        _conditionalSourceField = null;
        _conditionalValues = null;
        _fieldConditions = new FieldCondition[]
        {
            new FieldCondition(field1, value1),
            new FieldCondition(field2, value2),
            new FieldCondition(field3, value3)
        };
        _showWhenTrue = showWhenTrue;
        _conditionOperator = conditionOperator;
    }

    /// <summary>
    /// 다중 필드 조건 사용 여부 확인
    /// </summary>
    public bool IsMultiFieldCondition => _fieldConditions != null && _fieldConditions.Length > 0;
}