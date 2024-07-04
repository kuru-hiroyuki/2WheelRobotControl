using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotMechanics : MonoBehaviour
{
    //���W�A��
    [SerializeField] float currentRotation =>this.transform.localEulerAngles.y*Mathf.PI/180;
    public float currentSpeed = 5f;
    public float maxAngularVelocity = 0.6f;//���W�A��
    [SerializeField] float angularVelocity = 0;//���W�A��
    [SerializeField] Vector3 currentPosition => this.transform.position;

    /// <summary>
    /// �p�����x��ݒ肵�܂�
    /// </summary>
    /// <param name="aVel"></param>
    public void SetAngularvelocity(float aVel)
    {
        angularVelocity = Mathf.Clamp(aVel,-maxAngularVelocity,maxAngularVelocity);//�p�����x�𐧌����܂�
    }

    private void FixedUpdate()
    {
        float rotation = angularVelocity * Time.fixedDeltaTime*180/Mathf.PI;
        this.transform.Rotate(new Vector3(0,rotation,0));
        this.transform.position += currentSpeed * new Vector3(Mathf.Cos(currentRotation), 0, -Mathf.Sin(currentRotation)) * Time.fixedDeltaTime;
    }
}
