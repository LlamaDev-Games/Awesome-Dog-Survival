using System.Collections;
using System.Collections.Generic;
using NavMeshPlus.Extensions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Dog : MonoBehaviour
{
    //Public variables
    public AnimalController controller;

    //Private variables
    [Header("Stats")]
    [SerializeField] private DogStats stats;
    [SerializeField] private float thirstRefillSpeed;
    [SerializeField] private float hungerHealthDrainSpeed;
    [SerializeField] private float thristHealthDrainSpeed;

    [Header("Energy")]
    [SerializeField] private float movementEnergyCost;
    [SerializeField] private float energyCostPerAttack;
    [SerializeField] private float noEnergySpeed;
    [SerializeField] private float sleepEnergyRecharge;

    [Header("Attack")]
    [SerializeField] private float timeBtwAttack;
    private float timeUntilAttack;
    [SerializeField] private float attackDist;
    [SerializeField] private float attackDmg;

    [Header("Interaction")]
    [SerializeField] private float interactionDist;
    [SerializeField] private float pickupDist;
    [SerializeField] private SpriteRenderer heldItemSprite;
    [SerializeField] private LayerMask itemLayer;
    private Item heldItem;

    //Object targeting variables
    private enum TargetType { None, Interactable, Animal, Item }
    private TargetType targetType;
    private GameObject target;

    #region [Update]
    // Update is called once per frame
    void Update()
    {
        UpdateStats();
        CheckPlayerInputs();

        //Die when run out of health
        if(stats.health.value <= 0f){
            Die();
        }

        //Object targeting logic depending on type of target
        switch (targetType){
            case TargetType.None: //No current target
                break;
            case TargetType.Animal: //Check if dog can attack animal
                timeUntilAttack -= Time.deltaTime;
                CheckAttack();
                break;
            case TargetType.Item: //Check if item can be picked up
                CheckPickup();
                break;
            case TargetType.Interactable: //Check if dog has reached object
                CheckInteract();
                break; 
        }
    }

    private void CheckPlayerInputs(){
        //Attempt to craft using neaby items on player input
        if (Input.GetKeyDown(KeyCode.C)){
            GameManager.Craft(GetNearbyItems(), transform.position);
        }

        //Drop item on right click
        if (heldItem != null){
            if (Input.GetMouseButtonDown(1)){
                DropItem();
            }
        }
    }

    private void UpdateStats(){
        stats.Update();

        //Drain health if thirst or hunger is empty
        if (stats.thirst.value <= 0f){
            stats.health.value -= thirstRefillSpeed * Time.deltaTime;
        }
        if (stats.hunger.value <= 0f){
            stats.health.value -= hungerHealthDrainSpeed * Time.deltaTime;
        }
        
        //Drain energy when moving and stops moving when empty
        if (controller.IsMoving()){
            stats.energy.value -= movementEnergyCost * Time.deltaTime;
        }
        if (stats.energy.value <= 0f){
            controller.SetMovementSpeed(noEnergySpeed);
        }
        else{
            controller.ResetMovementSpeed();
        }

        //Refill thirst if colliding with a water source
        if(CheckCollisionWithTag("WaterSource")){
            stats.thirst.value += thirstRefillSpeed * Time.deltaTime;
        }
    }
    #endregion
    
    #region [Target Item Logic]
    //Sets target object to specified item
    public void SetTargetItem(Item item){
        controller.ChaseObject(item.transform);
        target = item.gameObject;
        targetType = TargetType.Item;
    }

    //Returns a list of all the nearby items
    private Item[] GetNearbyItems(){
        //Get a list of nearby items
        float radius = GetComponent<CircleCollider2D>().radius;
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, radius, Vector2.zero, Mathf.Infinity, itemLayer);

        //Iterate though the objects and add each item component to the list
        List<Item> ids = new List<Item>();
        foreach(var hit in hits){
            ids.Add(hit.collider.GetComponent<Item>());
        }

        return ids.ToArray();
    }

    //Checks if player can pickup targeted item
    private void CheckPickup(){
        //Check player is close enough
        float dist = Vector2.Distance(transform.position, target.transform.position);
        if (dist <= pickupDist){
            Item targetItem = target.GetComponent<Item>();
            //Eat if item is food, pickup if not
            if (targetItem.CompareType(Item.ItemType.food))
                EatItem();
            else
                PickupItem();
            controller.StopChasing();
        }
    }

    //Pickup targeted item
    private void PickupItem(){
        heldItemSprite.gameObject.SetActive(true);
        Item targetItem = target.GetComponent<Item>();
        heldItemSprite.sprite = targetItem.GetComponent<SpriteRenderer>().sprite;

        targetItem.gameObject.SetActive(false);
        heldItem = targetItem;
        target = null;
        targetType = TargetType.None;
    }

    //Eat targeted food item
    private void EatItem(){
        Item targetItem = target.GetComponent<Item>();
        Food foodItem = targetItem as Food;
        stats.hunger.value += foodItem.hungerToGive;

        Destroy(targetItem.gameObject);
        target = null;
        targetType = TargetType.None;
    }

    //Drop held item
    private void DropItem(){
        heldItemSprite.gameObject.SetActive(false);
        heldItemSprite.sprite = null;

        Vector3 newPos = new Vector3(transform.position.x, transform.position.y, 0);
        heldItem.transform.position = newPos;
        heldItem.gameObject.SetActive(true);
        heldItem = null;
    }
    #endregion

    #region [Target Animal Logic]
    //Sets target object to specified animal
    public void SetTargetAnimal(Animal animal){
        if (stats.energy.value <= 0f) return;
        controller.ChaseObject(animal.transform);
        target = animal.gameObject;
        targetType = TargetType.Animal;
    }

    //Check if animal can be attacked
    private void CheckAttack(){
        Animal targetAnimal = target.GetComponent<Animal>();

        //Get distance between this and target animal
        float dist = Vector2.Distance(transform.position, targetAnimal.transform.position);

        //Check animal is close enough and attack cooldown is over
        if (timeUntilAttack <= 0 && dist <= attackDist){
            timeUntilAttack = timeBtwAttack;
            stats.energy.value -= energyCostPerAttack;

            //Attack animal and stop chasing if it dies or dog runs out of energy
            bool died = targetAnimal.TakeDamage(attackDmg);
            if (stats.energy.value <= 0f) died = true;
            if (died){
                target = null;
                targetType = TargetType.None;
                controller.StopChasing();
            }
        }
    }
    #endregion

    #region [Target Interactable Logic]
    //Sets target object to specified interactable
    public void SetTargetInteractable(Interactable interactable){
        controller.ChaseObject(interactable.transform);
        target = interactable.gameObject;
        targetType = TargetType.Interactable;
    }

    //Checks if player can interact with targeted object
    private void CheckInteract(){
        float dist = Vector2.Distance(transform.position, target.transform.position);
        if (dist <= interactionDist){
            Interactable targetObject = target.GetComponent<Interactable>();
            target = null;
            targetType = TargetType.None;
            if (!targetObject) return;

            targetObject.Interact();
            controller.StopChasing();
        }
    }
    #endregion

    #region [Other]
    //Checks if this object is currently colliding with a specified tag
    private bool CheckCollisionWithTag(string tag){
        //Get a list of colliding objects
        float radius = GetComponent<CircleCollider2D>().radius;
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, radius, Vector2.zero);

        //Iterate though objects to check if any have the correct tag
        bool objectFound = false;
        foreach(var hit in hits){
            objectFound = hit.collider.CompareTag("WaterSource");
            if (objectFound) break;
        }

        return objectFound;
    }

    //Player sleeping logic
    public void Sleep(){
        controller.StopMovement();
        GameManager.FadeToBlack();
        Invoke("FinishSleep", 1f);
    }
    private void FinishSleep(){
        stats.energy.value += sleepEnergyRecharge;
        controller.TakePlayerControl();
        GameManager.FadeToBlack();
    }

    //Player death logic
    private void Die(){
        gameObject.SetActive(false);
    }
    #endregion
}
