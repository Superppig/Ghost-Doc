using System;
using DG.Tweening;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [Header("黑板")] 
    public Player player;

    private PlayerBlackboard _playerBlackboard;
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

    private void Awake()
    {
        _playerBlackboard = player.playerBlackboard;
        CamTrans = _playerBlackboard.cam.transform;
    }

    private void Update()
    {
        CalculateWeaponSway();
    }

    private void CalculateWeaponSway()
    {
        var targetPosition = LissajousCurve(swayTime, swayAmountA, swayAmountB)/swayScale;

        swayPosition = Vector3.Lerp(swayPosition, targetPosition, Time.smoothDeltaTime);

        swayTime += Time.deltaTime;

        if (swayTime>2*Mathf.PI)
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
