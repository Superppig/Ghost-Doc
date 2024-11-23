using Services;
using Services.Event;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerBaseUI : MonoBehaviour
{
    private IEventSystem eventSystem;

    public float amplitude = 0.2f;
    public Color color_fullEnergy;
    public Color color_noEnergy;

    private Slider healthSlider;
    private readonly List<Slider> energySliders = new List<Slider>();
    private readonly List<Image> energyImages = new List<Image>();
    private Transform father;
    private Player player;

    public TMP_Text scoreText;
    private ScoreManager scoreManager;

    private void Awake()
    {
        eventSystem = ServiceLocator.Get<IEventSystem>();
        scoreManager = ServiceLocator.Get<ScoreManager>();
        
        player = FindObjectOfType<Player>();
        Slider[] sliders = GetComponentsInChildren<Slider>();
        healthSlider = sliders[0];
        for (int i = 1; i < sliders.Length; i++)
        {
            energySliders.Add(sliders[i]);
            energyImages.Add(sliders[i].fillRect.GetComponent<Image>());
        }
        father = transform.parent;
    }

    private void OnEnable()
    {
        eventSystem.AddListener<float, float>(EEvent.PlayerHPChange, ShowHealth);
        eventSystem.AddListener<float, float>(EEvent.PlayerEnergyChange, ShowEnergy);
    }

    private void OnDisable()
    {
        eventSystem.RemoveListener<float, float>(EEvent.PlayerHPChange, ShowHealth);
        eventSystem.RemoveListener<float, float>(EEvent.PlayerEnergyChange, ShowEnergy);
    }

    private void FixedUpdate()
    {
        Flutter();

        scoreText.text = "Score: " + scoreManager.GetCurScore();
    }

    private void ShowHealth(float health, float max)
    {
        healthSlider.value = health / max;
    }

    private void ShowEnergy(float energy, float max)
    {
        float percent = energy * 3 / max;
        float temp;
        for (int i = 0; i < energySliders.Count; i++)
        {
            temp = Mathf.Clamp01(percent- i);
            energySliders[i].value = temp;
            energyImages[i].color = Color.Lerp(color_noEnergy, color_fullEnergy, temp);
        }
    }
    
    private void Flutter()
    {
        Vector3 velocity = player.blackboard.velocity;
        velocity = new Vector3(velocity.x, velocity.y, -velocity.z);
        father.localPosition = player.orientation.rotation * velocity * amplitude;
    }
}