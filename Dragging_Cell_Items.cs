using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

// Добавить компонент на каждую клетку в инвентаре чтобы с ней взаимодействовать
public class Dragging_Cell_Items : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    public Inventory own_inventory;
    Showing_Info current_info; // Showing_Info это тип компонента отвечающего за всплывающие подсказки

    public void OnBeginDrag(PointerEventData eventData)
    {
        own_inventory.Begin_Drag_Item(eventData.pointerDrag.gameObject, eventData.pointerDrag.transform.parent.GetSiblingIndex());
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        own_inventory.End_Drag_Item(eventData.pointerDrag.gameObject, eventData.pointerEnter.gameObject, Camera.main.ScreenToWorldPoint(eventData.position));
    }

    void Start()
    {
        EventTrigger trigger = GetComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerDown;
        entry.callback.AddListener((data) => { OnPointerDownDelegate((PointerEventData)data); });
        trigger.triggers.Add(entry);
    }

    // если кликнуть мышкой по клетке инвентаря с иконкой какой-либо вещи то мы используем вещь согласно ее Use_Item()
    // в данном случае если в клетке зелье лечения то персонаж его выпьет
    public void OnPointerDownDelegate(PointerEventData data)
    {
        if (!transform.name.StartsWith("Image_cell"))
        {
            if (Input.GetMouseButton(1))
            {
                Stats s = GetComponent<Showing_Info>().stats;
                if (s.object_link.GetComponent<Heal_Potion>() != null)
                {
                    s.object_link.GetComponent<Heal_Potion>().Use_Item(own_inventory.transform);
                    // Heal_Potion тип компонента который реализует все действия зелья на персонажа в зависимости от типа хилки (не нужно на каждый тип хилки писать свой тип компонента)
                }
            }
        }
    }
}
