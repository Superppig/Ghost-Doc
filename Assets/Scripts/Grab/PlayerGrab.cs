using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

public class PlayerGrab : MonoBehaviour
{
    [Header("抓取属性")]
    public float grabRange = 5f;//抓取范围
    public float grabScale = 1f;//抓取缩放
    public float grabTime = 0.5f;//抓取时间
    public Vector3 localGrabPos;//抓取位置
    
    public float throwForce= 20f;//扔出力

    [Header("图层属性")] 
    public string holdLayer;
    public string defaultLayer;
    
    
    private Transform camTrans;
    public GrabbedObject grabbedObject;
    [Header("按键")]
    public KeyCode grabKey = KeyCode.E;
    
    private bool isChoosing;
    private bool isGrabbing;
    
    void Start()
    {
        camTrans = Camera.main.transform;
    }
    void Update()
    {
        if (!isGrabbing)
        {
            ChooseObject();
        }
        PlayerInput();
        GrabObjectControl();
    }

    void PlayerInput()
    {
        if (Input.GetKeyDown(grabKey))
        {
            //抓取物体
            if (isChoosing && !isGrabbing)
            {
                grabbedObject.transform.SetParent(camTrans);
                grabbedObject.transform.DOLocalMove(localGrabPos, grabTime);
                grabbedObject.transform.DOLocalRotate(Vector3.zero, grabTime);
                grabbedObject.Grabbed();
                
                ChangeLayer(grabbedObject.transform, holdLayer);

                isGrabbing= true;
            }
            //扔出物体
            else if(isGrabbing)
            {
                Vector3 pos = grabbedObject.transform.position;
                grabbedObject.transform.SetParent(null);
                grabbedObject.transform.position = pos;
                
                ChangeLayer(grabbedObject.transform, defaultLayer);
                grabbedObject.Released();
    
                grabbedObject.Fly(camTrans.forward.normalized*throwForce);
                
                isGrabbing= false;
            }
        }
    }
    
    void ChooseObject()
    {
        RaycastHit hit;
        if (Physics.Raycast(camTrans.position, camTrans.forward, out hit, grabRange))
        {
            if (hit.collider.GetComponent<GrabbedObject>()!=null)
            {
                if (isChoosing)
                {
                    grabbedObject.CancelHighlight();
                }
                grabbedObject = hit.collider.GetComponent<GrabbedObject>();
                grabbedObject.Highlight();
                isChoosing = true;
            }
        }
        else
        {
            if (isChoosing)
            {
                grabbedObject.CancelHighlight();
            }
            
            isChoosing = false;
        }
    }
    void GrabObjectControl()
    {
        if (grabbedObject!=null)
        {
            if (isChoosing&&!isGrabbing)
            {
                grabbedObject.Highlight();
            }
            else
            {
                grabbedObject.CancelHighlight();
            }
        }
    }

    void ChangeLayer(Transform trans, string targetLayer)
    {
        if (LayerMask.NameToLayer(targetLayer) == -1)
        {
            Debug.LogWarning("Layer中不存在,请手动添加LayerName");
            return;
        }

        //遍历更改所有子物体layer
        trans.gameObject.layer = LayerMask.NameToLayer(targetLayer);
        foreach (Transform child in trans)
        {
            ChangeLayer(child, targetLayer);
        }
    }
}
