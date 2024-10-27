using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Airship_Circle : MonoBehaviour
{
    public float angulerSpeed;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        transform.Rotate(Vector3.forward, angulerSpeed * Time.deltaTime);
    }
}
