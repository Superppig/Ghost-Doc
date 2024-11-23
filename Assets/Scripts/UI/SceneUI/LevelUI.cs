using Services;
using Services.Event;
using UnityEngine.UI;

public class LevelUI : View
{
    public Slider procedualSlider;
    public Slider waveSlider;
    private IEventSystem eventSystem;
    
    public override void Init()
    {
        eventSystem = ServiceLocator.Get<IEventSystem>();
    }

    public override void Show()
    {
        base.Show();
        eventSystem.AddListener<float>(EEvent.LevelProgressChange, OnLevelProgressChange);
        eventSystem.AddListener<float>(EEvent.WaveProgressChange, OnWaveProgressChange);
    }

    

    public override void Hide()
    {
        base.Hide();
        eventSystem.RemoveListener<float>(EEvent.LevelProgressChange, OnLevelProgressChange);
    }


    private void OnLevelProgressChange(float arg0)
    {
        procedualSlider.value = arg0;
    }
    private void OnWaveProgressChange(float arg0)
    {
        waveSlider.value = arg0;
    }
}