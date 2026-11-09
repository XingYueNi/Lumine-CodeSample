using Cinemachine;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    private Animator playerAnimation;
    [SerializeField]
    private float AxisX, AxisY;
    private float rayDistance = 0.1f;
    public bool isAttack;
    private Transform lockEnemy;
    public static event Action<Transform> OnLockEnemyChanged;

    private List<Transform> enemys = new List<Transform>();
    public GameObject freeCamera;
    public CinemachineVirtualCamera lockCamera;
    public CinemachineVirtualCamera waitExecutionCamera;
    private Vector3 moveDirection;
    public Transform playerCamera;
    private Vector3 camaraForward, cameraRight;
    private Quaternion targetRotation;
    private float rotationSpeed = 0.125f;
    private float shiftTimer;
    public float lockEnemyDistance = 20f;

    [SerializeField]
    private float scrollDelta;//鼠标滚轮滚动，向下滚动拉远
    public float fov;
    public GameObject CurrentCamera;

    private void Start()
    {
        playerAnimation = GetComponent<Animator>();
        fov = freeCamera.GetComponent<CinemachineFreeLook>().m_Lens.FieldOfView;
        CurrentCamera = freeCamera;
    }
    private void Update()
    {
        InputData();//处理玩家输入
    }
    private void CamereSwith(GameObject _camera)
    {
        CurrentCamera.SetActive(false);
        _camera.GetComponent<CinemachineFreeLook>().m_Lens.FieldOfView = fov;
        CurrentCamera = _camera;
        CurrentCamera.SetActive(true);
    }
    private void FixedUpdate()
    {
        if (Physics.Raycast(transform.position, Vector3.down, rayDistance))//判断是否在地面上
        {
            playerAnimation.SetBool("isGround", true);
        }
        else
        {
            playerAnimation.SetBool("isGround", false);
        }
        if (!lockEnemy)//是否锁定敌人
        {
            lockCamera.gameObject.SetActive(false);
            //freeCamera.transform.position = lockCamera.transform.position + new Vector3(0, 0, -2);
            freeCamera.SetActive(true);
            if (!isAttack)//在玩家处于攻击状态下，不能改变玩家朝向
                FreeMove();
        }
        else
        {
            freeCamera.SetActive(false);
            lockCamera.gameObject.SetActive(true);
            LockMove();
        }
        CameraControl();
    }
    private void CameraControl()
    {
        if (scrollDelta == 0) return;
        fov -= scrollDelta * 20;
        fov = Mathf.Clamp(fov, 35, 60);
        CurrentCamera.GetComponent<CinemachineFreeLook>().m_Lens.FieldOfView = fov;
    }
    private void InputData()
    {
        AxisX = Input.GetAxis("AxisX");
        AxisY = Input.GetAxis("AxisY");
        playerAnimation.SetFloat("AxisX", AxisX);
        playerAnimation.SetFloat("AxisY", AxisY);
        scrollDelta = Input.GetAxis("Mouse ScrollWheel");//鼠标滚轮
        if (Input.GetMouseButtonDown(2))
        {
            if (lockEnemy)
            {
                lockEnemy = null;
                OnLockEnemyChanged?.Invoke(null);
            }
            else
            {
                enemys = EnemyManager.Instance.GetAllEnemies();
                if (enemys.Count == 0)
                {
                    Debug.LogWarning("该场景中不存在敌人");
                    return;
                }
                Transform targer = null;
                float distance, mindistance = lockEnemyDistance;
                foreach (Transform var in enemys)
                {
                    // 双重保险：列表里可能残留已销毁的敌人
                    if (var == null) continue;

                    distance = Vector3.Distance(var.position, gameObject.transform.position);
                    if (distance > mindistance)
                        continue;
                    else
                    {
                        mindistance = distance;
                        targer = var;
                    }
                }
                lockEnemy = targer;
                OnLockEnemyChanged?.Invoke(targer);
            }
        }
        if (Input.GetKey(KeyCode.LeftShift))//行走转冲刺
        {
            if (AxisX == 0 && AxisY == 0)//停止前进
            {
                playerAnimation.SetBool("isWalk", false);
                playerAnimation.SetBool("isRun", false);
                return;
            }
            playerAnimation.SetBool("isRun", true);
            playerAnimation.SetBool("isWalk", false);
            if (playerAnimation.GetBool("FreeMove"))
            {
                shiftTimer += Time.deltaTime;
                if (shiftTimer > 1.8f)
                {
                    playerAnimation.SetBool("isSprintRun", true);
                }
                else
                {
                    playerAnimation.SetBool("isSprintRun", false);
                }
            }

        }
        else
        {
            shiftTimer = 0;
            playerAnimation.SetBool("isRun", false);
            playerAnimation.SetBool("isWalk", true);
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (playerAnimation.GetBool("LockMove"))
            {
                //  Debug.Log("按下空格");
                playerAnimation.SetTrigger("EvasionTrigger");

            }


        }
    }
    private void FreeMove()//未锁定敌人下的正常移动
    {
        playerAnimation.SetBool("LockMove", false);
        playerAnimation.SetBool("FreeMove", true);
        camaraForward = playerCamera.transform.forward;
        cameraRight = playerCamera.transform.right;
        camaraForward.y = 0;
        cameraRight.y = 0;
        camaraForward.Normalize();
        cameraRight.Normalize();
        moveDirection = (AxisX * cameraRight + camaraForward * AxisY).normalized;
        if (moveDirection.magnitude > 0.1)//是否实际输入
        {
            playerAnimation.SetBool("FreeMove", true);
            //   PlayerManager.Player.PlayerCurrentstate = PlayerManager.PlayerState.Move;
            Rotation();
        }
        else
        {
            playerAnimation.SetBool("FreeMove", false);
        }


    }
    private void Rotation()//根据玩家摄像机方向确定玩家的前方
    {
        targetRotation = Quaternion.LookRotation(moveDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed);
    }
    private void LockMove()//锁定敌人后的移动
    {
        playerAnimation.SetBool("FreeMove", false);
        playerAnimation.SetBool("LockMove", true);
        Vector3 direction = lockEnemy.transform.position - transform.position;
        direction.y = 0; // 保持 y 轴不变，确保敌人只在水平面上旋转
        // 计算目标旋转
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        // 逐渐旋转到目标方向
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 3f * Time.deltaTime);

    }
}