using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempRoleControl : MonoBehaviour
{
    public Rigidbody BodyPos;
    public CapsuleCollider BodyCol;
    public Transform Cam;
    public Vector3 MoveDierction = Vector3.zero;
    public Vector3 Trans_MoveDierction = Vector3.zero;
    public Vector3 Rotation_Input = Vector3.zero;
    private Quaternion Rotation_Value_x;
    private Quaternion Rotation_Value_y;
    public float MoveSpeed = 10;
    public float rotatespeed = 1;
    public float JumpForce = 1;

    private Vector3 previousMousePosition;

    void Start()
    {
        // 在开始时记录初始的鼠标位置
        previousMousePosition = Input.mousePosition;
    }
    void Update()
    {
        Cursor.visible = false;
        updateRotateOffset();
        updateMovingDeirection();
        Cam.rotation *= Rotation_Value_x;
        this.transform.rotation *= Rotation_Value_y;
        if(Input.GetKeyDown(KeyCode.Space) && BodyPos.velocity.y < 0.1f)
        {
            BodyPos.AddForce(Vector3.up * JumpForce);
        }

        if (Input.GetKey(KeyCode.LeftControl) && BodyPos.velocity.y < 0.1f)
        {
            BodyCol.height = 2;
        }
        else if(BodyCol.height < 4)
        {
            this.transform.position += Vector3.up * 2f;
            BodyCol.height = 4;
        }


    }

    private void FixedUpdate()
    {   

        BodyPos.AddForce(-Trans_MoveDierction * MoveSpeed);
        if(Trans_MoveDierction.magnitude <= 0.1f&& BodyPos.velocity.y < 0.1f && BodyPos.velocity.y > -0.1f)
        {
            BodyPos.velocity = new Vector3(0f, BodyPos.velocity.y, 0f);
        }
    }

    private void updateMovingDeirection()
    {
        MoveDierction = new Vector3((Input.GetKey(KeyCode.A) ? 1f : 0f) - (Input.GetKey(KeyCode.D) ? 1f : 0f), 0f, (Input.GetKey(KeyCode.S) ? 1f : 0f) - (Input.GetKey(KeyCode.W) ? 1f : 0f));
        Trans_MoveDierction = -(this.transform.rotation * MoveDierction);
    }

    private void updateRotateOffset()
    {

        Rotation_Input = new Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0f);
        Rotation_Value_y = Quaternion.Euler(0f, Rotation_Input.y,0f);
        Rotation_Value_x = Quaternion.Euler(Rotation_Input.x, 0f, 0f);
    }
}
