using UnityEngine;
using System.Collections;
using JSONDataLoader;
using NCalc;

public class SystemStateMachine : MonoBehaviour {
    public string fallBackFilename = "TaskConfiguration.json";
    public string conditionConfigurationFilenamePlayerPrefsString = "conditionConfigurationFilename";
    public string placeNumberPlayerPrefsString = "";
    public KeyCode forceCloseKey = KeyCode.Escape;
    public KeyCode globalPauseKey = KeyCode.BackQuote;

    private JSONDataLoader.Configuration config;
    private ThreePhaseController controller;

    private bool globalPauseInEffect;
    private bool prevGlobalPauseKeyState;
    private float _pauseStartTime = float.MaxValue;
    private bool _forcedAbort = false;
    private Color savedBGColor;

    // Use this for initialization
    void Start () {
        NotificationManager.NotificationObject = FindObjectOfType<NotificationManager>().gameObject;

        Camera.main.orthographicSize = Screen.height / 2;

        //Load the JSON file which contains the task state machine and controller configuration
        if (PlayerPrefs.HasKey(conditionConfigurationFilenamePlayerPrefsString))
            config = JSONDataLoader.JSONDataLoader.LoadTaskConfigurationDataFromJSON(Application.persistentDataPath + "/" + PlayerPrefs.GetString(conditionConfigurationFilenamePlayerPrefsString));
        else
            config = JSONDataLoader.JSONDataLoader.LoadTaskConfigurationDataFromJSON(Application.persistentDataPath + "/" + fallBackFilename);

        if (PlayerPrefs.HasKey(placeNumberPlayerPrefsString))
        {
            int startIndex = PlayerPrefs.GetInt(placeNumberPlayerPrefsString);
            if (!(startIndex < 0 || startIndex > config.TaskProcedure.Tasks.Count))
                config.TaskProcedure.Index = startIndex; ;
        }

        ///GLOBAL CONFIG
        /// 
        globalPauseInEffect = false;
        prevGlobalPauseKeyState = false;

        Camera.main.backgroundColor = config.BackgroundColor;
        ///

        //Create a controller interface using the controller configuration from the JSON
        controller = new ThreePhaseController(config.Interfaces);
        Debug.Log("Beginning Task");
        //Begin the task
        config.TaskProcedure.startFromBeginning();
    }
	
	// Update is called once per frame
	void Update () {

        //Force close behavior
        if (Input.GetKey(forceCloseKey))
            Application.Quit();
        if (!_forcedAbort)
        {
            //Global pause key behavior
            bool currentGlobalPauseKeyState = Input.GetKey(globalPauseKey);
            if (config.GlobalPauseEnabled && !globalPauseInEffect && currentGlobalPauseKeyState && !prevGlobalPauseKeyState)
            {
                globalPauseInEffect = true;
                setPauseMode(true);
            }
            else if (config.GlobalPauseEnabled && globalPauseInEffect && currentGlobalPauseKeyState && !prevGlobalPauseKeyState)
            {
                globalPauseInEffect = false;
                setPauseMode(false);
            }
            prevGlobalPauseKeyState = currentGlobalPauseKeyState;

            //Primary state machine behavior
            if (!config.TaskProcedure.procedureComplete() && !globalPauseInEffect)
            {
                //Get the commands from the master controller
                string[] commands = controller.getMasterInterfaceCommands();
                //Update the task procedure with all commands that have been issued
                foreach (string command in commands)
                    Debug.Log("Issuing Command : " + command);
                config.TaskProcedure.setConditionStatus(commands);

                //Determine if the task is complete
                int? taskComplete = config.TaskProcedure.getCurrentTask().isTaskComplete();
                while (taskComplete.HasValue)
                {
                    Debug.Log("Task Complete; Transition To " + taskComplete.Value);
                    //If the task is complete, advance to the next task and determine if we're done
                    bool moreTasksLeft = config.TaskProcedure.setTask(taskComplete.Value);
                    if (!moreTasksLeft)
                    {
                        controller.safeShutdown();
                        NotificationManager.pushNotification("Done!", 1000f);
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
