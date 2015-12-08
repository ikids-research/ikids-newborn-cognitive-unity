using UnityEngine;
using System.Collections;
using JSONDataLoader;
using NCalc;
using System;

public class SystemStateMachine : MonoBehaviour {
    public string fallBackFilename = "TaskConfiguration.json";
    public string conditionConfigurationFilenamePlayerPrefsString = "conditionConfigurationFilename";
    public string placeNumberPlayerPrefsString = "placeNumber";
    public KeyCode forceCloseKey = KeyCode.Escape;
    public KeyCode globalPauseKey = KeyCode.BackQuote;

    private JSONDataLoader.Configuration config;
    private ThreePhaseController controller;
    private DataLogger log;
    private bool globalPauseInEffect;
    private bool prevGlobalPauseKeyState;
    private float _pauseStartTime = float.MaxValue;
    private bool _forcedAbort = false;
    private Color savedBGColor;

    // Use this for initialization
    void Start () {
        //Setup the logger and notification manager
        NotificationManager.NotificationObject = FindObjectOfType<NotificationManager>().gameObject;
        log = FindObjectOfType<DataLogger>();
        DataLogger.Log = log;

        Camera.main.orthographicSize = Screen.height / 2;

        //Load the JSON file which contains the task state machine and controller configuration
        if (PlayerPrefs.HasKey(conditionConfigurationFilenamePlayerPrefsString))
            config = JSONDataLoader.JSONDataLoader.LoadTaskConfigurationDataFromJSON(Application.persistentDataPath + "/" + PlayerPrefs.GetString(conditionConfigurationFilenamePlayerPrefsString));
        else
            config = JSONDataLoader.JSONDataLoader.LoadTaskConfigurationDataFromJSON(Application.persistentDataPath + "/" + fallBackFilename);

        ///GLOBAL CONFIG
        /// 
        globalPauseInEffect = false;
        prevGlobalPauseKeyState = false;

        Camera.main.backgroundColor = config.BackgroundColor;
        ///

        //Create a controller interface using the controller configuration from the JSON
        controller = new ThreePhaseController(config.Interfaces);

        log.LogConfig("Participant ID: " + PlayerPrefs.GetString("participantID"));
        log.LogConfig("Researcher Holding Baby: " + PlayerPrefs.GetString("researcherHoldingBaby"));
        log.LogConfig("Researcher Running Computer: " + PlayerPrefs.GetString("researcherRunningComputer"));
        log.LogConfig("Researcher Second Coder: " + PlayerPrefs.GetString("researcherSecondCoder"));
        log.LogConfig("Current Date: " + PlayerPrefs.GetString("currentDate"));
        log.LogConfig("Current Time: " + PlayerPrefs.GetString("currentTime"));
        log.LogConfig("Baby Birth Date: " + PlayerPrefs.GetString("babyBirthDate"));
        log.LogConfig("Baby Birth Time: " + PlayerPrefs.GetString("babyBirthTime"));
        log.LogConfig("Age: " + PlayerPrefs.GetString("age"));
        log.LogConfig("Gender: " + PlayerPrefs.GetString("gender"));
        log.LogConfig("Condition Configuration Filename: " + PlayerPrefs.GetString("conditionConfigurationFilename"));
        log.LogConfig("Place Number: " + PlayerPrefs.GetInt("placeNumber"));

        log.LogConfig("Controller Configuration");
        log.LogConfig("Keyboard Enabled=" + config.Interfaces.KeyboardInterfacePresent);
        log.LogConfig("Keyboard Keys=" + String.Join(",", config.Interfaces.KeyboardKeys));
        log.LogConfig("Keyboard Commands=" + String.Join(",", config.Interfaces.KeyboardCommands));
        log.LogConfig("XBox Controller Enabled=" + config.Interfaces.XBoxControllerInterfacePresent);
        log.LogConfig("XBox Controller Keys=" + String.Join(",", config.Interfaces.XBoxControllerKeys));
        log.LogConfig("XBox Controller Commands=" + String.Join(",", config.Interfaces.XBoxControllerCommands));
        log.LogConfig("TCP Enabled=" + config.Interfaces.TcpInterfacePresent);
        log.LogConfig("TCP Keys=" + String.Join(",", config.Interfaces.TcpKeys));
        log.LogConfig("TCP Commands=" + String.Join(",", config.Interfaces.TcpCommands));
        log.LogConfig("TCP Port=" + config.Interfaces.TcpPort);
        string masterInterfaceString = "No Master Interface Set";
        if (config.Interfaces.MasterInterface == InterfaceConfiguration.InterfaceType.Keyboard)
            masterInterfaceString = "Keyboard";
        else if (config.Interfaces.MasterInterface == InterfaceConfiguration.InterfaceType.XBoxController)
            masterInterfaceString = "XBoxController";
        else if (config.Interfaces.MasterInterface == InterfaceConfiguration.InterfaceType.TCP)
            masterInterfaceString = "TCP";
        log.LogConfig("Master Interface: " +  masterInterfaceString);
        log.LogConfig("Global Pause is " + (config.GlobalPauseEnabled?"Enabled":"Disabled"));
        log.LogConfig("Global Pause Maximum Timeout: " + config.MaximumAllowablePauseTime);
        log.LogConfig("Background Color: " + config.BackgroundColor.ToString());
        log.LogConfig("There are " + config.TaskProcedure.Tasks.Count + " states in this task procedure.");

        Debug.Log("Beginning Task");
        log.LogState("Finished loading configuration, beginning task...");
        //Begin the task
        config.TaskProcedure.startFromBeginning();

        if (PlayerPrefs.HasKey(placeNumberPlayerPrefsString))
        {
            int startIndex = PlayerPrefs.GetInt(placeNumberPlayerPrefsString);
            if (!(startIndex < 0 || startIndex > config.TaskProcedure.Tasks.Count))
                config.TaskProcedure.setTask(startIndex);
        }
    }
	
	// Update is called once per frame
	void Update () {

        //Force close behavior
        if (Input.GetKey(forceCloseKey))
        {
            log.LogState("Force quit setected, quitting...");
            Application.Quit();
        }
        if (!_forcedAbort)
        {
            //Global pause key behavior
            bool currentGlobalPauseKeyState = Input.GetKey(globalPauseKey);
            if (config.GlobalPauseEnabled && !globalPauseInEffect && currentGlobalPauseKeyState && !prevGlobalPauseKeyState)
            {
                globalPauseInEffect = true;
                log.LogState("Global pause enabled.");
                setPauseMode(true);
            }
            else if (config.GlobalPauseEnabled && globalPauseInEffect && currentGlobalPauseKeyState && !prevGlobalPauseKeyState)
            {
                globalPauseInEffect = false;
                log.LogState("Global pause disabled.");
                setPauseMode(false);
            }
            prevGlobalPauseKeyState = currentGlobalPauseKeyState;

            //Primary state machine behavior
            if (!config.TaskProcedure.procedureComplete() && !globalPauseInEffect)
            {
                log.LogInput("Keyboard Commands: " + String.Join(",", controller.getKeyboardCommands()));
                log.LogInput("XBox Controller Commands: " + String.Join(",", controller.getXBoxControllerCommands()));
                log.LogInput("TCP Commands: " + String.Join(",", controller.getTCPInterfaceCommands(false)));

                //Get the commands from the master controller
                string[] commands = controller.getMasterInterfaceCommands();
                //Update the task procedure with all commands that have been issued
                foreach (string command in commands)
                    Debug.Log("Issuing Command : " + command);
                log.LogState("Issuing Commands: " + string.Join(",", commands));
                config.TaskProcedure.setConditionStatus(commands);

                //Determine if the task is complete
                int? taskComplete = config.TaskProcedure.getCurrentTask().isTaskComplete();
                log.LogState("Current Task Index: " + config.TaskProcedure.Index);
                while (taskComplete.HasValue)
                {
                    log.LogState("Task Procedure Complete State: " + taskComplete == null ? "Not Complete" : "Complete, Transitioning To " + (taskComplete==-1?"Next State":""+taskComplete));
                    Debug.Log("Task Complete; Transition To " + taskComplete.Value);
                    //If the task is complete, advance to the next task and determine if we're done
                    bool moreTasksLeft = config.TaskProcedure.setTask(taskComplete.Value);
                    if (!moreTasksLeft)
                    {
                        controller.safeShutdown();
                        NotificationManager.pushNotification("Done!", 1000f);
                        log.LogState("Task Procedure Done");
                        Debug.Log("Done");
                        break;
                    }

                    taskComplete = config.TaskProcedure.getCurrentTask().isTaskComplete();
                }
            }
            else if (globalPauseInEffect)
            {
                if (_pauseStartTime != float.MaxValue)
                {
                    float currentTime = Time.unscaledTime;
                    float pausedTime = currentTime - _pauseStartTime;
                    if (pausedTime > config.MaximumAllowablePauseTime)
                    {
                        config.TaskProcedure.Index = int.MaxValue;
                        controller.safeShutdown();
                        _forcedAbort = true;
                        NotificationManager.pushNotification("Maximum Pause Time Reached - Aborted", 0.01f);
                    }
                    else
                    {
                        NotificationManager.pushNotification("Global Pause In Effect", 0.01f);
                        Debug.Log("Global Pause In Effect");
                    }
                }
            }
        }
	}

    void setPauseMode(bool paused)
    {
        if (paused)
        {
            _pauseStartTime = Time.unscaledTime;
            AudioListener.pause = true;
            Time.timeScale = 0f;
            savedBGColor = Camera.main.backgroundColor;
            Camera.main.backgroundColor = Color.black;
            Camera.main.cullingMask = 1 << 5; //Just UI layer '5'
        }
        else
        {
            _pauseStartTime = float.MaxValue;
            AudioListener.pause = false;
            Time.timeScale = 1f;
            Camera.main.backgroundColor = savedBGColor;
            Camera.main.cullingMask = ~0x00000000; //Everything
        }

    }

    public bool GlobalPauseEnabled
    {
        get { return config.GlobalPauseEnabled; }
        set { config.GlobalPauseEnabled = value; }
    }
}
