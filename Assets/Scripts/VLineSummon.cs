using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VLineSummon : MonoBehaviour
{
    [Header("属性")] 
    public LineRenderer line;
    public float r_min;//圆环内径
    public float r_max;//圆环外径
    public int count;//线数量
    public float exitTime;//存在时间
    public float deepth;//生成深度

    public void Summon(Transform trasform)
        => Summon(trasform.position, transform.forward.normalized);

    public void Summon(Vector3 position,Vector3 dir, float time = 0)
    {
        if (time < Mathf.Epsilon)
        {
            time = exitTime;
        }
        for (int i = 0; i < count; i++)
        {
            //生成一个随机点
            
            float angle = Random.Range(0f, 2f * Mathf.PI);
            float radius = Random.Range(r_min, r_max);
            //相对position的坐标
            Vector3 point = new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
            // 计算旋转矩阵，将局部坐标的法线调整为目标法线
            Quaternion rotation = Quaternion.FromToRotation(Vector3.up, dir);
            point = rotation * point;
            
            point += position;//旋转后坐标
            //在point点出生成line
            LineRenderer aLine = Instantiate(line, point, Quaternion.identity);
            
            aLine.SetPosition(0,point+dir.normalized*deepth);
            aLine.SetPosition(1,point-dir.normalized*deepth);
            StartCoroutine(LineDead(aLine, time));
        }
    }
    IEnumerator LineDead(LineRenderer line, float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(line.gameObject);
    }
}
