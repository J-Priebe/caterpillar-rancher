using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraCrop : MonoBehaviour {

    Camera _camera;

    void Start () {
        _camera = GetComponent<Camera>();
        UpdateCrop();
    }

    public void UpdateCrop() {

        float screenRatio = Screen.width / (float)Screen.height;
        float targetRatio = 9f/16.0f;

        // the total player area in world space is:
        // 8 units wide (7 tiles, 0.5 borders on each side)
        // 14  (12 tiles, 0.5 bottom, 1.5 top)

        float H;
        // if ratio is > 9/16 (e.g., 10:16 like some tablets), letterbox on side
        if (screenRatio > targetRatio)
        {
            // 7 tiles + borders
            H = 8f / targetRatio;
        }
        // else (9:16, longer phones like 9:19), letterbox on top/bottom
        else {
            H = 8f / screenRatio;
        }
        _camera.orthographicSize = H/2f;
    }
}