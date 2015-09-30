using UnityEngine;
using System.Collections;

public class SystemStateMachine : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Debug.Log(Application.persistentDataPath);
        JSONDataLoader.Configuration c = JSONDataLoader.LoadDataFromJSON(Application.persistentDataPath + "/TaskConfiguration.json");
    }
	
	// Update is called once per frame
	void Update () {
	}
}
