using UnityEngine;
using System.Collections;
using SimpleJSON;

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
                        return new CommandCondition(transitionToIndex, commands, conditionJSON["Duration"].AsInt);
                    }
                    break;
                case "CumulativeInputCommand":
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
                        return new CumulativeCommandCondition(transitionToIndex, commands, conditionJSON["Duration"].AsInt);
                    }
                    break;
                case "ChainCondition":
                    if (conditionJSON["Conditions"] == null)
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
                        JSONArray subconditionArray = conditionJSON["Conditions"].AsArray;
                        ICondition[] subconditions = new ICondition[subconditionArray.Count];
                        for (int k = 0; k < subconditionArray.Count; k++)
                            subconditions[k] = GetConditionFromJSON(subconditionArray[k].AsObject, taskIndex, conditionIndex);
                        return new ChainCondition(transitionToIndex, subconditions);
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

        public TimeoutCondition(int? transitionTarget, int timeoutInMilliseconds)
        {
            _timeout = (float)timeoutInMilliseconds;
            _transitionTarget = transitionTarget;
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
                return _transitionTarget;
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

        public CommandCondition(int? transitionTarget, string[] commands, int duration)
        {
            _watchCommands = commands;
            _duration = duration;
            _startTime = float.MaxValue;
            _transitionTarget = transitionTarget;
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
                return _transitionTarget;
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

        public CumulativeCommandCondition(int? transitionTarget, string[] commands, int duration)
        {
            _watchCommands = commands;
            _duration = duration;
            _previousTime = float.MaxValue;
            _transitionTarget = transitionTarget;
            _cumulativeTime = 0f;
        }

        public void startConditionMonitoring()
        {
            _isMonitoring = true;
            _cumulativeTime = 0f;
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
                _cumulativeTime += currentTime - _previousTime;
                _previousTime = currentTime;
            }
            else if (_isMonitoring && !containsCommand && _previousTime != float.MaxValue)
            {
                _cumulativeTime += Time.time - _previousTime;
                _previousTime = float.MaxValue;
            }
        }

        public int? isConditionMet()
        {
            Debug.Log(_cumulativeTime);
            if (_isMonitoring && _cumulativeTime >= _duration)
                return _transitionTarget;
            else return null;
        }
    }

    public class ChainCondition : ICondition
    {
        private int _currentIndex;
        private ICondition[] _subconditions;
        private int? _transitionTarget;
        public ChainCondition(int? transitionTarget, ICondition[] subconditions)
        {
            _currentIndex = 0;
            _subconditions = subconditions;
            _transitionTarget = transitionTarget;
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
                    return _transitionTarget;
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