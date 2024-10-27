using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Airship_SpotLight_Base : MonoBehaviour
{
    public Transform playerPosition;


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
        transform.LookAt(playerPosition.transform, Vector3.up);
    }
}
