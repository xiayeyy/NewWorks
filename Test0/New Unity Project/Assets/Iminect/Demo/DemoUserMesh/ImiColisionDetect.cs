using UnityEngine;
using System.Collections;

public class ImiColisionDetect : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
    
    void OnTriggerEnter(Collider hitObject)
    {
        Debug.Log("hit object name :"+hitObject.transform.gameObject.name);
        if (hitObject.transform.gameObject.name == "Human mesh")
        {
            //hitObject.transform.gameObject.GetComponent<Renderer>().material = Resources.Load<Material>("DepthMask"); ;
            hitObject.transform.gameObject.GetComponent<Renderer>().material = new Material(Shader.Find("Diffuse"));
            Debug.Log("hit the cube!");     
        }
    }


    void OnTriggerExit(Collider hitObject)
    {

        if (hitObject.transform.gameObject.name == "Human mesh")
        {
            hitObject.transform.gameObject.GetComponent<Renderer>().material = Resources.Load<Material>("DepthMask"); ;
            Debug.Log("hit the cube  exit!");
        }
    }


    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collision is " + collision.collider.name);
    }

}
