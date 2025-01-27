using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bed : Interactable
{
    //Called when dog interacts with this object
    public override void Interact()
    {
        GameManager.instance.dog.Sleep();
    }
}
