using UnityEngine;

// Handles player interaction with the bed.
//
// This script is responsible for:
//
// 1. Detecting when the player is near the bed.
// 2. Pressing E to sleep.
// 3. Advancing to the next day.
// 4. Moving to the next semester when currentDay exceeds daysPerSemester.
// 5. Checking graduation after Semester 3.
// 6. Resetting daily Learn/Test action count.
//
// Important:
// Normal Study should NOT advance the day.
// Normal Test should NOT advance the day immediately.
// Sleeping is the normal way to move to the next day.
public class SleepTrigger : MonoBehaviour
{
    // Tracks whether the player is inside the bed interaction area.
    private bool playerInRange = false;


    private void Update()
    {
        // If the player is not near the bed, ignore input.
        if (!playerInRange)
        {
            return;
        }

        // Press E to sleep.
        if (Input.GetKeyDown(KeyCode.E))
        {
            SleepAndAdvanceDay();
        }
    }


    // Handles the full sleep process.
    private void SleepAndAdvanceDay()
    {
        // Move to the next day.
        DayManager.currentDay++;

        // If the day exceeds the maximum days per semester,
        // move into the next semester.
        if (DayManager.currentDay > DayManager.daysPerSemester)
        {
            // If the player has already finished Semester 3,
            // check graduation instead of creating Semester 4.
            if (DayManager.currentSemester >= 3)
            {
                GraduationManager.CheckGraduation();
                return;
            }

            // Move to next semester.
            DayManager.currentSemester++;

            // New semester starts from Day 1.
            DayManager.currentDay = 1;
        }

        // Since sleeping creates a new day,
        // reset daily Learn/Test limits.
        DailyActionManager.ForceResetForCurrentDay();

        Debug.Log(
            "[SleepTrigger] Slept successfully. " +
            "Semester " +
            DayManager.currentSemester +
            " Day " +
            DayManager.currentDay
        );
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        // Only the Player can activate the bed trigger.
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }


    private void OnTriggerExit2D(Collider2D other)
    {
        // When the player leaves the bed area,
        // disable bed interaction.
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}