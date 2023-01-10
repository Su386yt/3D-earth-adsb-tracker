using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

public class Gamemanager : MonoBehaviour
{
    public TextAsset textAsset;
    public GameObject earthPrefab;
    public GameObject earth;
    public SpinFree earthSpinScript;

    public string username;
    public string password;
    public int RADIUS = 1000;
    public float ALTITUDE_SCALE_FACTOR = 7.5f;
    readonly public int EARTH_RADIUS_METRES = 6371000;

    public long lastRequestTime = 0;
    public long lastPlanePlaceTime = 0;
    public long lastRequestResponseTime = 0;
    public long lastSortTime = 0;
    public string jsonText;

    public Dictionary<string, PlaneData> dataMap = new();
    public Dictionary<string, GameObject> planeMap = new();

    // Start is called before the first frame update
    void Start()
    {

       
        Camera cam = Camera.main;
        cam.transform.position = new Vector3(RADIUS * 2, 0, 0);
        this.earth = CreateEarth(RADIUS);
    }


    // Update is called once per frame
    void Update()
    {
        if (!OnCooldown(lastRequestTime, 120 * 1000)) {
            StartCoroutine(GetRequest("https://opensky-network.org/api/states/all", username, password));
            lastRequestTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        if (lastRequestResponseTime != lastSortTime) {
            lastSortTime = lastRequestResponseTime;
            JSONObject json = JSON.Parse(jsonText) as JSONObject;
            if (json == null) {
                return;
            }

            // Gets the node calle "states" that has all of the plane data
            JSONNode statesNode = json.GetValueOrDefault("states", null);
            if (statesNode == null) {
                return;
            }
            JSONArray array = statesNode.AsArray;

            SortData(array);

            
        }

        AddAllPlanes();
        UpdateEarth(RADIUS);
    }

    void AddAllPlanes() {
        foreach (KeyValuePair<string, PlaneData> pair in dataMap) {
            if (planeMap.ContainsKey(pair.Key)) {
                continue;
            }

            GameObject plane = PlacePlane(pair.Key);
            planeMap.Add(pair.Key, plane);
        }
    }


    void SortData(JSONArray openskyStatesNodeData) {
        dataMap.Clear();
        for (int i = 0; i < openskyStatesNodeData.Count; i++) {
            string id = openskyStatesNodeData[i].AsArray[0];
            dataMap.Add(id, new PlaneData(openskyStatesNodeData[i].AsArray));
        }
    }


    GameObject PlacePlane(string icao24) {
        GameObject point;
        point = GameObject.CreatePrimitive(PrimitiveType.Cube);
        MeshRenderer renderer = point.GetComponent<MeshRenderer>();
        renderer.material.color = new Color32(255, 0, 0, 255);

        point.AddComponent<PlaneBehavior>();
        PlaneBehavior planeBehavior = point.GetComponent<PlaneBehavior>();
        planeBehavior.Initiate(this, icao24);

        return point;
    }

    GameObject CreateEarth(int radius) {
        GameObject earth = Instantiate(earthPrefab);
        earth.transform.position = Vector3.zero;
        earth.transform.localScale = new Vector3(radius / (40.39f / 2), radius / (40.39f / 2), radius / (40.39f / 2));
        return earth;
    }

    GameObject UpdateEarth(int radius) {
        earth.transform.localScale = new Vector3(radius / (40.39f / 2), radius / (40.39f / 2), radius / (40.39f / 2));
        return earth;
    }

  

    public static bool OnCooldown(long lastTime, long length) {
        if (DateTimeOffset.Now.ToUnixTimeMilliseconds() > lastTime + length) {
            return false;
        }
        return true;
    }


    IEnumerator GetRequest(string uri, string username, string password) {
        // Set the authorization header
        string auth = Convert.ToBase64String(Encoding.ASCII.GetBytes(username + ":" + password));
        Dictionary<string, string> headers = new();
        headers["Authorization"] = "Basic " + auth;

        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri)) {
            // Set the headers
            webRequest.SetRequestHeader("Authorization", "Basic " + auth);

            // Send the request and yield control until it completes
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError) {
                Debug.LogError("Error: " + webRequest.error);
            }
            else {
                // Print the response
                jsonText = webRequest.downloadHandler.text;

                JSONObject json = JSON.Parse(jsonText) as JSONObject;
                if (json != null) {
                    lastRequestResponseTime = json.GetValueOrDefault("time", 0).AsInt;
                }

                
            }
        }
    }

    float AltitudeToPixels(float altitude) {
        return (altitude * ALTITUDE_SCALE_FACTOR) / (EARTH_RADIUS_METRES / RADIUS);
    }

    float CalculateX(float latitude, float longitude, float radius) {
        return radius * Mathf.Cos(ToRadians(latitude)) * Mathf.Cos(ToRadians(longitude));
    }

    float CalculateY(float latitude, float radius) {
        return radius * Mathf.Sin(ToRadians(latitude));
    }
    float CalculateZ(float latitude, float longitude, float radius) {
        return radius * Mathf.Cos(ToRadians(latitude)) * Mathf.Sin(ToRadians(longitude));
    }

    public Vector3 CalculateCoordinates(float latitude, float longitude, float altitude) {
        return new(CalculateX(latitude, longitude, RADIUS + AltitudeToPixels(altitude)), CalculateY(latitude, RADIUS + AltitudeToPixels(altitude)), CalculateZ(latitude, longitude, RADIUS + AltitudeToPixels(altitude)));
    }

    public static float ToRadians(float degrees) {
        return (degrees * Mathf.PI) / 180;
    }

    //void PlaceCalibrationPonts() {
    //    PlacePlane(25.15752503516053f, -80.77371627203654f, radius);
    //    PlacePlane(28.027522188511185f, -80.56737993613616f, radius);
    //    PlacePlane(27.88029170652975f, -82.85486971189945f, radius);
    //}
}
