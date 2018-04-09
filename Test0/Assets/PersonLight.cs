using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersonLight : MonoBehaviour
{

    public int i;

    [SerializeField]
    public  Lighting[]
        Plight;
    void Start()
    {
        startlight();
    }

    // Update is called once per frame
    void Update()
    {
      
    }

    [System.Serializable]
    public   struct Lighting
    {
  
        public  GameObject[]
            ll;
    }

    IEnumerator plighting()
    {
        for ( i =0; i >-1 ; i++)
        {
          //  print(i);

            Plight[i].ll[0].SetActive(true);
            Plight[i].ll[1].SetActive(true);
            
            yield return new WaitForSeconds(0.1f);

            Plight[i].ll[0].SetActive(false );
            Plight[i].ll[1].SetActive(false);
            if (i >=8)
            {
                i = -1;
            }
        }

    }

    public void startlight()
    {
        StartCoroutine(plighting());
    }
}
