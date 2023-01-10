using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneData
{
    // Unique ICAO 24-bit address of the transponder in hex string representation.
    public string icao24Id;
    // Callsign of the vehicle (8 chars). Can be null if no callsign has been received.
    public string callsign;
    // Registration Country name inferred from the ICAO 24-bit adress
    public string originCountryName;

    // Unix timestamp (seconds) for the last position update. Can be null if no position report was received by OpenSky within the past 15s.
    public long lastPositionUpdateTime;
    // Unix timestamp in seconds for the last update in general
    public long lastContactTime;

    // Longitude
    public float latitude;
    // Latitude
    public float longitude;
    // Barometric altitude in metres
    public float altitudeAboveSea;
    // True if the plane is on the ground
    public bool onGround;

    // Speed in Metres per second
    public float velocity;
    // True track in decimal degrees clockwise from north (north=0°). Can be null.
    public float trueHeading;
    // Vertical rate in m/s. A positive value indicates that the airplane is climbing, a negative value indicates that it descends. Can be null.
    public float verticalRate;
    // Geometric altitude in meters. Can be null.
    public float altitudeAboveGround;
    // The transponder code aka Squawk. Can be null.
    public string transponderCode;
    // Whether flight status indicates special purpose indicator.
    public bool specialPurpose;
    // Types: https://openskynetwork.github.io/opensky-api/rest.html
    int aircraftGroupId;
    public string aircraftGroupName;

    // Constructs a new PlaneData instance from a given opensky data
    public PlaneData(JSONArray openskyData) {
        Update(openskyData);
    }

    public void Update(JSONArray openskyData) {
        icao24Id = openskyData[0];
        callsign = openskyData[1];
        originCountryName = openskyData[2];
        lastPositionUpdateTime = openskyData[3].AsInt;
        lastContactTime = openskyData[4].AsInt;
        longitude = openskyData[5].AsFloat;
        latitude = openskyData[6].AsFloat;
        altitudeAboveSea = openskyData[7].AsFloat;
        onGround = openskyData[8].AsBool;
        velocity = openskyData[9].AsFloat;
        trueHeading = openskyData[10].AsFloat;
        verticalRate = openskyData[11].AsFloat;
        altitudeAboveGround = openskyData[13].AsFloat;
        transponderCode = openskyData[14];
        specialPurpose = openskyData[15].AsBool;
        aircraftGroupId = openskyData[17].AsInt;
        aircraftGroupName = GetAircraftType(aircraftGroupId);
    } 

 
    

    // Returns the type of aircraft from given id
    static string GetAircraftType(int id) {
        return id switch {
            // 0 = No information at all
            0 => "No information at all",
            // 1 = No ADS-B Emitter Category Information
            1 => "No ADS-B Emitter Category Information",
            // 2 = Light (< 15500 lbs)
            2 => "Light (< 15500 lbs)",
            // 3 = Small (15500 to 75000 lbs)
            3 => "Small (15500 to 75000 lbs)",
            // 4 = Large (75000 to 300000 lbs)
            4 => "Large (75000 to 300000 lbs)",
            // 5 = High Vortex Large (aircraft such as B-757)
            5 => "High Vortex Large (aircraft such as B-757)",
            // 6 = Heavy (> 300000 lbs)
            6 => "Heavy (> 300000 lbs)",
            // 7 = High Performance (> 5g acceleration and 400 kts)
            7 => "High Performance (> 5g acceleration and 400 kts)",
            // 8 = Rotorcraft
            8 => "Rotorcraft",
            // 9 = Glider / sailplane
            9 => "Glider / sailplane",
            // 10 = Lighter-than-air
            10 => "Lighter-than-air",
            // 11 = Parachutist / Skydiver
            11 => "Parachutist / Skydiver",
            // 12 = Ultralight / hang-glider / paraglider
            12 => "Ultralight / hang-glider / paraglider",
            // 13 = Reserved
            13 => "Reserved",
            // 14 = Unmanned Aerial Vehicle
            14 => "Unmanned Aerial Vehicle",
            // 15 = Space / Trans-atmospheric vehicle
            15 => "Space / Trans-atmospheric vehicle",
            // 16 = Surface Vehicle – Emergency Vehicle
            16 => "Emergancy Car",
            // 17 = Surface Vehicle – Service Vehicle
            17 => "Service Car",
            // 18 = Point Obstacle (includes tethered balloons)
            18 => "Point Obstacle",
            // 19 = Cluster Obstacle
            19 => "Cluster Obstacle",
            // 20 = Line Obstacle
            20 => "Line Obstacle",
            // If wrong number is put in
            _ => null,
        };
    }
    

}
