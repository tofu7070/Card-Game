using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Unit Card", menuName = "Card/Unit")]
public class CardData : ScriptableObject
{
    public int cardID;//unique card id 
    public new string name;
    public string description;
    public int cardFaction;//卡片所属阵容 （尚未添加功能）
    public int cardType;//卡片的类别 进展 远程 攻城
    public Sprite typeImage;//卡片类别的图案
    public bool epic;//卡片是否为稀有卡
    public int attack;//卡片的攻击力
    public Sprite artwork;//卡片的卡面图案

    public Skills skill;
    public Sprite skillImage;//卡片技能图案
    public enum Skills
    {
        None,
        Groupup,
        Summon,
        Destory
    }
}

