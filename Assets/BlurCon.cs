using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class BlurCon : MonoBehaviour
{
    IPlayer iPlayer;
    public PostProcessVolume volume;
    void Start()
    {
        iPlayer = GameObject.FindWithTag("Player").GetComponent<IPlayer>();
        
    }

    void Update()
    {
        
    }
}
