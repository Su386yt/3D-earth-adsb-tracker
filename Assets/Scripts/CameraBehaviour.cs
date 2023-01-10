using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraBehaviour : MonoBehaviour {
    public float latitude;
    public float longitude;
    public float altitude;
    public GameObject gamemanagerObject;
    Gamemanager gamemanager;

    public bool lockMouse;
    public bool hideMouse;

    public float horizontalSpeed = 1.5f;
    public float verticalSpeed = 10000;
    public float rotationalSpeed = 1f;
    public long lastRotationAlignTime;

    public InputAction longitudeInput;
    public InputAction latitudeInput;
    public InputAction altitudeInput;

    public Vector2 mousePosition;
    // Start is called before the first frame update
    void Start() {
        gamemanager = gamemanagerObject.GetComponent<Gamemanager>();
        latitude = 47.449043628553724f;
        longitude = -122.31286478930248f;
        altitude = 13098.780000000093263313600000664f;
        Debug.Log(longitudeInput.GetBindingDisplayString());
        longitudeInput.Enable();
        latitudeInput.Enable();
        altitudeInput.Enable();
        transform.LookAt(new Vector3(0, 0, 0));
    }

    // Update is called once per frame
    void Update() {



        // Calculate the camera's position
        longitude += longitudeInput.ReadValue<float>() * horizontalSpeed * Time.deltaTime * (1 / Mathf.Cos(Gamemanager.ToRadians(latitude)));
        latitude += latitudeInput.ReadValue<float>() * horizontalSpeed * Time.deltaTime;
        altitude += altitudeInput.ReadValue<float>() * verticalSpeed * Time.deltaTime;

        // Adjusts the rotation of the camera so that the camera does not appear to rotate relative to the earth
        transform.rotation *= Quaternion.Euler(latitudeInput.ReadValue<float>() * horizontalSpeed * Time.deltaTime, 0, 0);
        transform.rotation *= Quaternion.Euler(0, -(longitudeInput.ReadValue<float>() * horizontalSpeed * Time.deltaTime * (1 / Mathf.Cos(Gamemanager.ToRadians(latitude)))), 0);

        if (latitude > 90) {
            latitude = 90 - (latitude-90);
        } 
        if(latitude < -90) {
            latitude = -90 - (latitude - -90);
        }
        if (altitude <= 0) {
            altitude = 0;
        }

        
        transform.position = gamemanager.CalculateCoordinates(latitude, longitude, altitude);

        // Get the mouse delta from the Input System
        Vector2 mouseDelta = InputSystem.GetDevice<Mouse>().delta.ReadValue();

        transform.rotation *= Quaternion.Euler(-mouseDelta.y * rotationalSpeed, mouseDelta.x * rotationalSpeed * Mathf.Abs(Mathf.Cos(Gamemanager.ToRadians(latitude))), 0);
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y , 0);

        // Lock the mouse to the center of the screen
        if (lockMouse) {
            Cursor.lockState = CursorLockMode.Locked;
        }
        else {
            Cursor.lockState = CursorLockMode.None;
        }

        // Hide the mouse cursor
        Cursor.visible = !hideMouse;
    }
}
