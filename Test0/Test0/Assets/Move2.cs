using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Move2 : MonoBehaviour
{
    public float aa;
    public Image Tag;
    public GameObject Box;


    public float Bx = -80;
    public float By = -140;
    // Use this for initialization
    void Start()
    {

       // Box.transform.position = Tag.transform.position;
        //Tag.GetComponent<RectTransform>().sizeDelta =
        //Tag.GetComponent<RectTransform>().localScale=
        //Box.transform.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, 0);
        // Box.transform.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, 0, 0);
         AnchorChange();

       // Box.transform.localPosition = new Vector3(Bx, By, aa);


       Box.transform. localPosition = new Vector3(Bx, By, aa);

    }
        // Update is called once per frame
        void Update()
        {
        //Box.transform.position = new Vector3 ( Tag.transform.position.x -5,Tag .transform .position .y-10, Box.transform.position.z );

        if (Input.GetKeyDown(KeyCode.L))
            {AnchorChange();
                Box.transform.localPosition = new Vector3(Bx, By, aa);
            
            }
            //Box.transform.position = new Vector3(Tag.transform.position.x, Tag.transform.position.y, Box.transform.position.z);
            // Box.transform.localPosition  = new Vector3((-80/t1 .position .x )*t2.position .x , (-140 / t1.position.y) * t2.position.y, aa)
        }

    public void AnchorChange()
    {
        if (Bx < -Tag.GetComponent<RectTransform>().sizeDelta.x / 6)
        {
            Box.transform.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, 0);
        }
        if (Bx > Tag.GetComponent<RectTransform>().sizeDelta.x / 6)
        {
            Box.transform.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 0, 0);
        }
        if (By > Tag.GetComponent<RectTransform>().sizeDelta.y / 6)
        {
            Box.transform.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, 0);
        }

        if (By < -Tag.GetComponent<RectTransform>().sizeDelta.y / 6)
        {
            Box.transform.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, 0, 0);
        }
    }
    }
