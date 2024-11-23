using Services;
using Services.Event;
using UnityEngine;
using UnityEngine.UI;

public class LevelCompeleteUI : View
{
    public Button nextLevelBtn;
    public Button exitBtn;

    private IEventSystem eventSystem;
    public override void Init()
    {
        eventSystem = ServiceLocator.Get<IEventSystem>();
        nextLevelBtn.onClick.AddListener(NextLevel);
        
        exitBtn.onClick.AddListener(() => { Application.Quit(); });
    }

    void NextLevel()
    {
        eventSystem.Invoke(EEvent.NextLevel);
    }
}