using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class Interact : MonoBehaviour
{
    public float EquivalentTexSize;
    public float EquivalentRange;
    public float FadeSpeed;

    Vector3 m_PrevPos;
    
    private void Start()
    {
        m_PrevPos = transform.position;
    }
    private void Update()
    {
        Shader.SetGlobalFloat("_InvEquivalentTexSize", 1.0f / EquivalentTexSize);
        Shader.SetGlobalFloat("_EquivalentRange", EquivalentRange);
        Shader.SetGlobalFloat("_FadeSpeed", FadeSpeed * Time.deltaTime);
        Shader.SetGlobalVector("_HitPos", transform.position);
        //if (Vector3.Distance(transform.position, m_PrevPos) > 0.001f)
        //{
        //    Vector4 moveDir = new Vector4(transform.position.x - m_PrevPos.x, transform.position.y - m_PrevPos.y, transform.position.z - m_PrevPos.z, 0);
        //    Shader.SetGlobalVector("_MoveDir", moveDir);
        //    m_PrevPos = transform.position;
        //}

        Vector4 moveDir = transform.position - m_PrevPos;
        Shader.SetGlobalVector("_MoveDir", moveDir);
        m_PrevPos = transform.position;
    }
    //private void OnCollisionEnter(Collision collision)
    //{
    //    if (collision.transform.tag == "Enemy")
    //    {
    //        m_PrevPos = collision.transform.position;
    //        Shader.SetGlobalVector("_HitPos",new Vector4(transform.position.x,transform.position.y,transform.position.z,0));
    //        Shader.SetGlobalVector("_MoveDir", Vector4.zero);
    //    }
    //}
    //private void OnCollisionExit(Collision collision)
    //{
    //    if (collision.transform.tag == "Enemy")
    //    {
    //        m_PrevPos=collision.transform.position;
    //        Shader.SetGlobalVector("_HitPos", Vector4.zero);
    //        Shader.SetGlobalVector("_MoveDir", Vector4.zero);
    //    }
    //}
    //private void OnCollisionStay(Collision collision)
    //{
    //    if (collision.transform.tag == "Enemy")
    //    {
    //        Shader.SetGlobalVector("_HitPos", transform.position);
    //        //if (Vector3.Distance(transform.position, m_PrevPos) > 0.001f)
    //        //{
    //        //    Vector4 moveDir = new Vector4(transform.position.x - m_PrevPos.x, transform.position.y - m_PrevPos.y, transform.position.z - m_PrevPos.z, 0);
    //        //    Shader.SetGlobalVector("_MoveDir", moveDir);
    //        //    m_PrevPos = transform.position;
    //        //}

    //        Vector4 moveDir = transform.position - m_PrevPos;
    //        Shader.SetGlobalVector("_MoveDir", moveDir);
    //        m_PrevPos = transform.position;
    //    }
    //}

}
