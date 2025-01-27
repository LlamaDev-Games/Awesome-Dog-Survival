using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
//Loot table class
public class Loot
{
    public GameObject[] possibleItems;
    public int minAmount;
    public int maxAmount;

    //Choses random amount of random loot items
    public GameObject[] GetRandomLoot(){
        if (possibleItems.Length == 0) return new GameObject[0];

        int amount = Random.Range(minAmount, maxAmount + 1);
        GameObject[] chosenItems = new GameObject[amount];
        
        for (int i = 0; i < amount; i++)
        {
            GameObject item = possibleItems[Random.Range(0, possibleItems.Length)];
            chosenItems[i] = item;
        }

        return chosenItems;
    }
}