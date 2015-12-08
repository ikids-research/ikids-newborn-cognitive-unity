using UnityEngine;
using System.Collections;

public class PushToScreen : MonoBehaviour {
    public int monitorNumber = 0;

    // Use this for initialization
    void Start()
    {
        string[] commandLineArgs = System.Environment.GetCommandLineArgs();
        bool displaySwitchEnabled = false;
        for (int i = 0; i < commandLineArgs.Length; i++)
            if (commandLineArgs[i].Contains("displaySwitch"))
            {
                displaySwitchEnabled = true;
                break;
            }
        if (displaySwitchEnabled)
        {
            PlayerPrefs.SetInt("UnitySelectMonitor", monitorNumber);
            Screen.SetResolution(Screen.width, Screen.height, Screen.fullScreen);
        }
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
