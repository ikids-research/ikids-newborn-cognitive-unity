using UnityEngine;
using System.Collections;
using System.IO;
using SimpleJSON;
using System.Collections.Generic;
using UnityEngine.UI;

public class JSONDataLoader : MonoBehaviour {
	
    public static Configuration LoadDataFromJSON(string fullJSONFilePathAndName)
    {
        //Get the JSON contents from file
        string contents = getFileContents(fullJSONFilePathAndName);

        //Create the root task node for parsing
        JSONNode rootNode = JSONNode.Parse(contents);
        JSONClass rootClass = rootNode.AsObject;
        //Validate that the file is at least remotely formatted correctly by checking for the Task property (which contains everything)
        if(rootClass["Task"]== null)
        {
            Debug.LogError("Error: JSON does not include root Task object. See example JSON for help.");
            Application.Quit();
        }
        JSONClass taskClass = rootClass["Task"].AsObject;

        //From here on out, individual items in the Task class can be parsed

        //REQUIRED INPUTS

        //Construct the interface configuration from the Task JSONClass
        InterfaceConfiguration interfaceConfig = getInterfaceConfigurationFromJSON(taskClass); 

        //Construct the task procedure from the Task JSON Class
        TaskProcedure taskProc = getTaskProcedureFromJSON(taskClass);

        //Construct the default configuration given the required interface and task procedure inputs
        Configuration c = new Configuration(interfaceConfig, taskProc);

        //OPTIONAL INPUTS

        //Attempt to load GlobalPauseEnabled property if present
        if (taskClass["GlobalPauseEnabled"] == null)
            Debug.LogWarning("Warning: No GlobalPauseEnabled property set, defaulting to " + c.GlobalPauseEnabled + " for value.");
        else
            c.GlobalPauseEnabled = taskClass["GlobalPauseEnabled"].AsBool;

        return c;
    }

    //Helper function which just gets the text contents of a file and returns them as a string
    private static string getFileContents(string fullFilePathAndName)
    {
        FileStream fileReader = File.OpenRead(fullFilePathAndName);
        StreamReader reader = new StreamReader(fileReader);
        string contents = reader.ReadToEnd();
        reader.Close();
        fileReader.Close();

        return contents;
    }

    //Helper function that gets a keymap from a JSON array (assumed to be 2D array of strings)
    private static List<string[]> getKeyMapStringArraysFromKeyMapJSON(JSONArray keyMap)
    {
        JSONArray keysJSON = keyMap[0].AsArray;
        JSONArray commandsJSON = keyMap[1].AsArray;
        string[] keys = new string[keysJSON.Count];
        for (int i = 0; i < keysJSON.Count; i++)
            keys[i] = keysJSON[i];
        string[] commands = new string[commandsJSON.Count];
        for (int i = 0; i < commandsJSON.Count; i++)
            commands[i] = commandsJSON[i];
        List<string[]> output = new List<string[]>();
        output.Add(keys);
        output.Add(commands);
        return output;
    }

    //Helper function that generates an InterfaceConfiguration object from a JSONClass which contains Interfaces in the appropriate format
    private static InterfaceConfiguration getInterfaceConfigurationFromJSON(JSONClass taskClass)
    {
        InterfaceConfiguration interfaceConfig = new InterfaceConfiguration();

        if (taskClass["Interfaces"] == null)
        {
            Debug.LogError("Error: JSON does not include Interfaces object. See example JSON for help.");
            Application.Quit();
        }
        else
        {
            JSONArray interfaces = taskClass["Interfaces"].AsArray;
            for(int i = 0; i < interfaces.Count; i++) { 
                JSONNode iFace = interfaces[i];
                string interfaceType = iFace["InterfaceType"];
                JSONArray keyMap = iFace["KeyMap"].AsArray;
                List<string[]> map = getKeyMapStringArraysFromKeyMapJSON(keyMap);
                switch (iFace["InterfaceType"])
                {
                    case "Keyboard":
                        interfaceConfig.setKeyboardMap(map[0], map[1]);
                        break;
                    case "XBoxController":
                        interfaceConfig.setXBoxControllerMap(map[0], map[1]);
                        break;
                    case "TCP":
                        interfaceConfig.setTCPMap(map[0], map[1]);
                        break;
                }
            }
        }

        if (taskClass["InterfaceMaster"] == null)
        {
            Debug.LogError("Error: JSON does not include InterfaceMaster object. See example JSON for help.");
            Application.Quit();
        }
        else
        {
            switch (taskClass["InterfaceMaster"])
            {
                case "Keyboard":
                    interfaceConfig.setMaster(InterfaceConfiguration.InterfaceType.Keyboard);
                    break;
                case "XBoxController":
                    interfaceConfig.setMaster(InterfaceConfiguration.InterfaceType.XBoxController);
                    break;
                case "TCP":
                    interfaceConfig.setMaster(InterfaceConfiguration.InterfaceType.TCP);
                    break;
                default:
                    Debug.LogError("Error: InterfaceMaster value is not recognized. Please select either Keyboard, XBoxController, or TCP.");
                    Application.Quit();
                    break;
            }
        }

        if (!interfaceConfig.isValidInterface())
        {
            Debug.LogError("Error: Interface is not valid. This is likely because none of the specified interfaces in the JSON properly loaded or no interface was specified. See example JSON for help.");
            Application.Quit();
        }

        return interfaceConfig;
    }

    //Helper function that generates a TaskProcedure object from a JSONClass that contains TaskProcedures in the proper format
    private static TaskProcedure getTaskProcedureFromJSON(JSONClass taskClass)
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
            JSONClass task = taskArray[i].AsObject;
            Task t = new Task();
            if (taskArray[i]["ConditionalEvent"]["EndConditions"] == null)
            {
                Debug.LogError("Error: All Conditional Events MUST have an end condition. Please add either a Timeout or InputEvent to the EndConditions property.");
                Application.Quit();
            }
            else
            {
                //Load Conditions
                JSONArray conditionArray = task["ConditionalEvent"]["EndConditions"].AsArray;
                for (int j = 0; j < conditionArray.Count; j++)
                {
                    switch (conditionArray[j]["ConditionType"])
                    {
                        case "Timeout":
                            if (conditionArray[j]["Duration"] == null)
                                Debug.LogWarning("Warning: There was a problem loading the timeout condition in event " + i + " condition " + j + ". Skipping...");
                            else
                                t.addCondition(new TimeoutCondition(conditionArray[j]["Duration"].AsInt));
                            break;
                        case "InputCommand":
                            if (conditionArray[j]["Duration"] == null || conditionArray[j]["CommandName"] == null)
                                Debug.LogWarning("Warning: There was a problem loading the command condition in event " + i + " condition " + j + ". Skipping...");
                            else
                                t.addCondition(new CommandCondition(conditionArray[j]["CommandName"], conditionArray[j]["Duration"].AsInt));
                            break;
                    }
                }
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
                                Debug.LogWarning("Warning: There was a problem loading the DisplayImage state in event " + i + " condition " + j + ". Skipping...");
                            else
                                t.addStimuli(new VisualStimuli(stateArray[j]["File"], new Vector2(stateArray[j]["X"].AsFloat, stateArray[j]["Y"].AsFloat), new Vector2(stateArray[j]["Width"].AsFloat, stateArray[j]["Height"].AsFloat)));
                            break;
                        case "PlaySound":
                            if (stateArray[j]["File"] == null || stateArray[j]["Loop"] == null)
                                Debug.LogWarning("Warning: There was a problem loading the PlaySound state in event " + i + " condition " + j + ". Skipping...");
                            else
                                t.addStimuli(new AudioStimuli(stateArray[j]["File"], stateArray[j]["Loop"].AsBool));
                            break;
                    }
                }
            }

            taskProcedure.addTask(t);
        }

        return taskProcedure;
    }

    //Object for storing the configuration of the task (requires properly configured interfaces and task procedure to run)
    public class Configuration
    {
        private bool _globalPauseEnabled;
        private InterfaceConfiguration _interfaces;
        private TaskProcedure _taskProcedure;

        public Configuration(InterfaceConfiguration interfaces, TaskProcedure taskProcedure)
        {
            GlobalPauseEnabled = false;
            _interfaces = interfaces;
            _taskProcedure = taskProcedure;
        }

        public bool GlobalPauseEnabled
        {
            get
            {
                return _globalPauseEnabled;
            }

            set
            {
                _globalPauseEnabled = value;
            }
        }

        public InterfaceConfiguration Interfaces
        {
            get
            {
                return _interfaces;
            }
        }

        public TaskProcedure TaskProcedure
        {
            get
            {
                return _taskProcedure;
            }
        }
    }

    //Object for storing the interface configuration which is compatible with any combination of Keyboard, XBoxController, and TCP stream (but only one of each at the moment)
    public class InterfaceConfiguration
    {
        public enum InterfaceType
        {
            Keyboard = 0, XBoxController = 1, TCP = 2
        };

        private bool _keyboardInterfacePresent;
        private bool _xBoxControllerInterfacePresent;
        private bool _tcpInterfacePresent;

        private string[] _keyboardKeys;
        private string[] _keyboardCommands;

        private string[] _xBoxControllerKeys;
        private string[] _xBoxControllerCommands;

        private string[] _tcpKeys;
        private string[] _tcpCommands;

        private InterfaceType _masterInterface;

        public InterfaceConfiguration()
        {
            _keyboardInterfacePresent = false;
            _xBoxControllerInterfacePresent = false;
            _tcpInterfacePresent = false;
            _masterInterface = InterfaceType.Keyboard;
        }

        public void setKeyboardMap(string[] keys, string[] commands)
        {
            if(keys.Length != commands.Length)
            {
                Debug.LogError("Error: KeyMap for Keyboard Interface has different length for keys and commands.");
                Application.Quit();
            }

            _keyboardInterfacePresent = true;
            _keyboardKeys = keys;
            _keyboardCommands = commands;
        }

        public void setXBoxControllerMap(string[] keys, string[] commands)
        {
            if (keys.Length != commands.Length)
            {
                Debug.LogError("Error: KeyMap for Keyboard Interface has different length for keys and commands.");
                Application.Quit();
            }

            _xBoxControllerInterfacePresent = true;
            _xBoxControllerKeys = keys;
            _xBoxControllerCommands = commands;
        }

        public void setTCPMap(string[] keys, string[] commands)
        {
            if (keys.Length != commands.Length)
            {
                Debug.LogError("Error: KeyMap for Keyboard Interface has different length for keys and commands.");
                Application.Quit();
            }

            _tcpInterfacePresent = true;
            _tcpKeys = keys;
            _tcpCommands = commands;
        }

        public void setMaster(InterfaceType type)
        {
            switch (type)
            {
                case InterfaceType.Keyboard:
                    if (KeyboardInterfacePresent)
                        _masterInterface = InterfaceType.Keyboard;
                    else
                    {
                        Debug.LogError("Error: Attempt to set Keyboard interface as master but no Keyboard KeyMap is present.");
                        Application.Quit();
                    }
                    break;
                case InterfaceType.XBoxController:
                    if (XBoxControllerInterfacePresent)
                        _masterInterface = InterfaceType.XBoxController;
                    else
                    {
                        Debug.LogError("Error: Attempt to set XBoxController interface as master but no XBoxController KeyMap is present.");
                        Application.Quit();
                    }
                    break;
                case InterfaceType.TCP:
                    if (TcpInterfacePresent)
                        _masterInterface = InterfaceType.TCP;
                    else
                    {
                        Debug.LogError("Error: Attempt to set TCP interface as master but no TCP KeyMap is present.");
                        Application.Quit();
                    }
                    break;
            }
        }

        public bool isValidInterface()
        {
            return KeyboardInterfacePresent || XBoxControllerInterfacePresent || TcpInterfacePresent;
        }

        public bool KeyboardInterfacePresent
        {
            get
            {
                return _keyboardInterfacePresent;
            }
        }

        public bool XBoxControllerInterfacePresent
        {
            get
            {
                return _xBoxControllerInterfacePresent;
            }
        }

        public bool TcpInterfacePresent
        {
            get
            {
                return _tcpInterfacePresent;
            }
        }

        public string[] KeyboardKeys
        {
            get
            {
                return _keyboardKeys;
            }
        }

        public string[] KeyboardCommands
        {
            get
            {
                return _keyboardCommands;
            }
        }

        public string[] XBoxControllerKeys
        {
            get
            {
                return _xBoxControllerKeys;
            }
        }

        public string[] XBoxControllerCommands
        {
            get
            {
                return _xBoxControllerCommands;
            }
        }

        public string[] TcpKeys
        {
            get
            {
                return _tcpKeys;
            }
        }

        public string[] TcpCommands
        {
            get
            {
                return _tcpCommands;
            }
        }

        public InterfaceType MasterInterface
        {
            get
            {
                return _masterInterface;
            }
        }
    }

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
            foreach(Task t in _tasks)
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
        
        public bool nextTask()
        {
            if(_index < _tasks.Count) _tasks[_index].setTaskState(false);
            if (_index + 1 < _tasks.Count)
            {
                _tasks[_index + 1].setTaskState(true);
                _tasks[_index + 1].startConditionMonitoring();
            }
            _index = _index + 1;
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

        public bool isTaskComplete()
        {
            bool complete = false;
            foreach (ICondition c in _endConditions)
                complete |= c.isConditionMet();
            return complete;
        }

        public void setTaskState(bool active)
        {
            foreach(IStimuli s in _stateStimuli)
            {
                if (active)
                    s.showStimuli();
                else
                    s.removeStimuli();
            }
        }

        public void startConditionMonitoring()
        {
            foreach(ICondition c in _endConditions)
                c.startConditionMonitoring();
        }

        public void setConditionStatus(string[] commands)
        {
            foreach (ICondition c in _endConditions)
                c.setConditionStatus(commands);
        }
    }

    public interface ICondition
    {
        void startConditionMonitoring();
        void setConditionStatus(string[] commands);
        bool isConditionMet();
    }

    public class TimeoutCondition : ICondition
    {
        private float _startTime;
        private float _timeout;

        public TimeoutCondition(int timeoutInMilliseconds)
        {
            _timeout = (float)timeoutInMilliseconds;
        }

        public void startConditionMonitoring()
        {
            _startTime = Time.time;
        }

        public void setConditionStatus(string[] commands)
        {
            //Do nothing because timeouts don't care about commands
        }

        public bool isConditionMet()
        {
            return (Time.time - _startTime) > _timeout;
        }
    }

    public class CommandCondition : ICondition
    {
        private float _startTime;
        private bool _isMonitoring;
        private float _duration;
        private string _watchCommand;
        public CommandCondition(string command, int duration)
        {
            _watchCommand = command;
            _duration = duration;
            _startTime = float.MaxValue;
        }

        public void startConditionMonitoring()
        {
            _isMonitoring = true;
        }

        public void setConditionStatus(string[] commands)
        {
            bool containsCommand = false;
            for (int i = 0; i < commands.Length; i++)
                if (commands[i] == _watchCommand)
                {
                    containsCommand = true;
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

        public bool isConditionMet()
        {
            return _isMonitoring && ((Time.time - _startTime) > _duration);
        }
    }

    public interface IStimuli
    {
        void showStimuli();
        void removeStimuli();
        bool isStimuliLoaded();
    }

    public class VisualStimuli : IStimuli
    {
        private Texture2D _stimuli;
        private GameObject _renderObject;
        private WWW _loaderObject;
        private SpriteRenderer _renderer;
        public VisualStimuli(string fullFileNameAndPathToImage, Vector2 location, Vector2 size)
        {
            string path = "file:///" + fullFileNameAndPathToImage.Replace('\\', '/');
            _loaderObject = new WWW(path);
            _renderObject = new GameObject();
            _renderObject.transform.position = Camera.main.ScreenToWorldPoint(location);
            _renderObject.transform.position = new Vector3(_renderObject.transform.position.x, _renderObject.transform.position.y, 0f);
            _renderer = _renderObject.AddComponent<SpriteRenderer>();
            while (!_loaderObject.isDone)
                Debug.Log("Loading visual asset " + path + " - " + _loaderObject.progress + "%");
            Debug.Log("Done loading visual asset" + path + ".");
            _stimuli = _loaderObject.texture;
            _renderer.sprite = Sprite.Create(_stimuli, new Rect(0f, 0f, _loaderObject.texture.width, _loaderObject.texture.height), Vector2.zero);
            _renderObject.transform.localScale = new Vector3(size.x / _loaderObject.texture.width, size.y / _loaderObject.texture.height, 0f);
        }

        public void showStimuli()
        {
            _renderObject.SetActive(true);
        }

        public void removeStimuli()
        {
            _renderObject.SetActive(false);
        }

        public bool isStimuliLoaded()
        {
            return _loaderObject.isDone;
        }
    }

    public class AudioStimuli : IStimuli
    {
        private AudioSource _audio;
        private GameObject _audioObject;
        private WWW _loaderObject;

        public AudioStimuli(string fullFileNameAndPathToAudio, bool loop)
        {
            _audioObject = new GameObject();
            _audio = _audioObject.AddComponent<AudioSource>();
            _audio.loop = loop;
            string path = "file:///" + fullFileNameAndPathToAudio.Replace('\\', '/');
            _loaderObject = new WWW(path);
            while (!_loaderObject.isDone)
                Debug.Log("Loading audio asset " + path + " - " + _loaderObject.progress + "%");
            Debug.Log("Done loading audio asset" + path + ".");
            _audio.clip = _loaderObject.audioClip;
        }

        public void showStimuli()
        {
            _audio.Play();
        }

        public void removeStimuli()
        {
            _audio.Stop();
        }

        public bool isStimuliLoaded()
        {
            return _loaderObject.isDone;
        }
    }
}
