using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
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
    public string jsonText;
    public string username;
    public string password;
    public long lastRequestTime = 0;
    public long lastPlanePlaceTime = 0;
    public List<GameObject> planeObjects;
    public GameObject earthPrefab;
    public GameObject earth;
    public SpinFree earthSpinScript;
    public int radius = 500;
    // Start is called before the first frame update
    void Start()
    {

       
        Camera cam = Camera.main;
        cam.transform.position = new Vector3(radius * 2, 0, 0);
        this.earth = CreateEarth(radius);
        earthSpinScript = earth.GetComponent<SpinFree>();
    }


    // Update is called once per frame
    void Update()
    {
        if (!OnCooldown(lastRequestTime, 120 * 1000)) {
            StartCoroutine(GetRequest("https://opensky-network.org/api/states/all", username, password));
            lastRequestTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        if (!OnCooldown(lastPlanePlaceTime, 500)) {
            PlaceAllPlanes(jsonText, radius);
            lastPlanePlaceTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }
    }
    float GetX(float latitude, float longitude, float radius) {
        return radius * Mathf.Cos(ToRadians(latitude)) * Mathf.Cos(ToRadians(longitude));
    }

    float GetY(float latitude, float radius) {
        return radius * Mathf.Sin(ToRadians(latitude));
    }
    float GetZ(float latitude, float longitude, float radius) {
        return radius * Mathf.Cos(ToRadians(latitude)) * Mathf.Sin(ToRadians(longitude));
    }

    // Places planes based off a rest request API from
    void PlaceAllPlanes(string jsonText, int radius) {
        JSONObject json = JSON.Parse(jsonText) as JSONObject;

        if (json == null) {
            return;
        }

        JSONNode statesNode = json.GetValueOrDefault("states", null);

        if (statesNode == null) {
            return;
        }

        JSONArray array = statesNode.AsArray;
        
        foreach (GameObject plane in planeObjects) {
            Destroy(plane);
        }

        foreach (JSONArray obj in array) {
            float latitude = obj[6];
            float longitude = obj[5];

            planeObjects.Add(PlacePlane(latitude, longitude, radius));

        }

    }

    GameObject PlacePlane(float latitude, float longitude, float radius) {
        GameObject point;

        point = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Vector3 position = new(GetX(latitude, longitude, radius), GetY(latitude, radius), GetZ(latitude, longitude, radius));
        point.transform.position = position;
        MeshRenderer renderer = point.GetComponent<MeshRenderer>();
        renderer.material.color = new Color32(255, 0, 0, 255);
        return point;
    }

    GameObject CreateEarth(int radius) {
        GameObject earth = Instantiate(earthPrefab);
        earth.transform.position = Vector3.zero;
        earth.transform.localScale = new Vector3(radius / (40.39f / 2), radius / (40.39f / 2), radius / (40.39f / 2));
        return earth;
    }

    //void PlaceCalibrationPonts() {
    //    PlacePlane(25.15752503516053f, -80.77371627203654f, radius);
    //    PlacePlane(28.027522188511185f, -80.56737993613616f, radius);
    //    PlacePlane(27.88029170652975f, -82.85486971189945f, radius);
    //}

    bool OnCooldown(long lastTime, long length) {
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
            }
        }
    }

    float ToRadians(float degrees) {
        return (degrees * Mathf.PI) / 180;
    }
}
