using UnityEngine;
using System.Collections;

public class NotificationManager : MonoBehaviour {

    public Vector3 showPosition = new Vector3(125f, 23.2f, 0f);
    public Vector3 hiddenPosition = new Vector3(125f, -30f, 0f);
    private UnityEngine.UI.Text textField;
    enum State { Hidden, Active };
    private State currentState;
    private float activeStartTime;
    private float currentActiveDuration;
    public float transitionTimeInSeconds = 0.25f;
    // Use this for initialization
    void Start () {
        textField = GetComponentInChildren<UnityEngine.UI.Text>();
        transform.position = hiddenPosition;
        currentState = State.Hidden;
    }
	
	// Update is called once per frame
	void Update () {
	    if(Time.time >= activeStartTime + currentActiveDuration && currentState == State.Active)
        {
            currentState = State.Hidden;
            Hashtable command = new Hashtable();
            command.Add("position", hiddenPosition);
            command.Add("time", transitionTimeInSeconds);
            command.Add("ignoretimescale", true);
            iTween.MoveTo(gameObject, command);
        }
	}

    public void pushNotification(string text, float durationInSeconds)
    {
        textField.text = text;
        currentActiveDuration = durationInSeconds;
        currentState = State.Active;
        Hashtable command = new Hashtable();
        command.Add("position", showPosition);
        command.Add("time", transitionTimeInSeconds);
        command.Add("ignoretimescale", true);
        iTween.MoveTo(gameObject, command);
        activeStartTime = Time.time;
    }
}
