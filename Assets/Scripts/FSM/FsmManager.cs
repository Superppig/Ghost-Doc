using UnityEngine;
using System;
using System.Collections.Generic;
using Services;

public class FsmManager: Service,IService
{

    private static int m_tempFsmId = 1;
    public override Type RegisterType => typeof(FsmManager);
    
    private Dictionary<int, FsmBase> m_FsmDic;
    protected internal override void Init()
    {
        base.Init();
        m_FsmDic = new Dictionary<int, FsmBase>();
    }
    /// <summary>
    /// 创建状态机
    /// </summary>
    /// <param name="owner">拥有者</param>
    /// <param name="status">状态队列</param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public Fsm<T> CreateFsm<T>(T owner, FsmState<T>[] status) where T : class
    {
        int fsmId = m_tempFsmId++;
        Fsm<T> fsm = new Fsm<T>(fsmId, owner, status);
        m_FsmDic[fsmId] = fsm;
        return fsm;
    }
    
    /// <summary>
    /// 是否存在状态机
    /// </summary>
    /// <param name="fsmId"></param>
    /// <returns></returns>
    public bool HasFsm(int fsmId)
    {
        return m_FsmDic.ContainsKey(fsmId);
    }
    
    /// <summary>
    /// 销毁状态机
    /// </summary>
    /// <param name="fsmId"></param>
    public void DestroyFsm(int fsmId)
    {
        if (m_FsmDic.TryGetValue(fsmId, out FsmBase fsm))
        {
            fsm.Shutdown();
            m_FsmDic.Remove(fsmId);
        }
    }
}
