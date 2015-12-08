using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GenerateTestFormData : MonoBehaviour {

    public InputField participantIDInput;
    public string defaultParticipantID = "TestParticipantID";
    public InputField researcherHoldingBabyInput;
    public string defaultResearcherHoldingBaby = "TestResearcherHoldingBaby";
    public InputField researcherRunningComputerInput;
    public string defaultResearcherRunningComputer = "TestResearcherRunningComputer";
    public InputField researcherSecondCoderInput;
    public string defaultResearcherSecondCoder = "None";

    public InputField babyBirthDateInput;
    public string defaultBabyBirthDate = "01/01/01";
    public InputField babyBirthTimeInput;
    public string defaultBabyBirthTime = "01:01 AM";

    public string inputObjectName = "Text";

    public void OnClick()
    {
        Debug.Log("Generating Test Inputs...");
        participantIDInput.text = defaultParticipantID;
        researcherHoldingBabyInput.text = defaultResearcherHoldingBaby;
        researcherRunningComputerInput.text = defaultResearcherRunningComputer;
        researcherSecondCoderInput.text = defaultResearcherSecondCoder;

        babyBirthDateInput.text = defaultBabyBirthDate;
        babyBirthTimeInput.text = defaultBabyBirthTime;
    }
}
