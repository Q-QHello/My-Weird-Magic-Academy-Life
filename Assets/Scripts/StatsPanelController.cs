using UnityEngine;

// Controls opening and closing the statistics panel
public class StatsPanelController : MonoBehaviour
{
// Reference to the statistics panel
public GameObject statsPanel;


// Called when the Stats button is clicked
public void OpenStatsPanel()
{
    statsPanel.SetActive(true);
}

// Called when the Close button is clicked
public void CloseStatsPanel()
{
    statsPanel.SetActive(false);
}


}
