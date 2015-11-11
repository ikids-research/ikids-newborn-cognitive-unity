using UnityEngine;
using System.Collections.Generic;
using SimpleJSON;

namespace JSONDataLoader
{
    //Object for storing the Task Procedure
    public class TaskProcedure
    {
        private int _index;
        private List<Task> _tasks;

        public TaskProcedure()
        {
            _tasks = new List<Task>();
            _index = 0;
        }

        public List<Task> Tasks
        {
            get
            {
                return _tasks;
            }
        }

        public void startFromBeginning()
        {
            foreach (Task t in _tasks)
                t.setTaskState(false);
            _index = 0;
            _tasks[0].setTaskState(true);
            _tasks[0].startConditionMonitoring();
        }

        public void addTask(Task t)
        {
            _tasks.Add(t);
        }

        public Task getCurrentTask()
        {
            if (_index < _tasks.Count)
                return _tasks[_index];
            else return null;
        }

        public bool setTask(int taskNumber)
        {
            if (taskNumber == -1)
                taskNumber = _index + 1;
            _tasks[_index].setTaskState(false);
            if (taskNumber < _tasks.Count)
            {
                _tasks[taskNumber].startConditionMonitoring();
                _tasks[taskNumber].setTaskState(true);
            }
            _index = taskNumber;
            return _index < _tasks.Count;
        }

        public void setConditionStatus(string[] commands)
        {
            if (_index < _tasks.Count)
                _tasks[_index].setConditionStatus(commands);
        }

        public bool procedureComplete()
        {
            return _index >= _tasks.Count;
        }

        public int Index
        {
            get { return _index; }
            set { _index = value; }
        }

        //Helper function that generates a TaskProcedure object from a JSONClass that contains TaskProcedures in the proper format
        public static TaskProcedure getTaskProcedureFromJSON(JSONClass taskClass)
        {
            if (taskClass["TaskProcedure"] == null)
            {
                Debug.LogError("Error: JSON does not include TaskProcedure object. See example JSON for help.");
                Application.Quit();
            }

            TaskProcedure taskProcedure = new TaskProcedure();

            JSONArray taskArray = taskClass["TaskProcedure"].AsArray;
            for (int i = 0; i < taskArray.Count; i++)
            {
                if (taskArray[i]["ConditionalEvent"] != null)
                {
                    JSONClass task = taskArray[i].AsObject;
                    taskProcedure.addTask(CreateConditionalEventFromJSON(taskClass, task, i));
                }
            }

            return taskProcedure;
        }

        private static Task CreateConditionalEventFromJSON(JSONClass taskClass, JSONClass task, int taskIndex)
        {
            Task t = new Task();

            if (task["ConditionalEvent"]["EndConditions"] == null)
            {
                Debug.LogError("Error: All Conditional Events MUST have an end condition. Please add either a Timeout or InputEvent to the EndConditions property.");
                Application.Quit();
            }
            else
            {
                JSONArray conditionArray = task["ConditionalEvent"]["EndConditions"].AsArray;
                for (int j = 0; j < conditionArray.Count; j++)
                    t.addCondition(Condition.GetConditionFromJSON(conditionArray[j].AsObject, taskIndex, j));
            }

            if (task["ConditionalEvent"]["State"] == null)
            {
                Debug.LogError("Error: All Conditional Events MUST have at least one State item. Please add a DisplayImage or PlaySound object to the State array.");
                Application.Quit();
            }
            else
            {
                //Load States
                JSONArray stateArray = task["ConditionalEvent"]["State"].AsArray;
                for (int j = 0; j < stateArray.Count; j++)
                {
                    switch (stateArray[j]["StateType"])
                    {
                        case "DisplayImage":
                            if (stateArray[j]["File"] == null || stateArray[j]["X"] == null || stateArray[j]["Y"] == null || stateArray[j]["Width"] == null || stateArray[j]["Height"] == null)
                                Debug.LogWarning("Warning: There was a problem loading the DisplayImage state in event " + taskIndex + " condition " + j + ". Skipping...");
                            else
                                t.addStimuli(new VisualStimuli(stateArray[j]["File"], new Vector2(stateArray[j]["X"].AsFloat, stateArray[j]["Y"].AsFloat), new Vector2(stateArray[j]["Width"].AsFloat, stateArray[j]["Height"].AsFloat)));
                            break;
                        case "PlaySound":
                            if (stateArray[j]["File"] == null || stateArray[j]["Loop"] == null)
                                Debug.LogWarning("Warning: There was a problem loading the PlaySound state in event " + taskIndex + " condition " + j + ". Skipping...");
                            else
                                t.addStimuli(new AudioStimuli(stateArray[j]["File"], stateArray[j]["Loop"].AsBool));
                            break;
                        case "MultiImageAnimation":
                            if (stateArray[j]["Files"] == null || stateArray[j]["X"] == null || stateArray[j]["Y"] == null || stateArray[j]["Width"] == null || stateArray[j]["Height"] == null || stateArray[j]["TimePerImage"] == null || stateArray[j]["Loop"] == null)
                                Debug.LogWarning("Warning: There was a problem loading the MultiImageAnimation state in event " + taskIndex + " condition " + j + ". Skipping...");
                            else
                            {
                                JSONArray JSONFiles = stateArray[j]["Files"].AsArray;
                                string[] files = new string[JSONFiles.Count];
                                for (int k = 0; k < JSONFiles.Count; k++)
                                    files[k] = JSONFiles[k];
                                t.addStimuli(new MultiImageAnimationStimuli(files, new Vector2(stateArray[j]["X"].AsFloat, stateArray[j]["Y"].AsFloat), new Vector2(stateArray[j]["Width"].AsFloat, stateArray[j]["Height"].AsFloat), stateArray[j]["TimePerImage"].AsFloat, stateArray[j]["Loop"].AsBool));
                            }
                            break;
                        case "DisableGlobalPause":
                            t.addStimuli(new GlobalPauseDisabledStimuli(taskClass["GlobalPauseEnabled"].AsBool));
                            break;
                    }
                }
            }

            return t;
        }
    }

    public class Task
    {
        private List<ICondition> _endConditions;
        private List<IStimuli> _stateStimuli;

        public Task()
        {
            _endConditions = new List<ICondition>();
            _stateStimuli = new List<IStimuli>();
        }

        public void addCondition(ICondition c)
        {
            _endConditions.Add(c);
        }

        public void addStimuli(IStimuli s)
        {
            _stateStimuli.Add(s);
        }

        public int? isTaskComplete()
        {
            foreach (ICondition c in _endConditions)
            {
                int? conditionResult = c.isConditionMet();
                if (conditionResult.HasValue)
                {
                    Debug.Log(c.GetType().ToString());
                    return conditionResult.Value;
                }
            }
            return null;
        }

        public void setTaskState(bool active)
        {
            foreach (IStimuli s in _stateStimuli)
            {
                if (active)
                    s.showStimuli();
                else
                    s.removeStimuli();
            }
        }

        public void startConditionMonitoring()
        {
            foreach (ICondition c in _endConditions)
                c.startConditionMonitoring();
        }

        public void setConditionStatus(string[] commands)
        {
            foreach (ICondition c in _endConditions)
                c.setConditionStatus(commands);
        }
    }
}
