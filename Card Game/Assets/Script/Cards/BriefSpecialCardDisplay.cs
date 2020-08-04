using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BriefSpecialCardDisplay : MonoBehaviour
{
    public int cardID;
    public SpecialCardData card;
    public Image artworkimage;
    public Image typeimage;
    public CardDeck handCards;
    public SpecialCardData.Function func;
    void Start()
    {


    }

    public void InistiateCard()
    {
        cardID = card.cardID;
        artworkimage.sprite = card.artwork;
        typeimage.sprite = card.typeImage;
    }

    public void OnCardSelected()
    {
        if (handCards != null)
        {
            handCards.HighLightCards(this.gameObject);
        }
    }
}
