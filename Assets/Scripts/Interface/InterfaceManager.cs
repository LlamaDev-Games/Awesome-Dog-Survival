using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class InterfaceManager : MonoBehaviour
{
    InterfaceElement[] interfaceElements;
    void Awake(){
        interfaceElements = FindObjectsOfType<InterfaceElement>(true);

        if (interfaceElements.Length == 0){
            this.enabled = false;
        }

        for (int i = 0; i < interfaceElements.Length; i++){
            foreach (var anim in interfaceElements[i].Animations){
                anim.element = interfaceElements[i];
            }

            foreach (var hoverAnim in interfaceElements[i].OnHover){
                hoverAnim.element = interfaceElements[i];
                if (hoverAnim.setBaseStateOnAwake){
                    hoverAnim.baseState = new HoverAnimState(
                        interfaceElements[i].GetComponent<RectTransform>().anchoredPosition,
                        interfaceElements[i].transform.localScale,
                        interfaceElements[i].GetComponent<RectTransform>().localRotation.z,
                        interfaceElements[i].gameObject.GetComponentInChildren<Image>().color,
                        interfaceElements[i].gameObject.activeSelf
                    );
                }
            }

            foreach (var anim in interfaceElements[i].OnClick){
                anim.element = interfaceElements[i];
            }

            foreach (var anim in interfaceElements[i].OnSwitch){
                if (anim.setToBaseOnStart){
                    anim.SetToBase();
                }
            }
        }
    }

    void Start(){
        
    }

    void Update(){
    }
}

#region [AnimState]

//Animation state base class
[System.Serializable] public class BaseAnimState{
    public Vector2 position = Vector2.zero;
    public Vector2 scale = Vector2.one;
    public float rotation = 0;
    public Color color = Color.white;
    public bool active = true;
}

//Animation state class for hover & swtich animations
[System.Serializable] public class HoverAnimState : BaseAnimState{
    public HoverAnimState(Vector2 _position, Vector2 _scale, float _rotation, Color _color, bool _active){
        position = _position;
        scale = _scale;
        color = _color;
        rotation = _rotation;
        active = _active;
    }
}

//Animation state class for normal animations
[System.Serializable]
public class AnimState : BaseAnimState{
    public float time;
    public LeanTweenType easingType;
    public AnimState(Vector2 _position, Vector2 _scale, float _rotation, Color _color, float _time, LeanTweenType _easingType){
        position = _position;
        scale = _scale;
        color = _color;
        rotation = _rotation;
        time = _time;
        easingType = _easingType;
    }
}
#endregion

#region [UiAnimation]

[System.Serializable]
public class UIAnimationBase{
    public string Name;
    public InterfaceElement element;
    public virtual UIAnimationBase Reset(){
        LeanTween.cancel(element.gameObject);
        return this;
    }

    protected void SetColour(Color color){
        element.GetComponent<Image>().color = color;
    }
}

[System.Serializable]
public class UIAnimation : UIAnimationBase{
    public AnimState[] states;

    public bool resetOnFinish;
    public bool loop;

    public override UIAnimationBase Reset(){
        LeanTween.cancel(element.gameObject);
        PlayState(0, false, true);
        return this;
    }

    public UIAnimationBase Play(bool reverse=false){
        Reset();
        PlayState(1, reverse);
        return this;
    }

    private UIAnimationBase PlayState(int state, bool reverse, bool reset=false){
        int previousState = reverse ? state+1 : state-1;

        if (previousState < 0) { previousState = states.Length-1; }
        else if (previousState >= states.Length) { previousState = 0; }

        if (state < 0){
            if (!loop) return this;
            state = states.Length-1;
        }
        else if (state >= states.Length){
            if (!loop) return this;
            state = 0;
        }

        float time = reset ? 0f : states[state].time;

        LeanTween.move(element.GetComponent<RectTransform>(), states[state].position, time).setEase(states[state].easingType)
        .setOnComplete(action => { if (reverse) PlayState(state-1, reverse); else PlayState(state+1, reverse); } );
        LeanTween.scale(element.GetComponent<RectTransform>(), states[state].scale, time).setEase(states[state].easingType);
        LeanTween.rotateLocal(element.gameObject, new Vector3(0, 0, states[state].rotation), time).setEase(states[state].easingType);
        LeanTween.value(element.gameObject, SetColour, states[previousState].color, states[state].color, time).setEase(states[state].easingType);
        return this;
    }
}

[System.Serializable]
public class UISwitchAnimation : UIAnimationBase{
    public HoverAnimState baseState;
    public HoverAnimState switchState;
    private bool switched;
    public LeanTweenType easingType;
    public float time;
    public float delay;

    public bool setToBaseOnStart;

    public UIAnimationBase SwitchState(){
        if (switched) SwitchToBase(); else SwitchToSwitched();
        switched = !switched;
        return this;
    }

    public UIAnimationBase SetToBase(){
        element.GetComponent<RectTransform>().localScale = baseState.scale;
        element.GetComponent<RectTransform>().anchoredPosition = baseState.position;
        LeanTween.rotateLocal(element.gameObject, new Vector3(0, 0, baseState.rotation), 0);
        SetColour(baseState.color);
        element.gameObject.SetActive(baseState.active);
        return this;
    }

    private UIAnimationBase SwitchToBase(){
        
        LeanTween.scale(element.GetComponent<RectTransform>(), baseState.scale, time).setEase(easingType);
        LeanTween.move(element.GetComponent<RectTransform>(), baseState.position, time).setEase(easingType);
        LeanTween.rotateLocal(element.gameObject, new Vector3(0, 0, baseState.rotation), time).setEase(easingType);
        LeanTween.value(element.gameObject, SetColour, switchState.color, baseState.color, time).setEase(easingType).setOnComplete(action => { element.gameObject.SetActive(baseState.active); });
        return this;
    }

    private UIAnimationBase SwitchToSwitched(){
        LeanTween.scale(element.GetComponent<RectTransform>(), switchState.scale, time).setEase(easingType);
        LeanTween.move(element.GetComponent<RectTransform>(), switchState.position, time).setEase(easingType);
        LeanTween.rotateLocal(element.gameObject, new Vector3(0, 0, switchState.rotation), time).setEase(easingType);
        LeanTween.value(element.gameObject, SetColour, baseState.color, switchState.color, time).setEase(easingType).setOnComplete(action => { element.gameObject.SetActive(switchState.active); });
        return this;
    }
}

[System.Serializable]
public class UIHoverAnimation : UIAnimationBase{
    public HoverAnimState onHoverState;
    public HoverAnimState baseState;
    public bool setBaseStateOnAwake;
    public bool lockPosition;
    public float time;
    public LeanTweenType easingType;

    public UIAnimationBase Enter(){
        LeanTween.scale(element.GetComponent<RectTransform>(), onHoverState.scale, time).setEase(easingType);
        if (!lockPosition) LeanTween.move(element.GetComponent<RectTransform>(), onHoverState.position, time).setEase(easingType);
        LeanTween.rotateLocal(element.gameObject, new Vector3(0, 0, onHoverState.rotation), time).setEase(easingType);
        LeanTween.value(element.gameObject, SetColour, baseState.color, onHoverState.color, time).setEase(easingType).setOnComplete(action => { element.gameObject.SetActive(onHoverState.active); });
        return this;
    }

    public UIAnimationBase Exit(){
        LeanTween.scale(element.GetComponent<RectTransform>(), baseState.scale, time).setEase(easingType);
        if (!lockPosition) LeanTween.move(element.GetComponent<RectTransform>(), baseState.position, time).setEase(easingType);
        LeanTween.rotateLocal(element.gameObject, new Vector3(0, 0, baseState.rotation), time).setEase(easingType);
        LeanTween.value(element.gameObject, SetColour, onHoverState.color, baseState.color, time).setEase(easingType).setOnComplete(action => { element.gameObject.SetActive(baseState.active); });
        return this;
    }
}

#endregion