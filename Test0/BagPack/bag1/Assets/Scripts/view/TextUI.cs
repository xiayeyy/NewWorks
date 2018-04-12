using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextUI : MonoBehaviour {

    public Text t1; 

    public void UpdateText(string text )
    {
        t1.text = text;
    }

    public void ShowText()
    {
        gameObject.SetActive(true);
    }
    public void HideText()
    {
        gameObject.SetActive(false);
    } 
}
