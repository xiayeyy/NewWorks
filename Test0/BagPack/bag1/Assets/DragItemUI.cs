using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragItemUI : ItemUI  {


	void Start () {
		
	}
	

	void Update () {
		
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
