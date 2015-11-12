using UnityEngine;
using SimpleJSON;
using System.Collections;
using UnityEngine.UI;
using JSONDataLoader;

public class ConditionAndPlaceConfigurationLoader : MonoBehaviour {
    public string filename = "MainFormConfiguration.json";
    public Dropdown conditionDropDown;
    public Dropdown placeDropDown;
    private string[] conditionConfigurationFiles;
    private string[] placeStateIndicies;

    // Use this for initialization
    void Start () {
        string fullPath = Application.persistentDataPath + "/" + filename;
        //Get the JSON contents from file
        string contents = JSONDataLoader.JSONDataLoader.getFileContents(fullPath);

        //Create the root task node for parsing
        JSONNode rootNode = JSONNode.Parse(contents);
        JSONClass rootClass = rootNode.AsObject;
        //Validate that the file is at least remotely formatted correctly by checking for the Task property (which contains everything)
        if (rootClass["Conditions"] == null || rootClass["Places"] == null)
        {
            Debug.LogError("Error: JSON does not include root Task object. See example JSON for help.");
            Application.Quit();
        }
        JSONClass conditionsClass = rootClass["Conditions"].AsObject;
        JSONClass placesClass = rootClass["Places"].AsObject;

        JSONArray conditionLabelsJSON = conditionsClass["ConditionLabels"].AsArray;
        JSONArray conditionConfigurationFilesJSON = conditionsClass["ConditionConfigurationFiles"].AsArray;

        JSONArray placeLabelsJSON = placesClass["PlaceLabels"].AsArray;
        JSONArray placeStateIndiciesJSON = placesClass["PlaceStateIndicies"].AsArray;

        string[] conditionLabels = new string[conditionLabelsJSON.Count];
        conditionConfigurationFiles = new string[conditionConfigurationFilesJSON.Count];

        string[] placeLabels = new string[placeLabelsJSON.Count];
        placeStateIndicies = new string[placeStateIndiciesJSON.Count];

        conditionDropDown.options = new System.Collections.Generic.List<Dropdown.OptionData>();
        placeDropDown.options = new System.Collections.Generic.List<Dropdown.OptionData>();

        for (int i = 0; i < conditionLabelsJSON.Count; i++)
        {
            conditionLabels[i] = conditionLabelsJSON[i];
            conditionDropDown.options.Add(new Dropdown.OptionData(conditionLabels[i]));
        }
        for (int i = 0; i < conditionConfigurationFilesJSON.Count; i++)
            conditionConfigurationFiles[i] = conditionConfigurationFilesJSON[i];

        for (int i = 0; i < placeLabelsJSON.Count; i++)
        {
            placeLabels[i] = placeLabelsJSON[i];
            placeDropDown.options.Add(new Dropdown.OptionData(placeLabels[i]));
        }
        for (int i = 0; i < placeStateIndiciesJSON.Count; i++)
            placeStateIndicies[i] = placeStateIndiciesJSON[i];

        Text[] placeTexts = placeDropDown.GetComponentsInChildren<Text>();
        Text placeText = placeTexts[0];
        for (int i = 0; i < placeTexts.Length; i++)
            if (placeTexts[i].text == "initme")
                placeText = placeTexts[i];
        placeText.text = placeLabels[0];

        Text[] conditionTexts = conditionDropDown.GetComponentsInChildren<Text>();
        Text conditionText = conditionTexts[0];
        for (int i = 0; i < conditionTexts.Length; i++)
            if (conditionTexts[i].text == "initme")
                conditionText = conditionTexts[i];
        conditionText.text = conditionLabels[0];
    }

    public string getSelectedCondition()
    {
        return conditionConfigurationFiles[conditionDropDown.value];
    }

    public int getSelectedPlace()
    {
        int parsedValue = 0;
        try
        {
            parsedValue = int.Parse(placeStateIndicies[placeDropDown.value]);
        }
        catch (System.Exception) { }
        return parsedValue;
    }
}
