using UnityEngine;
using System.Collections;
using Assets.Scripts;



public class MovementScript : MonoBehaviour {
	
	// Update is called once per frame
	void Update () {
	    
	}

    void OnMouseOver()
    {
    }

    void OnMouseDown()
    {
        //Destroy(gameObject);
        gameObject.SetActive(false);
    }

    
}
