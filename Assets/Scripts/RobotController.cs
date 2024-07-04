using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
public class RobotController : MonoBehaviour
{
    [SerializeField] RobotMechanics robot;
    [SerializeField] GameObject target;

    [SerializeField] int numStep = 150;
    [SerializeField] int anglenumstep = 8;

    [SerializeField] float testAngleDifference;
    [SerializeField] PositionTest positionTest;
    float maxAngularVelocity => robot.maxAngularVelocity;

    [SerializeField] Vector3 targetPosition => target.transform.position;
    [SerializeField] float targetRotation => target.transform.eulerAngles.y * Mathf.PI / 180;
    [SerializeField] Vector3 currentPosition => robot.transform.position;
    [SerializeField] float currentRotation => robot.transform.eulerAngles.y * Mathf.PI / 180;//ラジアン
    float currentSpeed => robot.currentSpeed;

    ///重みづけの割合
    [SerializeField] float w_position = 0.5f;
    [SerializeField] float w_rotation = 0.3f;


    private void FixedUpdate()
    {
       float nextangularvel = DecideNextAngularVelocity(anglenumstep, numStep);
        robot.SetAngularvelocity(nextangularvel);
    }

    //ほしい関数
    /// <summary>
    /// 2つの角度差を計算します(AngleA-AngleB)
    /// </summary>
    /// <param name="angleA"></param>
    /// <param name="angleB"></param>
    /// <returns></returns>
    float CalcAngleDifference(float angleA,float angleB)
    {
        if (Mathf.Abs(angleA - angleB) < Mathf.Abs(angleA + angleB - Mathf.PI * 2))
        {
            return angleA - angleB;
        }
        else
            return angleA + angleB - Mathf.PI * 2;
    }

    //重み関数を計算します
    float CalcWeightingFunction(float distance,float angledifference)
    {
        //Debug.Log(angledifference);
        return Mathf.Abs(distance) * w_position + Mathf.Abs(angledifference) * w_rotation;
    }

    /// <summary>
    /// 次の時刻における状態を計算します
    /// </summary>
    /// <param name="_angularVelocity">回転角速度(ラジアン/秒)</param>
    /// <param name="_currentPosition">今の座標</param>
    /// <param name="_currentSpeed">今の速度</param>
    /// <param name="_currentRotation">今の角度(ラジアン)</param>
    /// <returns></returns>
    PosAndRot CalcNextStep(float _angularVelocity,float _currentRotation,Vector3 _currentPosition,float _currentSpeed)
    {
        float nextRotation = _currentRotation + _angularVelocity * Time.fixedDeltaTime;
        Vector3 nextPosition = _currentPosition +_currentSpeed * new Vector3(Mathf.Cos(nextRotation), 0, -Mathf.Sin(nextRotation)) * Time.fixedDeltaTime;

        PosAndRot posandrot = new PosAndRot();
        posandrot.position = nextPosition;
        posandrot.rotation = nextRotation;
        return posandrot;
    }

    /// <summary>
    /// 設定したステップ数だけ先までの座標と回転角度を計算します
    /// </summary>
    /// <param name="_angularVelocity"></param>
    /// <param name="_currentRotation"></param>
    /// <param name="_currentPosition"></param>
    /// <param name="_currentSpeed"></param>
    /// <param name="steps">何ステップ分計算するか</param>
    /// <returns>設定したステップ分だけ先の座標と回転角度たち</returns>
    PosAndRot[] CalcNextPositions(float _angularVelocity, float _currentRotation, Vector3 _currentPosition, float _currentSpeed,int steps)
    {
        PosAndRot[] results = new PosAndRot[steps];

        for(int i = 0;i<steps;i++)
        {
            results[i] = CalcNextStep(_angularVelocity, _currentRotation, _currentPosition, currentSpeed);
            _currentPosition = results[i].position;
            _currentRotation = results[i].rotation;
        }
        return results;
    }
    /// <summary>
    /// 重み関数の値を計算します
    /// </summary>
    /// <returns></returns>
    float PosAndRotToWF(PosAndRot _posandrot)
    {
        float distance = Vector3.Magnitude(_posandrot.position - targetPosition);
        float rotateDifference = Mathf.Abs(CalcAngleDifference(_posandrot.rotation, targetRotation));

        return CalcWeightingFunction(distance, rotateDifference);
    }

    /// <summary>
    /// 試しに角速度を与えると重み関数の値たちを返す
    /// </summary>
    /// <param name="anglularVelocity"></param>
    /// <returns></returns>
    float CalcWeightingFunctions(float _anglularVelocity,int steps)
    {
        float result = 0;
        PosAndRot[] posandrots = CalcNextPositions(_anglularVelocity, currentRotation, currentPosition, currentSpeed, steps);
        foreach(PosAndRot posandrot in posandrots)
        {
            result += PosAndRotToWF(posandrot);//単純に足し合わせる(ほんとは台形積分のほうがいいかも)
        }
        return result;
    }

    /// <summary>
    /// 次のステップで入力する角度を決定します
    /// </summary>
    /// <returns></returns>
    float DecideNextAngularVelocity(int splitnum,int steps)
    {
        float[] results = new float[2 * splitnum];
        for(int i = -splitnum; i<splitnum;i++)
        {
            results[i + splitnum] = CalcWeightingFunctions(maxAngularVelocity / splitnum * i, steps);
        }

        int minindex = Array.IndexOf(results, results.Min());
        //Debug.Log(results.Min()+","+results.Max());
        PosAndRot[] posAndRots = CalcNextPositions(maxAngularVelocity / splitnum * (minindex - splitnum), currentRotation, currentPosition, currentSpeed, steps);
        for(int j=0;j<posAndRots.Length;j++)
        {
            positionTest.markers[j].transform.position = posAndRots[j].position;
        }//ここまでデバッグ用
        return maxAngularVelocity / splitnum * (minindex-splitnum);
    }
    
}
/// <summary>
/// ステップごとの位置と回転角度を格納するクラス
/// </summary>
public class PosAndRot
{
    public Vector3 position;
    public float rotation;

}
