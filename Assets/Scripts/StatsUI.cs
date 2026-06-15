using TMPro;
using UnityEngine;

// Updates the player statistics shown on the UI
public class StatsUI : MonoBehaviour
{
public TextMeshProUGUI arcaneText;
public TextMeshProUGUI potionText;
public TextMeshProUGUI runeText;


void Update()
{
    arcaneText.text = "Arcane: " + PlayerStats.arcane;
    potionText.text = "Potion: " + PlayerStats.potion;
    runeText.text = "Rune: " + PlayerStats.rune;
}


}
