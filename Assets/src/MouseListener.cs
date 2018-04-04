using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseListener : MonoBehaviour {
    public static MouseListener Instance;

    private Vector3 last_position;
    private Tile tile_under_cursor;
    private List<Tile> highlighted_tiles;
    private Color highlight_color;
    private List<Tile> highlighted_connected_building_tiles;
    private Color highlight_connected_color;
    public GameObject Transparent_Building;

    /// <summary>
    /// Initialization
    /// </summary>
	private void Start () {
        if(Instance != null) {
            Logger.Instance.Error("Start called multiple times");
        }
        Instance = this;
        Transparent_Building.SetActive(false);
        tile_under_cursor = null;
        highlighted_tiles = new List<Tile>();
        highlight_color = new Color(0.1f, 0.5f, 0.1f, 0.5f);
        highlighted_connected_building_tiles = new List<Tile>();
        highlight_connected_color = new Color(0.1f, 0.1f, 0.5f, 0.5f);
    }

    /// <summary>
    /// Per frame update
    /// </summary>
	private void Update () {
        Vector3 current_position = CameraManager.Instance.Camera.ScreenToWorldPoint(Input.mousePosition);
        if (Input.GetMouseButton(1) || Input.GetMouseButton(2)) {
            //Camera
            Vector3 difference = last_position - current_position;
            CameraManager.Instance.Move_Camera(difference);
            //Hide message
            MenuManager.Instance.Hide_Message();
            MenuManager.Instance.Close_Menus();
        }
        if (Input.GetMouseButtonDown(0)) {
            //Hide message
            MenuManager.Instance.Hide_Message();
            if (!EventSystem.current.IsPointerOverGameObject()) {
                MenuManager.Instance.Close_Menus();
            }

            //Right click
            if (Game.Instance.State == Game.GameState.RUNNING && !EventSystem.current.IsPointerOverGameObject()) {
                Tile tile = Get_Tile_At_Mouse();
                if (tile != null) {
                    if (BuildingPrototypes.Currently_Selected != null) {
                        if(!City.Instance.Build(BuildingPrototypes.Get(), tile)) {
                            MenuManager.Instance.Show_Message(City.Instance.Error_Message);
                        } else if(!Input.GetButton("Build Multiple")) {
                            BuildingPrototypes.Currently_Selected = null;
                        }
                    } else {
                        if(tile.Building != null && !ConsoleManager.Instance.Is_Open()) {
                            MenuManager.Instance.Select_Building(tile.Building);
                        } else if(!EventSystem.current.IsPointerOverGameObject()) {
                            MenuManager.Instance.Unselect_Building();
                        }
                    }
                }
            }
        }

        //Zoom
        if(Input.GetAxis("Mouse ScrollWheel") > 0.0f) {
            CameraManager.Instance.Zoom_Camera(CameraManager.Zoom.Out);
        } else if(Input.GetAxis("Mouse ScrollWheel") < 0.0f) {
            CameraManager.Instance.Zoom_Camera(CameraManager.Zoom.In);
        }

        //Transparent building
        if (Transparent_Building.activeSelf) {
            Tile tile = Get_Tile_At_Mouse();
            if(tile != null) {
                Transparent_Building.transform.position = tile.Position;
                if (tile != tile_under_cursor) {
                    //Highlight tiles
                    if(BuildingPrototypes.Get().Attribute("highlight_tiles_during_build") != 0.0f) {
                        //Circle
                        foreach (Tile t in highlighted_tiles) {
                            t.Highlight = highlight_color;
                        }
                        int y_fix = 0; //TODO: fix this
                        if(BuildingPrototypes.Get().Height % 2 != 0) {
                            y_fix = 1;
                        }
                        highlighted_tiles = tile.Map.Get_Tiles_In_Circle(tile.X, tile.Y + y_fix, BuildingPrototypes.Get().Attribute("highlight_tiles_during_build"),
                            BuildingPrototypes.Get().Width % 2 == 0, BuildingPrototypes.Get().Height % 2 == 0);
                        foreach (Tile t in highlighted_tiles) {
                            t.Highlight = highlight_color;
                        }
                    }
                    if(BuildingPrototypes.Get().Range != 0) {
                        //Connected buildings
                        foreach (Tile t in highlighted_connected_building_tiles) {
                            t.Highlight = highlight_connected_color;
                            if(t.Building != null) {
                                t.Building.Clear_Range_Marker();
                            }
                        }
                        List<Building> connected_buildings = BuildingPrototypes.Get().Get_Connected_Buildings(BuildingPrototypes.Get().Range, 0, tile, true);
                        highlighted_connected_building_tiles.Clear();
                        foreach(Building connected_building in connected_buildings) {
                            foreach(Tile t in connected_building.Tiles) {
                                highlighted_connected_building_tiles.Add(t);
                            }
                        }
                        foreach (Tile t in highlighted_connected_building_tiles) {
                            t.Highlight = highlight_connected_color;
                        }
                    }
                }
                tile_under_cursor = tile;
            } else {
                Transparent_Building.transform.position = Get_Mouse_Point();
            }
        }

        if((!Transparent_Building.activeSelf || BuildingPrototypes.Get().Attribute("highlight_tiles_during_build") == 0.0f) && highlighted_tiles.Count != 0) {
            //Clear circle highlights
            foreach (Tile t in highlighted_tiles) {
                t.Highlight = highlight_color;
            }
            highlighted_tiles.Clear();
        }
        if ((!Transparent_Building.activeSelf || BuildingPrototypes.Get().Range == 0) && highlighted_connected_building_tiles.Count != 0) {
            //Clear connected building highlights
            foreach (Tile t in highlighted_connected_building_tiles) {
                t.Highlight = highlight_connected_color;
                if(t.Building != null) {
                    t.Building.Clear_Range_Marker();
                }
            }
            highlighted_connected_building_tiles.Clear();
        }

        last_position = CameraManager.Instance.Camera.ScreenToWorldPoint(Input.mousePosition);
    }

    /// <summary>
    /// Returns world point under mouse
    /// </summary>
    /// <returns></returns>
    public Vector2 Get_Mouse_Point()
    {
        return CameraManager.Instance.Camera.ScreenToWorldPoint(Input.mousePosition);
    }

    /// <summary>
    /// Returns tile under mouse
    /// </summary>
    /// <returns></returns>
    public Tile Get_Tile_At_Mouse()
    {
        Vector2 point = CameraManager.Instance.Camera.ScreenToWorldPoint(Input.mousePosition);
        return Game.Instance.Map.Get_Tile_At(Mathf.RoundToInt(point.x - 0.5f), Mathf.RoundToInt(point.y + 0.5f));
    }
}
