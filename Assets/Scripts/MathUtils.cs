using UnityEngine;

public static class MathUtils
{
    public static Vector3 Floor(Vector3 vec)
    {
        return new Vector3(Mathf.Floor(vec.x), Mathf.Floor(vec.y), Mathf.Floor(vec.z));
    }

    public static Vector3Int ToVector3Int(Vector3 vec)
    {
        return new Vector3Int((int)vec.x, (int)vec.y, (int)vec.z);
    } 
}