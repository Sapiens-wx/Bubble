using UnityEngine;

public class MathUtil{
    public static Vector2 Rotate(Vector2 dir, float theta){
        float sin=Mathf.Sin(theta), cos=Mathf.Cos(theta);
        return new Vector2(dir.x*cos-dir.y*sin, dir.x*sin+dir.y*cos);
    }
}