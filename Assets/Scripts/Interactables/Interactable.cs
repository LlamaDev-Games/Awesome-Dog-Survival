using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Interactable : MonoBehaviour, IPointerClickHandler
{
    //Called when mouse clicks while hovering over the animal
    public void OnPointerClick(PointerEventData eventData){
        GameManager.instance.dog.SetTargetInteractable(this);
    }

    //Called when dog interacts with this object
    //Can be overridden by a child interactable class
    public virtual void Interact(){
        Debug.Log(gameObject.name + " was interacted with");
    }
}
