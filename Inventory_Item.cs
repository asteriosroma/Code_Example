using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Добавить компонент на каждую вещь, которую можно положить в инвентарь
public class Inventory_Item : MonoBehaviour
{
    public Stats stats;
    public bool is_using;
    Inventory somebody_bag;

    // игрок вызывает ее кликом мышки по вещи
    public void Pick_up_item(Inventory bag)
    {
        if (!is_using)
        {
            somebody_bag = bag;
            is_using = true;
            transform.gameObject.SetActive(false);
            somebody_bag.Add_item(stats);
        }
    }

    public void Drop_Item(Vector2 place)
    {
        transform.position = place;
        is_using = false;
        transform.gameObject.SetActive(true);
    }
}
