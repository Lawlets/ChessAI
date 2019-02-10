using UnityEngine;
using System.Collections;

/*
 * Simple static camera
 */

public class CameraLookAt : MonoBehaviour {

    [SerializeField]
    private Transform lookAt = null;
    [SerializeField]
    private float lookAtZ = 0.5f;
    [SerializeField]
    private float height = 32f;

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        Vector3 position = transform.position;
        position.y = height;
        transform.position = position;
        transform.LookAt(lookAt.position + Vector3.up * lookAtZ);
	}
}
