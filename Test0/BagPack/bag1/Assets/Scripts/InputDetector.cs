using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputDetector : MonoBehaviour {
    
	void Start () {
		
	}
	
	void Update () {
        if (Input.GetMouseButtonDown(1))
        {
            int index = Random.Range(0, 9);
            BagManger.Instance.StoreItem(index);
        }
	}
}
