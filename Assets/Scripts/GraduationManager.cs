using UnityEngine;
using UnityEngine.SceneManagement;

// Handles graduation evaluation and ending selection
public class GraduationManager : MonoBehaviour
{
// Checks whether the player graduates successfully
public static void CheckGraduation()
{
int totalStats =
PlayerStats.arcane +
PlayerStats.potion +
PlayerStats.rune;


    // Graduation requirement
    if (totalStats >= 400)
    {
        SceneManager.LoadScene("GoodEndingScene");
    }
    else
    {
        SceneManager.LoadScene("BadEndingScene");
    }
}


}
