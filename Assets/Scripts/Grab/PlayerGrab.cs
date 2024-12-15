using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using Services;

public class PlayerGrab : MonoBehaviour
{
    [Header("抓取属性")] public float grabRange = 10f; //抓取范围
    public float grabScale = 1f; //抓取缩放
    public float grabTime = 0.1f; //抓取时间
    public Vector3 localGrabPos; //抓取位置

    public float throwForce = 20f; //扔出力

    [Header("图层属性")] 
    public string holdLayer;

    private Transform camTrans;
    public IGrabObject grabbedObject;
    [Header("按键")]
    [SerializeField]
    private KeyCode grabKey = KeyCode.Mouse1;
    [SerializeField]
    private KeyCode useKey = KeyCode.E;
    
    private bool hasObject;
    private bool isGrabbing;

    [Header("特效")] 
    public ParticleSystem usePartical;
    public int num = 10;

    void Start()
    {
        camTrans = Camera.main.transform;
    }

    void Update()
    {
        PlayerInput();
        if (hasObject&& !isGrabbing)
        {
            GrabObjControl();
        }
    }

    private void GrabObjControl()
    {
        grabbedObject.GetTransform().DOLocalMove(CalGrabTransPos(),0.1f);
    }

    void PlayerInput()
    {
        if (Input.GetKeyDown(grabKey) && !hasObject)
        {
            //射线检测
            Ray ray = new Ray(camTrans.position, camTrans.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, grabRange, LayerMask.GetMask(holdLayer)))
            {
                Collider col = hit.collider;
                IGrabObject grabObject = col.GetComponent<IGrabObject>();
                if (grabObject != null&& col != null)
                {
                    Grab(grabObject);
                }
            }
        }
        else if(Input.GetKeyDown(useKey)&&hasObject)
        {
            grabbedObject.Use();
            UsePartical(grabbedObject.GetTransform().position);
            hasObject = false;
        }
        else if (Input.GetKeyUp(grabKey) && hasObject)
        {
            grabbedObject.Released();
            grabbedObject.Fly(camTrans.forward, throwForce);
            grabbedObject = null;
            hasObject = false;
        }
    }

    private void Grab(IGrabObject grabObject)
    {
        if (grabObject.CanGrab())
        {
            hasObject = true;
            isGrabbing = true;
            
            grabbedObject = grabObject.GetGrabObject();
            grabbedObject.Grabbed();
            Transform grabTrans = grabbedObject.GetTransform();
            grabTrans.parent = null;
            grabTrans.DOLocalMove(CalGrabTransPos(), grabTime).OnComplete(() =>
            {
                isGrabbing = false;
            });
        }
    }

    private Vector3 CalGrabTransPos()
    {
        Vector3 grabPos = camTrans.rotation * localGrabPos;
        return camTrans.position + grabPos;
    }

    void UsePartical(Vector3 position)
    {
        for (int i = 0; i < num; i++)
        {
            //在num个方向上释放粒子
            Quaternion rotation = Quaternion.Euler(0, 360 / num * i, 0);
            ServiceLocator.Get<ScreenControl>().ParticleRelease(usePartical, position,
                rotation.eulerAngles);
            ServiceLocator.Get<ScreenControl>().FrameFrozen(30, 0.1f);
        }
    }

    void ChangeLayer(Transform trans, string targetLayer)
    {
        if (LayerMask.NameToLayer(targetLayer) == -1)
        {
            //Debug.LogWarning("Layer中不存在,请手动添加LayerName");
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