using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Controls one clickable arcane node.
//
// This script is attached to ArcaneNodePrefab.
//
// Each node contains:
// - Image component
// - Button component
// - NumberText child
//
// The node does not judge correct or wrong.
// It only reports clicks to ArcaneGameManager.
public class ArcaneNode : MonoBehaviour
{
    [Header("UI References")]

    // The Image component of this node.
    public Image nodeImage;

    // The Button component of this node.
    public Button nodeButton;

    // The number shown during preview phase.
    //
    // Example:
    // 1
    // 2
    // 3
    public TextMeshProUGUI numberText;


    [Header("Visual Settings")]

    // Sprite for normal / uncompleted node.
    public Sprite normalSprite;

    // Sprite for completed node.
    public Sprite completedSprite;

    // Fallback color if normalSprite is not assigned.
    public Color normalColor = Color.white;

    // Fallback color if completedSprite is not assigned.
    public Color completedColor = Color.yellow;


    // Index assigned by ArcaneGameManager.
    public int nodeIndex;

    // Reference to the main game manager.
    private ArcaneGameManager gameManager;

    // Prevents completed nodes from being clicked again.
    private bool isCompleted = false;


    private void Awake()
    {
        if (nodeImage == null)
        {
            nodeImage = GetComponent<Image>();
        }

        if (nodeButton == null)
        {
            nodeButton = GetComponent<Button>();
        }

        if (numberText == null)
        {
            numberText = GetComponentInChildren<TextMeshProUGUI>();
        }
    }


    // Called by ArcaneGameManager after this node is instantiated.
    public void SetupNode(
        ArcaneGameManager newGameManager,
        int newNodeIndex,
        Sprite newNormalSprite,
        Sprite newCompletedSprite)
    {
        gameManager = newGameManager;
        nodeIndex = newNodeIndex;

        normalSprite = newNormalSprite;
        completedSprite = newCompletedSprite;

        isCompleted = false;

        SetNormalVisual();
        HidePreviewNumber();

        if (nodeButton != null)
        {
            nodeButton.onClick.RemoveAllListeners();
            nodeButton.onClick.AddListener(OnNodeClicked);
            nodeButton.interactable = false;
        }
    }


    // Called when the player clicks this node.
    private void OnNodeClicked()
    {
        if (gameManager == null)
        {
            return;
        }

        if (isCompleted)
        {
            return;
        }

        gameManager.OnArcaneNodeClicked(this);
    }


    // Shows the order number during preview.
    public void ShowPreviewNumber(int sequenceNumber)
    {
        if (numberText != null)
        {
            numberText.gameObject.SetActive(true);
            numberText.text = sequenceNumber.ToString();
        }
    }


    // Hides the preview number.
    public void HidePreviewNumber()
    {
        if (numberText != null)
        {
            numberText.gameObject.SetActive(false);
        }
    }


    // Enables or disables clicking.
    public void SetInteractable(bool value)
    {
        if (nodeButton != null)
        {
            nodeButton.interactable = value;
        }
    }


    // Called when the player clicks this node correctly.
    public void MarkCompleted()
    {
        isCompleted = true;

        if (nodeButton != null)
        {
            nodeButton.interactable = false;
        }

        if (nodeImage != null)
        {
            if (completedSprite != null)
            {
                nodeImage.sprite = completedSprite;
                nodeImage.color = Color.white;
            }
            else
            {
                nodeImage.color = completedColor;
            }
        }

        HidePreviewNumber();
    }


    // Restores normal visual state.
    private void SetNormalVisual()
    {
        if (nodeImage == null)
        {
            return;
        }

        if (normalSprite != null)
        {
            nodeImage.sprite = normalSprite;
            nodeImage.color = Color.white;
        }
        else
        {
            nodeImage.color = normalColor;
        }
    }
}