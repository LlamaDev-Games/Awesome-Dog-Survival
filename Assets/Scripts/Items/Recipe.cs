using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
//Crafting recipe class
public class Recipe
{
    public string name;
    public RecipeItem[] items;
    public GameObject resultPrefab;
}

[System.Serializable]
//Class for individual crafting recipe items
public class RecipeItem
{
    public int id;
    public int amount;
}