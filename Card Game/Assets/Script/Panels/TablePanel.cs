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
    public void ReceiveACard(CardData data)//双击卡片自动放置
    {
        GameObject card = Instantiate(cardPrefabs, CardTypeTransform(data.cardType));
        BriefCardDisplay CD = card.GetComponent<BriefCardDisplay>();
        CD.card = data;
        CD.InistiateCard();
        CardTypePanel(data.cardType).Add(card);
        StartCoroutine(DoingCardCacaulation(data.cardType));
    }

    public void ReceiveACard(SpecialCardData data,int num)//双击卡片自动放置
    {
        GameObject card = Instantiate(specialCardPrefabs, CardTypeTransform(num));
        BriefSpecialCardDisplay BCD = card.GetComponent<BriefSpecialCardDisplay>();
        BCD.card = data;
        BCD.InistiateCard();
        CardTypePanel(num).Add(card);
        StartCoroutine(DoingCardCacaulation(num));
        LockAllYourTables();//放置完成了卡片以后关闭event trigger
    }

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

   
    IEnumerator DoingCardCacaulation(int type)//卡片进行计算之前的延迟
    {
        yield return new WaitForSeconds(0.2f);
        CaclulateCards(type);
    }
    void CaclulateCards(int type)//计算单行的卡片数值，可以扩展成计算全部卡片的数值
    {
        int num = 0;
        int normalcard = 0;
        int epiccard = 0;
        bool isPowerUp = false;
        foreach (var card in CardTypePanel(type))
        {
            BriefCardDisplay BCD = card.GetComponent<BriefCardDisplay>();
            if ( BCD!= null)
            {
                if (BCD.epic)
                    epiccard += card.GetComponent<BriefCardDisplay>().attackDamage;
                else
                    normalcard += card.GetComponent<BriefCardDisplay>().attackDamage;
            }
            else
            {
                if (card.GetComponent<BriefSpecialCardDisplay>() != null)
                    isPowerUp = true;
            }    
            
        }
        if (isPowerUp)
            normalcard = normalcard * 2;
        num = normalcard + epiccard;
        CardNumberText(type).text = num.ToString();
    }

    public void ReceiveYourTableData(int num)//点击己方桌面以后返回数据
    {
        Debug.Log("Your table Clicked" + num);
        cd.ReceivePanelInfo(num);
    }

    public void ReceiveOpponentTableData(int num)//点击对手桌面以后返回数据
    {
        Debug.Log("Opponent table Clicked" + num);
    }

    #region 根据不同的情况解锁相应的桌面点击

    public void UnlockAllYourTables()
    {
        AddTriggerEventsForSlot(panelTransform.y1);
        
    }
    private void LockAllYourTables()
    {
        RemoveTriggerEventFromSlot(panelTransform.y1);
    }

    private void AddTriggerEventsForSlot(Transform slot)//添加event trigger 加入鼠标进出，还有点击event
    {
        EventTrigger trigger = slot.GetComponent<EventTrigger>();
        if (trigger == null)//如果存在event trigger则不添加
        {
            trigger = slot.gameObject.AddComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerClick;
            entry.callback.AddListener((eventData) => { ReceiveYourTableData(0); });
            trigger.triggers.Add(entry);
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
        Debug.Log("Pointer Enter");
        Color newcolor = slot.GetComponent<Image>().color;
        newcolor.a = alphaNum + 0.2f;
        slot.GetComponent<Image>().color= newcolor;
    }

    private void DimSlot(Transform slot)
    {
        Debug.Log("Pointer Exit");
        Color newcolor = slot.GetComponent<Image>().color;
        newcolor.a = alphaNum;
        slot.GetComponent<Image>().color = newcolor;
    }
    #endregion
}
