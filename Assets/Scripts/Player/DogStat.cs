using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
//Class for a dog stat
public class DogStat
{
    public float value;
    public float maxValue;
    public float drainSpeed;
    public Slider slider;

    //Updates the UI element
    public void UpdateSlider(){
        slider.maxValue = maxValue;
        slider.value = value;
        value = Mathf.Clamp(value, 0, maxValue);
    }

    //Drains the current value depending on drain speed
    public void DrainValue(){
        if(value > 0){
            value -= drainSpeed * Time.deltaTime;
        }
    }
}

[System.Serializable]
//Class for all of the dog's stats
public class DogStats
{
    public DogStat health;
    public DogStat energy;
    public DogStat thirst;
    public DogStat hunger;

    //To be called in Update() within the Dog monobehaviour class
    public void Update(){
        UpdateSliders();
        DrainValues();
    }

    private void UpdateSliders(){
        health.UpdateSlider();
        energy.UpdateSlider();
        thirst.UpdateSlider();
        hunger.UpdateSlider();
    }

    private void DrainValues(){
        thirst.DrainValue();
        hunger.DrainValue();
    }
}
