using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Special Card", menuName = "Card/SpecialCard")]
public class SpecialCardData : ScriptableObject
{
    //Special card ID begin with 900
    public int cardID;//unique card id 
    public new string name;
    public string description;
    public Sprite typeImage;//卡片功能的图案
    public Sprite artwork;//卡片的卡面图案
    public Function function;

    public enum Function
    {
        Frost,
        Fog,
        Rain,
        PowerUP
    }
}
