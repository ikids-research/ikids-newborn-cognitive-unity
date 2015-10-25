using UnityEngine;
using System.Collections;

public class MultiImageAnimationStimuliBehavior : MonoBehaviour {
    private JSONDataLoader.MultiImageAnimationStimuli _script;
    public JSONDataLoader.MultiImageAnimationStimuli Script
    {
        get { return _script; }
        set { _script = value; }
    }
    // Use this for initialization
    void Start () {
	    
	}

    // Update is called once per frame
    void Update()
    {
        _script.updateStimuli();
    }
}
