using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.EventSystems;

public class Item : MonoBehaviour, IPointerClickHandler
{
    //Item type enum
    public enum ItemType { normal, food }

    //Item type (Can be overridden by child classes)
    public virtual ItemType itemType { get { return ItemType.normal; } }

    [SerializeField] private int id;

    //Called when mouse clicks while hovering over the animal
    public void OnPointerClick(PointerEventData eventData){
        GameManager.instance.dog.SetTargetItem(this);
    }

    //Compare item type
    public bool CompareType(ItemType type){
        return itemType == type;
    }

    //Returns item ID
    public int GetID(){
        return id;
    }
}