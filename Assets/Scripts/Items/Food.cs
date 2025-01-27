using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : Item
{
    //Overrides item type as food
    public override ItemType itemType { get { return ItemType.food; } }
    public float hungerToGive;
}

