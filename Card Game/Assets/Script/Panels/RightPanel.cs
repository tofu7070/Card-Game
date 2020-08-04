using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RightPanel : MonoBehaviour
{
    public GameObject cardPrefabs;
    public GameObject specialCardPrefabs;

    public List<int> deckCards;

    public List<int> graveCards;
    // Start is called before the first frame update
    void Start()
    {
        HideCardInfo();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowCardInfo(CardData data)//这个为单位牌
    {
        CardDisplay CD = cardPrefabs.GetComponent<CardDisplay>();
        CD.card = data;
        CD.InistiateCard();
        cardPrefabs.SetActive(true);
    }

    public void ShowCardInfo(SpecialCardData data)//这个为特殊牌
    {
        SpecialCardDisplay SCD = specialCardPrefabs.GetComponent<SpecialCardDisplay>();
        SCD.card = data;
        SCD.InistiateCard();
        specialCardPrefabs.SetActive(true);
    }

    public void HideCardInfo()
    {
        cardPrefabs.SetActive(false);
        specialCardPrefabs.SetActive(false);
    }
}
