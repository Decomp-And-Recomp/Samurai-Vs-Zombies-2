using UnityEngine;

[RequireComponent(typeof(Camera))]
public class MaxAspectRatio : MonoBehaviour
{
    public float maxAspectRatio = 16f / 9f;

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        UpdateViewport();
    }

    void UpdateViewport()
    {
        float windowAspect = (float)Screen.width / (float)Screen.height;

        if (windowAspect > maxAspectRatio)
        {
            float scaleWidth = maxAspectRatio / windowAspect;
            Rect rect = new Rect((1f - scaleWidth) / 2f, 0f, scaleWidth, 1f);
            cam.rect = rect;
        }
        else
        {
            cam.rect = new Rect(0f, 0f, 1f, 1f);
        }
    }

    void OnPreCull()
    {
        GL.Clear(true, true, Color.black);
    }
}
