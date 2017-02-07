using UnityEngine;
using System.Collections;

public class FixFrameRate : MonoBehaviour {

    public int frameRate = 60;

	// Use this for initialization
	void Start () {
	
	}

    void Awake()
    {
        Application.targetFrameRate = frameRate;
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
