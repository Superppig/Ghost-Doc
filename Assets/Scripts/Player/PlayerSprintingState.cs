using DG.Tweening;
using Player_FSM;
using UnityEngine;

public class PlayerSprintingState : IState
{
    private PlayerBlackboard _playerBlackboard;
    private Rigidbody rb;
    private Transform oritation;
    
    private float sprintSpeed;
    private float firstSpeed;
    
    private Vector3 sprintDir;
    
    //相机行为
    private Transform cam;


    public PlayerSprintingState(PlayerBlackboard playerBlackboard)
    {
        _playerBlackboard = playerBlackboard;
    }
    
    
    public void OnEnter()
    {
        rb = _playerBlackboard.m_rigidbody;
        sprintSpeed = _playerBlackboard.sprintSpeed;
        oritation = _playerBlackboard.oritation;
        
        cam = Camera.main.GetComponent<Transform>();

        firstSpeed = rb.velocity.magnitude;
        sprintDir = (_playerBlackboard.moveDir.x * oritation.right + _playerBlackboard.moveDir.z * oritation.forward)
            .normalized;

        rb.velocity = Vector3.zero;
        
        //镜头行为
        if (_playerBlackboard.moveDir.x!=0)
        {
            if (_playerBlackboard.moveDir.x >0)
            {
                //镜头晃动
                cam.DOLocalRotate(new Vector3(0, 0, 1), 0.1f);
            }
            else
            {
                cam.DOLocalRotate(new Vector3(0, 0, -1), 0.1f);
            }
        }
    }

    public void OnExit()
    {
        rb.velocity = sprintDir * firstSpeed;
        _playerBlackboard.speed=sprintDir * firstSpeed;
        
        
        
        cam.DOLocalRotate(new Vector3(0, 0, 0), 0.1f);

    }

    public void OnUpdate()
    {
        rb.velocity = sprintDir * sprintSpeed;
    }

    public void OnCheck()
    {
        
    }

    public void OnFixUpdate()
    {
        
    }
}
