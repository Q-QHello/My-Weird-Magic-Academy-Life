using UnityEngine;
using UnityEngine.UI;

// Represents a single card in the potion memory game
public class PotionCard : MonoBehaviour
{
// Ingredient stored in this card
public string ingredientName;


// Image displayed on the card
public Image ingredientImage;

// Original ingredient sprite
private Sprite ingredientSprite;

// Card back sprite
private Sprite cardBackSprite;

// Reference to the game manager
private PotionGameManager gameManager;

// Whether this card is currently revealed
private bool revealed = false;

// Sets ingredient and sprite
public void SetupCard(
    string ingredient,
    Sprite sprite,
    Sprite backSprite,
    PotionGameManager manager)
{
    ingredientName = ingredient;

    ingredientSprite = sprite;

    cardBackSprite = backSprite;

    gameManager = manager;

    ingredientImage.sprite = ingredientSprite;
}

// Shows ingredient
public void Reveal()
{
    ingredientImage.sprite = ingredientSprite;

    revealed = true;
}

// Shows card back
public void Hide()
{
    ingredientImage.sprite = cardBackSprite;

    revealed = false;
}

// Called when the player clicks this card
public void OnCardClicked()
{
    // Ignore clicks on already revealed cards
    if (revealed)
    {
        return;
    }

    Reveal();

    gameManager.CheckCard(this);
}

}
