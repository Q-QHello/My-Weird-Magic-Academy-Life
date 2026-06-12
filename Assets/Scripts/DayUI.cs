using TMPro;
using UnityEngine;

// Updates the day display on the user interface
public class DayUI : MonoBehaviour
{
// Reference to the UI text component
public TextMeshProUGUI dayText;

void Update()
{
    dayText.text = "Day " + DayManager.currentDay;
}

}
