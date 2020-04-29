using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Game_Menu_Controller; // передает дату между сценами

// Добавить компонент любому персонажу у которого должен быть свой инвентарь и возможность экипироваться предметами (items)
public class Inventory : MonoBehaviour 
{
    public bool npc;
    public GameObject equipped_items_panel; // обьект из грида и нескольких картинок, играет роль "манекена" отображая одетые вещи
    public GameObject bag_items_panel; // аналогично equipped_items_panel, только отображает иконки вещей в инвентаре, оба размещены рядом на UI Canvas как диаблоподобный инвентарь
    public List<Stats> items;
    public List<Stats> equipped_items;
    Person_controller own_controller;
    string old_name_draging_item;
    public GameObject all_terrain_items; // root элемент для всех "лежащих на земле" вещей на сцене

    private void Start()
    {
        if (Scenes.getParam("load") == "false") // инициализация при отсутствии файла сохранения игры
        {
            items = new List<Stats>(25);
            equipped_items = new List<Stats>(16);
        }
        old_name_draging_item = string.Empty;
        own_controller = transform.GetComponent<Person_controller>();
    }

    public void Add_item(Stats stats) // Stats это компонент-скрипт, который хранит дату вещи (у каждой свой Stats)
    {
        items.Add(stats);
        Render_inventory_panel();
    }

    public void Remove_item(Stats stats, Vector2 place, GameObject drag_item)
    {
        items.Remove(stats);
        stats.object_link.GetComponent<Inventory_Item>().Drop_Item(place);
        Render_inventory_panel();
    }

    public void Equip_item(Stats stats)
    {
        if (!equipped_items.Contains(stats))
        {
            equipped_items.Add(stats);
        }
        Render_equip_inventory_panel(stats);
    }

    void Disequip_item(Stats stats, GameObject drag_item)
    {
        equipped_items.Remove(stats);
        stats.object_link.transform.SetParent(all_terrain_items.transform);
        stats.object_link.GetComponent<BoxCollider2D>().enabled = true;
        stats.object_link.GetComponent<SpriteRenderer>().sortingOrder = 90;
        stats.object_link.GetComponent<Inventory_Item>().is_using = true;
        stats.object_link.SetActive(false);
    }

    // Render_inventory_panel() отрисовывает вещи в инвентаре, каждый child bag_items_panel это дерево из вложенных Image, 
    // верхний служит бекграундом клетки инвентаря, нижний хранит иконку вещи либо повторяет бекграунд если клетка пустая
    public void Render_inventory_panel() 
    {
        for (int i = 0; i < bag_items_panel.transform.childCount; i++) // стирает предыдущую неактуальную отрисовку
        {
            Transform temp = bag_items_panel.transform.GetChild(i).GetChild(0);
            Image temp_image = temp.GetComponent<Image>();
            temp_image.name = temp.GetChild(0).name;
            temp_image.sprite = temp.GetChild(0).GetComponent<SpriteRenderer>().sprite;
            temp.GetComponent<Showing_Info>().stats = null;
        }

        for (int i = 0; i < items.Count; i++) // рисует иконки в инвентаре "с чистого листа"
        {
            Transform temp = bag_items_panel.transform.GetChild(i).GetChild(0);
            Image temp_image = temp.GetComponent<Image>();
            temp_image.sprite = items[i].icon;
            temp_image.name = items[i].name;
            temp.GetComponent<Showing_Info>().stats = items[i];
        }
    }

    // Render_equip_inventory_panel() отрисовывает экипированные вещи в инвентаре-манекене, каждый child equipped_items_panel это дерево из 3 вложенных Image, 
    // верхний служит бекграундом клетки инвентаря, средний хранит Placeholder который изображает что за вещь может быть экипирована в клетку, 
    // нижний повторяет Placeholder либо отображает иконку вещи
    void Render_equip_inventory_panel(Stats stats)
    {
        for(int i=0; i < equipped_items_panel.transform.childCount; i++)
        {
            Transform child = equipped_items_panel.transform.GetChild(i).GetChild(0);
            if (child.tag == stats.tag && child.GetChild(0).name.StartsWith("Placeholder_"))
            {
                Transform temp = child.GetChild(0);
                temp.GetComponent<Image>().sprite = stats.icon;
                temp.name = stats.name;
                temp.GetComponent<Showing_Info>().stats = stats;
                items.Remove(stats);
                Render_equipped_item_model(stats);
                break;
            }
        }
    }

    // Render_equipped_item_model() отрисовывает экипированные вещи на модельке персонажа на сцене поверх нужных частей тела. Использую разные layer для размещения вещей, 
    // информацию для отображения беру из специльного скрипта Stats, который хранит дату вещи(у каждой свой)
    void Render_equipped_item_model(Stats stats)
    {
        Transform bodypart = transform.Find(stats.bodypart_to_equip_path);

        stats.object_link.transform.position = bodypart.position;
        stats.object_link.transform.SetParent(bodypart);
        stats.object_link.GetComponent<SpriteRenderer>().sortingOrder = 105;
        stats.object_link.GetComponent<BoxCollider2D>().enabled = false;
        stats.object_link.GetComponent<Inventory_Item_>().is_using = true;
        stats.object_link.SetActive(true);
    }

    // Перетягивание вещей мышкой
    public void Begin_Drag_Item(GameObject drag_item, int drag_item_index)
    {
        old_name_draging_item = drag_item.name;

        if (!drag_item.name.StartsWith("Placeholder_") && !drag_item.name.StartsWith("Image_"))
        {
            drag_item.name = drag_item.transform.GetChild(0).name;
            drag_item.GetComponent<Image>().sprite = drag_item.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite;
        }
    }

    public void End_Drag_Item(GameObject drag_item, GameObject target_drag_item, Vector2 pointer_position)
    {
        if (!old_name_draging_item.StartsWith("Placeholder_") && !old_name_draging_item.StartsWith("Image_")) // выброс из инвентаря
        {
            if (target_drag_item.tag == "terrain")
            {
                Remove_item(items.Find(x => x.name == old_name_draging_item), pointer_position, drag_item);
            }
            Render_inventory_panel();
        }

        if (target_drag_item.name.StartsWith("Placeholder_") && !drag_item.transform.IsChildOf(equipped_items_panel.transform) && !old_name_draging_item.StartsWith("Image_")) // экипировать
        {
            Stats temp = drag_item.GetComponent<Showing_Info>().stats;
            Equip_item(temp);
            Render_inventory_panel();
        }

        if (!target_drag_item.name.StartsWith("Placeholder_") && drag_item.transform.IsChildOf(equipped_items_panel.transform) && !old_name_draging_item.StartsWith("Image_")) // снять экипировку
        {
            Stats temp = drag_item.GetComponent<Showing_Info>().stats;
            Disequip_item(temp, drag_item);
            Add_item(temp);
            Render_inventory_panel();
        }
    }
}
