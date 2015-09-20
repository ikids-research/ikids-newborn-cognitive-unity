using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TabBetweenItemsBehavior : MonoBehaviour {

    private EventSystem system;

	// Use this for initialization
	void Start () {
        system = EventSystem.current;
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Selectable next = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ?
            system.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnUp() :
            system.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnDown();

            if (next != null)
            {
                InputField inputfield = next.GetComponent<InputField>();
                if (inputfield != null)
                    inputfield.OnPointerClick(new PointerEventData(system));

                system.SetSelectedGameObject(next.gameObject);
            }

            //Here is the navigating back part:
            else
            {
                next = Selectable.allSelectables[0];
                system.SetSelectedGameObject(next.gameObject);
            }

        }
    }
}
