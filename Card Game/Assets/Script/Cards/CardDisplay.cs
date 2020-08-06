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
    public CardData.Skills skills;
    public GameObject skillImage;


    void Start()
    {
        
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
        if (skills != CardData.Skills.None)
        {
            skillImage.SetActive(true);//显示技能并且赋给技能图标
            skillImage.transform.Find("Image").GetComponent<Image>().sprite = card.skillImage;
        }
        else
        {
            skillImage.SetActive(false);
        }
    }
}
