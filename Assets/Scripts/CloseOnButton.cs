using UnityEngine;
using System.Collections;

public class CloseOnButton : MonoBehaviour {
    public KeyCode closeButton = KeyCode.Escape;
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKey(closeButton))
            Application.Quit();
	}
}
