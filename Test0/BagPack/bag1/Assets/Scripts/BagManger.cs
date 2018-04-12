using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class BagManger : MonoBehaviour
{
    private static BagManger instance;
    public static BagManger Instance { get { return instance; } }

    public PanelView panelview;
    public TextUI textui;
    public DragItemUI dragitemui;

    private bool Isshow;
    private bool Isdrag;

    void Awake()
    {
        instance = this;

        Load();//数据

        //事件
        GridUI.OnEnter += GridUI_OnEnter;
        GridUI.OnExit += GridUI_OnExit;
        GridUI.OnDrag += GridUI_OnDrag;
        GridUI.ExitDrag += GridUI_ExitDrag;
    }


    void Update()
    {
        if (Isdrag)
        {
            dragitemui.ShowText();
            dragitemui.transform.position = Input.mousePosition;
        }


        else if (Isshow)
        {
            textui.ShowText();
            textui.transform.position = Input.mousePosition;
        }

    }

    private Dictionary<int, Item> ItemList;

    public void StoreItem(int itemid)
    {
        if (!ItemList.ContainsKey(itemid))
        {
            return;
        }

        Transform traimg = panelview.GetGrid();

        if (traimg == null)
        {
            Debug.Log("背包满了！！");
            return;
        }

        Item temp = ItemList[itemid];
        CreatNewItem(temp, traimg);

    }
    private void Load()
    {
        ItemList = new Dictionary<int, Item>();

        Weapon w1 = new Weapon(0, "牛刀", "牛B的刀！", 20, 10, "", 100);
        Weapon w2 = new Weapon(1, "羊刀", "杀羊刀。", 15, 10, "", 20);
        Weapon w3 = new Weapon(2, "宝剑", "大宝剑！", 120, 50, "", 500);
        Weapon w4 = new Weapon(3, "军枪", "可以对敌人射击，很厉害的一把枪。", 1500, 125, "", 720);

        Consumable c1 = new Consumable(4, "红瓶", "加血", 25, 11, "", 20, 0);
        Consumable c2 = new Consumable(5, "蓝瓶", "加蓝", 39, 19, "", 0, 20);

        Armor a1 = new Armor(6, "头盔", "保护脑袋！", 128, 83, "", 5, 40, 1);
        Armor a2 = new Armor(7, "护肩", "上古护肩，锈迹斑斑。", 1000, 0, "", 15, 40, 11);
        Armor a3 = new Armor(8, "胸甲", "皇上御赐胸甲。", 153, 0, "", 25, 30, 11);
        Armor a4 = new Armor(9, "护腿", "预防风寒，从腿做起", 999, 60, "", 19, 30, 51);

        ItemList.Add(w1.Id, w1);
        ItemList.Add(w2.Id, w2);
        ItemList.Add(w3.Id, w3);
        ItemList.Add(w4.Id, w4);
        ItemList.Add(c1.Id, c1);
        ItemList.Add(c2.Id, c2);
        ItemList.Add(a1.Id, a1);
        ItemList.Add(a2.Id, a2);
        ItemList.Add(a3.Id, a3);
        ItemList.Add(a4.Id, a4);

    }

    //事件回调
    public void GridUI_OnEnter(Transform transform)
    {
        Item item = ItemModel.GetItem(transform.name);
        if (item == null)
        { return; }
        string text = GetTooltipText(item);
        textui.UpdateText(text);
        Isshow = true;
    }

    private void GridUI_OnExit()
    {
        textui.HideText();
        Isshow = false;
    }



    private void GridUI_OnDrag(Transform transform)
    {
        if (transform.childCount == 0)
        {
           // return;
        }
        else
        {

            dragitemui.ShowText();

            Item item = ItemModel.GetItem(transform.name);

            dragitemui.NameUpdate(item.Name);
            dragitemui.ImgUpdate(item.Icon);
            Isdrag = true;
            Destroy(transform.GetChild(0).gameObject);
          
        }
    }
    private void GridUI_ExitDrag(Transform lasttransform, Transform transform)
    {
        Isdrag = false;
        dragitemui.HideText();

        if (transform == null)   //ui 外
        {
            ItemModel.DeleteItem(lasttransform.name);
            Debug.Log("物品已移除");
        }
        else if (transform.tag == "Grid")  //拖到格子里
        {
            if (transform.childCount == 0)   //空格子
            {
                Item item = ItemModel.GetItem(lasttransform.name);
                if (item!=null)
                ItemModel.DeleteItem(lasttransform.name);
                CreatNewItem(item, transform);
            }
            else          //已存在交换
            {
                //删除原来的物品
                Destroy(transform.GetChild(0).gameObject);
                //获取数据
                Item prevGirdItem = ItemModel.GetItem(lasttransform.name);
                Item enterGirdItem = ItemModel.GetItem(transform.name);
                //交换的两个物体
                this.CreatNewItem(prevGirdItem, transform);
                this.CreatNewItem(enterGirdItem, lasttransform);
            }
        }
        else      //格子外
        {
           // Item item = ItemModel.GetItem(lasttransform.name);

           // CreatNewItem(item, lasttransform);
        }
    }

    public void CreatNewItem(Item item, Transform parent)
    {
        if (item !=null )
        {
            GameObject itemimg = Instantiate(Resources.Load<GameObject>("Prefab/Item"), parent);
            itemimg.transform.localPosition = Vector3.zero;
            itemimg.transform.localScale = Vector3.one;

            itemimg.GetComponent<ItemUI>().NameUpdate(item.Name);
            // itemimg.GetComponent<ItemUI>().ImgUpdate(item.Icon);

            ItemModel.StoreItem(parent.name, item);
        }
    }

    private string GetTooltipText(Item item)
    {
        if (item == null)
            return "";
        StringBuilder strB = new StringBuilder();
        strB.AppendFormat("<color=red>{0}</color>\n\n", item.Name);
        switch (item.ItemType)
        {
            case "Armor":
                Armor armor = item as Armor;
                strB.AppendFormat("力量:{0}\n防御:{1}\n敏捷:{2}\n\n", armor.Power, armor.Defend, armor.Agility);
                break;
            case "Consumable":
                Consumable consumable = item as Consumable;
                strB.AppendFormat("HP:{0}\nMP:{1}\n\n", consumable.BackHp, consumable.BackMp);
                break;
            case "Weapon":
                Weapon weapon = item as Weapon;
                strB.AppendFormat("攻击:{0}\n\n", weapon.Damage);
                break;
            default:
                break;
        }
        strB.AppendFormat("<size=25><color=green>购买价格：{0}\n出售价格：{1}</color></size>\n\n<color=yellow><size=20>描述：{2}</size></color>", item.BuyPrice, item.SellPrice, item.Description);
        return strB.ToString();
    }
}
