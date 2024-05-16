using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class Interact : MonoBehaviour
{
    public float FadeSize = 10.0f;
    public float HitSize = 1.0f;
    public float FadeSpeed = 1.0f;

    Vector3 m_PrevPos;
    public LayerMask layer;
    RaycastHit hit;

    private void Start()
    {
        m_PrevPos = transform.position;
    }
    private void Update()
    {
        Shader.SetGlobalFloat("_InvEquivalentTexSize", 1.0f / FadeSize);
        Shader.SetGlobalFloat("_EquivalentRange", HitSize);
        Shader.SetGlobalFloat("_FadeSpeed", FadeSpeed * Time.deltaTime);
        Vector4 moveDir=Vector4.zero;

        if (Vector3.Distance(m_PrevPos, transform.position) > 2f)
        {
            Shader.SetGlobalVector("_HitPos", transform.position);
            moveDir = transform.position - m_PrevPos;
            m_PrevPos = transform.position;
        }
        Shader.SetGlobalVector("_MoveDir", moveDir);

        Shader.SetGlobalFloat("_isHit", 1);
        if (Physics.Raycast(transform.position, -Vector3.up, out hit, 0.6f, layer.value))
        {
            Shader.SetGlobalVector("_HitUV", new Vector4(hit.textureCoord.x, hit.textureCoord.y, 0, 0));
        }
        else if(Physics.Raycast(transform.position, Vector3.right, out hit, 0.6f, layer.value))
        {
            Shader.SetGlobalVector("_HitUV", new Vector4(hit.textureCoord.x, hit.textureCoord.y, 0, 0));
        }
        else if (Physics.Raycast(transform.position, Vector3.left, out hit, 0.6f, layer.value))
        {
            Shader.SetGlobalVector("_HitUV", new Vector4(hit.textureCoord.x, hit.textureCoord.y, 0, 0));
        }
        else if (Physics.Raycast(transform.position, Vector3.forward, out hit, 0.6f, layer.value))
        {
            Shader.SetGlobalVector("_HitUV", new Vector4(hit.textureCoord.x, hit.textureCoord.y, 0, 0));
        }
        else if (Physics.Raycast(transform.position, Vector3.back, out hit, 0.6f, layer.value))
        {
            Shader.SetGlobalVector("_HitUV", new Vector4(hit.textureCoord.x, hit.textureCoord.y, 0, 0));
        }
        else
        {
            Shader.SetGlobalFloat("_isHit", 0);
        }

    }


}
