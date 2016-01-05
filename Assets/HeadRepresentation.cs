using UnityEngine;
using System.Collections;

public class HeadRepresentation : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        transform.localPosition  = GameObject.Find("Main Camera").transform.localPosition;
	}
}
