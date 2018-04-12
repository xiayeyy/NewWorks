using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;


public class GridUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{

    public static Action<Transform> OnEnter;
    public static Action OnExit;

    public static Action<Transform> OnDrag;
    public static Action<Transform, Transform> ExitDrag;

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left && eventData.clickCount >= 1)
        {
            if (OnDrag != null)
            {
                OnDrag(transform);
            }
        }
    }


    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left && eventData.clickCount >=1)
        {
            if (ExitDrag != null)
            {
                if (eventData.pointerEnter == null)
                {
                    ExitDrag(transform, null);
                }
                else
                    ExitDrag(transform, eventData.pointerEnter.transform);
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerEnter.tag == "Grid" )
        {
            if (OnEnter != null)
            {
                OnEnter(transform);
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (eventData.pointerEnter.tag == "Grid" )
        {
            if (OnExit != null)
            {
                OnExit();
            }
        }
    }

    void IDragHandler.OnDrag(PointerEventData eventData)
    {

    }
}
