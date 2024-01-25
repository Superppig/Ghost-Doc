using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerBaseUI : MonoBehaviour
{
    private Player player;
    private IPlayer iPlayer;
    
    private float maxHp;
    private float maxEnerge;
    private float currentHp;
    private float currentEnerge;


    private Slider health;
    private Slider energe1;
    private Slider energe2;
    private Slider energe3;

    private Image energeImage1;
    private Image energeImage2;
    private Image energeImage3;

    public Color energeColor1=Color.cyan;
    public Color energeColor2=Color.blue;

    public float MoveRange=0.2f;
    private Transform father;


    void Start()
    {
        player = GameObject.FindWithTag("Player").GetComponent<Player>();
        iPlayer=player.GetComponent<IPlayer>();
        father = transform.parent;
        
        health = GameObject.Find("Health").GetComponent<Slider>();
        energe1 = GameObject.Find("1").GetComponent<Slider>();
        energe2 = GameObject.Find("2").GetComponent<Slider>();
        energe3 = GameObject.Find("3").GetComponent<Slider>();

        energeImage1 = energe1.fillRect.GetComponent<Image>();
        energeImage2 = energe2.fillRect.GetComponent<Image>();
        energeImage3 = energe3.fillRect.GetComponent<Image>();
    }

    void FixedUpdate()
    {
        GetValue();
        ShowHp();
        ShowEnerge();
        Move();
    }

    void GetValue()
    {
        maxHp = player.maxHealth;
        maxEnerge = player.maxEnerge;
        currentHp = player.health;
        currentEnerge = player.energe;
    }

    void ShowHp()
    {
        health.value = currentHp / maxHp;
    }

    void ShowEnerge()
    {
        if (currentEnerge < maxEnerge / 3)
        {
            energe1.value = currentEnerge * 1.0f / (maxEnerge / 3.0f);
            energe2.value = 0f;
            energe3.value = 0f;
        }
        else if (currentEnerge >= maxEnerge / 3 && currentEnerge < 2 * maxEnerge / 3.0f)
        {
            energe1.value = 1f;
            energe2.value = (currentEnerge - maxEnerge / 3.0f) * 1.0f / (maxEnerge / 3.0f);
            energe3.value = 0f;
        }
        else if (currentEnerge >= 2 * maxEnerge / 3)
        {
            energe1.value = 1f;
            energe2.value = 1f;
            energe3.value = (currentEnerge - 2.0f * maxEnerge / 3.0f) * 1.0f / (maxEnerge / 3.0f);
        }
        //改变颜色
        if ((1-energe1.value) > Mathf.Epsilon)
        {
            energeImage1.color = energeColor2;
        }
        else
        {
            energeImage1.color = energeColor1;
        }
        
        if ((1-energe2.value) > Mathf.Epsilon)
        {
            energeImage2.color = energeColor2;
        }
        else
        {
            energeImage2.color = energeColor1;
        }       
        
        if ((1-energe3.value) > Mathf.Epsilon)
        {
            energeImage3.color = energeColor2;
        }
        else
        {
            energeImage3.color = energeColor1;
        }
    }
    
    //移动
    void Move()
    {
        Vector3 speed = iPlayer.GetSpeed();
        father.localPosition = iPlayer.GetOriRotation() *
                               new Vector3(speed.x * MoveRange, speed.y *MoveRange, -speed.z * MoveRange);
    }
}