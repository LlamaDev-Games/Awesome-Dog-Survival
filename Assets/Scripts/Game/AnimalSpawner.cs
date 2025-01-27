using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalSpawner : MonoBehaviour
{
    //Private varaibles
    [SerializeField] private float timeBtwSpawns;
    [SerializeField] private Vector2 minBound;
    [SerializeField] private Vector2 maxBound;
    [SerializeField] private Transform animalParent;
    [SerializeField] private GameObject[] animalPrefabs;

    private List<GameObject> animalObjects;
    private float timeUntilSpawn;

    void Start(){
        animalObjects = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        timeUntilSpawn -= Time.deltaTime;

        if (timeUntilSpawn <= 0f){
            animalObjects.Add(SpawnAnimal());
            timeUntilSpawn = timeBtwSpawns;
        }
    }

    //Spawns in a random animal
    private GameObject SpawnAnimal(){
        int rand = Random.Range(0, animalPrefabs.Length);
        GameObject animal = animalPrefabs[rand];

        Vector2 pos = new Vector2(
            Random.Range(minBound.x, maxBound.x),
            Random.Range(minBound.y, maxBound.y)
        );
        Vector2 navMeshPos = GameManager.ClosestNavMeshPos(pos);

        return Instantiate(animal, navMeshPos, Quaternion.identity, animalParent);
    }
}
