using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CD_Spin : MonoBehaviour
{
    public bool spinning;
    public float spinSpeed;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (spinning)
            transform.Rotate(Vector3.up * spinSpeed * Time.deltaTime, Space.Self);
    }
}
