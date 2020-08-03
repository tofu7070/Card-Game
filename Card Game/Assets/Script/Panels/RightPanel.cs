using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RightPanel : MonoBehaviour
{
    public GameObject cardPrefabs;

    public List<int> deckCards;

    public List<int> graveCards;
    // Start is called before the first frame update
    void Start()
    {
        cardPrefabs.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowCardInfo(CardData data)
    {
        CardDisplay CD = cardPrefabs.GetComponent<CardDisplay>();
        CD.card = data;
        CD.InistiateCard();
        cardPrefabs.SetActive(true);
    }

    public void HideCardInfo()
    {
        cardPrefabs.SetActive(false);
    }
}
