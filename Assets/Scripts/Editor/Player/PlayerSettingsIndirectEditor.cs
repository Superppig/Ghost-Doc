using EditorExtend;
using UnityEditor;

[CustomEditor(typeof(PlayerSettings))]
public class PlayerSettingsIndirectEditor : IndirectEditor
{
    protected override string DefaultLabel => "参数设置";
    [AutoProperty]
    public SerializedProperty walkSettings,jumpSettings, sprintSettings, crouchSettings, slideSettings, airSettings, wallRunSettings, keySettings, otherSettings;

    public WalkSettingsIndirectEditor walk;
    public JumpSettingsIndirectEditor jump;
    public SprintSettingsIndirectEditor sprint;
    public SlideSettingsIndirectEditor slide;
    public CrouchSettingIndirectsEditor crouch;
    public AirSettingsIndirectEditor air;
    public WallRunSettingsIndirectEditor wallRun;
    public OtherSettingsIndirectEditor other;

    public PlayerSettingsIndirectEditor(SerializedProperty serializedProperty, string label = null) : base(serializedProperty, label)
    {
        walk = new WalkSettingsIndirectEditor(walkSettings);
        jump = new JumpSettingsIndirectEditor(jumpSettings);
        sprint = new SprintSettingsIndirectEditor(sprintSettings);
        crouch = new CrouchSettingIndirectsEditor(crouchSettings);
        slide = new SlideSettingsIndirectEditor(slideSettings);
        air = new AirSettingsIndirectEditor(airSettings);
        wallRun = new WallRunSettingsIndirectEditor(wallRunSettings);
        other = new OtherSettingsIndirectEditor(otherSettings);
    }

    protected override void MyOnInspectorGUI()
    {
        walk.OnInspectorGUI();
        jump.OnInspectorGUI();
        sprint.OnInspectorGUI();
        crouch.OnInspectorGUI();
        slide.OnInspectorGUI();
        air.OnInspectorGUI();
        wallRun.OnInspectorGUI();
        other.OnInspectorGUI();
        keySettings.PropertyField("按键设置");
    }
}