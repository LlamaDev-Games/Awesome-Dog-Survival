using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bush : Interactable
{
    //Private variables
    [SerializeField] private float timeBtwBerries;
    [SerializeField] private GameObject[] berrySprites;
    [SerializeField] private GameObject berryPrefab;
    [SerializeField] private float berrySpreadDist;
    private float timeUntilBerry;
    private int berries;

    // Update is called once per frame
    void Update()
    {
        //Grow berries over time
        timeUntilBerry -= Time.deltaTime;
        if (timeUntilBerry <= 0f && berries < berrySprites.Length){
            berries += 1;
            timeUntilBerry = timeBtwBerries;
            berrySprites[berries-1].SetActive(true);
        }
    }

    //Drops the grown berries as items
    private void DropBerries(){
        //Disable berry sprites
        foreach (GameObject berrySprite in berrySprites)
        {
            berrySprite.SetActive(false);
        }

        //Spawn berry items
        for (int i = 0; i < berries; i++)
        {
            Vector2 randomPos = Random.insideUnitCircle * Random.Range(0f, berrySpreadDist);
            Vector2 position = (Vector2)transform.position + randomPos;
            Instantiate(berryPrefab, position, Quaternion.identity);
        }

        //Reset bush
        berries = 0;
        timeUntilBerry = timeBtwBerries;
    }

    //Called when dog interacts with this object
    public override void Interact()
    {
        if (berries > 0){
            DropBerries();
        }
    }
}
