using UnityEngine;

// Manages daily action limits for the whole game.
//
// Rules:
//
// 1. The player can perform at most 3 academy actions per day.
//    Examples:
//      Learn Potion = 1 action
//      Learn Arcane = 1 action
//      Learn Rune = 1 action
//      Test / Minigame = 1 action
//
// 2. Test / Minigame is available only every 10 days.
//    Valid test days:
//      Day 10
//      Day 20
//      Day 30
//      Day 40
//      ...
//
// 3. Test / Minigame can only be done once per day.
//
// 4. The action counter resets automatically when the day changes.
public static class DailyActionManager
{
    // Maximum total actions allowed per day.
    public static int maxActionsPerDay = 3;

    // Number of Learn + Test actions already used today.
    public static int actionsUsedToday = 0;

    // Whether the player already started a test today.
    public static bool testUsedToday = false;

    // Stores the last day this manager checked.
    // This allows the manager to reset itself when DayManager.currentDay changes.
    private static int lastCheckedDay = -1;


    // Should be called before checking or consuming any action.
    //
    // If the current day has changed, reset daily action data.
    public static void ResetIfNewDay()
    {
        if (lastCheckedDay != DayManager.currentDay)
        {
            lastCheckedDay = DayManager.currentDay;
            actionsUsedToday = 0;
            testUsedToday = false;

            Debug.Log(
                "[DailyActionManager] New day detected. Daily actions reset. Day = " +
                DayManager.currentDay
            );
        }
    }


    // Returns true if the player still has daily action points left.
    public static bool HasActionLeft()
    {
        ResetIfNewDay();

        return actionsUsedToday < maxActionsPerDay;
    }


    // Returns how many actions are still available today.
    public static int GetActionsLeft()
    {
        ResetIfNewDay();

        return Mathf.Max(0, maxActionsPerDay - actionsUsedToday);
    }


    // Checks whether today is a valid test day.
    //
    // Valid:
    //   Day 10
    //   Day 20
    //   Day 30
    //
    // Invalid:
    //   Day 1-9
    //   Day 11-19
    //   Day 21-29
    public static bool IsTestDay()
    {
        ResetIfNewDay();

        if (DayManager.currentDay <= 0)
        {
            return false;
        }

        return DayManager.currentDay % 10 == 0;
    }


    // Gets the next valid test day.
    //
    // Examples:
    //   Day 1  -> Day 10
    //   Day 9  -> Day 10
    //   Day 10 -> Day 10
    //   Day 11 -> Day 20
    public static int GetNextTestDay()
    {
        ResetIfNewDay();

        int currentDay = DayManager.currentDay;

        if (currentDay <= 0)
        {
            return 10;
        }

        if (currentDay % 10 == 0)
        {
            return currentDay;
        }

        int passedTestCycles = currentDay / 10;

        return (passedTestCycles + 1) * 10;
    }


    // Tries to register one Learn action.
    //
    // Returns true if the action is allowed.
    // Returns false if the daily action limit has been reached.
    public static bool TryUseLearnAction(out string message)
    {
        ResetIfNewDay();

        if (!HasActionLeft())
        {
            message =
                "You are too tired today.\n" +
                "Learn and Test together cannot exceed 3 times per day.";

            return false;
        }

        actionsUsedToday++;

        message =
            "Learn completed.\n" +
            "Actions left today: " +
            GetActionsLeft();

        Debug.Log(
            "[DailyActionManager] Learn action used. Actions used today = " +
            actionsUsedToday
        );

        return true;
    }


    // Tries to register one Test / Minigame action.
    //
    // Returns true if:
    //   1. Today is Day 10 / 20 / 30...
    //   2. The player has not already tested today
    //   3. The player still has daily action points left
    public static bool TryUseTestAction(out string message)
    {
        ResetIfNewDay();

        if (!IsTestDay())
        {
            message =
                "Test is not available today.\n" +
                "Next test day: Day " +
                GetNextTestDay();

            return false;
        }

        if (testUsedToday)
        {
            message =
                "You already took a test today.\n" +
                "Come back another test day.";

            return false;
        }

        if (!HasActionLeft())
        {
            message =
                "You are too tired today.\n" +
                "Learn and Test together cannot exceed 3 times per day.";

            return false;
        }

        actionsUsedToday++;
        testUsedToday = true;

        message =
            "Test started.\n" +
            "Actions left today: " +
            GetActionsLeft();

        Debug.Log(
            "[DailyActionManager] Test action used. Actions used today = " +
            actionsUsedToday
        );

        return true;
    }


    // Call this manually after something changes the day directly.
    //
    // Example:
    // Potion minigame failure:
    //   DayManager.currentDay += 3;
    //   DailyActionManager.ForceResetForCurrentDay();
    public static void ForceResetForCurrentDay()
    {
        lastCheckedDay = DayManager.currentDay;
        actionsUsedToday = 0;
        testUsedToday = false;

        Debug.Log(
            "[DailyActionManager] Forced reset. Day = " +
            DayManager.currentDay
        );
    }
}