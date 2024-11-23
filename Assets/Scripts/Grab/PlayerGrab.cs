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

    [Header("图层属性")] public string holdLayer;
    public string defaultLayer;


    private Transform camTrans;
    public IGrabObject grabbedObject;
    [Header("按键")] 
    private KeyCode grabKey = KeyCode.Mouse1;
    [SerializeField]
    private KeyCode useKey = KeyCode.E;

    private bool isChoosing;
    private bool isGrabbing;

    [Header("特效")] public ParticleSystem usePartical;
    public int num = 10;

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
        
        if (Input.GetKeyUp(grabKey) && isGrabbing)
        {
            Vector3 pos = grabbedObject.GetTransform().position;
            grabbedObject.GetTransform().SetParent(null);
            grabbedObject.GetTransform().position = pos;

            ChangeLayer(grabbedObject.GetTransform(), defaultLayer);
            grabbedObject.Released();

            grabbedObject.Fly(camTrans.forward.normalized, throwForce);

            isGrabbing = false;
        }
        
        if(Input.GetKeyDown(useKey) && isGrabbing)
        {
            if(grabbedObject.CanUse())
            {
                UsePartical(grabbedObject.GetTransform().position);
                grabbedObject.Use();
                isGrabbing = false;
            }
        }
        
        
        //吸收
        if (Input.GetKeyDown(grabKey) && isChoosing && !isGrabbing)
        {
            grabbedObject.GetTransform().SetParent(camTrans);
            grabbedObject.GetTransform().DOLocalMove(localGrabPos, grabTime);
            grabbedObject.GetTransform().DOLocalRotate(Vector3.zero, grabTime);
            grabbedObject.Grabbed();

            ChangeLayer(grabbedObject.GetTransform(), holdLayer);
            isGrabbing = true;
        }
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
    

    void ChooseObject()
    {
        RaycastHit hit;
        if (Physics.Raycast(camTrans.position, camTrans.forward, out hit, grabRange))
        {
            if (hit.collider.TryGetComponent(out IGrabObject obj))
            {
                if (obj.CanGrab())
                {
                    if (isChoosing)
                    {
                        //grabbedObject.CancelHighlight();
                    }

                    grabbedObject = obj as IGrabObject;

                    //效果
                    //grabbedObject.Highlight();

                    isChoosing = true;
                }
            }
        }
        else
        {
            if (isChoosing)
            {
                //grabbedObject.CancelHighlight();
            }

            isChoosing = false;
        }
    }

    void GrabObjectControl()
    {
        if (grabbedObject != null)
        {
            if (isChoosing && !isGrabbing)
            {
                //grabbedObject.Highlight();
            }
            else
            {
                //grabbedObject.CancelHighlight();
            }
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