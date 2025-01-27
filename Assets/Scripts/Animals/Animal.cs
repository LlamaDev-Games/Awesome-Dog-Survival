using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Animal : MonoBehaviour, IPointerClickHandler
{
    //Private variables
    private AnimalController controller;

    [Header("Stats")]
    [SerializeField] private float health;
    [SerializeField] private float scareDist;

    [Header("Loot")]
    [SerializeField] private Loot loot;
    [SerializeField] private float lootSpreadDist;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<AnimalController>();
    }

    // Update is called once per frame
    void Update()
    {
        CheckScared();
    }  

    //Check if animal is scared by dog
    private void CheckScared(){
        Dog dog = GameManager.instance.dog;

        //Check if this animal is being chased
        bool beingChased = dog.controller.CheckMovementState(AnimalController.MovementState.Chasing);
        if (beingChased){
            
            //Check that animal is close enough to the dog to be scared
            float dist = Vector2.Distance(transform.position, dog.transform.position);
            if (dist <= scareDist){

                if (dog.controller.CheckMovementState(AnimalController.MovementState.Chasing)){
                    controller.RunFrom(dog.transform);
                }
            }
            else{
                controller.StopRunning();
            }
        }
        else{
            controller.StopRunning();
        }
    }

    //Called when mouse clicks while hovering over the animal
    public void OnPointerClick(PointerEventData eventData){
        GameManager.instance.dog.SetTargetAnimal(this);
    }

    //Take damage and returns if the animal has died
    public bool TakeDamage(float dmg){
        health -= dmg;
        if (health <= 0){
            Die();
            return true;
        }
        return false;
    }

    //Drop items from loot table
    private void DropItems(){
        if (loot != null){
            //Get random loot
            GameObject[] items = loot.GetRandomLoot();

            //Instantiate each item
            foreach (GameObject item in items)
            {
                Vector2 randomPos = Random.insideUnitCircle * Random.Range(0f, lootSpreadDist);
                Vector2 position = (Vector2)transform.position + randomPos;
                Instantiate(item, position, Quaternion.identity);
            }
        }
    }

    private void Die(){
        DropItems();
        Destroy(gameObject);
    }
}
