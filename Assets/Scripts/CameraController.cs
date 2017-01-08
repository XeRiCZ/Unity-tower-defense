using UnityEngine;
using System.Collections;

// (c) Jan Urubek

// Camera controller with smooth movement and smooth rotations

public class CameraController : MonoBehaviour {

    public float turnSmoothness = 3.0f;
    public float mouseSensitivity = 2.0f;
    public float movingSmoothness = 1.5f;
    public float normalMovingSpeed = 1.0f;  // normal speed
    public float slowMovingSpeed = 0.25f;   // ctrl is held
    public float fastMovingSpeed = 3.0f;    // shift is held
    public float speedTransition = 0.5f;   // transition between 3 types of speed
    public float zoomSpeed = 3.0f;

    Vector2 turnVector = Vector2.zero;
    Vector3 movingVector = Vector3.zero;
    float actualSpeed = 1.0f;
    float mouseWheelLerp = 0.0f;

    // Update is called once per frame
    void Update()
    {

        // Rotation when right mouse button is held
        if (Input.GetMouseButton(1))
        {
            turnVector.x += Input.GetAxis("Mouse X") * mouseSensitivity;
            turnVector.y += Input.GetAxis("Mouse Y") * mouseSensitivity;

            
            Vector3 newRot = Camera.main.transform.eulerAngles;

            if (turnVector.y > 0 && newRot.x > 200 && newRot.x <= 270)
                turnVector.y = 0;

            else if (turnVector.y < 0 && newRot.x >= 90 && newRot.x < 200)
                turnVector.y = 0;

            newRot.y = Mathf.Lerp(newRot.y, newRot.y + turnVector.x, Time.deltaTime * turnSmoothness);
            newRot.x = Mathf.Lerp(newRot.x, newRot.x - turnVector.y, Time.deltaTime * turnSmoothness);
            newRot.z = 0;
            transform.rotation = Quaternion.Euler(new Vector3(0, newRot.y, 0));
            Camera.main.transform.localRotation = Quaternion.Euler(new Vector3(newRot.x,0,0));
        }

        if(Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            mouseWheelLerp = Mathf.Lerp(mouseWheelLerp, Input.GetAxis("Mouse ScrollWheel"), Time.deltaTime * movingSmoothness);
        }

        float targetSpeed = normalMovingSpeed;
        if (Input.GetKey(KeyCode.LeftShift)) targetSpeed = fastMovingSpeed;
        else if (Input.GetKey(KeyCode.Space)) targetSpeed = slowMovingSpeed;

        actualSpeed = Mathf.Lerp(actualSpeed, targetSpeed, Time.deltaTime * speedTransition);

        // Moving
        movingVector.x = Mathf.Lerp(movingVector.x, Input.GetAxisRaw("Horizontal"), Time.deltaTime * movingSmoothness);
        movingVector.y = Mathf.Lerp(movingVector.y, Input.GetAxisRaw("Flying"), Time.deltaTime * movingSmoothness);
        movingVector.z = Mathf.Lerp(movingVector.z, Input.GetAxisRaw("Vertical"), Time.deltaTime * movingSmoothness);

       Vector3 newPosition =
            transform.position +
            ((transform.forward * movingVector.z) * actualSpeed +
            (transform.right * movingVector.x) * actualSpeed +
            (transform.up * movingVector.y)) * actualSpeed +
            Camera.main.transform.forward * mouseWheelLerp * zoomSpeed;

       newPosition.x = Mathf.Clamp(newPosition.x, -20, Terrain.activeTerrain.terrainData.size.x + 20);
       newPosition.z = Mathf.Clamp(newPosition.z, -20, Terrain.activeTerrain.terrainData.size.z + 20);
       newPosition.y = Mathf.Clamp(newPosition.y, Terrain.activeTerrain.SampleHeight(newPosition)+1, 150);

       transform.position = newPosition;

        // Slowly resets vectors
        turnVector.x = Mathf.Lerp(turnVector.x, 0, Time.deltaTime * turnSmoothness);
        turnVector.y = Mathf.Lerp(turnVector.y, 0, Time.deltaTime * turnSmoothness);

        movingVector.x = Mathf.Lerp(movingVector.x, 0, Time.deltaTime * movingSmoothness / 2);
        movingVector.y = Mathf.Lerp(movingVector.y, 0, Time.deltaTime * movingSmoothness / 2);
        movingVector.z = Mathf.Lerp(movingVector.z, 0, Time.deltaTime * movingSmoothness / 2);

        mouseWheelLerp = Mathf.Lerp(mouseWheelLerp, 0, Time.deltaTime * movingSmoothness / 2);
    }
}
