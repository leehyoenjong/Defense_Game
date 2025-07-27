using System;
using UnityEngine;

/// <summary>
/// 조건부 필드를 표시하기 위한 PropertyAttribute
/// 지정된 필드의 값이 특정 조건과 일치할 때만 인스펙터에서 표시됩니다.
/// </summary>
public class ConditionalFieldAttribute : PropertyAttribute
{
    public string _conditionalSourceField;
    public object[] _conditionalValues;
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
        _showWhenTrue = showWhenTrue;
    }

    /// <summary>
    /// 조건부 필드 생성자 (다중 값)
    /// </summary>
    /// <param name="conditionalSourceField">조건을 체크할 필드명</param>
    /// <param name="showWhenTrue">조건이 참일 때 표시할지 여부</param>
    /// <param name="conditionalValues">조건 값들 (여러 개 중 하나라도 일치하면 조건 만족)</param>
    public ConditionalFieldAttribute(string conditionalSourceField, bool showWhenTrue, params object[] conditionalValues)
    {
        _conditionalSourceField = conditionalSourceField;
        _conditionalValues = conditionalValues;
        _showWhenTrue = showWhenTrue;
    }
}