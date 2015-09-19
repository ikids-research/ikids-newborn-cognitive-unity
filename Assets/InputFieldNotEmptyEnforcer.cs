using UnityEngine;
using UnityEngine.UI;

public class InputFieldNotEmptyEnforcer : MonoBehaviour {

    private InputField inputText;
    private GameObject errorObject;

    public string errorObjectName = "Error";

    private bool valid = false;

    // Use this for initialization
    void Start()
    {
        inputText = gameObject.GetComponent<InputField>();
        foreach (Transform child in transform)
            if (child.name == errorObjectName)
                errorObject = child.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        valid = inputText.text.Trim() != "";
        errorObject.SetActive(!valid);
    }

    public bool isValid()
    {
        return valid;
    }
}
