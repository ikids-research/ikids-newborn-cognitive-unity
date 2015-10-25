using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.UI;

public class ValidateAndSubmitForm : MonoBehaviour {

    public InputField participantIDInput;
    public InputField researcherHoldingBabyInput;
    public InputField researcherRunningComputerInput;

    public InputField currentDateInput;
    public InputField currentTimeInput;

    public InputField babyBirthDateInput;
    public InputField babyBirthTimeInput;

    public Dropdown genderDropdown;

    public Dropdown conditionNumberDropdown;
    public Dropdown placeNumberDropdown;
    public ConditionAndPlaceConfigurationLoader conditionAndPlaceConfigurationLoader;

    public string inputObjectName = "Text";

    public Vector3 punchRotationVector = new Vector3(10f, 10f, 10f);

    public float punchRotationDuration = 0.5f;
    public float resetTime = 0.25f;

    public string participantIDPlayerPrefsString = "participantID";
    public string researcherHoldingBabyPlayerPrefsString = "researcherHoldingBaby";
    public string researcherRunningComputerPlayerPrefsString = "researcherRunningComputer";

    public string currentDatePlayerPrefsString = "currentDate";
    public string currentTimePlayerPrefsString = "currentTime";

    public string babyBirthDatePlayerPrefsString = "babyBirthDate";
    public string babyBirthTimePlayerPrefsString = "babyBirthTime";

    public string genderPlayerPrefsString = "gender";

    public string conditionConfigurationFilenamePlayerPrefsString = "conditionNumber";
    public string placeNumberPlayerPrefsString = "placeNumber";

    public int transitionSceneNumber = 1;

    public void OnClick () {
        bool allValid = true;

        if (!participantIDInput.GetComponent<InputFieldNotEmptyEnforcer>().isValid())
        {
            iTween.PunchRotation(participantIDInput.gameObject, punchRotationVector, punchRotationDuration);
            allValid = false;
        }
        if (!researcherHoldingBabyInput.GetComponent<InputFieldNotEmptyEnforcer>().isValid())
        {
            iTween.PunchRotation(researcherHoldingBabyInput.gameObject, punchRotationVector, punchRotationDuration);
            allValid = false;
        }
        if (!researcherRunningComputerInput.GetComponent<InputFieldNotEmptyEnforcer>().isValid())
        {
            iTween.PunchRotation(researcherRunningComputerInput.gameObject, punchRotationVector, punchRotationDuration);
            allValid = false;
        }

        if (!currentDateInput.GetComponent<InputFieldDateTimeFormatEnforcer>().isValid())
        {
            iTween.PunchRotation(currentDateInput.gameObject, punchRotationVector, punchRotationDuration);
            allValid = false;
        }
        if (!currentTimeInput.GetComponent<InputFieldDateTimeFormatEnforcer>().isValid())
        {
            iTween.PunchRotation(currentTimeInput.gameObject, punchRotationVector, punchRotationDuration);
            allValid = false;
        }
        if (!babyBirthDateInput.GetComponent<InputFieldDateTimeFormatEnforcer>().isValid())
        {
            iTween.PunchRotation(babyBirthDateInput.gameObject, punchRotationVector, punchRotationDuration);
            allValid = false;
        }
        if (!babyBirthTimeInput.GetComponent<InputFieldDateTimeFormatEnforcer>().isValid())
        {
            iTween.PunchRotation(babyBirthTimeInput.gameObject, punchRotationVector, punchRotationDuration);
            allValid = false;
        }

        if (!allValid)
        {
            iTween.PunchRotation(gameObject, iTween.Hash("amount", punchRotationVector, "time", punchRotationDuration, "onComplete", "OnComplete"));
            Debug.Log("Invalid inputs. No submission is made.");
        }
        else
        {
            Debug.Log("Valid inputs. Submitting and transitioning.");

            PlayerPrefs.SetString(participantIDPlayerPrefsString, participantIDInput.text);
            PlayerPrefs.SetString(researcherHoldingBabyPlayerPrefsString, researcherHoldingBabyInput.text);
            PlayerPrefs.SetString(researcherRunningComputerPlayerPrefsString, researcherRunningComputerInput.text);

            PlayerPrefs.SetString(currentDatePlayerPrefsString, currentDateInput.text);
            PlayerPrefs.SetString(currentTimePlayerPrefsString, currentTimeInput.text);

            PlayerPrefs.SetString(babyBirthDatePlayerPrefsString, babyBirthDateInput.text);
            PlayerPrefs.SetString(babyBirthTimePlayerPrefsString, babyBirthTimeInput.text);

            PlayerPrefs.SetInt(genderPlayerPrefsString, genderDropdown.value);

            PlayerPrefs.SetString(conditionConfigurationFilenamePlayerPrefsString, conditionAndPlaceConfigurationLoader.getSelectedCondition());
            PlayerPrefs.SetInt(placeNumberPlayerPrefsString, conditionAndPlaceConfigurationLoader.getSelectedPlace());

            Application.LoadLevel(transitionSceneNumber);
        }
    }

    void OnComplete()
    {
        iTween.RotateTo(gameObject, Vector3.zero, resetTime);
        iTween.RotateTo(participantIDInput.gameObject, Vector3.zero, resetTime);
        iTween.RotateTo(researcherHoldingBabyInput.gameObject, Vector3.zero, resetTime);
        iTween.RotateTo(researcherRunningComputerInput.gameObject, Vector3.zero, resetTime);
        iTween.RotateTo(currentDateInput.gameObject, Vector3.zero, resetTime);
        iTween.RotateTo(currentTimeInput.gameObject, Vector3.zero, resetTime);
        iTween.RotateTo(babyBirthDateInput.gameObject, Vector3.zero, resetTime);
        iTween.RotateTo(babyBirthTimeInput.gameObject, Vector3.zero, resetTime);
}
}
