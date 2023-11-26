using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    public Transform CamTrans;
    public float smoothness = 0.5f;
    [Header("摇摆")] 
    public Transform weaponSwayObject;

    public float swayAmountA = 1;
    public float swayAmountB = 2;
    public float swayScale = 600;
    public float swayLerpSpeed = 14;

    public float swayTime;
    public Vector3 swayPosition;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        AnimCon();
        CalculateWeaponSway();
    }

    void AnimCon()
    {
        transform.position = Vector3.Lerp(transform.position, CamTrans.position, smoothness * Time.deltaTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, CamTrans.rotation, smoothness * Time.deltaTime);
    }

    private void CalculateWeaponSway()
    {
        var targetPosition = LissajousCurve(swayTime, swayAmountA, swayAmountB)/swayScale;

        swayPosition = Vector3.Lerp(swayPosition, targetPosition, Time.smoothDeltaTime);

        swayTime += Time.deltaTime;

        if (swayTime>6.3f)
        {
            swayTime = 0;
        }
        weaponSwayObject.localPosition = swayPosition;
    }

    private Vector3 LissajousCurve(float Time, float A, float B)
    {
        return new Vector3(Mathf.Sin(Time), A * Mathf.Sin(B * Time + Mathf.PI));
    }
}
