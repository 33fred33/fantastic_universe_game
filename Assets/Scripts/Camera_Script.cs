using UnityEngine;
using System.Collections;

public class Camera_Script : MonoBehaviour {

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
        float Value = Input.GetAxis("Horizontal");
        transform.Translate(Input.GetAxis("Horizontal") * 0.1f, 0, Input.GetAxis("Vertical") * 0.1f);

	}
}
