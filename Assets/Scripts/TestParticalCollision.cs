using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Services;
using Services.ObjectPools;
using UnityEngine;
public class TestParticalCollision : MonoBehaviour
{
    ParticleSystem m_ps = null;
    List<ParticleCollisionEvent> collisionEvents;
    public GameObject Blood;
    
    
    private IObjectManager objectManager;
    [SerializeField]
    private String bloodPerfabName;
    private IMyObject bloodObject;
    [SerializeField]
    private float bloodRecycleTime;

    // Start is called before the first frame update
    void Start()
    {
        m_ps = GetComponent<ParticleSystem>();
        collisionEvents = new List<ParticleCollisionEvent>();
        
        
        objectManager = ServiceLocator.Get<IObjectManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnParticleCollision(GameObject other)
    {
        int eventNum=m_ps.GetCollisionEvents(other,collisionEvents);
        for(int i = 0; i < eventNum; i++)
        {
            var pos = collisionEvents[i].intersection;
            var normal = collisionEvents[i].normal;
            //Debug.Log(pos);
            
            
            var go=Instantiate(Blood, pos,Quaternion.LookRotation(-normal));
            Destroy(go, 1);
            
            
            /*bloodObject=objectManager.Activate(bloodPerfabName, pos, Quaternion.LookRotation(-normal).eulerAngles, null);
            DOTween.To(() => bloodRecycleTime, a => bloodRecycleTime = a, 0, 1)
                .OnComplete(() => BloodRecycle(bloodObject));*/
        }
    }
    
    void BloodRecycle(IMyObject bloodObject)
    {
        bloodObject.Recycle();
    }
}
