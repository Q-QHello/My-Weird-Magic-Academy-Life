using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Controls the Potion Academy memory-card minigame.
//
// This script is ONLY responsible for the Potion Test / Minigame.
//
// It does NOT control normal StudyButton behavior.
// StudyButton behavior should be handled by AcademyTeacherMenuController.
//
// Main game rules:
//
// 1. The game generates a potion recipe.
//    Example:
//      Herb -> Crystal -> Mushroom
//
// 2. The game creates a card grid based on the current semester.
//
//    Semester 1:
//      3x3 grid
//      3 ingredients
//      9 cards total
//
//    Semester 2:
//      4x4 grid
//      4 ingredients
//      16 cards total
//
//    Semester 3 or higher:
//      5x5 grid
//      5 ingredients
//      25 cards total
//
// 3. Cards are shown face-up for a memorization period.
//
// 4. Memorization time increases based on the player's Potion stat.
//
//    Base time:
//      5 seconds
//
//    Potion bonus:
//      Potion > 100 -> +3 seconds
//      Potion > 200 -> +6 seconds
//      Potion > 300 -> +9 seconds
//      Potion > 400 -> +12 seconds
//
// 5. After memorization time ends, all cards turn face-down.
//
// 6. The player must click cards in recipe order.
//
//    Example recipe:
//      Herb -> Crystal -> Mushroom
//
//    If the grid is 3x3, the correct click order is:
//      Herb, Herb, Herb,
//      Crystal, Crystal, Crystal,
//      Mushroom, Mushroom, Mushroom
//
// 7. Success:
//      Potion +10
//      Load PotionAcademy scene
//
// 8. Failure:
//      The cauldron explodes.
//      The player loses 3 days.
//      If day exceeds daysPerSemester, semester rolls over correctly.
//      Example:
//        Semester 1 Day 30 + 3 days
//        -> Semester 2 Day 3
//      Then the player loads into ResurrectionHallScene.
public class PotionGameManager : MonoBehaviour
{
    [Header("UI References")]

    // Text that displays the current potion recipe.
    //
    // Example:
    // Semester 1
    //
    // Recipe:
    // Herb -> Crystal -> Mushroom
    public TextMeshProUGUI recipeText;

    // Text that displays game feedback.
    //
    // Example:
    // "Memorize the cards!"
    // "Follow the recipe!"
    // "Success! Potion +10"
    // "The cauldron exploded. You died."
    public TextMeshProUGUI resultText;


    [Header("Card References")]

    // Card prefab used to generate the card grid.
    //
    // Required components on the prefab:
    // 1. Image
    // 2. Button
    // 3. PotionCard
    public GameObject cardPrefab;

    // Parent object that holds all generated cards.
    //
    // This should usually be a UI object under Canvas.
    // It should usually have a GridLayoutGroup component.
    public Transform cardGrid;

    // GridLayoutGroup used to arrange cards into:
    // - 3 columns for 3x3
    // - 4 columns for 4x4
    // - 5 columns for 5x5
    //
    // If this is not assigned in the Inspector,
    // this script will try to find it automatically from cardGrid.
    public GridLayoutGroup gridLayoutGroup;


    [Header("Ingredient Sprites")]

    // Front sprites for each ingredient card.
    public Sprite herbSprite;
    public Sprite crystalSprite;
    public Sprite mushroomSprite;
    public Sprite boneSprite;
    public Sprite featherSprite;

    // Back sprite shown after memorization time ends.
    public Sprite cardBackSprite;


    [Header("Time Settings")]

    // Base memorization time before Potion stat bonus.
    public float baseMemorizationTime = 5f;

    // Extra seconds gained for each Potion threshold.
    //
    // Example:
    // If this value is 3:
    // Potion > 100 gives +3 seconds.
    // Potion > 200 gives +6 seconds.
    public float bonusSecondsPerPotionThreshold = 3f;

    // Potion stat interval for each bonus tier.
    //
    // If this is 100, thresholds are:
    // 100, 200, 300, 400
    public int potionThresholdStep = 100;

    // Maximum Potion threshold counted for bonus time.
    //
    // If this is 400:
    // Potion > 500 does not give extra bonus beyond the >400 bonus.
    public int maxPotionBonusThreshold = 400;

    // Final memorization time used in the current round.
    private float currentRoundMemorizationTime = 5f;


    [Header("Reward And Penalty Settings")]

    // Potion stat reward after successful completion.
    public int potionReward = 10;

    // Number of days lost after failure.
    public int failureDayPenalty = 3;

    // Delay before changing scene after success or failure.
    public float sceneChangeDelay = 1.5f;


    [Header("Grid Layout Settings")]

    // If true, the script automatically resizes cards
    // so the overall grid area stays visually stable between:
    // - 3x3
    // - 4x4
    // - 5x5
    public bool autoResizeCards = true;

    // Semester 1 uses 3x3.
    //
    // This value is used as the base layout size.
    public int semesterOneGridSize = 3;

    // Cell size used by Semester 1.
    //
    // You said Semester 1 cell size is 80.
    // Semester 2 and Semester 3 will shrink based on this.
    public float semesterOneCellSize = 80f;

    // Space between cards.
    public Vector2 cardSpacing = new Vector2(10f, 10f);


    [Header("Debug Testing")]

    // Turn this on ONLY for testing.
    //
    // If false:
    //   The script uses DayManager.currentSemester.
    //
    // If true:
    //   The script uses debugSemester instead.
    //
    // Important:
    // For real gameplay, keep this false.
    public bool useDebugSemester = false;

    // Test semester value.
    //
    // 1 = 3x3
    // 2 = 4x4
    // 3 = 5x5
    [Range(1, 3)]
    public int debugSemester = 1;


    // Scene names are constants.
    //
    // This prevents old Inspector string values from overriding scene names.
    //
    // Make sure these exact scenes are added to:
    // File -> Build Settings -> Scenes In Build
    private const string PotionAcademySceneName = "PotionAcademy";
    private const string ResurrectionHallSceneName = "ResurrectionHallScene";


    // All possible ingredients.
    //
    // Semester 1 uses 3 of them.
    // Semester 2 uses 4 of them.
    // Semester 3 uses 5 of them.
    private readonly string[] allIngredients =
    {
        "Herb",
        "Crystal",
        "Mushroom",
        "Bone",
        "Feather"
    };

    // Recipe shown to the player.
    //
    // Example:
    // [Herb, Crystal, Mushroom]
    private readonly List<string> currentRecipe = new List<string>();

    // Full required click sequence.
    //
    // Example:
    // Recipe:
    // [Herb, Crystal, Mushroom]
    //
    // Difficulty size:
    // 3
    //
    // Required click sequence:
    // [Herb, Herb, Herb, Crystal, Crystal, Crystal, Mushroom, Mushroom, Mushroom]
    private readonly List<string> requiredClickSequence = new List<string>();

    // Stores all generated PotionCard objects in this round.
    private readonly List<PotionCard> cards = new List<PotionCard>();

    // Current index inside requiredClickSequence.
    private int currentStep = 0;

    // Semester used for this minigame round.
    private int currentRoundSemester = 1;

    // Difficulty size used for this minigame round.
    //
    // Semester 1 = 3
    // Semester 2 = 4
    // Semester 3 = 5
    private int currentDifficultySize = 3;

    // True while cards are still face-up for memorization.
    //
    // During this time, player clicks are ignored.
    private bool inputLocked = true;

    // True after success or failure.
    //
    // Prevents repeated clicks from triggering multiple scene changes.
    private bool gameOver = false;


    private void Awake()
    {
        // Automatically find GridLayoutGroup if it was not assigned manually.
        //
        // This is useful because the cardGrid object usually owns the GridLayoutGroup.
        if (gridLayoutGroup == null && cardGrid != null)
        {
            gridLayoutGroup = cardGrid.GetComponent<GridLayoutGroup>();
        }
    }


    private void Start()
    {
        StartGame();
    }


    // Starts one full Potion minigame round.
    private void StartGame()
    {
        // Clear old cards if this scene somehow restarts or reloads.
        ClearOldCards();

        currentRecipe.Clear();
        requiredClickSequence.Clear();
        cards.Clear();

        currentStep = 0;
        inputLocked = true;
        gameOver = false;

        // Read the semester once at the start of the round.
        //
        // This prevents recipe size and grid size from becoming inconsistent.
        currentRoundSemester = GetCurrentSemester();

        // Convert semester into difficulty size.
        currentDifficultySize = GetDifficultySizeFromSemester(currentRoundSemester);

        // Calculate final memorization time for this round.
        currentRoundMemorizationTime = CalculateMemorizationTime();

        Debug.Log(
            "[PotionGameManager] Round started. " +
            "Semester = " + currentRoundSemester +
            ", Difficulty Size = " + currentDifficultySize +
            ", Expected Cards = " + (currentDifficultySize * currentDifficultySize) +
            ", Potion = " + PlayerStats.potion +
            ", Memorization Time = " + currentRoundMemorizationTime
        );

        GenerateRecipe();
        GenerateRequiredClickSequence();
        GenerateGrid();

        if (resultText != null)
        {
            resultText.text =
                "Memorize the cards!\n" +
                "Time: " +
                currentRoundMemorizationTime +
                "s";
        }

        StartCoroutine(HideCardsAfterMemorization());
    }


    // Gets current semester from either debug setting or DayManager.
    private int GetCurrentSemester()
    {
        if (useDebugSemester)
        {
            return debugSemester;
        }

        return DayManager.currentSemester;
    }


    // Converts semester number into grid size.
    //
    // Semester 1:
    //   return 3
    //
    // Semester 2:
    //   return 4
    //
    // Semester 3 or higher:
    //   return 5
    private int GetDifficultySizeFromSemester(int semester)
    {
        if (semester <= 1)
        {
            return 3;
        }

        if (semester == 2)
        {
            return 4;
        }

        return 5;
    }


    // Calculates final memorization time for this round.
    //
    // Formula:
    //   final time = base time + Potion bonus
    //
    // Potion bonus:
    //   Potion > 100 -> +3s
    //   Potion > 200 -> +6s
    //   Potion > 300 -> +9s
    //   Potion > 400 -> +12s
    //
    // Important:
    // The rule uses "greater than", not "greater than or equal".
    //
    // Potion = 100:
    //   no bonus
    //
    // Potion = 101:
    //   +3 seconds
    private float CalculateMemorizationTime()
    {
        int potionStat = PlayerStats.potion;

        int bonusTier = 0;

        for (int threshold = potionThresholdStep;
             threshold <= maxPotionBonusThreshold;
             threshold += potionThresholdStep)
        {
            if (potionStat > threshold)
            {
                bonusTier++;
            }
        }

        float bonusTime = bonusTier * bonusSecondsPerPotionThreshold;

        return baseMemorizationTime + bonusTime;
    }


    // Generates a random recipe for the current semester.
    //
    // Semester 1:
    //   3 ingredients
    //
    // Semester 2:
    //   4 ingredients
    //
    // Semester 3:
    //   5 ingredients
    private void GenerateRecipe()
    {
        int recipeLength = currentDifficultySize;

        List<string> pool = new List<string>(allIngredients);

        currentRecipe.Clear();

        for (int i = 0; i < recipeLength; i++)
        {
            int randomIndex = Random.Range(0, pool.Count);

            currentRecipe.Add(pool[randomIndex]);

            // Remove selected ingredient so recipe has no duplicates.
            pool.RemoveAt(randomIndex);
        }

        if (recipeText != null)
        {
            recipeText.text =
                "Semester " +
                currentRoundSemester +
                "\n\nRecipe:\n" +
                string.Join(" → ", currentRecipe);
        }
    }


    // Builds the full required click order.
    private void GenerateRequiredClickSequence()
    {
        requiredClickSequence.Clear();

        foreach (string ingredient in currentRecipe)
        {
            for (int i = 0; i < currentDifficultySize; i++)
            {
                requiredClickSequence.Add(ingredient);
            }
        }
    }


    // Generates the visual card grid.
    private void GenerateGrid()
    {
        ApplyGridLayoutSettings();

        List<string> cardIngredients = new List<string>();

        // Add multiple copies of each recipe ingredient.
        //
        // Example:
        // Semester 2:
        // 4 ingredients x 4 copies = 16 cards.
        foreach (string ingredient in currentRecipe)
        {
            for (int i = 0; i < currentDifficultySize; i++)
            {
                cardIngredients.Add(ingredient);
            }
        }

        ShuffleList(cardIngredients);

        foreach (string ingredient in cardIngredients)
        {
            GameObject cardObject = Instantiate(cardPrefab, cardGrid);

            PotionCard potionCard = cardObject.GetComponent<PotionCard>();

            if (potionCard == null)
            {
                Debug.LogError(
                    "[PotionGameManager] PotionCard component is missing on the card prefab."
                );

                continue;
            }

            Sprite ingredientSprite = GetSprite(ingredient);

            potionCard.SetupCard(
                ingredient,
                ingredientSprite,
                cardBackSprite,
                this
            );

            cards.Add(potionCard);
        }

        Debug.Log(
            "[PotionGameManager] Generated Cards = " +
            cards.Count +
            ", Grid = " +
            currentDifficultySize +
            "x" +
            currentDifficultySize
        );

        ForceRefreshGridLayout();
    }


    // Applies GridLayoutGroup settings.
    private void ApplyGridLayoutSettings()
    {
        if (gridLayoutGroup == null)
        {
            Debug.LogWarning(
                "[PotionGameManager] gridLayoutGroup is missing. " +
                "Cards may be generated correctly, but the layout will not be square."
            );

            return;
        }

        gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayoutGroup.constraintCount = currentDifficultySize;

        gridLayoutGroup.startAxis = GridLayoutGroup.Axis.Horizontal;
        gridLayoutGroup.childAlignment = TextAnchor.MiddleCenter;
        gridLayoutGroup.spacing = cardSpacing;

        if (autoResizeCards)
        {
            ResizeCardsToKeepSameOverallGridSize();
        }
    }


    // Automatically resizes card cells while keeping the overall grid area stable.
    //
    // Example:
    //
    // Semester 1:
    //   3x3
    //   cell size = 80
    //   spacing = 10
    //   total size = 3 * 80 + 2 * 10 = 260
    //
    // Semester 2:
    //   4x4
    //   total size still = 260
    //   cell size = (260 - 3 * 10) / 4 = 57.5
    //
    // Semester 3:
    //   5x5
    //   total size still = 260
    //   cell size = (260 - 4 * 10) / 5 = 44
    private void ResizeCardsToKeepSameOverallGridSize()
    {
        if (gridLayoutGroup == null)
        {
            return;
        }

        float baseGridTotalSize =
            semesterOneGridSize * semesterOneCellSize +
            (semesterOneGridSize - 1) * cardSpacing.x;

        float currentTotalSpacing =
            (currentDifficultySize - 1) * cardSpacing.x;

        float newCellSize =
            (baseGridTotalSize - currentTotalSpacing) / currentDifficultySize;

        // Prevent extremely tiny or negative cards.
        newCellSize = Mathf.Max(newCellSize, 20f);

        gridLayoutGroup.cellSize = new Vector2(newCellSize, newCellSize);

        Debug.Log(
            "[PotionGameManager] Cell size adjusted. " +
            "Grid = " + currentDifficultySize + "x" + currentDifficultySize +
            ", Cell Size = " + newCellSize +
            ", Overall Grid Size = " + baseGridTotalSize
        );
    }


    // Forces Unity UI layout to refresh immediately.
    private void ForceRefreshGridLayout()
    {
        Canvas.ForceUpdateCanvases();

        RectTransform gridRect = cardGrid as RectTransform;

        if (gridRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(gridRect);
        }
    }


    // Waits for memorization time, then hides all cards.
    private IEnumerator HideCardsAfterMemorization()
    {
        yield return new WaitForSeconds(currentRoundMemorizationTime);

        foreach (PotionCard card in cards)
        {
            card.Hide();
        }

        inputLocked = false;

        if (resultText != null)
        {
            resultText.text = "Follow the recipe!";
        }
    }


    // Called by PotionCard when a card is clicked.
    public void CheckCard(PotionCard clickedCard)
    {
        if (gameOver)
        {
            return;
        }

        if (inputLocked)
        {
            return;
        }

        if (clickedCard == null)
        {
            return;
        }

        Button button = clickedCard.GetComponent<Button>();

        if (button != null && button.interactable == false)
        {
            return;
        }

        if (currentStep < 0 || currentStep >= requiredClickSequence.Count)
        {
            Debug.LogError(
                "[PotionGameManager] currentStep out of range. " +
                "currentStep = " + currentStep +
                ", requiredClickSequence.Count = " + requiredClickSequence.Count
            );

            return;
        }

        string expectedIngredient = requiredClickSequence[currentStep];

        if (clickedCard.ingredientName == expectedIngredient)
        {
            HandleCorrectCard(clickedCard);
        }
        else
        {
            HandleWrongCard();
        }
    }


    // Handles a correct card click.
    private void HandleCorrectCard(PotionCard clickedCard)
    {
        Button button = clickedCard.GetComponent<Button>();

        if (button != null)
        {
            button.interactable = false;
        }

        currentStep++;

        if (currentStep >= requiredClickSequence.Count)
        {
            HandleSuccess();
            return;
        }

        if (resultText != null)
        {
            resultText.text = "Follow the recipe!";
        }
    }


    // Handles a wrong card click.
    private void HandleWrongCard()
    {
        gameOver = true;
        inputLocked = true;

        if (resultText != null)
        {
            resultText.text = "The cauldron exploded. You died.";
        }

        bool graduationTriggered = AddFailureDayPenalty();

        // If graduation was triggered after Semester 3,
        // do not also load ResurrectionHallScene.
        if (graduationTriggered)
        {
            return;
        }

        StartCoroutine(LoadSceneAfterDelay(ResurrectionHallSceneName));
    }


    // Handles successful completion.
    private void HandleSuccess()
    {
        gameOver = true;
        inputLocked = true;

        PlayerStats.potion += potionReward;

        if (resultText != null)
        {
            resultText.text = "Success! Potion +" + potionReward;
        }

        StartCoroutine(LoadSceneAfterDelay(PotionAcademySceneName));
    }


    // Adds the failure day penalty after the player fails the Potion test.
    //
    // Important:
    // Do NOT simply write:
    //
    //     DayManager.currentDay += failureDayPenalty;
    //
    // because that can create invalid dates such as:
    //
    //     Semester 1 Day 33
    //
    // Instead, this method advances one day at a time.
    // This allows the game to correctly roll over:
    //
    //     Semester 1 Day 30 + 3 days
    //     -> Semester 2 Day 3
    //
    // Returns true if graduation check was triggered.
    private bool AddFailureDayPenalty()
    {
        bool graduationTriggered =
            AdvanceDaysWithSemesterRollOver(failureDayPenalty);

        DailyActionManager.ForceResetForCurrentDay();

        Debug.Log(
            "[PotionGameManager] Failure penalty applied. " +
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
    // DayManager.daysPerSemester = 30
    //
    // Current date:
    //   Semester 1 Day 30
    //
    // Add 3 days:
    //
    // Step 1:
    //   Day 31 exceeds 30
    //   -> Semester 2 Day 1
    //
    // Step 2:
    //   -> Semester 2 Day 2
    //
    // Step 3:
    //   -> Semester 2 Day 3
    //
    // Returns true if graduation check was triggered.
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


    // Loads a scene after a short delay.
    private IEnumerator LoadSceneAfterDelay(string sceneName)
    {
        yield return new WaitForSeconds(sceneChangeDelay);

        SceneManager.LoadScene(sceneName);
    }


    // Returns the correct sprite for each ingredient.
    private Sprite GetSprite(string ingredient)
    {
        switch (ingredient)
        {
            case "Herb":
                return herbSprite;

            case "Crystal":
                return crystalSprite;

            case "Mushroom":
                return mushroomSprite;

            case "Bone":
                return boneSprite;

            case "Feather":
                return featherSprite;

            default:
                Debug.LogWarning(
                    "[PotionGameManager] No sprite found for ingredient: " +
                    ingredient
                );

                return null;
        }
    }


    // Fisher-Yates shuffle.
    private void ShuffleList(List<string> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i, list.Count);

            string temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }


    // Removes old cards before starting a new round.
    private void ClearOldCards()
    {
        if (cardGrid == null)
        {
            return;
        }

        for (int i = cardGrid.childCount - 1; i >= 0; i--)
        {
            Destroy(cardGrid.GetChild(i).gameObject);
        }
    }
}