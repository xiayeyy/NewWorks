using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemUI : MonoBehaviour
{
    public Text itemText;
    public Image itemImg;

    void Start()
    {
        itemImg = GetComponent<Image>();
        itemText = GetComponentInChildren<Text>();

        //itemImg.GetComponent<Image>().sprite = Resources.Load<Sprite>("ada");
        //itemImg.GetComponent<Image>().sprite = Resources.Load("ada", typeof(Sprite)) as Sprite;
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            //this.GetComponent<Image>().sprite = Resources.Load<Sprite>("Img/timg");
            //this.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Img/timg");


            itemImg.sprite = Resources.Load<Sprite>("Img/zb");

            print(1);
        }
    }

    public void NameUpdate(string name)
    {
        itemText.text = name;
    }

    public void ImgUpdate(string name)
    {
        itemImg.sprite = Resources.Load<Sprite>(name);
    }
}
