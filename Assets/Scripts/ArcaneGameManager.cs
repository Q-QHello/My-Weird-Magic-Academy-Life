using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Controls the Arcane Academy Magic Glyph Tracing minigame.
//
// Rules:
//
// 1. Randomly generate nodes inside GlyphArea.
// 2. Randomly generate click order.
// 3. Show numbers on nodes for 3 seconds.
// 4. Hide numbers.
// 5. Player clicks nodes in memorized order.
// 6. Correct click:
//      node becomes completed
//      line appears between previous node and this node
// 7. Wrong click:
//      The spell exploded. You died.
//      Day +3
//      Load ResurrectionHallScene
// 8. Timer reaches 0:
//      failure
// 9. All nodes completed:
//      Arcane +10
//      Load ArcaneAcademy
public class ArcaneGameManager : MonoBehaviour
{
    [Header("UI References")]

    // Parent area where nodes and lines are generated.
    public RectTransform glyphArea;

    // Arcane node prefab.
    //
    // Required components:
    // - Image
    // - Button
    // - ArcaneNode
    // - child TextMeshProUGUI named NumberText
    public GameObject nodePrefab;

    // Main message text.
    public TextMeshProUGUI resultText;

    // Progress text.
    public TextMeshProUGUI progressText;

    // Timer text.
    public TextMeshProUGUI timerText;


    [Header("Node Visual Sprites")]

    // Normal node sprite.
    public Sprite nodeNormalSprite;

    // Completed node sprite.
    public Sprite nodeCompletedSprite;


    [Header("Preview Settings")]

    // The order numbers are shown for this many seconds.
    public float previewDuration = 3f;


    [Header("Semester Difficulty Settings")]

    public int semester1NodeCount = 5;
    public int semester2NodeCount = 7;
    public int semester3NodeCount = 10;

    public float semester1BaseTime = 15f;
    public float semester2BaseTime = 13f;
    public float semester3BaseTime = 10f;


    [Header("Arcane Stat Bonus Settings")]

    // Arcane > 100, > 200, > 300, > 400.
    public int arcaneThresholdStep = 100;

    // Maximum threshold counted.
    public int maxArcaneBonusThreshold = 400;

    // Extra seconds per threshold.
    public float bonusSecondsPerThreshold = 3f;


    [Header("Random Placement Settings")]

    // Visual size of generated node.
    public Vector2 nodeSize = new Vector2(80f, 80f);

    // Keeps nodes away from GlyphArea edge.
    public float areaPadding = 70f;

    // Keeps nodes from spawning too close to each other.
    public float minimumNodeDistance = 130f;

    // Attempts before fallback.
    public int maxPlacementAttempts = 100;


    [Header("Magic Line Settings")]

    // Thickness of generated lines.
    public float lineThickness = 8f;

    // Color of generated lines.
    public Color lineColor = new Color(0.3f, 0.85f, 1f, 0.9f);

    // Put lines behind nodes.
    public bool putLinesBehindNodes = true;


    [Header("Reward And Penalty Settings")]

    public int arcaneReward = 10;
    public int failureDayPenalty = 3;
    public float sceneChangeDelay = 1.5f;


    [Header("Debug Testing")]

    // Use this to test difficulty directly inside ArcaneMinigameScene.
    //
    // For real gameplay, keep this false.
    public bool useDebugSemester = false;

    [Range(1, 3)]
    public int debugSemester = 1;


    // Scene names.
    //
    // These must match Build Settings exactly.
    private const string ArcaneAcademySceneName = "ArcaneAcademy";
    private const string ResurrectionHallSceneName = "ResurrectionHallScene";


    // All generated nodes.
    private readonly List<ArcaneNode> allNodes = new List<ArcaneNode>();

    // Randomized click sequence.
    private readonly List<ArcaneNode> clickSequence = new List<ArcaneNode>();

    // Used positions for random placement.
    private readonly List<Vector2> usedPositions = new List<Vector2>();

    // Generated magic lines.
    private readonly List<GameObject> generatedLines = new List<GameObject>();

    // Current progress index.
    private int currentStep = 0;

    // Current semester.
    private int currentSemester = 1;

    // Current node count.
    private int currentNodeCount = 5;

    // Time limit after Arcane bonus.
    private float currentTimeLimit = 15f;

    // Remaining time.
    private float timeLeft = 15f;

    // True during the 3-second preview.
    private bool isPreviewing = false;

    // True after preview ends.
    private bool isPlaying = false;

    // True after success or failure.
    private bool gameOver = false;


    private void Start()
    {
        StartCoroutine(StartGameRoutine());
    }


    private void Update()
    {
        if (!isPlaying)
        {
            return;
        }

        if (gameOver)
        {
            return;
        }

        UpdateTimer();
    }


    // Full game startup.
    private IEnumerator StartGameRoutine()
    {
        ClearOldObjects();

        currentStep = 0;
        isPreviewing = true;
        isPlaying = false;
        gameOver = false;

        currentSemester = GetCurrentSemester();

        currentNodeCount = GetNodeCountFromSemester(currentSemester);

        currentTimeLimit =
            GetBaseTimeFromSemester(currentSemester) +
            CalculateArcaneBonusTime();

        timeLeft = currentTimeLimit;

        GenerateRandomNodes(currentNodeCount);
        GenerateRandomClickSequence();

        ShowPreviewNumbers();

        RefreshProgressText();
        RefreshTimerText();

        if (resultText != null)
        {
            resultText.text =
                "Memorize the glyph order!\n" +
                "Numbers disappear in " +
                previewDuration +
                " seconds.";
        }

        Debug.Log(
            "[ArcaneGameManager] Preview started. " +
            "Semester = " + currentSemester +
            ", Node Count = " + currentNodeCount +
            ", Time Limit = " + currentTimeLimit +
            ", Arcane = " + PlayerStats.arcane
        );

        yield return new WaitForSeconds(previewDuration);

        HidePreviewNumbers();
        EnableAllNodes();

        isPreviewing = false;
        isPlaying = true;

        if (resultText != null)
        {
            resultText.text = "Trace the glyph!";
        }

        Debug.Log("[ArcaneGameManager] Play phase started.");
    }


    // Gets semester from DayManager or debug setting.
    private int GetCurrentSemester()
    {
        if (useDebugSemester)
        {
            return debugSemester;
        }

        return DayManager.currentSemester;
    }


    // Returns number of nodes by semester.
    private int GetNodeCountFromSemester(int semester)
    {
        if (semester <= 1)
        {
            return semester1NodeCount;
        }

        if (semester == 2)
        {
            return semester2NodeCount;
        }

        return semester3NodeCount;
    }


    // Returns base time by semester.
    private float GetBaseTimeFromSemester(int semester)
    {
        if (semester <= 1)
        {
            return semester1BaseTime;
        }

        if (semester == 2)
        {
            return semester2BaseTime;
        }

        return semester3BaseTime;
    }


    // Calculates extra time from Arcane stat.
    //
    // Arcane > 100 -> +3s
    // Arcane > 200 -> +6s
    // Arcane > 300 -> +9s
    // Arcane > 400 -> +12s
    private float CalculateArcaneBonusTime()
    {
        int arcaneStat = PlayerStats.arcane;

        int bonusTier = 0;

        for (int threshold = arcaneThresholdStep;
             threshold <= maxArcaneBonusThreshold;
             threshold += arcaneThresholdStep)
        {
            if (arcaneStat > threshold)
            {
                bonusTier++;
            }
        }

        return bonusTier * bonusSecondsPerThreshold;
    }


    // Generates random nodes.
    private void GenerateRandomNodes(int amount)
    {
        if (glyphArea == null)
        {
            Debug.LogError("[ArcaneGameManager] GlyphArea is missing.");
            return;
        }

        if (nodePrefab == null)
        {
            Debug.LogError("[ArcaneGameManager] NodePrefab is missing.");
            return;
        }

        allNodes.Clear();
        usedPositions.Clear();

        for (int i = 0; i < amount; i++)
        {
            Vector2 randomPosition = GetValidRandomPosition();

            GameObject nodeObject = Instantiate(nodePrefab, glyphArea);

            nodeObject.name = "ArcaneNode_" + (i + 1);

            RectTransform nodeRect =
                nodeObject.GetComponent<RectTransform>();

            if (nodeRect != null)
            {
                nodeRect.anchorMin = new Vector2(0.5f, 0.5f);
                nodeRect.anchorMax = new Vector2(0.5f, 0.5f);
                nodeRect.pivot = new Vector2(0.5f, 0.5f);
                nodeRect.sizeDelta = nodeSize;
                nodeRect.anchoredPosition = randomPosition;
            }

            ArcaneNode node =
                nodeObject.GetComponent<ArcaneNode>();

            if (node == null)
            {
                Debug.LogError(
                    "[ArcaneGameManager] NodePrefab is missing ArcaneNode component."
                );

                continue;
            }

            node.SetupNode(
                this,
                i,
                nodeNormalSprite,
                nodeCompletedSprite
            );

            allNodes.Add(node);
            usedPositions.Add(randomPosition);
        }
    }


    // Finds a random position that is not too close to existing nodes.
    private Vector2 GetValidRandomPosition()
    {
        Rect rect = glyphArea.rect;

        float minX = -rect.width * 0.5f + areaPadding;
        float maxX = rect.width * 0.5f - areaPadding;

        float minY = -rect.height * 0.5f + areaPadding;
        float maxY = rect.height * 0.5f - areaPadding;

        Vector2 chosenPosition = Vector2.zero;

        for (int attempt = 0; attempt < maxPlacementAttempts; attempt++)
        {
            float x = Random.Range(minX, maxX);
            float y = Random.Range(minY, maxY);

            chosenPosition = new Vector2(x, y);

            bool tooClose = false;

            foreach (Vector2 existingPosition in usedPositions)
            {
                float distance =
                    Vector2.Distance(chosenPosition, existingPosition);

                if (distance < minimumNodeDistance)
                {
                    tooClose = true;
                    break;
                }
            }

            if (!tooClose)
            {
                return chosenPosition;
            }
        }

        Debug.LogWarning(
            "[ArcaneGameManager] Could not find ideal node position. " +
            "Using fallback position."
        );

        return chosenPosition;
    }


    // Randomizes click sequence.
    private void GenerateRandomClickSequence()
    {
        clickSequence.Clear();

        foreach (ArcaneNode node in allNodes)
        {
            clickSequence.Add(node);
        }

        ShuffleList(clickSequence);
    }


    // Shows 1, 2, 3, 4... order numbers on nodes.
    private void ShowPreviewNumbers()
    {
        for (int i = 0; i < clickSequence.Count; i++)
        {
            ArcaneNode node = clickSequence[i];

            node.ShowPreviewNumber(i + 1);
            node.SetInteractable(false);
        }
    }


    // Hides all numbers.
    private void HidePreviewNumbers()
    {
        foreach (ArcaneNode node in allNodes)
        {
            node.HidePreviewNumber();
        }
    }


    // Enables node clicking after preview.
    private void EnableAllNodes()
    {
        foreach (ArcaneNode node in allNodes)
        {
            node.SetInteractable(true);
        }
    }


    // Called by ArcaneNode when clicked.
    public void OnArcaneNodeClicked(ArcaneNode clickedNode)
    {
        if (gameOver)
        {
            return;
        }

        if (isPreviewing)
        {
            return;
        }

        if (!isPlaying)
        {
            return;
        }

        if (clickedNode == null)
        {
            return;
        }

        if (currentStep < 0 || currentStep >= clickSequence.Count)
        {
            Debug.LogError(
                "[ArcaneGameManager] currentStep out of range. " +
                "currentStep = " + currentStep +
                ", clickSequence.Count = " + clickSequence.Count
            );

            return;
        }

        ArcaneNode expectedNode = clickSequence[currentStep];

        if (clickedNode == expectedNode)
        {
            HandleCorrectNode(clickedNode);
        }
        else
        {
            HandleFailure();
        }
    }


    // Handles correct click.
    private void HandleCorrectNode(ArcaneNode node)
    {
        if (currentStep > 0)
        {
            ArcaneNode previousNode = clickSequence[currentStep - 1];

            CreateMagicLineBetweenNodes(previousNode, node);
        }

        node.MarkCompleted();

        currentStep++;

        RefreshProgressText();

        if (currentStep >= clickSequence.Count)
        {
            HandleSuccess();
            return;
        }

        if (resultText != null)
        {
            resultText.text = "Correct!";
        }
    }


    // Creates an angled UI line between two nodes.
    private void CreateMagicLineBetweenNodes(ArcaneNode startNode, ArcaneNode endNode)
    {
        if (startNode == null || endNode == null)
        {
            return;
        }

        if (glyphArea == null)
        {
            return;
        }

        RectTransform startRect =
            startNode.GetComponent<RectTransform>();

        RectTransform endRect =
            endNode.GetComponent<RectTransform>();

        if (startRect == null || endRect == null)
        {
            return;
        }

        Vector2 startPosition = startRect.anchoredPosition;
        Vector2 endPosition = endRect.anchoredPosition;

        Vector2 direction = endPosition - startPosition;

        float rawDistance = direction.magnitude;

        if (rawDistance <= 0.01f)
        {
            return;
        }

        Vector2 normalizedDirection = direction.normalized;

        float startNodeRadius = startRect.rect.width * 0.5f;
        float endNodeRadius = endRect.rect.width * 0.5f;

        Vector2 lineStart =
            startPosition + normalizedDirection * startNodeRadius;

        Vector2 lineEnd =
            endPosition - normalizedDirection * endNodeRadius;

        Vector2 lineDirection = lineEnd - lineStart;

        float lineDistance = lineDirection.magnitude;

        if (lineDistance <= 0.01f)
        {
            return;
        }

        Vector2 middlePosition =
            (lineStart + lineEnd) * 0.5f;

        GameObject lineObject = new GameObject(
            "ArcaneMagicLine",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );

        lineObject.transform.SetParent(glyphArea, false);

        RectTransform lineRect =
            lineObject.GetComponent<RectTransform>();

        lineRect.anchorMin = new Vector2(0.5f, 0.5f);
        lineRect.anchorMax = new Vector2(0.5f, 0.5f);
        lineRect.pivot = new Vector2(0.5f, 0.5f);

        lineRect.anchoredPosition = middlePosition;

        lineRect.sizeDelta =
            new Vector2(lineDistance, lineThickness);

        float angle =
            Mathf.Atan2(lineDirection.y, lineDirection.x) *
            Mathf.Rad2Deg;

        lineRect.localRotation =
            Quaternion.Euler(0f, 0f, angle);

        Image lineImage =
            lineObject.GetComponent<Image>();

        if (lineImage != null)
        {
            lineImage.color = lineColor;
        }

        if (putLinesBehindNodes)
        {
            lineObject.transform.SetAsFirstSibling();
        }

        generatedLines.Add(lineObject);
    }


    // Updates timer.
    private void UpdateTimer()
    {
        timeLeft -= Time.deltaTime;

        if (timeLeft <= 0f)
        {
            timeLeft = 0f;
            RefreshTimerText();
            HandleFailure();
            return;
        }

        RefreshTimerText();
    }


    // Handles success.
    private void HandleSuccess()
    {
        gameOver = true;
        isPlaying = false;

        PlayerStats.arcane += arcaneReward;

        DisableAllNodes();

        if (resultText != null)
        {
            resultText.text = "Success! Arcane +" + arcaneReward;
        }

        Debug.Log(
            "[ArcaneGameManager] Success. Arcane +" +
            arcaneReward +
            ". Current Arcane = " +
            PlayerStats.arcane
        );

        StartCoroutine(LoadSceneAfterDelay(ArcaneAcademySceneName));
    }


    // Handles failure.
    private void HandleFailure()
    {
        if (gameOver)
        {
            return;
        }

        gameOver = true;
        isPlaying = false;

        DisableAllNodes();

        if (resultText != null)
        {
            resultText.text = "The spell exploded. You died.";
        }

        bool graduationTriggered = AddFailureDayPenalty();

        if (graduationTriggered)
        {
            return;
        }

        StartCoroutine(LoadSceneAfterDelay(ResurrectionHallSceneName));
    }


    // Adds failure day penalty.
    private bool AddFailureDayPenalty()
    {
        bool graduationTriggered =
            AdvanceDaysWithSemesterRollOver(failureDayPenalty);

        DailyActionManager.ForceResetForCurrentDay();

        Debug.Log(
            "[ArcaneGameManager] Failure penalty applied. " +
            "Current Semester = " +
            DayManager.currentSemester +
            ", Current Day = " +
            DayManager.currentDay
        );

        return graduationTriggered;
    }


    // Advances days with semester rollover.
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


    // Disables all nodes.
    private void DisableAllNodes()
    {
        foreach (ArcaneNode node in allNodes)
        {
            node.SetInteractable(false);
        }
    }


    // Updates progress UI.
    private void RefreshProgressText()
    {
        if (progressText == null)
        {
            return;
        }

        progressText.text =
            "Semester: " +
            currentSemester +
            "\nProgress: " +
            currentStep +
            " / " +
            currentNodeCount;
    }


    // Updates timer UI.
    private void RefreshTimerText()
    {
        if (timerText == null)
        {
            return;
        }

        timerText.text =
            "Time: " +
            timeLeft.ToString("F1");
    }


    // Loads scene after delay.
    private IEnumerator LoadSceneAfterDelay(string sceneName)
    {
        yield return new WaitForSeconds(sceneChangeDelay);

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError(
                "[ArcaneGameManager] Scene cannot be loaded: " +
                sceneName +
                ". Check Build Settings and scene name spelling."
            );

            yield break;
        }

        SceneManager.LoadScene(sceneName);
    }


    // Clears old nodes and lines.
    private void ClearOldObjects()
    {
        if (glyphArea == null)
        {
            return;
        }

        for (int i = glyphArea.childCount - 1; i >= 0; i--)
        {
            Destroy(glyphArea.GetChild(i).gameObject);
        }

        allNodes.Clear();
        clickSequence.Clear();
        usedPositions.Clear();
        generatedLines.Clear();
    }


    // Fisher-Yates shuffle.
    private void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i, list.Count);

            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}