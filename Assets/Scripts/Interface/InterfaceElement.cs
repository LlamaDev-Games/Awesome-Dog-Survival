using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;

public class InterfaceElement : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Space]
    public UIAnimation[] Animations;
    [Space]
    public UISwitchAnimation[] OnSwitch;
    [Space]
    public UIHoverAnimation[] OnHover;
    [Space]
    public UIAnimation[] OnClick;

    private InterfaceManager interfaceManager;

    private void Awake()
    {
        interfaceManager = FindObjectOfType<InterfaceManager>();
        if (!interfaceManager){
            Debug.LogWarning("No active interface manager in scene");
            this.enabled = false;
        }
    }

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        foreach (UIAnimation animation in OnClick)
        {
            animation.Play();
        }
    }

    public void OnPointerEnter(PointerEventData pointerEventData){
        foreach (var anim in OnHover)
        {
            anim.Enter();
        }
    }

    public void OnPointerExit(PointerEventData pointerEventData){
        foreach (var anim in OnHover)
        {
            anim.Exit();
        }
    }

    public void AnimationSwitchState(int switchAnimIndex){
        StartCoroutine(SwitchAfterDelay(OnSwitch[switchAnimIndex].delay, switchAnimIndex));
    }
    private IEnumerator SwitchAfterDelay(float delay, int switchAnimIndex){
        yield return new WaitForSeconds(delay);
        OnSwitch[switchAnimIndex].SwitchState();
    }  

    public void AnimationPlay(int animIndex){
        Animations[animIndex].Play();
    }

    public void AnimationReset(int animIndex){
        Animations[animIndex].Reset();
    }
}
