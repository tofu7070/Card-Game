using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class CardDeck : MonoBehaviour
{
    public List<int> handCardsID = new List<int>();
    public List<GameObject> handCardsObj = new List<GameObject>();

    public GameObject cardPrefabs;
   
    public Transform handTransform;

    private GridLayoutGroup layout;

    private int handCardNumber = 0;

    private float oriYPosition = 0; //手牌的y轴位置 用于在非选择状态的时候回原

    private int selectedIndex = 100;//选择状态时的手牌指针

    private RightPanel rp;// 右边ui界面的reference

    private TablePanel tp;//桌面的卡牌
    // Start is called before the first frame update
    void Start()
    {
        layout = this.GetComponent<GridLayoutGroup>();
        rp = FindObjectOfType<RightPanel>().GetComponent<RightPanel>();
        tp = FindObjectOfType<TablePanel>().GetComponent<TablePanel>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))// 测试功能 生成随机的卡片
        {
            int num = Random.Range(1, 6);
            Debug.Log("Generate a card " + num.ToString());
            GeneraterCard(num);
            StartCoroutine(EndFixcardSpacing());
        }
    }

    public void GeneraterCard(int ID)
    {
        string cardID = "Card" + ID.ToString();
        LoadCard(cardID);
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
    public void HighLightCards(GameObject selectedCard)//在选择了卡片以后将所选的卡片向上移动
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

    void PlaceACard()
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

    void SendHeightLightInfo()//显示卡片详细信息在右手边的ui上
    {
        if (rp != null)
        {
            rp.ShowCardInfo(handCardsObj[selectedIndex].GetComponent<BriefCardDisplay>().card);
        }
    }

    public void HideHeightLightInfo()
    {
        rp.HideCardInfo();
    }


}
