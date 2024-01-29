using UnityEngine;
using UnityEngine.UI;

public class PlayerBaseUI : MonoBehaviour
{
    private Player player;
    private IPlayer iPlayer;

    private float MaxHealth => player.settings.otherSettings.maxHealth;
    private float MaxEnergy => player.settings.otherSettings.maxEnergy;
    private float Health => player.blackboard.health;
    private float Energy => player.blackboard.energy;


    private Slider healthSlider;
    private Slider energySlider1;
    private Slider energySlider2;
    private Slider energySlider3;

    private Image energeImage1;
    private Image energeImage2;
    private Image energeImage3;

    public Color energeColor1 = Color.cyan;
    public Color energeColor2 = Color.blue;

    public float MoveRange=0.2f;
    private Transform father;


    void Start()
    {
        player = GameObject.FindWithTag("Player").GetComponent<Player>();
        iPlayer = player.GetComponent<IPlayer>();
        father = transform.parent;

        Slider[] sliders = GetComponentsInChildren<Slider>();
        healthSlider = sliders[0];
        energySlider1 = sliders[1];
        energySlider2 = sliders[2];
        energySlider3 = sliders[3];

        energeImage1 = energySlider1.fillRect.GetComponent<Image>();
        energeImage2 = energySlider2.fillRect.GetComponent<Image>();
        energeImage3 = energySlider3.fillRect.GetComponent<Image>();
    }

    void FixedUpdate()
    {
        ShowHp();
        ShowEnerge();
        Move();
    }

    void ShowHp()
    {
        healthSlider.value = Health / MaxHealth;
    }

    void ShowEnerge()
    {
        if (Energy < MaxEnergy / 3)
        {
            energySlider1.value = Energy * 1.0f / (MaxEnergy / 3.0f);
            energySlider2.value = 0f;
            energySlider3.value = 0f;
        }
        else if (Energy >= MaxEnergy / 3 && Energy < 2 * MaxEnergy / 3.0f)
        {
            energySlider1.value = 1f;
            energySlider2.value = (Energy - MaxEnergy / 3.0f) * 1.0f / (MaxEnergy / 3.0f);
            energySlider3.value = 0f;
        }
        else if (Energy >= 2 * MaxEnergy / 3)
        {
            energySlider1.value = 1f;
            energySlider2.value = 1f;
            energySlider3.value = (Energy - 2.0f * MaxEnergy / 3.0f) * 1.0f / (MaxEnergy / 3.0f);
        }
        //改变颜色
        if ((1-energySlider1.value) > Mathf.Epsilon)
        {
            energeImage1.color = energeColor2;
        }
        else
        {
            energeImage1.color = energeColor1;
        }
        
        if ((1-energySlider2.value) > Mathf.Epsilon)
        {
            energeImage2.color = energeColor2;
        }
        else
        {
            energeImage2.color = energeColor1;
        }       
        
        if ((1-energySlider3.value) > Mathf.Epsilon)
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