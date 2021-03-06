﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using Random = UnityEngine.Random;

/// <summary>
/// 功能为：生成手牌
/// 凸显手牌，显示手牌详细信息
/// </summary>
public class CardDeck : MonoBehaviour
{
    public List<int> handCardsID = new List<int>();//手牌
    public List<int> graveCardID = new List<int>();//墓地牌
    public List<GameObject> handCardsObj = new List<GameObject>();

    public GameObject cardPrefabs;
    public GameObject specialCardPrefabs;
   
    public Transform handTransform;

    private GridLayoutGroup layout;

    private int handCardNumber = 0;

    private float oriYPosition = 0; //手牌的y轴位置 用于在非选择状态的时候回原

    private int selectedIndex = 100;//选择状态时的手牌指针

    private RightPanel rp;// 右边ui界面的reference

    private TablePanel tp;//桌面的卡牌

    private CardCollection cc;//所有卡牌信息

    public GameInfoPanel GIP;
    // Start is called before the first frame update
    void Start()
    {
        layout = this.GetComponent<GridLayoutGroup>();
        rp = FindObjectOfType<RightPanel>().GetComponent<RightPanel>();
        tp = FindObjectOfType<TablePanel>().GetComponent<TablePanel>();
        tp.cd = this;
        cc = FindObjectOfType<CardCollection>().GetComponent<CardCollection>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))// 测试功能 生成随机的卡片
        {
            UnitCard();
        }

        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            SpecialCard();
        }
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            GeneraterCard(6);
            StartCoroutine(EndFixcardSpacing());
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))//测试功能
        {
            Debug.Log("Initialize Handcards");
            InitilizeHandCards();
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SortHandCard();
        }
    }

    public void UnitCard()//测试功能
    {
        int num = Random.Range(1, 6);
        //Debug.Log("Generate a card " + num.ToString());
        GeneraterCard(num);
        StartCoroutine(EndFixcardSpacing());
    }

    public void SpecialCard()//Start with 900
    {
        //Debug.Log("Generate a special card ");
        GeneraterCard(901);
        StartCoroutine(EndFixcardSpacing());
    }

    public void GeneraterCard(int ID)
    {
        string cardID = "Card" + ID.ToString();
        if (ID >= 900)
        {
            LoadSpecialCard(cardID);
        }
        else
        {
            LoadCard(cardID);
        }
    }

    #region 初始化

    public void InitilizeHandCards()
    {
        handCardsID.Clear();
        for (int i = 0; i < 10; i++)
        {
            //初始化10张手牌
            int ID = cc.GenerateACard(cc.getYourCard);
            if(ID != -1)
                GeneraterCard(ID);
        }
    }
    

    #endregion

    public void SortHandCard() //将手牌排序，特殊牌-单位牌-史诗牌(根据战斗力排序)
    {
        //需要使用 handcardObj
        int num = handCardsObj.Count;
        Dictionary<GameObject,int> cardDic = new Dictionary<GameObject, int>();//存放卡片的物件和该物件的权重
        for (int i = 0; i < num; i++)
        {
            BriefCardDisplay BCD = handCardsObj[i].GetComponent<BriefCardDisplay>();
            int weight = 0;
            if (BCD != null)
                weight = CardWeight(BCD);
            else
                weight = CardWeight(handCardsObj[i].GetComponent<BriefSpecialCardDisplay>());
            cardDic.Add(handCardsObj[i],weight);
        }
        //对dictionary排序
        var dicSort = cardDic.OrderBy(d => d.Value);
        //var dicSort = from objDic in cardDic orderby objDic.Value select objDic;
        int index = 0;
        foreach (var d in dicSort)
        {
            d.Key.transform.SetSiblingIndex(index);
            index++;
        }

    }
    public int CardWeight(BriefCardDisplay bcd)//计算卡片权重多少 用于卡片排序
    {
        int num = 100;//单位牌权重从100开始 放置在特殊牌后
        num += bcd.attackDamage;
        return num;
    }
    public int CardWeight(BriefSpecialCardDisplay bcd)//计算卡片权重多少 用于卡片排序
    {
        int num = bcd.cardID - 900;
        return num;
    }

    public void LoadCard(string cardID)//Using unity addressable to instantiate cards
    {
        Addressables.LoadAssetAsync<CardData>(cardID).Completed += OnLoadDone;

        void OnLoadDone(AsyncOperationHandle<CardData> handle)
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                GameObject card = Instantiate(cardPrefabs, handTransform);
                BriefCardDisplay CD = card.GetComponent<BriefCardDisplay>();
                CD.card = handle.Result;
                CD.InistiateCard();
                CD.handCards = this;
                handCardNumber++;
                handCardsID.Add(CD.cardID);
                handCardsObj.Add(card);
                oriYPosition = card.transform.position.y;
                FixcardSpacing();
            }
            else
                Debug.Log("<color=yellow> Can not find Card Data </color>");
        }
    }

    public void LoadSpecialCard(string cardID)
    {
        Addressables.LoadAssetAsync<SpecialCardData>(cardID).Completed += OnLoadDone;

        void OnLoadDone(AsyncOperationHandle<SpecialCardData> handle)
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                GameObject card = Instantiate(specialCardPrefabs, handTransform);
                BriefSpecialCardDisplay BCD = card.GetComponent<BriefSpecialCardDisplay>();
                BCD.card = handle.Result;
                BCD.InistiateCard();
                BCD.handCards = this;
                handCardNumber++;
                handCardsID.Add(BCD.cardID);
                handCardsObj.Add(card);
                oriYPosition = card.transform.position.y;
                FixcardSpacing();
            }
            else
                Debug.Log("<color=yellow> Can not find Special Card Data </color>");
        }
    }

    void FixcardSpacing()//在卡片生成超过10个以后改变卡片之间间距
    {
        ResetHighlightCards();
        HideHeightLightInfo();
        layout.enabled = true;
        if (handCardNumber > 10)
        {
            Vector2 spacing = layout.spacing;
            float spacex = spacing.x;
            spacex = (1100-100 * handCardNumber) / handCardNumber;
            layout.spacing = new Vector2(spacex,spacing.y);
        }
    }

    IEnumerator EndFixcardSpacing()//在生成完了卡片以后关闭自动grid layout
    {
        yield return new WaitForEndOfFrame();
        layout.enabled = false;
    }

    private void ResetHighlightCards()//在卡片重新排列了以后将所选的卡片恢复到原来的位置
    {
        if (selectedIndex <= handCardsObj.Count)
        {
            Vector3 newcard = handCardsObj[selectedIndex].transform.position;
            handCardsObj[selectedIndex].transform.position = new Vector3(newcard.x, newcard.y + 5, newcard.z);
            selectedIndex = 100;
        }
    }
   
    public void HighLightCards(GameObject selectedCard)//在选择了卡片以后将所选的卡片向上移动(第一次点击)
    {
        for (int i = 0; i < handCardsObj.Count; i++)
        {
            if (handCardsObj[i] == selectedCard)
            {
                if (i != selectedIndex)//如果切换了手牌则hightlight其他的牌
                {
                    if (selectedIndex <= handCardsObj.Count)
                    {
                        Vector3 oldcard = handCardsObj[selectedIndex].transform.position;
                        handCardsObj[selectedIndex].transform.position = new Vector3(oldcard.x, oldcard.y - 5, oldcard.z);
                    }         
                    selectedIndex = i;
                    Vector3 newcard = handCardsObj[selectedIndex].transform.position;
                    selectedCard.transform.position = new Vector3(newcard.x, newcard.y + 5, newcard.z);
                    SendHeightLightInfo();
                }
                else//如果再次选择该手牌 将手牌打出
                {
                    PlaceACard();
                }
            }
            
        }
    }

    void PlaceACard()//需要添加特殊卡片的代码
    {
        try//执行单位牌的输出
        {
            CardData data = handCardsObj[selectedIndex].GetComponent<BriefCardDisplay>().card;
            int cardid = data.cardID;
            GameObject temp = handCardsObj[selectedIndex];
            handCardsObj.Remove(handCardsObj[selectedIndex]);
            handCardsID.RemoveAt(selectedIndex);
            selectedIndex = 100;
            Destroy(temp);
            FixcardSpacing();
            tp.ReceiveACard(data);
        }
        catch (Exception e)
        {
            Debug.Log("This is a special card " + e);
        }
        
    }

    void SendHeightLightInfo()//显示卡片详细信息在右手边的ui上
    {
        if (rp != null)
        {
            if (handCardsObj[selectedIndex].GetComponent<BriefCardDisplay>() != null)
            {
                rp.ShowCardInfo(handCardsObj[selectedIndex].GetComponent<BriefCardDisplay>().card);
                //tp.LockAllYourTables();

                tp.UnlockYourTable(handCardsObj[selectedIndex].GetComponent<BriefCardDisplay>().card.cardType);//解锁卡片所在位置的点击
            }
            else if (handCardsObj[selectedIndex].GetComponent<BriefSpecialCardDisplay>() != null)
            {
                BriefSpecialCardDisplay BSCD = handCardsObj[selectedIndex].GetComponent<BriefSpecialCardDisplay>();
                rp.ShowCardInfo(BSCD.card);
                //开启桌面的点击功能
                if (BSCD.card.function == SpecialCardData.Function.PowerUP)
                {
                    tp.UnlockAllYourTables();
                }
                else
                {
                    tp.UnlockYourTable(GIP.ConvertCardType(BSCD.card));
                }
                GIP.RegesterPanelEvent();//开启天气界面的点击
            }
        }
    }
    
    public void ReceivePanelInfo(int num)//得到TablePanel传来的值并给相对应的位置发送一张卡片
    {
        BriefCardDisplay BCD = handCardsObj[selectedIndex].GetComponent<BriefCardDisplay>();
        if (BCD != null)
        {
            CardData data = handCardsObj[selectedIndex].GetComponent<BriefCardDisplay>().card;
            int cardid = data.cardID;
            GameObject temp = handCardsObj[selectedIndex];
            handCardsObj.Remove(handCardsObj[selectedIndex]);
            handCardsID.RemoveAt(selectedIndex);
            selectedIndex = 100;
            Destroy(temp);
            FixcardSpacing();
            tp.ReceiveACard(data,num);
        }
        else//特殊卡片情况
        {
            SpecialCardData data = handCardsObj[selectedIndex].GetComponent<BriefSpecialCardDisplay>().card;
            int cardid = data.cardID;
            GameObject temp = handCardsObj[selectedIndex];
            handCardsObj.Remove(handCardsObj[selectedIndex]);
            handCardsID.RemoveAt(selectedIndex);
            selectedIndex = 100;
            Destroy(temp);
            FixcardSpacing();
            tp.ReceiveACard(data, num);
        }
        
    }
    public void HideHeightLightInfo()
    {
        rp.HideCardInfo();
    }

    public void AcquireAllSameNameCard(CardData data)//从 TablePanel传入数据 获取同名的牌并将牌移除
    {
        StartCoroutine(AcquireSameCards(data));        
    }

    IEnumerator AcquireSameCards(CardData data)
    {
        //获取手牌
        List<CardData> result = new List<CardData>();
        List<GameObject> deleteCards = new List<GameObject>();
        string name = data.name;
        foreach (var card in handCardsObj)
        {
            BriefCardDisplay bcd = card.GetComponent<BriefCardDisplay>();
            if (bcd != null)
            {
                if (bcd.card.name == name)
                {
                    result.Add(bcd.card);
                    deleteCards.Add(card);
                }
            }
        }

        //获取牌组的牌
        List<int> deckCards = cc.getYourCard;
        List<int> removeCards = new List<int>();
        foreach (var cardID in deckCards)
        {
            if (cardID < 900)
            {
                string ID = "Card" + cardID.ToString();
                Addressables.LoadAssetAsync<CardData>(ID).Completed += OnLoadDone;

                void OnLoadDone(AsyncOperationHandle<CardData> handle)
                {
                    if (handle.Status == AsyncOperationStatus.Succeeded)
                    {
                        CardData card = handle.Result;
                        if (card.name.Equals(name))
                        {
                            result.Add(card);
                            removeCards.Add(cardID);
                        }
                    }
                    else
                        Debug.Log("<color=yellow> Can not find Card Data </color>");
                }
            }
        }

        foreach (var card in deleteCards)//移除手牌
        {
            handCardsObj.Remove(card);
            Destroy(card);
        }
        yield return null;
        cc.RemoveYourCards(removeCards);

        yield return new WaitForEndOfFrame();
        tp.ReceiveSummonCards(result);
    }

}
