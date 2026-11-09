using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public CharacterController cc;
    public Camera mainCamera;
    public float speed = 5f;
    public float mouseSensitivity = 2f;
    public float yMinLimit = -40f;
    public float yMaxLimit = 80f;

    private float rotationY = 0f;

    void Start()
    {
        // 锁定鼠标光标
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // 获取玩家输入
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        // 计算移动方向
        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);
        movement = transform.rotation * movement;
        movement *= speed;

        // 移动角色
        cc.Move(movement * Time.deltaTime);
        if (mainCamera != null)
        {
            rotationY += Input.GetAxis("Mouse Y") * mouseSensitivity;
            rotationY = Mathf.Clamp(rotationY, yMinLimit, yMaxLimit);

            float rotationX = Input.GetAxis("Mouse X") * mouseSensitivity;

            mainCamera.transform.RotateAround(transform.position, Vector3.up, rotationX);

            Vector3 localRotation = mainCamera.transform.localEulerAngles;
            localRotation.x = -rotationY;
            mainCamera.transform.localEulerAngles = localRotation;
        }
        
}
}
