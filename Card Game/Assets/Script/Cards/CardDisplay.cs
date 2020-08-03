using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardDisplay : MonoBehaviour
{
    public CardData card;
    public Text nameText;
    public Text descriptionText;
    public Text typeText;
    public Text attackText;
    public Image epicOutline;
    public Image artworkimage;
    public Image cardType;

    void Start()
    {
        //if(card != null)
            //InistiateCard();
        
    }
    
    public void InistiateCard()
    {
        nameText.text = card.name;
        descriptionText.text = card.description;
        artworkimage.sprite = card.artwork;
        cardType.sprite = card.typeImage;
        attackText.text = card.attack.ToString();
        if (card.epic)
            epicOutline.color = Color.yellow;
    }
}
