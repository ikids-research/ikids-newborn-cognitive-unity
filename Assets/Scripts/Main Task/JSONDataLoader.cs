using UnityEngine;
using System.Collections;
using System.IO;
using SimpleJSON;
using System.Collections.Generic;
using UnityEngine.UI;

public class JSONDataLoader : MonoBehaviour
{
    public static Configuration LoadTaskConfigurationDataFromJSON(string fullJSONFilePathAndName)
    {
        //Get the JSON contents from file
        string contents = getFileContents(fullJSONFilePathAndName);

        //Create the root task node for parsing
        JSONNode rootNode = JSONNode.Parse(contents);
        JSONClass rootClass = rootNode.AsObject;
        //Validate that the file is at least remotely formatted correctly by checking for the Task property (which contains everything)
        if (rootClass["Task"] == null)
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

        //Attempt to load BackgroundColor property if present
        if (taskClass["BackgroundColor"] == null)
            Debug.LogWarning("Warning: No BackgroundColor property set, defaulting to " + c.BackgroundColor + " for value.");
        else
            c.BackgroundColor = HexToColor(taskClass["BackgroundColor"]);

        return c;
    }

    // Note that Color32 and Color implictly convert to each other. You may pass a Color object to this method without first casting it.
    static string ColorToHex(Color32 color)
    {
        string hex = color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
        return hex;
    }

    static Color HexToColor(string hex)
    {
        byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        return new Color32(r, g, b, 255);
    }

    //Helper function which just gets the text contents of a file and returns them as a string
    public static string getFileContents(string fullFilePathAndName)
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
            for (int i = 0; i < interfaces.Count; i++)
            {
                JSONNode iFace = interfaces[i];
                string interfaceType = iFace["InterfaceType"];
                int port = iFace["Port"].AsInt;
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
                        if (port != 0) interfaceConfig.setTCPPort(port);
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
                            {
                                int? transitionToIndex = -1;
                                if (conditionArray[j]["TransitionToIndex"] != null)
                                    transitionToIndex = conditionArray[j]["TransitionToIndex"].AsInt;
                                t.addCondition(new TimeoutCondition(transitionToIndex, conditionArray[j]["Duration"].AsInt));
                            }
                            break;
                        case "InputCommand":
                            if (conditionArray[j]["Duration"] == null || conditionArray[j]["CommandNames"] == null)
                                Debug.LogWarning("Warning: There was a problem loading the command condition in event " + i + " condition " + j + ". Skipping...");
                            else
                            {
                                int? transitionToIndex = -1;
                                if (conditionArray[j]["TransitionToIndex"] != null)
                                    transitionToIndex = conditionArray[j]["TransitionToIndex"].AsInt;
                                JSONArray commandArray = conditionArray[j]["CommandNames"].AsArray;
                                string[] commands = new string[commandArray.Count];
                                for (int k = 0; k < commandArray.Count; k++)
                                    commands[k] = commandArray[k];
                                t.addCondition(new CommandCondition(transitionToIndex, commands, conditionArray[j]["Duration"].AsInt));
                            }
                            break;
                        case "CumulativeInputCommand":
                            if (conditionArray[j]["Duration"] == null || conditionArray[j]["CommandNames"] == null)
                                Debug.LogWarning("Warning: There was a problem loading the command condition in event " + i + " condition " + j + ". Skipping...");
                            else
                            {
                                int? transitionToIndex = -1;
                                if (conditionArray[j]["TransitionToIndex"] != null)
                                    transitionToIndex = conditionArray[j]["TransitionToIndex"].AsInt;
                                JSONArray commandArray = conditionArray[j]["CommandNames"].AsArray;
                                string[] commands = new string[commandArray.Count];
                                for (int k = 0; k < commandArray.Count; k++)
                                    commands[k] = commandArray[k];
                                t.addCondition(new CumulativeCommandCondition(transitionToIndex, commands, conditionArray[j]["Duration"].AsInt));
                            }
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
                        case "MultiImageAnimation":
                            if (stateArray[j]["Files"] == null || stateArray[j]["X"] == null || stateArray[j]["Y"] == null || stateArray[j]["Width"] == null || stateArray[j]["Height"] == null || stateArray[j]["TimePerImage"] == null || stateArray[j]["Loop"] == null)
                                Debug.LogWarning("Warning: There was a problem loading the MultiImageAnimation state in event " + i + " condition " + j + ". Skipping...");
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

            taskProcedure.addTask(t);
        }

        return taskProcedure;
    }

    //Object for storing the configuration of the task (requires properly configured interfaces and task procedure to run)
    public class Configuration
    {
        private bool _globalPauseEnabled;
        private Color _backgroundColor;
        private InterfaceConfiguration _interfaces;
        private TaskProcedure _taskProcedure;

        public Configuration(InterfaceConfiguration interfaces, TaskProcedure taskProcedure)
        {
            _globalPauseEnabled = false;
            _backgroundColor = Color.black;
            _interfaces = interfaces;
            _taskProcedure = taskProcedure;
        }

        public Color BackgroundColor
        {
            get { return _backgroundColor; }
            set { _backgroundColor = value; }
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
        private int _tcpPort;

        private InterfaceType _masterInterface;

        public InterfaceConfiguration()
        {
            _keyboardInterfacePresent = false;
            _xBoxControllerInterfacePresent = false;
            _tcpInterfacePresent = false;
            _masterInterface = InterfaceType.Keyboard;
            _tcpPort = 11235;
        }

        public void setKeyboardMap(string[] keys, string[] commands)
        {
            if (keys.Length != commands.Length)
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

        public void setTCPPort(int port)
        {
            _tcpPort = port;
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

        public int TcpPort
        {
            get
            {
                return _tcpPort;
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
            if (taskNumber < _tasks.Count) {
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
    }

    public class Task
    {
        private List<ICondition> _endConditions;
        private List<IStimuli> _stateStimuli;
        private int _transitionIndex;

        public Task()
        {
            _endConditions = new List<ICondition>();
            _stateStimuli = new List<IStimuli>();
            _transitionIndex = -1;
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

    public interface ICondition
    {
        void startConditionMonitoring();
        void setConditionStatus(string[] commands);
        int? isConditionMet();
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
                for(int j = 0; j < _watchCommands.Length;j++)
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
            _renderer.sprite = Sprite.Create(_stimuli, new Rect(0f, 0f, _loaderObject.texture.width, _loaderObject.texture.height), Vector2.zero, 1f);
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

    public class MultiImageAnimationStimuli : IStimuli
    {
        private WWW[] _loaderObjects;
        private GameObject[] _renderObjects;
        private Texture2D[] _stimuli;
        private SpriteRenderer[] _renderers;
        private GameObject _rootObject;
        private int _index;
        private float _currentIndexStartTime;
        private float _timePerImage;
        private bool _loop;
        public MultiImageAnimationStimuli(string[] files, Vector2 location, Vector2 size, float timePerImage, bool loop)
        {
            _rootObject = new GameObject();
            _loaderObjects = new WWW[files.Length];
            _renderObjects = new GameObject[files.Length];
            _stimuli = new Texture2D[files.Length];
            _renderers = new SpriteRenderer[files.Length];
            for (int i = 0; i < files.Length; i++)
            {
                string path = "file:///" + files[i].Replace('\\', '/');
                _loaderObjects[i] = new WWW(path);
                _renderObjects[i] = new GameObject();
                _renderObjects[i].transform.position = Camera.main.ScreenToWorldPoint(location);
                _renderObjects[i].transform.position = new Vector3(_renderObjects[i].transform.position.x, _renderObjects[i].transform.position.y, 0f);
                _renderers[i] = _renderObjects[i].AddComponent<SpriteRenderer>();
                while (!_loaderObjects[i].isDone)
                    Debug.Log("Loading visual asset " + path + " - " + _loaderObjects[i].progress + "%");
                Debug.Log("Done loading visual asset" + path + ".");
                _stimuli[i] = _loaderObjects[i].texture;
                _renderers[i].sprite = Sprite.Create(_stimuli[i], new Rect(0f, 0f, _loaderObjects[i].texture.width, _loaderObjects[i].texture.height), Vector2.zero, 1f);
                _renderObjects[i].transform.localScale = new Vector3(size.x / _loaderObjects[i].texture.width, size.y / _loaderObjects[i].texture.height, 0f);
                _renderObjects[i].transform.parent = _rootObject.transform;
                _renderObjects[i].AddComponent<MultiImageAnimationStimuliBehavior>().Script = this;
                _renderObjects[i].SetActive(false);
            }
            _index = 0;
            _timePerImage = timePerImage;
            _loop = loop;
        }

        public void updateStimuli()
        {
            if (_rootObject.activeSelf)
            {
                if (Time.time - _currentIndexStartTime >= _timePerImage)
                {
                    _renderObjects[_index].SetActive(false);
                    _index++;
                    if (_loop) _index %= _renderObjects.Length;
                    if (_index < _renderObjects.Length)
                    {
                        _renderObjects[_index].SetActive(true);
                        _currentIndexStartTime = Time.time;
                    }
                }
            }
        }

        public void showStimuli()
        {
            _rootObject.SetActive(true);
            _index = 0;
            _renderObjects[_index].SetActive(true);
            _currentIndexStartTime = Time.time;
        }

        public void removeStimuli()
        {
            _rootObject.SetActive(false);
        }

        public bool isStimuliLoaded()
        {
            bool isLoaded = true;
            for (int i = 0; i < _loaderObjects.Length; i++)
                isLoaded &= _loaderObjects[i].isDone;
            return isLoaded;
        }
    }

    public class GlobalPauseDisabledStimuli : IStimuli
    {
        private SystemStateMachine _stateMachine;
        private bool _stateMachineGlobalPauseEnabledValue;

        public GlobalPauseDisabledStimuli(bool defaultValue)
        {
            _stateMachine = FindObjectOfType<SystemStateMachine>();
            _stateMachineGlobalPauseEnabledValue = defaultValue;
        }

        public void showStimuli() { _stateMachine.GlobalPauseEnabled = false; }
        public void removeStimuli() { _stateMachine.GlobalPauseEnabled = _stateMachineGlobalPauseEnabledValue; }
        public bool isStimuliLoaded() { return true; }
    }
}
