using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 这个用于显示和计算桌面上的卡牌
/// </summary>
public class TablePanel : MonoBehaviour
{
    public GameObject cardPrefabs;
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
    // Start is called before the first frame update
    void Start()
    {
        InitiateCardNumbers();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ReceiveACard(CardData data)
    {

        GameObject card = Instantiate(cardPrefabs, CardTypeTransform(data.cardType));
        BriefCardDisplay CD = card.GetComponent<BriefCardDisplay>();
        CD.card = data;
        CD.InistiateCard();
        CardTypePanel(data.cardType).Add(card);
        StartCoroutine(DoingCardCacaulation(data.cardType));
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
    IEnumerator DoingCardCacaulation(int type)//卡片进行计算之前的延迟
    {
        yield return new WaitForSeconds(0.2f);
        CaclulateCards(type);
    }
    void CaclulateCards(int type)//计算单行的卡片数值，可以扩展成计算全部卡片的数值
    {
        int num = 0;
        foreach (var card in CardTypePanel(type))
        {
            num += card.GetComponent<BriefCardDisplay>().attackDamage;
        }

        CardNumberText(type).text = num.ToString();
    }
}
