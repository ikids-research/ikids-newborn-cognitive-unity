using UnityEngine;
using UnityEngine.UI;

public class InputFieldDateTimeFormatEnforcer : MonoBehaviour {

    private InputField inputText;
    private GameObject errorObject;

    public string dateTimeFormat = "dd/MM/yy";
    public string errorObjectName = "Error";
    public string cultureInfoString = "en-US";
    public System.Globalization.DateTimeStyles dateTimeStyles = System.Globalization.DateTimeStyles.None;

    public bool defaultToCurrent = false;

    private bool valid = false;

	// Use this for initialization
	void Start () {
        inputText = gameObject.GetComponent<InputField>();
        foreach (Transform child in transform)
            if (child.name == errorObjectName)
                errorObject = child.gameObject;
        if (defaultToCurrent)
        {
            System.DateTime currentDateTime = System.DateTime.Now;
            inputText.text = currentDateTime.ToString(dateTimeFormat);
        }
    }

    // Update is called once per frame
    void Update() {
        System.DateTime inputDateTime;
        valid = System.DateTime.TryParseExact(inputText.text, dateTimeFormat, new System.Globalization.CultureInfo(cultureInfoString), dateTimeStyles, out inputDateTime);
        errorObject.SetActive(!valid);
	}

    public bool isValid()
    {
        return valid;
    }
}
