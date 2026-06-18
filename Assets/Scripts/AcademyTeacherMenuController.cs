using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Controls the teacher dialogue menu.
//
// This script should be attached to a manager object in the academy scene.
// Example:
//
// PotionAcademy Scene
// └── PotionManager
//     └── AcademyTeacherMenuController
//
// This script controls three buttons:
//
// 1. StudyButton
//    - Consumes 1 daily action.
//    - Shows the LearningPanel / standby study image.
//    - Waits for a short duration.
//    - Adds academy stat.
//    - Hides the LearningPanel.
//
// 2. TestButton
//    - Consumes 1 daily action.
//    - Only works on Day 10 / 20 / 30...
//    - Only works once per day.
//    - Loads the assigned minigame scene.
//    - Does NOT show the LearningPanel.
//
// 3. LeaveButton
//    - Closes the DialoguePanel.
//
// Important:
// If clicking Test shows the standby image, then your Inspector references
// are probably connected incorrectly:
//
// Correct:
//   Learn Button = StudyButton
//   Test Button  = TestButton
//
// Wrong:
//   Learn Button = TestButton
//   Test Button  = StudyButton
public class AcademyTeacherMenuController : MonoBehaviour
{
    // Defines which academy this teacher belongs to.
    //
    // This allows the same script to be reused for:
    // - Arcane teacher
    // - Potion teacher
    // - Rune teacher
    public enum AcademyType
    {
        Arcane,
        Potion,
        Rune
    }


    [Header("Academy Settings")]

    // Choose the academy type in the Inspector.
    //
    // Potion teacher:
    //   Academy Type = Potion
    //
    // Arcane teacher:
    //   Academy Type = Arcane
    //
    // Rune teacher:
    //   Academy Type = Rune
    public AcademyType academyType = AcademyType.Potion;

    // Teacher name shown in dialogue text.
    //
    // Example:
    // Professor Willion
    public string teacherName = "Professor Willion";

    // Amount of stat gained from one Study action.
    //
    // Example:
    // Study Potion -> Potion +5
    public int learnStatGain = 5;

    // Exact scene name of the minigame scene.
    //
    // This must match the scene name in:
    // File -> Build Settings -> Scenes In Build
    //
    // Example:
    // PotionMinigameScene
    public string testSceneName = "PotionMinigameScene";


    [Header("Study Visual Settings")]

    // Panel shown when the player clicks Study.
    //
    // This should be your standby / learning image panel.
    //
    // Example hierarchy:
    //
    // Canvas
    // └── LearningPanel
    //     └── Image
    //
    // The image inside LearningPanel can be:
    // - open magic book
    // - potion cauldron
    // - rune workbench
    // - "learning..." screen
    public GameObject learningPanel;

    // How long the LearningPanel stays visible.
    //
    // Example:
    // 2 seconds
    public float studyDuration = 2f;

    // If true, the dialogue menu will open again after studying.
    //
    // Recommended for your current game:
    // false
    //
    // Because after studying, the player usually returns to normal gameplay.
    public bool reopenDialogueAfterStudy = false;


    [Header("UI References")]

    // Main dialogue panel.
    //
    // Example hierarchy:
    //
    // Canvas
    // └── DialoguePanel
    public GameObject dialoguePanel;

    // Text inside DialoguePanel.
    //
    // Used to show:
    // - teacher message
    // - action limit warning
    // - test unavailable warning
    public TextMeshProUGUI dialogueText;

    // Optional text showing action information.
    //
    // Example:
    // Actions left today: 2 / 3
    // Next test day: Day 10
    //
    // If you do not want this text, you can leave it empty in Inspector.
    public TextMeshProUGUI actionInfoText;

    // Study button.
    //
    // This must be the real StudyButton object.
    public Button learnButton;

    // Test button.
    //
    // This must be the real TestButton object.
    public Button testButton;

    // Leave button.
    //
    // This must be the real LeaveButton object.
    public Button leaveButton;


    // Prevents the player from clicking Study multiple times
    // while the study image is already showing.
    private bool isStudying = false;


    private void Awake()
    {
        // Bind buttons as early as possible.
        //
        // This helps prevent old Inspector OnClick events from causing bugs.
        BindButtons();
    }


    private void Start()
    {
        // Make sure the learning image is hidden when the scene starts.
        //
        // If LearningPanel is active by default in the scene,
        // it would appear immediately when entering the academy.
        if (learningPanel != null)
        {
            learningPanel.SetActive(false);
        }

        // Refresh the dialogue text and button states at scene start.
        RefreshMenu();
    }


    private void OnEnable()
    {
        // Rebind buttons when this object becomes enabled.
        //
        // This is useful if the manager object is disabled/enabled during gameplay.
        BindButtons();

        // Refresh UI when enabled.
        RefreshMenu();
    }


    private void Update()
    {
        // Keep button states updated.
        //
        // Example:
        // If the player uses all 3 actions today,
        // StudyButton and TestButton should become non-interactable.
        RefreshButtonsOnly();
    }


    // Connects all buttons to this script.
    //
    // RemoveAllListeners() is important.
    //
    // Why?
    // Because Unity buttons may still have old OnClick events in the Inspector.
    //
    // Example bad old events:
    // - StudyActivity.StartStudy()
    // - Old StudyPotion()
    // - Old LearnPotion()
    //
    // Those can cause:
    // - Study adding stats twice
    // - Test showing the study image
    // - Day increasing incorrectly
    //
    // This method clears old listeners and assigns the correct ones.
    private void BindButtons()
    {
        if (learnButton != null)
        {
            learnButton.onClick.RemoveAllListeners();
            learnButton.onClick.AddListener(OnClickLearn);
        }

        if (testButton != null)
        {
            testButton.onClick.RemoveAllListeners();
            testButton.onClick.AddListener(OnClickTest);
        }

        if (leaveButton != null)
        {
            leaveButton.onClick.RemoveAllListeners();
            leaveButton.onClick.AddListener(OnClickLeave);
        }
    }


    // Opens the teacher dialogue menu.
    //
    // Call this when the player talks to the teacher.
    //
    // Example:
    // Player presses E near the teacher.
    public void OpenMenu()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }

        RefreshMenu();
    }


    // Closes the teacher dialogue menu.
    public void CloseMenu()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }


    // Called when the StudyButton is clicked.
    //
    // Correct flow:
    //
    // StudyButton clicked
    // ↓
    // Check daily action limit
    // ↓
    // If allowed, consume 1 daily action
    // ↓
    // Start StudyRoutine()
    //
    // StudyRoutine() is where the LearningPanel is shown.
    public void OnClickLearn()
    {
        // Prevent double-clicking while study is already running.
        if (isStudying)
        {
            return;
        }

        string message;

        // Ask DailyActionManager whether Study is allowed today.
        //
        // This enforces:
        // Learn + Test <= 3 times per day.
        bool allowed = DailyActionManager.TryUseLearnAction(out message);

        // If the player has no actions left,
        // show the warning message and stop.
        if (!allowed)
        {
            ShowMessage(message);
            RefreshMenu();
            return;
        }

        // If allowed, start the study visual process.
        StartCoroutine(StudyRoutine());
    }


    // Handles the actual study visual process.
    //
    // This is the function that shows the standby / learning image.
    //
    // Flow:
    //
    // 1. Set isStudying = true
    // 2. Hide DialoguePanel
    // 3. Show LearningPanel
    // 4. Wait studyDuration seconds
    // 5. Add stat
    // 6. Hide LearningPanel
    // 7. Optional: reopen DialoguePanel
    private IEnumerator StudyRoutine()
    {
        isStudying = true;

        // Hide the teacher dialogue while studying.
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        // Show the standby / learning image.
        //
        // If this does not appear, check Inspector:
        // AcademyTeacherMenuController -> Learning Panel
        // must be assigned to Canvas/LearningPanel.
        if (learningPanel != null)
        {
            learningPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning(
                "[AcademyTeacherMenuController] LearningPanel is not assigned. " +
                "Study works, but no standby image will appear."
            );
        }

        // Keep the learning image visible for a short time.
        yield return new WaitForSeconds(studyDuration);

        // Add the corresponding academy stat.
        AddAcademyStat(learnStatGain);

        // Hide the standby / learning image.
        if (learningPanel != null)
        {
            learningPanel.SetActive(false);
        }

        isStudying = false;

        // Optional:
        // If you want the dialogue menu to return after studying,
        // enable reopenDialogueAfterStudy in Inspector.
        if (reopenDialogueAfterStudy)
        {
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(true);
            }

            ShowMessage(
                "Study completed.\n" +
                GetAcademyDisplayName() +
                " +" +
                learnStatGain +
                "\nActions left today: " +
                DailyActionManager.GetActionsLeft()
            );
        }

        RefreshMenu();

        Debug.Log(
            "[AcademyTeacherMenuController] Study completed. " +
            GetAcademyDisplayName() +
            " +" +
            learnStatGain +
            ". Actions left today = " +
            DailyActionManager.GetActionsLeft()
        );
    }


    // Called when the TestButton is clicked.
    //
    // Important:
    // Test should NOT show LearningPanel.
    //
    // If clicking Test shows LearningPanel, then:
    // 1. TestButton is assigned to Learn Button by mistake, or
    // 2. TestButton still has an old Inspector OnClick event.
    public void OnClickTest()
    {
        // Scene name cannot be empty.
        if (string.IsNullOrEmpty(testSceneName))
        {
            ShowMessage("Test scene name is missing.");
            return;
        }

        // Check whether the scene can actually be loaded.
        //
        // This prevents consuming a daily test action if:
        // - scene name is wrong
        // - scene is not added to Build Settings
        if (!Application.CanStreamedLevelBeLoaded(testSceneName))
        {
            ShowMessage(
                "Test scene cannot be loaded.\n" +
                "Check Build Settings:\n" +
                testSceneName
            );

            Debug.LogError(
                "[AcademyTeacherMenuController] Scene cannot be loaded: " +
                testSceneName
            );

            return;
        }

        string message;

        // Ask DailyActionManager whether Test is allowed today.
        //
        // This enforces:
        // - Test only on Day 10 / 20 / 30...
        // - Test only once per day
        // - Learn + Test <= 3 times per day
        bool allowed = DailyActionManager.TryUseTestAction(out message);

        if (!allowed)
        {
            ShowMessage(message);
            RefreshMenu();
            return;
        }

        // Load the minigame scene.
        //
        // This does not increase day.
        // Day only changes when:
        // - player sleeps
        // - player fails the test and PotionGameManager applies penalty
        SceneManager.LoadScene(testSceneName);
    }


    // Called when LeaveButton is clicked.
    public void OnClickLeave()
    {
        CloseMenu();
    }


    // Refreshes the whole teacher menu.
    //
    // This updates:
    // - dialogue text
    // - action info text
    // - button interactability
    private void RefreshMenu()
    {
        DailyActionManager.ResetIfNewDay();

        if (dialogueText != null)
        {
            dialogueText.text =
                teacherName +
                ":\n\n" +
                "What would you like to do?";
        }

        RefreshActionInfo();
        RefreshButtonsOnly();
    }


    // Updates the optional action info text.
    private void RefreshActionInfo()
    {
        if (actionInfoText == null)
        {
            return;
        }

        actionInfoText.text =
            "Actions left today: " +
            DailyActionManager.GetActionsLeft() +
            " / " +
            DailyActionManager.maxActionsPerDay +
            "\nNext test day: Day " +
            DailyActionManager.GetNextTestDay();
    }


    // Updates button interactability only.
    //
    // StudyButton is enabled when:
    // - player still has daily actions left
    // - player is not currently studying
    //
    // TestButton is enabled when:
    // - player still has daily actions left
    // - today is Day 10 / 20 / 30...
    // - test has not already been used today
    // - player is not currently studying
    private void RefreshButtonsOnly()
    {
        DailyActionManager.ResetIfNewDay();

        if (learnButton != null)
        {
            learnButton.interactable =
                DailyActionManager.HasActionLeft() &&
                isStudying == false;
        }

        if (testButton != null)
        {
            testButton.interactable =
                DailyActionManager.HasActionLeft() &&
                DailyActionManager.IsTestDay() &&
                DailyActionManager.testUsedToday == false &&
                isStudying == false;
        }

        RefreshActionInfo();
    }


    // Adds stat based on academy type.
    //
    // Potion teacher:
    //   PlayerStats.potion += amount
    //
    // Arcane teacher:
    //   PlayerStats.arcane += amount
    //
    // Rune teacher:
    //   PlayerStats.rune += amount
    private void AddAcademyStat(int amount)
    {
        switch (academyType)
        {
            case AcademyType.Arcane:
                PlayerStats.arcane += amount;
                break;

            case AcademyType.Potion:
                PlayerStats.potion += amount;
                break;

            case AcademyType.Rune:
                PlayerStats.rune += amount;
                break;
        }
    }


    // Returns academy display name for messages.
    private string GetAcademyDisplayName()
    {
        switch (academyType)
        {
            case AcademyType.Arcane:
                return "Arcane";

            case AcademyType.Potion:
                return "Potion";

            case AcademyType.Rune:
                return "Rune";

            default:
                return "Unknown";
        }
    }


    // Shows a message inside DialogueText.
    private void ShowMessage(string message)
    {
        if (dialogueText != null)
        {
            dialogueText.text = message;
        }
    }
}