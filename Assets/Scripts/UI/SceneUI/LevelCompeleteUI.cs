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
        
        exitBtn.onClick.AddListener(() =>
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        });
    }

    public override void Show()
    {
        base.Show();
        Time.timeScale = 0;
    }

    public override void Hide()
    {
        base.Hide();
        Time.timeScale = 1;
    }

    void NextLevel()
    {
        eventSystem.Invoke(EEvent.NextLevel);
    }
}