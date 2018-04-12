using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelView : MonoBehaviour {
    public Transform[] GridTra;


    public Transform  GetGrid()
    {
        for (int i = 0; i < GridTra.Length; i++)
        {
            if (GridTra[i].childCount == 0)
            {
                return GridTra[i];
            }
        }
        return null;
    }
}
