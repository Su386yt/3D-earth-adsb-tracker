using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class PlaneBehavior : MonoBehaviour
{
    bool initiated = false;
    Gamemanager gamemanager;
    string icao24Id;

    readonly int METRES_PER_LATITITUDE = 111111;

    public void Initiate(Gamemanager gamemanager, string icao24) {
        icao24Id = icao24;
        this.gamemanager = gamemanager;
        initiated = true;
        gameObject.AddComponent<BoxCollider>();
    }

    public void DeletePlane() {
        gamemanager.planeMap.Remove(icao24Id);
        gamemanager.dataMap.Remove(icao24Id);
        Destroy(gameObject);
    }

    PlaneData GetPlaneData() {
        return gamemanager.dataMap[icao24Id];
    }

    

    void UpdateLocation() {
        PlaneData planeData = GetPlaneData();

        Vector3 position = gamemanager.CalculateCoordinates(PredictLatitude(), PredictLongitude(), planeData.altitudeAboveSea);
        transform.position = position;
    }

    // Based on last known location speed and heading, calculate the position
    float PredictLatitude() {
        PlaneData planeData = GetPlaneData();
        float latitudeDistanceTraveled = PredictDistanceTraveled() * Mathf.Cos(planeData.trueHeading);
        return MetresToLatitude(latitudeDistanceTraveled) + planeData.latitude;       
    }

    float PredictLongitude() {
        PlaneData planeData = GetPlaneData();
        float longitudeDistanceTraveled = PredictDistanceTraveled() * Mathf.Sin(planeData.trueHeading);
        return MetresToLongitude(longitudeDistanceTraveled, planeData.latitude) + planeData.longitude;
    }

    // Predicts the distance traveled in metres from the last given location
    float PredictDistanceTraveled() {
        PlaneData planeData = GetPlaneData();
        return planeData.velocity * ((DateTimeOffset.Now.ToUnixTimeSeconds()) - planeData.lastPositionUpdateTime);
    }

    float MetresToLatitude(float metres) {
        return metres / METRES_PER_LATITITUDE;
    }

    float MetresToLongitude(float metres, float latitude) {
        return metres / (METRES_PER_LATITITUDE * Mathf.Cos(Gamemanager.ToRadians(latitude)));
    }

    bool IfLanded() {
        if (gamemanager.dataMap.ContainsKey(icao24Id)) {
            return false;
        }

        return true;
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!initiated) {
            return;
        }
        if (IfLanded()) {
            DeletePlane();
            return;
        }
        UpdateLocation();
        
    }

    void OnMouseEnter() {
        Debug.Log("tset");
        GetComponent<Renderer>().material.color = new Color32(0, 255, 0, 0);
    }

    void OnMouseOver() {
        // The mouse is over the GameObject so it will be highlighted.
    }

    void OnMouseExit() {
        GetComponent<Renderer>().material.color = new Color32(255, 0, 0, 0);
    }
}
