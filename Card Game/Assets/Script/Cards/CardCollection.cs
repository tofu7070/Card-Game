using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = System.Diagnostics.Debug;

public class CardCollection : MonoBehaviour
{
    public static readonly string Save_Address = "Sace_Address";
    public int unitCardNum;
    public int specialCardNum;

    [System.Serializable]
    public class AllCardCollection//所有收集的卡片 通过卡片的ID存放
    {
        public CardSavingData[] typeA = new CardSavingData[0];
        public CardSavingData[] typeS = new CardSavingData[0];
        //后续添加其他阵营的牌组
    }

    public AllCardCollection allCardCollection;

    [System.Serializable]
    public class CurrentCardDeck
    {
        public CardSavingData[] typeA = new CardSavingData[0];
        public CardSavingData[] typeS = new CardSavingData[0];
    }

    public CurrentCardDeck currentCardDeck;

    private List<int> yourCards = new List<int>();//现有可以使用的牌

    public List<int> getYourCard
    {
        get
        {
            return yourCards;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        InitializeHandCard(0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    #region JsonFile

    public void SaveJson()
    {
        SaveCardFile safeFile = new SaveCardFile();
        safeFile.typeA = allCardCollection.typeA;
        safeFile.typeS = allCardCollection.typeS;
        safeFile.currentA = currentCardDeck.typeA;
        safeFile.currentS = currentCardDeck.typeS;
        string json = JsonUtility.ToJson(safeFile);
        PlayerPrefs.SetString(Save_Address,json);
    }

    public void ReadJson()
    {
        string json = PlayerPrefs.GetString(Save_Address, "Null");
        if (json == "Null")
        {
            //这里给初始的数据赋值
            SaveJson();
        }
        else
        {
            SaveCardFile safeFile = JsonUtility.FromJson<SaveCardFile>(json);
            allCardCollection.typeA = safeFile.typeA;
            allCardCollection.typeS = safeFile.typeS;
            currentCardDeck.typeA = safeFile.currentA;
            currentCardDeck.typeS = safeFile.currentS;
        }
        
    }
    

    #endregion

    public void InitializeHandCard(int num)
    {
        switch (num)
        {
            case 0:
                InitializeYourCards(currentCardDeck.typeA, currentCardDeck.typeS);
                break;
            default:
                InitializeYourCards(currentCardDeck.typeA, currentCardDeck.typeS);
                break;
        }
    }
    public void InitializeYourCards(CardSavingData[] unit, CardSavingData[] special)//初始化当前可以使用的牌
    {
        yourCards.Clear();
        foreach (var card in special)//添加特殊牌
        {
            int num = card.cardStack;
            for (int i = 0; i < num; i++)
            {
                yourCards.Add(card.cardID);
            }
        }
        foreach (var card in unit)//添加单位牌
        {
            int num = card.cardStack;
            for (int i = 0; i < num; i++)
            {
                yourCards.Add(card.cardID);
            }
        }
    }

    public int GenerateACard(List<int> deck)
    {
        int len = deck.Count;
        if (len > 0)
        {
            //int rnd = Random.Range(0, len);
            int rnd = Random.Range(0, 100);
            rnd = rnd % (len - 1);
            int result = deck[rnd];
            deck.RemoveAt(rnd);
            return result;
        }
        else
            return -1;
    }
    [System.Serializable]
    private class SaveCardFile
    {
        //存放的卡牌
        public CardSavingData[] typeA = new CardSavingData[0];
        public CardSavingData[] typeS = new CardSavingData[0];
        //手牌
        public CardSavingData[] currentA = new CardSavingData[0];
        public CardSavingData[] currentS = new CardSavingData[0];
    }
}
