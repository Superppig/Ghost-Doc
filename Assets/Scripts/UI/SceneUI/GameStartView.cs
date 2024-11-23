using UnityEngine;
using UnityEngine.UI;

public class GameStartView : View
{
    public Button startBtn;
    public Button exitBtn;
    
    public override void Init()
    {
        exitBtn.onClick.AddListener(() =>
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        });
    }
}