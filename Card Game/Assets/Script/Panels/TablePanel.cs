using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
/// <summary>
/// 这个用于显示和计算桌面上的卡牌
/// </summary>
public class TablePanel : MonoBehaviour
{
    public GameObject cardPrefabs;
    public GameObject specialCardPrefabs;
    public CardDeck cd;
    [System.Serializable]
    public class PanelCards {
        public List<GameObject> op3 = new List<GameObject>();//敌方攻城牌
        public List<GameObject> op2 = new List<GameObject>();//敌方远程牌
        public List<GameObject> op1 = new List<GameObject>();//敌方近战牌
        public List<GameObject> y1 = new List<GameObject>();//己方近战牌
        public List<GameObject> y2 = new List<GameObject>();//己方远程牌
        public List<GameObject> y3 = new List<GameObject>();//己方攻城牌
    }

    public PanelCards panelCards;//所有放置的卡片gameobject

    [System.Serializable]
    public class PanelTransform
    {
        public Transform op3;
        public Transform op2;
        public Transform op1;
        public Transform y1;
        public Transform y2;
        public Transform y3;
    }

    public PanelTransform panelTransform;//卡片放置的位置

    [System.Serializable]
    public class PanelNumbers
    {
        public int op3;
        public int op2;
        public int op1;
        public int y1;
        public int y2;
        public int y3;
        public Text op3Text;
        public Text op2Text;
        public Text op1Text;
        public Text y1Text;
        public Text y2Text;
        public Text y3Text;
    }

    public PanelNumbers panelNumbers;//卡片计算的数字 和数字的显示text

    [System.Serializable]
    public class PowerUP
    {
        public GameObject[] yUP = new GameObject[3];//从0到2
        public GameObject[] opUP = new GameObject[3];
        public bool[] ybool = new bool[3];
        public bool[] opbool = new bool[3];
    }

    public PowerUP powerUps;//增幅的图标
    private float alphaNum = 0.235f;

    public GameInfoPanel GIP;
    // Start is called before the first frame update
    void Start()
    {
        InitiateCardNumbers();
        InitiatePowerUps();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            UnlockAllYourTables();
        }

        if (Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            LockAllYourTables();
        }

        if (Input.GetMouseButtonDown(0))
        {
            ClickSelectedPanel();
        }
    }


    #region 初始化
    void InitiateCardNumbers()//hard coding :) 初始化场上卡片的
    {
        panelNumbers.op3 = 0;
        panelNumbers.op3Text.text = "0";
        panelNumbers.op2 = 0;
        panelNumbers.op2Text.text = "0";
        panelNumbers.op1 = 0;
        panelNumbers.op1Text.text = "0";
        panelNumbers.y3 = 0;
        panelNumbers.y3Text.text = "0";
        panelNumbers.y2 = 0;
        panelNumbers.y2Text.text = "0";
        panelNumbers.y1 = 0;
        panelNumbers.y1Text.text = "0";
    }

    void InitiatePowerUps()//初始化增幅
    {
        for (int i = 0; i < 3; i++)
        {
            powerUps.yUP[i].SetActive(false);
            powerUps.opUP[i].SetActive(false);
            powerUps.ybool[i] = false;
            powerUps.opbool[i] = false;
        }
    }
    #endregion
    //-------------------------------------------------------------------------
    public void ReceiveACard(CardData data)//添加卡片 双击卡片自动放置方法
    {
        GameObject card = Instantiate(cardPrefabs, CardTypeTransform(data.cardType));
        BriefCardDisplay CD = card.GetComponent<BriefCardDisplay>();
        CD.card = data;
        CD.InistiateCard();
        FixCardDamage(CD, data.cardType);
        CardTypePanel(data.cardType).Add(card);
        FixCardSpacing(data.cardType);
        LockAllYourTables();//放置完成了卡片以后关闭event trigger
        //此处添加技能的判定
        ActivateCardSkill(CD.skills, data.cardType, data, card);
        StartCoroutine(DoingCardCacaulation(data.cardType));
    }

    public void ReceiveACard(CardData data, int num)//选择一个位置以后放置特护的卡片
    {
        GameObject card = Instantiate(cardPrefabs, CardTypeTransform(num));
        BriefCardDisplay CD = card.GetComponent<BriefCardDisplay>();
        CD.card = data;
        CD.InistiateCard();
        FixCardDamage(CD, num);
        CardTypePanel(num).Add(card);
        FixCardSpacing(num);
        if (powerUps.ybool[num])
            CD.isPowerUp = true;
        LockAllYourTables();//放置完成了卡片以后关闭event trigger
        //此处添加技能的判定
        ActivateCardSkill(CD.skills, num, data, card);
        StartCoroutine(DoingCardCacaulation(num));
    }

    public void ReceiveACard(SpecialCardData data,int num)//选择一个位置以后放置特护的卡片
    {        
        ActivateSpecialCard(data,num);
    }

    private void FixCardDamage(BriefCardDisplay BCD, int type)
    {
        if (GIP.debuffs[type])
        {
            BCD.DeBuff();
        }
    }

    #region 获取信息
    Transform CardTypeTransform(int type)//给出卡片放置的位置
    {
        Transform location = panelTransform.y1;
        switch (type)
        {
            case 0:
                location = panelTransform.y1;
                break;
            case 1:
                location = panelTransform.y2;
                break;
            case 2:
                location = panelTransform.y3;
                break;
        }

        return location;
    }
    int CardTypeTransform(Transform slot)//给出卡片放置的位置
    {
        int index = 0;

        if (slot == panelTransform.y1)
            index = 0;
        if (slot == panelTransform.y2)
            index = 1;
        if (slot == panelTransform.y3)
            index = 2;

        return index;
    }
    List<GameObject> CardTypePanel(int type)//给出存放卡片gameobject的地址
    {
        switch (type)
        {
            case 0:
                return panelCards.y1;
            case 1:
                return panelCards.y2;
            case 2:
                return panelCards.y3;
            default:
                return panelCards.y1;
        }
    }

    Text CardNumberText(int type)//返回卡片显示数字text的位置
    {
        switch (type)
        {
            case 0:
                return panelNumbers.y1Text;
            case 1:
                return panelNumbers.y2Text;
            case 2:
                return panelNumbers.y3Text;
            default:
                return panelNumbers.y1Text;
        }
    }


    #endregion

    #region 计算功能
    void FixCardSpacing(int type)//在卡片生成超过10个以后改变卡片之间间距
    {
        int num = CardTypePanel(type).Count;
        if (num > 8)
        {
            GridLayoutGroup layout = CardTypeTransform(type).GetComponent<GridLayoutGroup>();
            Vector2 spacing = layout.spacing;
            float spacex = spacing.x;
            spacex = (900 - 100 * num) / (num - 1);
            layout.spacing = new Vector2(spacex, spacing.y);
        }
    }

    IEnumerator DoingCardCacaulation(int type)//卡片进行计算之前的延迟
    {
        yield return new WaitForSeconds(0.2f);
        CaclulateCards(type);
    }
    IEnumerator DoingCardCacaulation(int type, float time)//等待输入的时间然后 卡片进行计算 
    {
        yield return new WaitForSeconds(time);
        CaclulateCards(type);
    }
    void CaclulateCards(int type)//计算单行的卡片数值，可以扩展成计算全部卡片的数值
    {
        int num = 0;
        int normalcard = 0;
        int epiccard = 0;
        foreach (var card in CardTypePanel(type))
        {
            BriefCardDisplay BCD = card.GetComponent<BriefCardDisplay>();
            if (BCD != null)
            {
                if (BCD.epic)
                    epiccard += BCD.attackDamage;
                else
                {
                    normalcard += BCD.attackDamage * BCD.stack;
                    BCD.RefreshCardText();
                }
            }
        }
        if (powerUps.ybool[type])
            normalcard = normalcard * 2;
        num = normalcard + epiccard;
        CardNumberText(type).text = num.ToString();
    }

    public void ActivateCardSkill(CardData.Skills skill, int type, CardData data, GameObject card)//根据卡片技能进行计算
    {
        switch (skill)
        {
            case CardData.Skills.None:
                //如果没有技能直接退出
                return;
            case CardData.Skills.Groupup:
                //进行卡片的组合技能计算
                GroupUpSkill(type);               
                break;
            case CardData.Skills.Summon:
                //SummonSkill(type, data);
                StartCoroutine(SummonSkillActivate(data, card));
                break;
        }
    }

    private void GroupUpSkill(int type)
    {
        Dictionary<string, int> cardDic = new Dictionary<string, int>();//字符串为卡片的名称，整形为数量
        bool haveGroupCard = false;
        foreach (var card in CardTypePanel(type))
        {
            BriefCardDisplay BCD = card.GetComponent<BriefCardDisplay>();
            if (BCD != null)
            {
                string cardName = card.GetComponent<BriefCardDisplay>().name;
                if (cardDic.ContainsKey(cardName))
                {
                    cardDic[cardName] += 1;
                    haveGroupCard = true;
                }
                else
                {
                    cardDic.Add(cardName, 1);
                }
            }
        }
        if (haveGroupCard)
        {
            List<BriefCardDisplay> BCDs = new List<BriefCardDisplay>();
            foreach (var card in CardTypePanel(type))
            {
                BriefCardDisplay BCD = card.GetComponent<BriefCardDisplay>();
                if (BCD != null)
                {
                    string cardName = card.GetComponent<BriefCardDisplay>().name;
                    if (cardDic[cardName] > 1)
                    {
                        BCD.stack = cardDic[cardName];//修改同名卡片堆积数量
                        BCDs.Add(BCD);//登记卡片
                    }
                }
            }
            //在这里添加卡片登记以后的动画效果 如果是相同的数值则不播放动画
            foreach (var cardBDC in BCDs)
            {
                cardBDC.PlayNumberPopAnimation();
            }
        }
    }

    IEnumerator SummonSkillActivate(CardData data, GameObject card)
    {
        BriefCardDisplay BCD = card.GetComponent<BriefCardDisplay>();
        BCD.PlaySummonAnimation();
        yield return new WaitForSeconds(1f);
        cd.AcquireAllSameNameCard(data);
    }

    private void SummonSkill(int type, CardData data)//不使用代码
    {
        //通过呼叫carddeck 获取存在的手牌
        //再通过carddeck 获取牌组中的同名牌
        //再获取所有可以使用的牌以后 生成一个 array 传入TablePanel
        //所有的牌上场了以后播放组合动画
        cd.AcquireAllSameNameCard(data);

    }

    public void ReceiveSummonCards(List<CardData> datas)//接收从Card Deck传来的信息
    {
        if (datas.Count == 0)
            return;
        int type = 0;
        List<BriefCardDisplay> BCDs = new List<BriefCardDisplay>();
        foreach (var data in datas)
        {
            type = data.cardType;
            GameObject card = Instantiate(cardPrefabs, CardTypeTransform(type));
            BriefCardDisplay CD = card.GetComponent<BriefCardDisplay>();
            BCDs.Add(CD);
            CD.card = data;
            FixCardDamage(CD,type);
            CD.InistiateCard();
            CardTypePanel(type).Add(card);
            FixCardSpacing(type);
        }

        foreach (var BCD in BCDs)
        {
            BCD.PlaySummonAnimation();
        }
        StartCoroutine(DoingCardCacaulation(type));
    }

    private void ActivateSpecialCard(SpecialCardData data, int type)
    {
        SpecialCardData.Function skills = data.function;
        switch (skills)
        {
            case SpecialCardData.Function.PowerUP:
                PowerUpFunction(type);
                break;
            case SpecialCardData.Function.Frost:
                FrostFunction(data);
                DebuffTypeUnit(type);
                StartCoroutine(DoingCardCacaulation(type));
                break;
            case SpecialCardData.Function.Fog:
                break;
            case SpecialCardData.Function.Rain:
                break;
        }
    }

    private void PowerUpFunction(int type)//加成选择区域的牌组攻击力
    {
        powerUps.ybool[type] = true;//将增幅的标记改为true
        powerUps.yUP[type].SetActive(true);
        foreach (var card in CardTypePanel(type))
        {
            card.GetComponent<BriefCardDisplay>().isPowerUp = true;
        }
        LockAllYourTables();//放置完成了卡片以后关闭event trigger
        StartCoroutine(DoingCardCacaulation(type));
    }

    private void FrostFunction(SpecialCardData data)
    {
        GIP.ReceiveACard(data);

    }

    private void DebuffTypeUnit(int type)
    {
        switch (type)
        {
            case 0://冰霜
                DebuffCards(panelCards.op1);
                DebuffCards(panelCards.y1);
                break;
            case 1:
                break;
            case 2:
                break;

        }
    }

    private void DebuffCards(List<GameObject> cards)
    {
        foreach (var card in cards)
        {
            BriefCardDisplay BCD = card.GetComponent<BriefCardDisplay>();
            if (BCD != null)
            {
                BCD.DeBuff();
            }
        }
    }

    #endregion




    //public void ReceiveYourTableData(int num)//点击己方桌面以后返回数据
    //{
    //    Debug.Log("Your table Clicked" + num);
    //    cd.ReceivePanelInfo(num);
    //}

    public void ReceiveOpponentTableData(int num)//点击对手桌面以后返回数据
    {
        Debug.Log("Opponent table Clicked" + num);
    }

    #region 根据不同的情况解锁相应的桌面点击

    public void UnlockYourTable(int type)
    {
        
        for (int i = 0; i < 3; i++)
        {
            if (i == type)
            {
                AddTriggerEventsForSlot(CardTypeTransform(type));
            }
            else
            {
                RemoveTriggerEventFromSlot(CardTypeTransform(i));
            }
        }
    }
    public void UnlockAllYourTables()
    {
        if (!powerUps.ybool[0])//这里可以添加别的条件，如果已经增幅则不显示可以增幅
        {
            AddTriggerEventsForSlot(panelTransform.y1);
        }
        if (!powerUps.ybool[1])
        {
            AddTriggerEventsForSlot(panelTransform.y2);
        }
        if (!powerUps.ybool[2])
        {
            AddTriggerEventsForSlot(panelTransform.y3);
        }

    }
    public void LockAllYourTables()
    {
        RemoveTriggerEventFromSlot(panelTransform.y1);
        RemoveTriggerEventFromSlot(panelTransform.y2);
        RemoveTriggerEventFromSlot(panelTransform.y3);
    }

    private void AddTriggerEventsForSlot(Transform slot)//添加event trigger 加入鼠标进出，还有点击event
    {
        EventTrigger trigger = slot.GetComponent<EventTrigger>();
        if (trigger == null)//如果存在event trigger则不添加
        {
            int slotIndex = CardTypeTransform(slot);
            trigger = slot.gameObject.AddComponent<EventTrigger>();
            //EventTrigger.Entry entry = new EventTrigger.Entry();
            //entry.eventID = EventTriggerType.PointerDown;
            //entry.callback.AddListener((eventData) => { ReceiveYourTableData(slotIndex); DimSlot(slot); });
            //trigger.triggers.Add(entry);
            EventTrigger.Entry entry2 = new EventTrigger.Entry();
            entry2.eventID = EventTriggerType.PointerEnter;
            entry2.callback.AddListener((eventData) => { HeightlightSlot(slot); });
            trigger.triggers.Add(entry2);
            EventTrigger.Entry entry3 = new EventTrigger.Entry();
            entry3.eventID = EventTriggerType.PointerExit;
            entry3.callback.AddListener((eventData) => { DimSlot(slot); });
            trigger.triggers.Add(entry3);
        }
    }

    private void RemoveTriggerEventFromSlot(Transform slot)
    {
        EventTrigger trigger = slot.GetComponent<EventTrigger>();
        if (trigger != null)
        {
            Destroy(trigger);
        }
    }

    private void HeightlightSlot(Transform slot)
    {
        selectedPanel = slot;
        //Debug.Log("Pointer Enter");
        Color newcolor = slot.GetComponent<Image>().color;
        newcolor.a = alphaNum + 0.2f;
        slot.GetComponent<Image>().color= newcolor;
    }

    private void DimSlot(Transform slot)
    {
        selectedPanel = null;
        //Debug.Log("Pointer Exit");
        Color newcolor = slot.GetComponent<Image>().color;
        newcolor.a = alphaNum;
        slot.GetComponent<Image>().color = newcolor;
    }

    private int RetrunSlotInfo(Transform slot)
    {
        int num = 0;

        return num;
    }

    private Transform selectedPanel;//目前高亮的区域

    private void ClickSelectedPanel()//改写的点击方法 
    {
        if (selectedPanel != null)
        {
            int num = CardTypeTransform(selectedPanel);
            //Debug.Log("Your clicked a Select panel" + num);
            cd.ReceivePanelInfo(num);
            DimSlot(selectedPanel);
        }
    }

    #endregion
}
