using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameInfoPanel : MonoBehaviour
{
    public GameObject weatherPanel;
    public List<GameObject> currentWeatherCards = new List<GameObject>();
    public bool[] debuffs = new bool[3];

    private float alphaNum = 0.235f;

    private TablePanel TP;

    private CardDeck CD;
    // Start is called before the first frame update
    void Start()
    {
        TP = FindObjectOfType<TablePanel>().GetComponent<TablePanel>();
        TP.GIP = this;
        CD = FindObjectOfType<CardDeck>().GetComponent<CardDeck>();
        CD.GIP = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            ClickSelectedPanel();
        }
    }

    public void ReceiveACard(SpecialCardData data)
    {
        int type = ConvertCardType(data);
        if (!debuffs[type])
        {
            debuffs[type] = true;
            GameObject card = Instantiate(TP.specialCardPrefabs, weatherPanel.transform);
            currentWeatherCards.Add(card);
            BriefSpecialCardDisplay CD = card.GetComponent<BriefSpecialCardDisplay>();
            CD.card = data;
            CD.InistiateCard();
            RemoveTriggerEvent();
        }
        
        //在这个后面可以添加动画
    }

    public int ConvertCardType(SpecialCardData data)
    {
        SpecialCardData.Function function = data.function;
        switch (function)
        {
            case SpecialCardData.Function.Frost:
                return 0;
            case SpecialCardData.Function.Fog:
                return 1;
            case SpecialCardData.Function.Rain:
                return 2;
        }

        return 0;
    }
    public void RegesterPanelEvent()
    {
        EventTrigger trigger = weatherPanel.GetComponent<EventTrigger>();
        if (trigger == null)//如果存在event trigger则不添加
        {
            trigger = weatherPanel.AddComponent<EventTrigger>();
            EventTrigger.Entry entry2 = new EventTrigger.Entry();
            entry2.eventID = EventTriggerType.PointerEnter;
            entry2.callback.AddListener((eventData) => { HeightlightSlot(weatherPanel.transform); });
            trigger.triggers.Add(entry2);
            EventTrigger.Entry entry3 = new EventTrigger.Entry();
            entry3.eventID = EventTriggerType.PointerExit;
            entry3.callback.AddListener((eventData) => { DimSlot(weatherPanel.transform); });
            trigger.triggers.Add(entry3);
        }
    }

    public void RemoveTriggerEvent()
    {
        EventTrigger trigger = weatherPanel.GetComponent<EventTrigger>();
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
        slot.GetComponent<Image>().color = newcolor;
    }

    private void DimSlot(Transform slot)
    {
        selectedPanel = null;
        //Debug.Log("Pointer Exit");
        Color newcolor = slot.GetComponent<Image>().color;
        newcolor.a = alphaNum;
        slot.GetComponent<Image>().color = newcolor;
    }

    private Transform selectedPanel;//目前高亮的区域

    private void ClickSelectedPanel()//改写的点击方法 
    {
        if (selectedPanel != null)
        {
            //添加点击该区域的功能
            WeatherPanelClicked();
            DimSlot(selectedPanel);
        }
    }

    private void WeatherPanelClicked()
    {
        CD.ReceivePanelInfo(0);
        TP.LockAllYourTables();
    }
}
