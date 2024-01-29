using EditorExtend;
using UnityEditor;

[CustomEditor(typeof(PlayerSettings))]
public class PlayerSettingsIndirectEditor : IndirectEditor
{
    protected override string DefaultLabel => "参数设置";
    [AutoProperty]
    public SerializedProperty walkSettings,jumpSettings, sprintSettings, crouchSettings, slideSettings, airSettings, wallRunningSettings, keySettings, otherSettings;

    public WalkSettingsIndirectEditor walk;
    public JumpSettingsIndirectEditor jump;
    public SprintSettingsIndirectEditor sprint;
    public SlideSettingsIndirectEditor slide;
    public CrouchSettingIndirectsEditor crouch;
    public AirSettingsIndirectEditor air;
    public WallRunSettingsIndirectEditor wallRunning;
    public OtherSettingsIndirectEditor other;

    public override void Initialize(SerializedProperty serializedProperty, string label)
    {
        base.Initialize(serializedProperty, label);
        walk.Initialize(walkSettings);
        jump.Initialize(jumpSettings);
        sprint.Initialize(sprintSettings);
        crouch.Initialize(crouchSettings);
        slide.Initialize(slideSettings);
        air.Initialize(airSettings);
        wallRunning.Initialize(wallRunningSettings);
        other.Initialize(otherSettings);
    }

    protected override void MyOnInspectorGUI()
    {
        walk.OnInspectorGUI();
        jump.OnInspectorGUI();
        sprint.OnInspectorGUI();
        crouch.OnInspectorGUI();
        slide.OnInspectorGUI();
        air.OnInspectorGUI();
        wallRunning.OnInspectorGUI();
        other.OnInspectorGUI();
        keySettings.PropertyField("按键设置");
    }
}