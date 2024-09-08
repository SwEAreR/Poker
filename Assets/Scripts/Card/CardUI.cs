using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardUI : MonoBehaviour,IPointerDownHandler,IPointerEnterHandler
{
    private Card card;
    private bool isSelected;
    private bool isUp;
    private Color darkColor = new Color(0.6f, 0.6f, 0.6f);
    private Color lightColor = Color.white;

    private Image img;

    public bool IsSelected
    {
        get => isSelected;
        set
        {
            isSelected = value;
            img.color = isSelected ? darkColor : lightColor;
        }
    }

    private void Start()
    {
        card = CardManager.GetCard(gameObject.name);
        img = GetComponent<Image>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            GameManager.isPressing = false;
            if (IsSelected)
            {
                IsSelected = false;
                transform.localPosition += isUp ? -Vector3.up * 30 : Vector3.up * 30;
                if (isUp)
                    GameManager.selectedCards.Remove(card);
                else
                    GameManager.selectedCards.Add(card);
                isUp = !isUp;
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        GameManager.isPressing = true;
        IsSelected = !IsSelected;
        Debug.Log($"OnPointerDown\nisPressing:{GameManager.isPressing}\nIsSelected:{IsSelected}");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (GameManager.isPressing)
        {
            IsSelected = !IsSelected;
        }
        Debug.Log($"OnPointerEnter\nisPressing:{GameManager.isPressing}\nIsSelected:{IsSelected}");
    }
}