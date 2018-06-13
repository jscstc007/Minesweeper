using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AButton : Button {

    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            Debug.Log("左键点击");
            //TODO
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            Debug.Log("右键点击");
            //TODO
        }
    }
}
