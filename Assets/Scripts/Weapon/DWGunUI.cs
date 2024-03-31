using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DWGunUI : MonoBehaviour
{
    private Canvas _canvas;
    private Player _player;
    public DualWieldGun gun;
    public float shiningTime;
    
    public Slider temperatureSlider;
    
    private float _temperatureMax;
    
    private Color startColor = Color.cyan;
    private Color endColor = Color.red;
    private Image fill;

    private float timer;
    private bool changeColor;
    
    void Start()
    {
        _canvas = GetComponent<Canvas>();
        _player = GameObject.FindWithTag("Player").GetComponent<Player>();
        _canvas.worldCamera = Camera.main;
        _temperatureMax = gun.data.temperatureMax;
        fill = temperatureSlider.fillRect.GetComponent<Image>();
    }

    void Update()
    {
        ShowTemp();
    }

    void ShowTemp()
    {
        float value = gun.currentTemperature / _temperatureMax;
        temperatureSlider.value = value;
        Color color = Color.Lerp(startColor, endColor, value);
        if (gun.isReachMax)
        {
            timer += Time.deltaTime;
            if (timer >= shiningTime)
            {
                changeColor= !changeColor;
                timer = 0;
            }
            fill.color=changeColor?Color.black:color;
        }
        else
        {
            fill.color = color;
        }
    }
}
