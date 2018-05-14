using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void MyDelegateEventHandler();
public class DelegateTest : MonoBehaviour {

    // Use this for initialization
    MyDelegateEventHandler myDelegate;
	void Start () {
       myDelegate += Test;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    public void Test()
    {

    }
}
