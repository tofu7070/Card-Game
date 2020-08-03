using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BriefCardDisplay : MonoBehaviour
{
    public int cardID;
    public CardData card;
    public Text attackText;
    public Image epicOutline;
    public Image artworkimage;
    public int attackDamage;
    public Image cardType;
    public CardDeck handCards;
    void Start()
    {
        //if(card != null)
        //InistiateCard();

    }

    public void InistiateCard()
    {
        cardID = card.cardID;
        artworkimage.sprite = card.artwork;
        cardType.sprite = card.typeImage;
        attackText.text = card.attack.ToString();
        attackDamage = card.attack;
        if (card.epic)
            epicOutline.color = Color.yellow;
    }

    public void OnCardSelected()
    {
        if (handCards != null)
        {
            handCards.HighLightCards(this.gameObject);
        }
    }
    
}
