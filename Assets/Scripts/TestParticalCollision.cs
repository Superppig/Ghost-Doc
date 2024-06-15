using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class TestParticalCollision : MonoBehaviour
{
    ParticleSystem m_ps = null;
    List<ParticleCollisionEvent> collisionEvents;
    public GameObject Blood;
    // Start is called before the first frame update
    void Start()
    {
        m_ps = GetComponent<ParticleSystem>();
        collisionEvents = new List<ParticleCollisionEvent>();
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
            Debug.Log(pos);
            var go=Instantiate(Blood, pos,Quaternion.LookRotation(-normal));
            Destroy(go, 1);
        }
    }
}
