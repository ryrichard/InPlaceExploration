using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    Transform userTransform;
    Transform camera;
    Vector3 pos;
    float y = 7.0f;

    // Start is called before the first frame update
    void Start()
    {
        userTransform = GameObject.Find("User").GetComponent<Transform>();
        camera = this.gameObject.GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        camera.position = new Vector3(userTransform.position.x, y, userTransform.position.z);;
    }
}
