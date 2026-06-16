using UnityEngine;

// Controls the dialogue panel
public class DialogueController : MonoBehaviour
{
// Closes the dialogue panel
public void CloseDialogue()
{
gameObject.SetActive(false);
}
}
