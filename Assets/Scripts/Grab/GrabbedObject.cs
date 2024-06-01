using UnityEngine;

public class GrabbedObject : MonoBehaviour
{
    private Rigidbody rb;
    private Collider col;
    private QuickOutline outline;
    void Start()
    {        
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        outline = GetComponent<QuickOutline>();
        CancelHighlight();
    }

    void Update()
    {
        
    }
    
    public void Highlight()
    {
        outline.enabled = true;
    }
    public void CancelHighlight()
    {
        outline.enabled = false;
    }
    
    public void Grabbed()
    {
        rb.isKinematic = true;
        col.enabled = false;
    }
    public void Released()
    {
        rb.isKinematic = false;
        col.enabled = true;
    }
    
    public void Fly(Vector3 speed)
    {
        rb.velocity = speed;
    }
}
