using UnityEngine;

public class Game {
    private static Game instance;
    public static float VERSION = 0.1f;
    public enum GameState { NOT_INITIALIZED, READY, RUNNING, ERROR }
    public GameState State { get; private set; }
    public Map Map { get; private set; }
    public float Speed { get; private set; } //Days / minute
    private float speed_before_pause;

    private Game ()
    {
        State = GameState.NOT_INITIALIZED;
        Speed = 1.0f;
        speed_before_pause = Speed;

        State = GameState.READY;
    }

    /// <summary>
    /// Accessor for singleton instance
    /// </summary>
    public static Game Instance
    {
        get {
            if (instance == null) {
                instance = new Game();
            }
            return instance;
        }
        private set {
            instance = value;
        }
    }

    /// <summary>
    /// Starts a new game
    /// </summary>
    public void New()
    {
        if (State != GameState.READY) {
            Logger.Instance.Debug("Game: Can\' start a new game! Invalid state: " + State.ToString());
            return;
        }
        
        //Generate a new map
        Map = new Map(100, 100);
        //Create city
        City.Instance.New("City Name");
        //Center camera
        State = GameState.RUNNING;
        CameraManager.Instance.Set_Camera_Location(new Vector2(Map.Width / 2, Map.Height / 2));
        MenuManager.Instance.Set_Speed(Speed_Index());
        MenuManager.Instance.City_Menu_Enabled = true;

        //TEST
        /*float min = 1.0f;
        float max = 10.0f;
        float break_point = 7.0f;
        float multiplier_1 = 2.0f;
        float multiplier_2 = 1.0f;

        for(int i = -1; i < 12; i++) {
            Debug.Log( i + " -> " + Helper.Break_Point_Multiply((float)i, min, max, break_point, multiplier_1, multiplier_2));
        }*/


        /*for (float i = -5.0f; i < 10.0f; i += 0.5f) {
            float appeal_effect = 0.0f;
            float average_appeal = i;
            if (average_appeal < 0.5f) {
                appeal_effect = -1.0f * Helper.Break_Point_Multiply(-1.0f * average_appeal, 0.5f, 3.0f, 5.0f, 0.1f, 0.05f);
            } else if (average_appeal > 0.5f && average_appeal <= 5.0f) {
                appeal_effect = Helper.Break_Point_Multiply(average_appeal, 0.5f, 2.0f, 5.0f, 0.05f, 0.025f);
            } else if (average_appeal > 5.0f) {
                appeal_effect = 0.175f + ((average_appeal - 5.0f) * 0.01f);
            }
            Debug.Log(average_appeal + " -> " + appeal_effect);
        }*/
    }

    /// <summary>
    /// Closes game
    /// </summary>
    public void Quit()
    {
        Application.Quit();
    }

    public int Speed_Index()
    {
        if(Speed == 1.0f) {
            return 1;
        } else if(Speed == 0.0f) {
            return 0;
        } else if (Speed == 2.0f) {
            return 2;
        } else if (Speed == 5.0f) {
            return 3;
        }
        Logger.Instance.Error("Invalid game speed: " + Speed);
        Speed = 1.0f;
        return 1;
    }

    public void Speed_Up()
    {
        if(State != GameState.RUNNING) {
            return;
        }
        if (Speed == 1.0f) {
            Speed = 2.0f;
        } else if (Speed == 0.0f) {
            Speed = 1.0f;
        } else if (Speed == 2.0f) {
            Speed = 5.0f;
        }
        MenuManager.Instance.Set_Speed(Speed_Index());
    }

    public void Speed_Down()
    {
        if (State != GameState.RUNNING) {
            return;
        }
        if (Speed == 1.0f) {
            Speed = 0.0f;
        } else if (Speed == 2.0f) {
            Speed = 1.0f;
        } else if (Speed == 5.0f) {
            Speed = 2.0f;
        }
        MenuManager.Instance.Set_Speed(Speed_Index());
    }

    public void Toggle_Pause()
    {
        if (State != GameState.RUNNING) {
            return;
        }
        if (Speed != 0.0f) {
            speed_before_pause = Speed;
            Speed = 0.0f;
        } else {
            Speed = speed_before_pause;
        }
        MenuManager.Instance.Set_Speed(Speed_Index());
    }

    public bool Is_Paused
    {
        get {
            return Speed == 0;
        }
    }
}
