using UnityEngine;

// Handles player interaction with the bed
public class SleepTrigger : MonoBehaviour
{
    // Tracks whether the player is inside the interaction area
    private bool playerInRange = false;

    void Update()
    {
        // Press E to sleep and advance to the next day
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            // Advance to the next day
            DayManager.currentDay++;

            // Advance to the next semester when the day limit is reached
            // Advance to the next semester when the day limit is reached
            if (DayManager.currentDay > DayManager.daysPerSemester)
            {
                // Check graduation after Semester 3
                if (DayManager.currentSemester >= 3)
                {
                    GraduationManager.CheckGraduation();
                    return;
                }


                DayManager.currentSemester++;
                DayManager.currentDay = 1;

            }


            Debug.Log(
                "Semester " +
                DayManager.currentSemester +
                " Day " +
                DayManager.currentDay);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Detect when the player enters the sleep area
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Detect when the player leaves the sleep area
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

}
