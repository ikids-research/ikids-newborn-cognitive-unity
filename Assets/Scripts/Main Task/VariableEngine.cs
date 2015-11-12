using UnityEngine;
using System.Collections.Generic;

public class VariableEngine : MonoBehaviour {

    private static Dictionary<string, string> _variableDict = new Dictionary<string, string>();

    public static void SetVariable(string key, string value)
    {
        if (_variableDict.ContainsKey(key))
            _variableDict[key] = value;
        else
            _variableDict.Add(key, value);
    }

    public static string GetVariable(string key)
    {
        if (_variableDict.ContainsKey(key))
            return _variableDict[key];
        else return "";
    }

    public static string substituteVariablesInString(string expression)
    {
        string newExpression = expression;
        foreach (string key in _variableDict.Keys)
            newExpression = newExpression.Replace(key, _variableDict[key].ToString());
        return newExpression;
    }
}
