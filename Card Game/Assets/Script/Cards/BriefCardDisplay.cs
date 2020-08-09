using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BriefCardDisplay : MonoBehaviour
{
    public int cardID;
    public CardData card;
    public bool epic;
    public Text attackText;
    public Image epicOutline;
    public Image artworkimage;
    public int attackDamage;
    public Image cardType;
    public CardData.Skills skills;
    public GameObject skillImage;

    public CardDeck handCards;
    //增幅的变量
    private Color defaultTextColor;
    public bool isPowerUp;
    public int stack = 1;//用于合作卡片堆叠使用
    private int currentStack = 1;
    //卡片动画效果
    public Animator numberAnimator;
    public Animator skillAnimator;
    public GameObject numberPopObj;
    public TextMeshProUGUI numberPopText;
    void Start()
    {
        //if(card != null)
        //InistiateCard();

    }

    public void InistiateCard()
    {
        defaultTextColor = attackText.color;
        cardID = card.cardID;
        epic = card.epic;
        artworkimage.sprite = card.artwork;
        cardType.sprite = card.typeImage;
        attackText.text = card.attack.ToString();
        attackDamage = card.attack;
        skills = card.skill;
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

    public void OnCardSelected()
    {
        if (handCards != null)
        {
            handCards.HighLightCards(this.gameObject);
        }
    }

    public void RefreshCardText()
    {
        if (isPowerUp && !epic)
        {
            attackText.text = (attackDamage * 2 * stack).ToString();
            attackText.color = Color.green;
        }
        else
        {
            attackText.text = (attackDamage * stack).ToString();
            attackText.color = defaultTextColor;
        }
    }

    public void DeBuff()
    {
        if (!epic)
        {
            attackDamage = 1 * stack;
            attackText.text = attackDamage.ToString();
            attackText.color = Color.red;
        }
    }

    public void PlayNumberPopAnimation()
    {
        if (stack > currentStack)
        {
            currentStack = stack;
            string temp = "×" + stack;
            numberPopText.text = temp;
            numberPopObj.SetActive(true);
            numberAnimator.SetTrigger("PlayPop");
            Invoke("StopPlayNumberPopAnimation",1f);
            Invoke("RefreshCardText",1f);
        }
        
    }

    public void StopPlayNumberPopAnimation()
    {
        numberPopObj.SetActive(false);
    }

    public void PlaySummonAnimation()
    {
        skillAnimator.SetTrigger("PlaySummon");
    }
}
