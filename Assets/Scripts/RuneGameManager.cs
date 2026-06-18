using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Controls the Rune Academy timing minigame.
//
// Main rules:
//
// 1. HammerMarker moves left and right on TrackBar.
// 2. Player presses Space.
// 3. If HammerMarker is inside HitZone:
//      Hit +1
// 4. If HammerMarker is outside HitZone:
//      Miss +1
// 5. If Hits reach requiredHits:
//      Rune +10
//      Load RuneAcademy
// 6. If Misses reach maxMisses:
//      Day +3
//      Load ResurrectionHallScene
//
// Difficulty:
//
// Semester 1:
//   Bigger HitZone
//   Fewer required hits
//
// Semester 2:
//   Smaller HitZone
//   More required hits
//
// Semester 3:
//   Smallest HitZone
//   Most required hits
//
// Rune stat effect:
//
// Higher Rune stat makes HammerMarker move slower.
// This is similar to Potion stat increasing memorization time.
public class RuneGameManager : MonoBehaviour
{
    [Header("UI References")]

    // RectTransform of the track.
    //
    // This should be TrackBar.
    //
    // The script uses this width to calculate how far the hammer can move.
    public RectTransform trackRect;

    // RectTransform of the hit zone.
    //
    // This should be HitZone.
    //
    // The script changes this width based on semester.
    public RectTransform hitZoneRect;

    // RectTransform of the moving hammer marker.
    //
    // This should be HammerMarker.
    //
    // The script moves this left and right.
    public RectTransform hammerMarkerRect;

    // Text that shows current result.
    //
    // Example:
    // "Hit!"
    // "Miss!"
    // "Success! Rune +10"
    public TextMeshProUGUI resultText;

    // Text that shows progress.
    //
    // Example:
    // "Hits: 2 / 5    Misses: 1 / 3"
    public TextMeshProUGUI progressText;


    [Header("Input Settings")]

    // Key used to strike the rune.
    //
    // Default:
    // Space
    public KeyCode strikeKey = KeyCode.Space;


    [Header("Semester Difficulty Settings")]

    // HitZone width for Semester 1.
    //
    // Easier because the zone is wide.
    public float semester1HitZoneWidth = 160f;

    // HitZone width for Semester 2.
    public float semester2HitZoneWidth = 110f;

    // HitZone width for Semester 3.
    public float semester3HitZoneWidth = 70f;

    // Required successful hits for Semester 1.
    public int semester1RequiredHits = 5;

    // Required successful hits for Semester 2.
    public int semester2RequiredHits = 7;

    // Required successful hits for Semester 3.
    public int semester3RequiredHits = 10;

    // Max misses before failure.
    //
    // Recommended:
    // 3
    public int maxMisses = 3;


    [Header("Hammer Speed Settings")]

    // Base hammer speed for Semester 1.
    public float semester1BaseSpeed = 500f;

    // Base hammer speed for Semester 2.
    public float semester2BaseSpeed = 550f;

    // Base hammer speed for Semester 3.
    public float semester3BaseSpeed = 600f;


    [Header("Rune Stat Bonus Settings")]

    // Rune stat threshold interval.
    //
    // 100 means:
    // Rune > 100
    // Rune > 200
    // Rune > 300
    // Rune > 400
    public int runeThresholdStep = 100;

    // Maximum Rune threshold counted.
    //
    // If this is 400:
    // Rune > 500 does not reduce speed more than Rune > 400.
    public int maxRuneBonusThreshold = 400;

    // Speed reduction per Rune threshold.
    //
    // 0.1 means each threshold reduces speed by 10%.
    //
    // Rune 0-100:
    //   speed x 1.0
    //
    // Rune 101-200:
    //   speed x 0.9
    //
    // Rune 201-300:
    //   speed x 0.8
    //
    // Rune 301-400:
    //   speed x 0.7
    //
    // Rune 401+:
    //   speed x 0.6
    public float speedReductionPerThreshold = 0.1f;

    // Lowest allowed speed multiplier.
    //
    // This prevents the hammer from becoming too slow.
    public float minimumSpeedMultiplier = 0.6f;


    [Header("Reward And Penalty Settings")]

    // Rune stat reward after success.
    public int runeReward = 10;

    // Day penalty after failure.
    public int failureDayPenalty = 3;

    // Delay before changing scenes.
    public float sceneChangeDelay = 1.5f;


    [Header("Debug Testing")]

    // Turn this on only for testing difficulty directly in this scene.
    //
    // If false:
    //   Uses DayManager.currentSemester.
    //
    // If true:
    //   Uses debugSemester.
    public bool useDebugSemester = false;

    // Test semester value.
    //
    // 1 = Semester 1 difficulty
    // 2 = Semester 2 difficulty
    // 3 = Semester 3 difficulty
    [Range(1, 3)]
    public int debugSemester = 1;


    // Scene names.
    //
    // These must match your Unity scene names exactly.
    private const string RuneAcademySceneName = "RuneAcademy";
    private const string ResurrectionHallSceneName = "ResurrectionHallScene";


    // Current semester used for this round.
    private int currentSemester = 1;

    // Current HitZone width after difficulty calculation.
    private float currentHitZoneWidth = 160f;

    // Current required hits after difficulty calculation.
    private int requiredHits = 5;

    // Current hammer movement speed after semester + Rune stat calculation.
    private float currentHammerSpeed = 500f;

    // Current number of successful hits.
    private int currentHits = 0;

    // Current number of misses.
    private int currentMisses = 0;

    // Current hammer X position.
    private float hammerX = 0f;

    // Hammer movement direction.
    //
    // 1 = moving right
    // -1 = moving left
    private float moveDirection = 1f;

    // True after success or failure.
    //
    // Prevents repeated input and repeated scene loading.
    private bool gameOver = false;

    // True briefly after pressing Space.
    //
    // This prevents the player from spamming the key too fast.
    private bool inputCooldown = false;

    // Small cooldown after each strike.
    //
    // This makes each hit feel intentional.
    public float strikeCooldown = 0.15f;


    private void Start()
    {
        StartGame();
    }


    private void Update()
    {
        if (gameOver)
        {
            return;
        }

        MoveHammer();

        if (Input.GetKeyDown(strikeKey))
        {
            TryStrike();
        }
    }


    // Starts the rune minigame.
    private void StartGame()
    {
        currentHits = 0;
        currentMisses = 0;
        gameOver = false;
        inputCooldown = false;

        currentSemester = GetCurrentSemester();

        ApplyDifficultyFromSemester(currentSemester);

        ApplyHitZoneSize();

        // Start hammer on the left side of the track.
        hammerX = -GetHammerMoveLimit();

        if (hammerMarkerRect != null)
        {
            hammerMarkerRect.anchoredPosition =
                new Vector2(hammerX, hammerMarkerRect.anchoredPosition.y);
        }

        if (resultText != null)
        {
            resultText.text =
                "Press " +
                strikeKey +
                " when the hammer is inside the rune zone!";
        }

        RefreshProgressText();

        Debug.Log(
            "[RuneGameManager] Game started. " +
            "Semester = " + currentSemester +
            ", HitZoneWidth = " + currentHitZoneWidth +
            ", RequiredHits = " + requiredHits +
            ", Rune = " + PlayerStats.rune +
            ", HammerSpeed = " + currentHammerSpeed
        );
    }


    // Gets semester from either Debug setting or DayManager.
    private int GetCurrentSemester()
    {
        if (useDebugSemester)
        {
            return debugSemester;
        }

        return DayManager.currentSemester;
    }


    // Applies difficulty based on semester.
    //
    // Semester affects:
    // - HitZone width
    // - Required hits
    // - Base hammer speed
    private void ApplyDifficultyFromSemester(int semester)
    {
        float baseSpeed = semester1BaseSpeed;

        if (semester <= 1)
        {
            currentHitZoneWidth = semester1HitZoneWidth;
            requiredHits = semester1RequiredHits;
            baseSpeed = semester1BaseSpeed;
        }
        else if (semester == 2)
        {
            currentHitZoneWidth = semester2HitZoneWidth;
            requiredHits = semester2RequiredHits;
            baseSpeed = semester2BaseSpeed;
        }
        else
        {
            currentHitZoneWidth = semester3HitZoneWidth;
            requiredHits = semester3RequiredHits;
            baseSpeed = semester3BaseSpeed;
        }

        float runeSpeedMultiplier = CalculateRuneSpeedMultiplier();

        currentHammerSpeed = baseSpeed * runeSpeedMultiplier;
    }


    // Calculates hammer speed reduction from Rune stat.
    //
    // Example:
    //
    // Rune = 0:
    //   multiplier = 1.0
    //
    // Rune = 101:
    //   multiplier = 0.9
    //
    // Rune = 201:
    //   multiplier = 0.8
    //
    // Rune = 301:
    //   multiplier = 0.7
    //
    // Rune = 401:
    //   multiplier = 0.6
    private float CalculateRuneSpeedMultiplier()
    {
        int runeStat = PlayerStats.rune;

        int bonusTier = 0;

        for (int threshold = runeThresholdStep;
             threshold <= maxRuneBonusThreshold;
             threshold += runeThresholdStep)
        {
            if (runeStat > threshold)
            {
                bonusTier++;
            }
        }

        float multiplier =
            1f - bonusTier * speedReductionPerThreshold;

        multiplier =
            Mathf.Max(multiplier, minimumSpeedMultiplier);

        return multiplier;
    }


    // Applies HitZone visual width.
    private void ApplyHitZoneSize()
    {
        if (hitZoneRect == null)
        {
            Debug.LogError("[RuneGameManager] HitZone Rect is missing.");
            return;
        }

        hitZoneRect.sizeDelta =
            new Vector2(
                currentHitZoneWidth,
                hitZoneRect.sizeDelta.y
            );

        // Keep HitZone in the center of the track.
        hitZoneRect.anchoredPosition =
            new Vector2(
                0f,
                hitZoneRect.anchoredPosition.y
            );

        LayoutRebuilder.ForceRebuildLayoutImmediate(hitZoneRect);
    }


    // Moves the hammer left and right.
    private void MoveHammer()
    {
        if (hammerMarkerRect == null)
        {
            return;
        }

        float moveLimit = GetHammerMoveLimit();

        hammerX += moveDirection * currentHammerSpeed * Time.deltaTime;

        if (hammerX >= moveLimit)
        {
            hammerX = moveLimit;
            moveDirection = -1f;
        }
        else if (hammerX <= -moveLimit)
        {
            hammerX = -moveLimit;
            moveDirection = 1f;
        }

        hammerMarkerRect.anchoredPosition =
            new Vector2(
                hammerX,
                hammerMarkerRect.anchoredPosition.y
            );
    }


    // Gets max X distance the hammer can move.
    //
    // Uses TrackBar width minus half of HammerMarker width.
    private float GetHammerMoveLimit()
    {
        if (trackRect == null)
        {
            return 300f;
        }

        float trackHalfWidth = trackRect.rect.width * 0.5f;

        float hammerHalfWidth = 0f;

        if (hammerMarkerRect != null)
        {
            hammerHalfWidth = hammerMarkerRect.rect.width * 0.5f;
        }

        float limit = trackHalfWidth - hammerHalfWidth;

        return Mathf.Max(limit, 10f);
    }


    // Called when player presses Space.
    //
    // You can also connect this to a UI Button if you later want a clickable
    // StrikeButton.
    public void TryStrike()
    {
        if (gameOver)
        {
            return;
        }

        if (inputCooldown)
        {
            return;
        }

        bool hit = IsHammerInsideHitZone();

        if (hit)
        {
            HandleHit();
        }
        else
        {
            HandleMiss();
        }

        StartCoroutine(StrikeCooldownRoutine());
    }


    // Short cooldown after each strike.
    private IEnumerator StrikeCooldownRoutine()
    {
        inputCooldown = true;

        yield return new WaitForSeconds(strikeCooldown);

        inputCooldown = false;
    }


    // Checks whether the hammer is inside HitZone.
    private bool IsHammerInsideHitZone()
    {
        if (hitZoneRect == null)
        {
            return false;
        }

        float hitZoneCenterX = hitZoneRect.anchoredPosition.x;

        float halfHitZoneWidth = currentHitZoneWidth * 0.5f;

        float leftBound = hitZoneCenterX - halfHitZoneWidth;
        float rightBound = hitZoneCenterX + halfHitZoneWidth;

        return hammerX >= leftBound && hammerX <= rightBound;
    }


    // Handles successful timing.
    private void HandleHit()
    {
        currentHits++;

        if (resultText != null)
        {
            resultText.text = "Hit!";
        }

        RefreshProgressText();

        if (currentHits >= requiredHits)
        {
            HandleSuccess();
        }
    }


    // Handles failed timing.
    private void HandleMiss()
    {
        currentMisses++;

        if (resultText != null)
        {
            resultText.text = "Miss!";
        }

        RefreshProgressText();

        if (currentMisses >= maxMisses)
        {
            HandleFailure();
        }
    }


    // Handles success.
    private void HandleSuccess()
    {
        gameOver = true;

        PlayerStats.rune += runeReward;

        if (resultText != null)
        {
            resultText.text = "Success! Rune +" + runeReward;
        }

        Debug.Log(
            "[RuneGameManager] Success. Rune +" +
            runeReward +
            ". Current Rune = " +
            PlayerStats.rune
        );

        StartCoroutine(LoadSceneAfterDelay(RuneAcademySceneName));
    }


    // Handles failure.
    private void HandleFailure()
    {
        gameOver = true;

        if (resultText != null)
        {
            resultText.text = "The rune exploded. You died.";
        }

        bool graduationTriggered = AddFailureDayPenalty();

        if (graduationTriggered)
        {
            return;
        }

        StartCoroutine(LoadSceneAfterDelay(ResurrectionHallSceneName));
    }


    // Adds failure day penalty.
    //
    // Important:
    // Do NOT simply use:
    //
    //     DayManager.currentDay += failureDayPenalty;
    //
    // because that can create invalid dates:
    //
    //     Semester 1 Day 33
    //
    // This method advances days one by one,
    // so semester rollover works correctly.
    private bool AddFailureDayPenalty()
    {
        bool graduationTriggered =
            AdvanceDaysWithSemesterRollOver(failureDayPenalty);

        DailyActionManager.ForceResetForCurrentDay();

        Debug.Log(
            "[RuneGameManager] Failure penalty applied. " +
            "Current Semester = " +
            DayManager.currentSemester +
            ", Current Day = " +
            DayManager.currentDay
        );

        return graduationTriggered;
    }


    // Advances days while correctly handling semester rollover.
    //
    // Example:
    //
    // Current:
    //   Semester 1 Day 30
    //
    // Add 3 days:
    //   Semester 2 Day 3
    private bool AdvanceDaysWithSemesterRollOver(int daysToAdd)
    {
        for (int i = 0; i < daysToAdd; i++)
        {
            DayManager.currentDay++;

            if (DayManager.currentDay > DayManager.daysPerSemester)
            {
                if (DayManager.currentSemester >= 3)
                {
                    GraduationManager.CheckGraduation();
                    return true;
                }

                DayManager.currentSemester++;
                DayManager.currentDay = 1;
            }
        }

        return false;
    }


    // Updates progress text.
    private void RefreshProgressText()
    {
        if (progressText == null)
        {
            return;
        }

        progressText.text =
            "Semester: " +
            currentSemester +
            "\nHits: " +
            currentHits +
            " / " +
            requiredHits +
            "    Misses: " +
            currentMisses +
            " / " +
            maxMisses;
    }


    // Loads a scene after a short delay.
    private IEnumerator LoadSceneAfterDelay(string sceneName)
    {
        yield return new WaitForSeconds(sceneChangeDelay);

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError(
                "[RuneGameManager] Scene cannot be loaded: " +
                sceneName +
                ". Check Build Settings and scene name spelling."
            );

            yield break;
        }

        SceneManager.LoadScene(sceneName);
    }
}