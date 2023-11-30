
using UnityEngine;

public class GridConvertTool {
    static Matrix4x4 convertMatrix = new Matrix4x4(
        new Vector4(0.5f, 0, 0, 0),
        new Vector4(0, 0, 0.5f, 0),
        new Vector4(0, 0, 0, 0),
        new Vector4(0, 0, 0, 0)
    );

    //get grid position
    public static Vector2Int GetGridPosition(Vector3 position) {
        Vector4 pos = new Vector4(position.x, position.y, position.z, 1);
        Vector4 result = convertMatrix * pos;
        return new Vector2Int((int)result.x, (int)result.y);
    }
}