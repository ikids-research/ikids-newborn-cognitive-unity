using UnityEngine;
using System.Collections;

public class SystemStateMachine : MonoBehaviour {
    public string filename = "TaskConfiguration.json";

    public KeyCode forceCloseKey = KeyCode.Escape;
    public KeyCode globalPauseKey = KeyCode.BackQuote;

    private JSONDataLoader.Configuration config;
    private ThreePhaseController controller;

    private bool globalPauseInEffect;
    private bool prevGlobalPauseKeyState;
    private Color savedBGColor;

    // Use this for initialization
    void Start () {
        //Load the JSON file which contains the task state machine and controller configuration
        config = JSONDataLoader.LoadDataFromJSON(Application.persistentDataPath + "/" + filename);

        ///GLOBAL CONFIG
        /// 
        globalPauseInEffect = false;
        prevGlobalPauseKeyState = false;
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
            bool taskComplete = config.TaskProcedure.getCurrentTask().isTaskComplete();
            if (taskComplete)
            {
                Debug.Log("Task Complete");
                //If the task is complete, advance to the next task and determine if we're done
                bool moreTasksLeft = config.TaskProcedure.nextTask();
                if (!moreTasksLeft)
                {
                    Debug.Log("Done");
                    //If we're done - do something
                }
            }
        }
        else if (globalPauseInEffect)
        {
            Debug.Log("Global Pause In Effect");
        }
	}

    void setPauseMode(bool paused)
    {
        if (paused)
        {
            AudioListener.pause = true;
            Camera.main.cullingMask = 0;
            Time.timeScale = 0f;
            savedBGColor = Camera.main.backgroundColor;
            Camera.main.backgroundColor = Color.black;
        }
        else
        {
            AudioListener.pause = false;
            Camera.main.cullingMask = 1;
            Time.timeScale = 1f;
            Camera.main.backgroundColor = savedBGColor;
        }

    }
}
