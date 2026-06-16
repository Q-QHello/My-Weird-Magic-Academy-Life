using TMPro;
using UnityEngine;

// Updates the day and semester display on the UI
public class DayUI : MonoBehaviour
{
// Reference to the UI text component
public TextMeshProUGUI dayText;

void Update()
{
    dayText.text =
        "Semester " +
        DayManager.currentSemester +
        " Day " +
        DayManager.currentDay;
}

}
