using UnityEngine;
using System.Collections;

public class AlignSelector : MonoBehaviour {

    public Camera ActiveCamera;
    private float CameraDistance = 0.0f;
    private MeshRenderer MyMeshRenderer;
    public float CameraDistanceScale;

    void Awake()
    {
    }

    void Start ()
    {
        MyMeshRenderer = GetComponent<MeshRenderer>();
        MyMeshRenderer.enabled = false;
        ActiveCamera = Camera.main;
    }


	void OnEnable () {
        CameraDistance = (ActiveCamera.transform.position - transform.position).magnitude;
        this.transform.localScale = new Vector3(CameraDistanceScale * CameraDistance, 0, CameraDistanceScale * CameraDistance);
        Debug.Log("Camera Distance Initial" + CameraDistance);
    }
}
