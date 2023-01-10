using UnityEngine;

public static class Utils
{

    public static Vector3Int GetCardinalDirection(Vector3 v)
    {
        // left or right
        if (Mathf.Abs(v.x) > Mathf.Abs(v.y))
        {
            return v.x > 0 ? Vector3Int.right : Vector3Int.left;
        }
        // up or down
        else
        {
            return v.y > 0 ? Vector3Int.up : Vector3Int.down;
        }
    }

    public static float GetCardinalAngle(Vector3 v)
    {
        // left or right
        if (Mathf.Abs(v.x) > Mathf.Abs(v.y))
        {
            return v.x > 0 ? Constants.ANGLE_RIGHT : Constants.ANGLE_LEFT;
        }
        // up or down
        else
        {
            return v.y > 0 ? Constants.ANGLE_UP : Constants.ANGLE_DOWN;
        }
    }

}