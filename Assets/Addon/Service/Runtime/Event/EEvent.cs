namespace Services
{
    public enum EEvent
    {
        /// <summary>
        /// 加载场景前，参数：即将加载的场景号
        /// </summary>
        BeforeLoadScene,
        /// <summary>
        /// 加载场景后（至少一帧以后），参数：刚加载好的场景号
        /// </summary>
        AfterLoadScene,
        PlayerHPChange,
        PlayerEnergyChange,
        
        /// <summary>
        /// 关卡进度进展
        /// </summary>
        LevelProgressChange,
        WaveProgressChange,
        /// <summary>
        /// 关卡完成
        /// </summary>
        LevelComplete,
        NextLevel,
        
    }
}