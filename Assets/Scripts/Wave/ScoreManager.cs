using System;
using Services;

public class ScoreManager:Service,IService
{
    public override Type RegisterType => typeof(ScoreManager);
    
    
}