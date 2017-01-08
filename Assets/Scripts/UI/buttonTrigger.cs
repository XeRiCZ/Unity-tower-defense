using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public enum triggerState
{
    state_default,
    state_overlay,
    state_beginClicked,
    state_endClicked
};

public class buttonTrigger : EventTrigger {

    public triggerState thisState = triggerState.state_default;

    public override void OnPointerDown(PointerEventData data)
    {
        thisState = triggerState.state_beginClicked;
    }

    public override void OnPointerUp(PointerEventData data)
    {
        thisState = triggerState.state_endClicked;
    }

    public override void OnPointerEnter(PointerEventData data)
    {
        if (thisState != triggerState.state_beginClicked)
            thisState = triggerState.state_overlay;
    }

    public override void OnPointerExit(PointerEventData data)
    {
        if (thisState != triggerState.state_beginClicked)
            thisState = triggerState.state_default;
    }

  
}
