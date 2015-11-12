using UnityEngine;
using System.Collections;
using SimpleJSON;
using System;

namespace JSONDataLoader
{
    public interface ICondition
    {
        void startConditionMonitoring();
        void setConditionStatus(string[] commands);
        int? isConditionMet();
    }

    public class Condition
    {
        public static ICondition GetConditionFromJSON(JSONClass conditionJSON, int taskIndex, int conditionIndex)
        {
            //Load Conditions

            switch (conditionJSON["ConditionType"])
            {
                case "Timeout":
                    if (conditionJSON["Duration"] == null)
                        Debug.LogWarning("Warning: There was a problem loading the timeout condition in event " + taskIndex + " condition " + conditionIndex + ". Skipping...");
                    else
                    {
                        int? transitionToIndex = -1;
                        if (conditionJSON["TransitionToIndex"] != null)
                            transitionToIndex = conditionJSON["TransitionToIndex"].AsInt;
                        if (conditionJSON["TransitionToRelativeIndex"] != null)
                        {
                            transitionToIndex = conditionJSON["TransitionToRelativeIndex"].AsInt;
                            transitionToIndex += taskIndex;
                        }

                        if (conditionJSON["NotificationText"] != null)
                        {
                            string notificationText = conditionJSON["NotificationText"];
                            return new TimeoutCondition(transitionToIndex, conditionJSON["Duration"].AsInt, notificationText);
                        }
                        return new TimeoutCondition(transitionToIndex, conditionJSON["Duration"].AsInt);
                    }
                    break;
                case "InputCommand":
                    if (conditionJSON["Duration"] == null || conditionJSON["CommandNames"] == null)
                        Debug.LogWarning("Warning: There was a problem loading the command condition in event " + taskIndex + " condition " + conditionIndex + ". Skipping...");
                    else
                    {
                        int? transitionToIndex = -1;
                        if (conditionJSON["TransitionToIndex"] != null)
                            transitionToIndex = conditionJSON["TransitionToIndex"].AsInt;
                        if (conditionJSON["TransitionToRelativeIndex"] != null)
                        {
                            transitionToIndex = conditionJSON["TransitionToRelativeIndex"].AsInt;
                            transitionToIndex += taskIndex;
                        }
                        JSONArray commandArray = conditionJSON["CommandNames"].AsArray;
                        string[] commands = new string[commandArray.Count];
                        for (int k = 0; k < commandArray.Count; k++)
                            commands[k] = commandArray[k];

                        if (conditionJSON["NotificationText"] != null)
                        {
                            string notificationText = conditionJSON["NotificationText"];
                            return new CommandCondition(transitionToIndex, commands, conditionJSON["Duration"].AsInt, notificationText);
                        }

                        return new CommandCondition(transitionToIndex, commands, conditionJSON["Duration"].AsInt);
                    }
                    break;
                case "CumulativeInputCommand":
                    if (conditionJSON["Duration"] == null || conditionJSON["CommandNames"] == null)
                        Debug.LogWarning("Warning: There was a problem loading the cumulative command condition in event " + taskIndex + " condition " + conditionIndex + ". Skipping...");
                    else
                    {
                        int? transitionToIndex = -1;
                        if (conditionJSON["TransitionToIndex"] != null)
                            transitionToIndex = conditionJSON["TransitionToIndex"].AsInt;
                        if (conditionJSON["TransitionToRelativeIndex"] != null)
                        {
                            transitionToIndex = conditionJSON["TransitionToRelativeIndex"].AsInt;
                            transitionToIndex += taskIndex;
                        }
                        JSONArray commandArray = conditionJSON["CommandNames"].AsArray;
                        string[] commands = new string[commandArray.Count];
                        for (int k = 0; k < commandArray.Count; k++)
                            commands[k] = commandArray[k];
                        string storageVariableName = "";
                        if (conditionJSON["StoreValueInVariableName"] != null)
                            storageVariableName = conditionJSON["StoreValueInVariableName"];

                        if (conditionJSON["NotificationText"] != null)
                        {
                            string notificationText = conditionJSON["NotificationText"];
                            return new CumulativeCommandCondition(transitionToIndex, commands, conditionJSON["Duration"].AsInt, storageVariableName, notificationText);
                        }

                        return new CumulativeCommandCondition(transitionToIndex, commands, conditionJSON["Duration"].AsInt, storageVariableName);
                    }
                    break;
                case "ChainCondition":
                    if (conditionJSON["Conditions"] == null)
                        Debug.LogWarning("Warning: There was a problem loading the chain condition in event " + taskIndex + " condition " + conditionIndex + ". Skipping...");
                    else
                    {
                        int? transitionToIndex = -1;
                        if (conditionJSON["TransitionToIndex"] != null)
                            transitionToIndex = conditionJSON["TransitionToIndex"].AsInt;
                        if (conditionJSON["TransitionToRelativeIndex"] != null)
                        {
                            transitionToIndex = conditionJSON["TransitionToRelativeIndex"].AsInt;
                            transitionToIndex += taskIndex;
                        }
                        JSONArray subconditionArray = conditionJSON["Conditions"].AsArray;
                        ICondition[] subconditions = new ICondition[subconditionArray.Count];
                        for (int k = 0; k < subconditionArray.Count; k++)
                            subconditions[k] = GetConditionFromJSON(subconditionArray[k].AsObject, taskIndex, conditionIndex);

                        if (conditionJSON["NotificationText"] != null)
                        {
                            string notificationText = conditionJSON["NotificationText"];
                            return new ChainCondition(transitionToIndex, subconditions, notificationText);
                        }

                        return new ChainCondition(transitionToIndex, subconditions);
                    }
                    break;
                case "ExpressionCondition":
                    if (conditionJSON["Expression"] == null)
                        Debug.LogWarning("Warning: There was a problem loading the expression condition in event " + taskIndex + " condition " + conditionIndex + ". Skipping...");
                    else
                    {
                        int? transitionToIndex = -1;
                        if (conditionJSON["TransitionToIndex"] != null)
                            transitionToIndex = conditionJSON["TransitionToIndex"].AsInt;
                        if (conditionJSON["TransitionToRelativeIndex"] != null)
                        {
                            transitionToIndex = conditionJSON["TransitionToRelativeIndex"].AsInt;
                            transitionToIndex += taskIndex;
                        }
                        string expression = conditionJSON["Expression"];

                        if (conditionJSON["NotificationText"] != null)
                        {
                            string notificationText = conditionJSON["NotificationText"];
                            return new ExpressionCondition(transitionToIndex, expression, notificationText);
                        }

                        return new ExpressionCondition(transitionToIndex, expression);
                    }
                    break;
            }

            return null;
        }
    }

    public class TimeoutCondition : ICondition
    {
        private float _startTime;
        private float _timeout;
        private int? _transitionTarget;
        private bool _hasNotification;
        private string _notificationText;

        public TimeoutCondition(int? transitionTarget, int timeoutInMilliseconds)
        {
            _timeout = (float)timeoutInMilliseconds;
            _transitionTarget = transitionTarget;
            _hasNotification = false;
        }

        public TimeoutCondition(int? transitionTarget, int timeoutInMilliseconds, string notificationText) : this(transitionTarget, timeoutInMilliseconds)
        {
            _hasNotification = true;
            _notificationText = notificationText;
        }

        public void startConditionMonitoring()
        {
            _startTime = Time.time;
        }

        public void setConditionStatus(string[] commands)
        {
            //Do nothing because timeouts don't care about commands
        }

        public int? isConditionMet()
        {
            if ((Time.time - _startTime) > _timeout)
            {
                DataLogger.Log.LogState("Timeout condition met with Timeout=" + _timeout);
                if (_hasNotification) NotificationManager.pushNotification(_notificationText, NotificationManager.DefaultDuration);
                return _transitionTarget;
            }
            else return null;
        }
    }

    public class CommandCondition : ICondition
    {
        private float _startTime;
        private bool _isMonitoring;
        private float _duration;
        private string[] _watchCommands;
        private int? _transitionTarget;
        private bool _hasNotification;
        private string _notificationText;

        public CommandCondition(int? transitionTarget, string[] commands, int duration)
        {
            _watchCommands = commands;
            _duration = duration;
            _startTime = float.MaxValue;
            _transitionTarget = transitionTarget;
            _hasNotification = false;
        }

        public CommandCondition(int? transitionTarget, string[] commands, int duration, string notificationText) : this(transitionTarget, commands, duration)
        {
            _hasNotification = true;
            _notificationText = notificationText;
        }

        public void startConditionMonitoring()
        {
            _isMonitoring = true;
            _startTime = float.MaxValue;
        }

        public void setConditionStatus(string[] commands)
        {
            bool containsCommand = false;
            for (int i = 0; i < commands.Length; i++)
                for (int j = 0; j < _watchCommands.Length; j++)
                    if (commands[i] == _watchCommands[j])
                    {
                        containsCommand = true;
                        i = commands.Length; //Break outer loop for efficiency
                        break;
                    }
            if (_isMonitoring && containsCommand && _startTime == float.MaxValue)
            {
                _startTime = Time.time;
            }
            else if (_isMonitoring && !containsCommand)
            {
                _startTime = float.MaxValue;
            }
        }

        public int? isConditionMet()
        {
            if (_isMonitoring && ((Time.time - _startTime) >= _duration))
            {
                DataLogger.Log.LogState("Command condition met with Duration=" + _duration);
                if (_hasNotification) NotificationManager.pushNotification(_notificationText, NotificationManager.DefaultDuration);
                return _transitionTarget;
            }
            else return null;
        }
    }

    public class CumulativeCommandCondition : ICondition
    {
        private float _previousTime;
        private float _cumulativeTime;
        private bool _isMonitoring;
        private float _duration;
        private string[] _watchCommands;
        private int? _transitionTarget;
        private string _storageVariableName;
        private bool _hasNotification;
        private string _notificationText;

        public CumulativeCommandCondition(int? transitionTarget, string[] commands, int duration, string storageVariableName)
        {
            _watchCommands = commands;
            _duration = duration;
            _storageVariableName = storageVariableName.Trim();
            _previousTime = float.MaxValue;
            _transitionTarget = transitionTarget;
            CumulativeTime = 0f;
            _hasNotification = false;
        }

        public CumulativeCommandCondition(int? transitionTarget, string[] commands, int duration, string storageVariableName, string notificationText) : this(transitionTarget, commands, duration, storageVariableName)
        {
            _hasNotification = true;
            _notificationText = notificationText;
        }

        float CumulativeTime
        {
            get { return _cumulativeTime; }
            set
            {
                if (_storageVariableName != "")
                    VariableEngine.SetVariable(_storageVariableName, value.ToString());
                _cumulativeTime = value;
            }
        }

        public void startConditionMonitoring()
        {
            _isMonitoring = true;
            CumulativeTime = 0f;
            _previousTime = float.MaxValue;
        }

        public void setConditionStatus(string[] commands)
        {
            bool containsCommand = false;
            for (int i = 0; i < commands.Length; i++)
                for (int j = 0; j < _watchCommands.Length; j++)
                    if (commands[i] == _watchCommands[j])
                    {
                        containsCommand = true;
                        i = commands.Length; //Break outer loop for efficiency
                        break;
                    }
            if (_isMonitoring && containsCommand && _previousTime == float.MaxValue)
            {
                _previousTime = Time.time;
            }
            else if (_isMonitoring && containsCommand)
            {
                float currentTime = Time.time;
                CumulativeTime += currentTime - _previousTime;
                _previousTime = currentTime;
            }
            else if (_isMonitoring && !containsCommand && _previousTime != float.MaxValue)
            {
                CumulativeTime += Time.time - _previousTime;
                _previousTime = float.MaxValue;
            }
        }

        public int? isConditionMet()
        {
            Debug.Log(CumulativeTime);
            if (_isMonitoring && CumulativeTime >= _duration)
            {
                DataLogger.Log.LogState("Cumulative command condition met with Duration=" + _duration);
                if (_hasNotification) NotificationManager.pushNotification(_notificationText, NotificationManager.DefaultDuration);
                return _transitionTarget;
            }
            else return null;
        }
    }

    public class ExpressionCondition : ICondition
    {
        private string _expressionString;
        private bool _isMonitoring;
        private int? _transitionTarget;
        private bool _hasNotification;
        private string _notificationText;
        private string _latestExpressionSubstitutionString;

        public ExpressionCondition(int? transitionTarget, string expression)
        {
            _expressionString = expression;
            _transitionTarget = transitionTarget;
            _isMonitoring = false;
            _hasNotification = false;
        }

        public ExpressionCondition(int? transitionTarget, string expression, string notificationText) : this(transitionTarget, expression)
        {
            _hasNotification = true;
            _notificationText = notificationText;
        }

        private bool? evaluateExpressionString()
        {
            string newExpressionString = VariableEngine.substituteVariablesInString(_expressionString);
            NCalc.Expression ex = new NCalc.Expression(newExpressionString);
            _latestExpressionSubstitutionString = newExpressionString;
            Debug.Log(newExpressionString);
            bool result = false;
            try
            {
                result = (bool)ex.Evaluate();
            }
            catch (Exception) { return null; }

            return result;
        }

        public void startConditionMonitoring()
        {
            _isMonitoring = true;
        }
        public void setConditionStatus(string[] commands)
        {
            //Do nothing because this condition only cares about the expression value
        }

        public int? isConditionMet()
        {
            if (_isMonitoring)
            {
                bool? result = evaluateExpressionString();
                if (result.HasValue && result.Value)
                {
                    DataLogger.Log.LogState("Expression condition met with Expression=" + _expressionString + " and Latest Expression Substitution=" + _latestExpressionSubstitutionString);
                    if (_hasNotification) NotificationManager.pushNotification(_notificationText, NotificationManager.DefaultDuration);
                    return _transitionTarget;
                }
                else return null;
            }
            return null;
        }
    }

    public class ChainCondition : ICondition
    {
        private int _currentIndex;
        private ICondition[] _subconditions;
        private int? _transitionTarget;
        private bool _hasNotification;
        private string _notificationText;

        public ChainCondition(int? transitionTarget, ICondition[] subconditions)
        {
            _currentIndex = 0;
            _subconditions = subconditions;
            _transitionTarget = transitionTarget;
            _hasNotification = false;
        }

        public ChainCondition(int? transitionTarget, ICondition[] subconditions, string notificationText) : this(transitionTarget, subconditions)
        {
            _hasNotification = true;
            _notificationText = notificationText;
        }

        public void startConditionMonitoring()
        {
            _subconditions[_currentIndex].startConditionMonitoring();
        }
        public void setConditionStatus(string[] commands)
        {
            _subconditions[_currentIndex].setConditionStatus(commands);
        }
        public int? isConditionMet() {
            if (_subconditions[_currentIndex].isConditionMet() != null)
            {
                int nextIndex = _currentIndex + 1;
                if (nextIndex >= _subconditions.Length)
                {
                    DataLogger.Log.LogState("Chain condition met after " + _subconditions.Length + " subconditions.");
                    if (_hasNotification) NotificationManager.pushNotification(_notificationText, NotificationManager.DefaultDuration);
                    return _transitionTarget;
                }
                else
                {
                    _subconditions[nextIndex].startConditionMonitoring();
                    _currentIndex = nextIndex;
                }
            }
            return null;
        }
    }
}