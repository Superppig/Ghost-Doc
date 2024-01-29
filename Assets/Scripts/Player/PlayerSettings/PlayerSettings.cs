using System;

[Serializable]
public class PlayerSettings
{
    public WalkSettings walkSettings;
    public JumpSettings jumpSettings;
    public SprintSettings sprintSettings;
    public CrouchSettings crouchSettings;
    public SlideSettings slideSettings;
    public AirSettings airSettings;
    public WallRunSettings wallRunSettings;
    public KeySettings keySettings;
    public OtherSettings otherSettings;
}
