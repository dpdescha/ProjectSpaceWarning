using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    public GameObject Player1;
    public GameObject Player2;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        float averageX = (Player1.transform.position.x + Player2.transform.position.x) / 2;
        transform.position = new Vector3(averageX, transform.position.y, transform.position.z);
	}
}
