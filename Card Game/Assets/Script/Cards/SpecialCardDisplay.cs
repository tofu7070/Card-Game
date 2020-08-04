using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpecialCardDisplay : MonoBehaviour
{
    public int cardID;
    public SpecialCardData card;
    public Image artworkimage;
    public Image typeimage;
    public Text cardName;
    public Text cardDescription;
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
        cardName.text = card.name;
        cardDescription.text = card.description;
        func = card.function;
    }

}
