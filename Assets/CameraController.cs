using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float cameraMoveSpeed;
    public GameObject mouseCursor;
    Vector2 lastMouseDelta;
    Camera maincamera;
    // Start is called before the first frame update
    void Start()
    {
        lastMouseDelta = Vector2.zero;
        maincamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        // Notice when mouse cursor is close to screen border

        // move camera

        Vector2 md = Input.mousePosition;
        // notice when mouse is near screen border
        Vector3 movement = Vector3.zero;
        float borderWidth = 10.0f;
        // TODO increase movement strength 
        // when cursor is closer to the edge
        if (md.x < borderWidth)
        {
            // move left
            movement.x = -cameraMoveSpeed;
        }
        else if (md.x > Screen.width - borderWidth)
        {
            // move right
            movement.x = cameraMoveSpeed;
        }
        if (md.y < borderWidth)
        {
            // move up
            movement.y = -cameraMoveSpeed;
        }
        else if (md.y > Screen.height - borderWidth)
        {
            // Move down
            movement.y = cameraMoveSpeed;
        }
		maincamera.transform.Translate(movement * Time.deltaTime, Space.World);

        // read scroll wheel input to coom zamera
        Vector2 mouseScrollDelta = Input.mouseScrollDelta;
        float scrollDiff = mouseScrollDelta.y;
        maincamera.orthographicSize = Mathf.Clamp(maincamera.orthographicSize - scrollDiff, 6.0f, 14.0f);
    }
}
