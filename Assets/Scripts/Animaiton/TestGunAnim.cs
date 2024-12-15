using System;
using DG.Tweening;
using UnityEngine;

public class TestGunAnim : MonoBehaviour
{
    private AnimationController animationController;
    private void Awake()
    {
        animationController = GetComponent<AnimationController>();
    }

    private void Start()
    {
        animationController.Play("breath");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            animationController.Play("shoot");
            float timer = 0;
            DOTween.To(() => timer, x => timer = x, 0.5f, 0.5f).OnComplete(() =>
            {
                animationController.Play("breath");
            });
        }
    }
}