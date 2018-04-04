using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager {
    public enum Zoom { In, Out }

    public bool Lock_Zoom { get; set; }

    private static CameraManager instance;
    private float speed;
    private float zoom_speed;
    private float default_zoom;
    private float min_zoom;
    private float max_zoom;

    private CameraManager()
    {
        speed = 0.1f;
        zoom_speed = 1.0f;
        default_zoom = 10.0f;
        min_zoom = 1.0f;
        max_zoom = 12.0f;
        Lock_Zoom = false;
        Camera.main.orthographicSize = default_zoom;
    }

    /// <summary>
    /// Accessor for singleton instance
    /// </summary>
    public static CameraManager Instance
    {
        get {
            if (instance == null) {
                instance = new CameraManager();
            }
            return instance;
        }
        private set {
            instance = value;
        }
    }

    /// <summary>
    /// Moves main camera
    /// </summary>
    /// <param name="delta"></param>
    /// <returns></returns>
    public bool Move_Camera(Vector2 delta)
    {
        if(Game.Instance.State != Game.GameState.RUNNING) {
            return false;
        }
        Camera.main.transform.Translate(delta);
        return true;
    }

    /// <summary>
    /// Moves main camera
    /// </summary>
    /// <param name="delta"></param>
    /// <returns></returns>
    public bool Move_Camera(Map.Direction direction)
    {
        if (Game.Instance.State != Game.GameState.RUNNING) {
            return false;
        }
        bool success = true;
        switch(direction) {
            case Map.Direction.North:
                Camera.main.transform.Translate(new Vector3(0.0f, speed, 0.0f));
                break;
            case Map.Direction.East:
                Camera.main.transform.Translate(new Vector3(speed, 0.0f, 0.0f));
                break;
            case Map.Direction.South:
                Camera.main.transform.Translate(new Vector3(0.0f, -speed, 0.0f));
                break;
            case Map.Direction.West:
                Camera.main.transform.Translate(new Vector3(-speed, 0.0f, 0.0f));
                break;
            default:
                success = false;
                break;
        }
        return success;
    }

    /// <summary>
    /// Sets main camera to a new location
    /// </summary>
    /// <param name="location"></param>
    /// <returns></returns>
    public bool Set_Camera_Location(Vector2 location)
    {
        if (Game.Instance.State != Game.GameState.RUNNING) {
            return false;
        }
        Game.Instance.Map.Update_GOs();
        Camera.main.transform.position = new Vector3(location.x, location.y, Camera.main.transform.position.z);
        return true;
    }

    /// <summary>
    /// Zooms main camera
    /// </summary>
    /// <param name="zoom"></param>
    /// <returns></returns>
    public bool Zoom_Camera(Zoom zoom)
    {
        if (Lock_Zoom) {
            return false;
        }

        if (Game.Instance.State != Game.GameState.RUNNING) {
            return false;
        }
        if(zoom == Zoom.In) {
            Camera.main.orthographicSize += zoom_speed;
            if (Camera.main.orthographicSize > max_zoom) {
                Camera.main.orthographicSize = max_zoom;
            }
        } else {
            Camera.main.orthographicSize -= zoom_speed;
            if(Camera.main.orthographicSize < min_zoom) {
                Camera.main.orthographicSize = min_zoom;
            }
        }
        return true;
    }

    /// <summary>
    /// Resets current zoom level to default zoom
    /// </summary>
    /// <returns></returns>
    public bool Reset_Zoom()
    {
        if (Game.Instance.State != Game.GameState.RUNNING) {
            return false;
        }
        Camera.main.orthographicSize = default_zoom;
        return true;
    }

    public Camera Camera {
        get {
            return Camera.main;
        }
        private set {
            //WIP?
        }
    }

    /// <summary>
    /// Returns world coordinate points that make screen
    /// </summary>
    /// <returns></returns>
    public List<Vector2> Get_Screen_Location()
    {
        List<Vector2> points = new List<Vector2>();
        points.Add(Camera.ScreenToWorldPoint(new Vector3(0.0f, 0.0f, 0.0f)));
        points.Add(Camera.ScreenToWorldPoint(new Vector3(Screen.width, 0.0f, 0.0f)));
        points.Add(Camera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0.0f)));
        points.Add(Camera.ScreenToWorldPoint(new Vector3(0.0f, Screen.height, 0.0f)));
        return points;
    }
}
