using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class ValidateAndSubmitForm : MonoBehaviour {

    public GameObject participantIDInput;
    public GameObject researcherHoldingBabyInput;
    public GameObject researcherRunningComputerInput;

    public GameObject currentDateInput;
    public GameObject currentTimeInput;

    public GameObject babyBirthDateInput;
    public GameObject babyBirthTimeInput;

    public GameObject genderDropdown;

    public GameObject conditionNumberDropdown;
    public GameObject placeNumberDropdown;

    public string inputObjectName = "Text";

    public Vector3 punchRotationVector = new Vector3(10f, 10f, 10f);
    public float punchRotationDuration = 0.5f;
    public float resetTime = 0.25f;

    // Update is called once per frame
    public void OnClick () {
        bool allValid = true;

        if (!participantIDInput.GetComponent<InputFieldNotEmptyEnforcer>().isValid())
        {
            iTween.PunchRotation(participantIDInput, punchRotationVector, punchRotationDuration);
            allValid = false;
        }
        if (!researcherHoldingBabyInput.GetComponent<InputFieldNotEmptyEnforcer>().isValid())
        {
            iTween.PunchRotation(researcherHoldingBabyInput, punchRotationVector, punchRotationDuration);
            allValid = false;
        }
        if (!researcherRunningComputerInput.GetComponent<InputFieldNotEmptyEnforcer>().isValid())
        {
            iTween.PunchRotation(researcherRunningComputerInput, punchRotationVector, punchRotationDuration);
            allValid = false;
        }

        if (!currentDateInput.GetComponent<InputFieldDateTimeFormatEnforcer>().isValid())
        {
            iTween.PunchRotation(currentDateInput, punchRotationVector, punchRotationDuration);
            allValid = false;
        }
        if (!currentTimeInput.GetComponent<InputFieldDateTimeFormatEnforcer>().isValid())
        {
            iTween.PunchRotation(currentTimeInput, punchRotationVector, punchRotationDuration);
            allValid = false;
        }
        if (!babyBirthDateInput.GetComponent<InputFieldDateTimeFormatEnforcer>().isValid())
        {
            iTween.PunchRotation(babyBirthDateInput, punchRotationVector, punchRotationDuration);
            allValid = false;
        }
        if (!babyBirthTimeInput.GetComponent<InputFieldDateTimeFormatEnforcer>().isValid())
        {
            iTween.PunchRotation(babyBirthTimeInput, punchRotationVector, punchRotationDuration);
            allValid = false;
        }

        if (!allValid)
            iTween.PunchRotation(gameObject, iTween.Hash("amount", punchRotationVector, "time", punchRotationDuration, "onComplete", "OnComplete"));
        else
        {
            //Valid form - submit and move on...
        }
    }

    void OnComplete()
    {
        iTween.RotateTo(gameObject, Vector3.zero, resetTime);
        iTween.RotateTo(participantIDInput, Vector3.zero, resetTime);
        iTween.RotateTo(researcherHoldingBabyInput, Vector3.zero, resetTime);
        iTween.RotateTo(researcherRunningComputerInput, Vector3.zero, resetTime);
        iTween.RotateTo(currentDateInput, Vector3.zero, resetTime);
        iTween.RotateTo(currentTimeInput, Vector3.zero, resetTime);
        iTween.RotateTo(babyBirthDateInput, Vector3.zero, resetTime);
        iTween.RotateTo(babyBirthTimeInput, Vector3.zero, resetTime);
}
}
