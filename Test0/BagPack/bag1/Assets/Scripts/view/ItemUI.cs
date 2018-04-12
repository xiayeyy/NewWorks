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
      

    }


    void Update()
    {
        
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
