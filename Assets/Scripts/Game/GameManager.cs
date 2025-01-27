using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance; //Singleton instance

    //Private variables
    [SerializeField] private Recipe[] recipes;
    [SerializeField] private InterfaceElement blackScreen;
    [SerializeField] private TMP_Text timer;
    private float time;

    //Public variables
    public Dog dog;

    //Called when an instance of the script is being loaded
    private void Awake()
    {
        //Singleton creation
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    } 
    
    // Update is called once per frame
    private void Update(){
        time += Time.deltaTime;
        timer.text = Mathf.RoundToInt(time).ToString();
        if (Input.GetKeyDown(KeyCode.R)){
            ResetGame();
        }
    }

    //Reloads scene
    public void ResetGame(){
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    //Triggers black screen fade animation
    public static void FadeToBlack(){
        instance.blackScreen.AnimationSwitchState(0);
    }

    //Finds the closest point on the navmesh from a point outside of it
    public static Vector2 ClosestNavMeshPos(Vector2 position){
        NavMeshHit hit;
        NavMesh.SamplePosition(position, out hit, Mathf.Infinity, NavMesh.AllAreas);
        return hit.position;
    }

    //Attempts to craft from a list of items
    public static void Craft(Item[] items, Vector2 position){
        //Create item id list
        int[] ids = new int[items.Length];
        for (int i = 0; i < items.Length; i++)
        {
            ids[i] = items[i].GetID();
        }
        
        //Get a list of recipies that can be crafted from the given items
        List<Recipe> validRecipes = new List<Recipe>();
        foreach (var recipe in instance.recipes)
        {
            bool hasAllItems = true;
            foreach (RecipeItem item in recipe.items)
            {
                if (ids.Contains(item.id)){
                    int amount = 0;
                    foreach (int i in ids) if (i == item.id) amount++;
                    if (amount >= item.amount){
                        break;
                    }
                }
                hasAllItems = false;
            }

            if (hasAllItems){
                validRecipes.Add(recipe);
            }
        }
        
        if (validRecipes.Count > 0){
            //Find the crafting recipe with the most items from the valid recipes
            Recipe biggestRecipe = validRecipes[0];
            foreach (var recipe in validRecipes){
                if (recipe.items.Count() > biggestRecipe.items.Count())
                    biggestRecipe = recipe;
            }

            //Destroy the items used up in the recipe
            foreach (RecipeItem item in biggestRecipe.items){
                for (int i = 0; i < item.amount; i++){
                    int index = ids.ToList().IndexOf(item.id);
                    Destroy(items[index].gameObject);
                    ids[index] = -1;
                }
                
            }
            
            //Instantiate the result of the recipe
            Instantiate(biggestRecipe.resultPrefab, position, Quaternion.identity);
        }
        else{
            Debug.LogWarning("No recipe for items");
        }
        
    }
}