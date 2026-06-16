using UnityEngine;
using System.Collections;

// Handles all academy study activities
public class StudyActivity : MonoBehaviour
{
// Available study types
public enum StudyType
{
Arcane,
Potion,
Rune
}


// Determines which attribute will be increased
public StudyType studyType;

// Reference to the learning screen
public GameObject learningPanel;

// Reference to the dialogue panel
public GameObject dialoguePanel;

// Called when the Study button is pressed
public void StartStudy()
{
    StartCoroutine(StudyRoutine());
}

// Controls the study process
private IEnumerator StudyRoutine()
{
    // Close the dialogue window
    dialoguePanel.SetActive(false);

    // Show the learning screen
    learningPanel.SetActive(true);

    // Simulate study time
    yield return new WaitForSeconds(2f);

    // Increase the corresponding attribute
    switch (studyType)
    {
        case StudyType.Arcane:
            PlayerStats.arcane += 5;
            break;

        case StudyType.Potion:
            PlayerStats.potion += 5;
            break;

        case StudyType.Rune:
            PlayerStats.rune += 5;
            break;
    }

    // Advance to the next day
    DayManager.currentDay++;

    // Hide the learning screen
    learningPanel.SetActive(false);
}


}
