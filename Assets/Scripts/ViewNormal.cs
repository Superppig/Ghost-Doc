using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class ViewNormal : MonoBehaviour
{
    private Mesh mesh;
    // Start is called before the first frame update
    void Start()
    {
        if(GetComponent<MeshFilter>() != null)
        {
            mesh = GetComponent<MeshFilter>().sharedMesh;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        for(int i=0; i < mesh.vertices.Length; i++)
        {
            
            Gizmos.DrawLine(transform.TransformPoint(mesh.vertices[i]), transform.TransformDirection(mesh.normals[i]));

        }
    }
}
