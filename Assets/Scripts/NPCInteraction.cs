using UnityEngine;

// Handles interaction with the teacher NPC
public class NPCInteraction : MonoBehaviour
{
// Reference to the dialogue panel
public GameObject dialoguePanel;


// Checks whether the player is near the NPC
private bool playerInRange = false;

void Update()
{
    // Open dialogue when the player presses E
    if (playerInRange && Input.GetKeyDown(KeyCode.E))
    {
        dialoguePanel.SetActive(true);
    }
}

private void OnTriggerEnter2D(Collider2D other)
{
    // Detect player entering interaction range
    if (other.CompareTag("Player"))
    {
        playerInRange = true;
    }
}

private void OnTriggerExit2D(Collider2D other)
{
    // Detect player leaving interaction range
    if (other.CompareTag("Player"))
    {
        playerInRange = false;
    }
}


}
