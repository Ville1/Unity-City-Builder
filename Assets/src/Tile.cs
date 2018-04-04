using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This class represents individual tile on a map
/// </summary>
public class Tile {
    private static float rendering_distance = 0.00f;
    private static bool limited_rendering = false;

    public int X { get; private set; }
    public int Y { get; private set; }
    private bool buildable;
    public string Texture { get; private set; }
    public string Terrain { get; private set; }
    public Building Building { get; set; }
    public Dictionary<string, Dictionary<string, float>> Yields { get; private set; }//TODO ?
    public Dictionary<string, float> Ore { get; private set; }
    public Dictionary<string, object> Data { get; private set; }
    public Map Map { get; private set; }
    public float Appeal { get; set; }
    public float Terrain_Appeal_Effect { get; private set; }
    public float Terrain_Appeal_Range { get; private set; }
    private bool no_rendering;
    private GameObject parent;
    private GameObject game_object;
    private SpriteRenderer renderer;
    private Color highlight_color;

    private Map.Overlay current_overlay;
    private GameObject info_text;

    /// <summary>
    /// Tile constructor
    /// </summary>
    /// <param name="map"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public Tile (Map map, int x, int y, Tile prototype)
    {
        Map = map;
        X = x;
        Y = y;

        Terrain = prototype.Terrain;
        Texture = prototype.Texture;
        buildable = prototype.buildable;
        highlight_color = Color.clear;
        Building = null;
        Yields = new Dictionary<string, Dictionary<string, float>>();
        foreach(KeyValuePair<string, Dictionary<string, float>> yield in Yields) {
            Yields.Add(yield.Key, new Dictionary<string, float>());
            foreach(KeyValuePair<string, float> resource in yield.Value) {
                Yields[yield.Key].Add(resource.Key, resource.Value);
            }
        }

        Ore = new Dictionary<string, float>();
        foreach(KeyValuePair<string, float> ore in prototype.Ore) {
            Ore.Add(ore.Key, ore.Value);
        }

        Data = new Dictionary<string, object>();
        parent = GameObject.Find("TileListObject");

        //GOs
        game_object = new GameObject();
        game_object.name = "Tile(" + X + "," + Y + ")";
        game_object.transform.position = new Vector3(X, Y, 0.0f);
        game_object.transform.parent = GameObject.Find("TileListObject").transform;
        renderer = game_object.AddComponent<SpriteRenderer>();
        renderer.sprite = SpriteManager.Instance.Get_Sprite(Texture);

        //Appeal
        Appeal = prototype.Terrain_Appeal_Effect;
        Terrain_Appeal_Effect = prototype.Terrain_Appeal_Effect;
        Terrain_Appeal_Range = prototype.Terrain_Appeal_Range;

        //Info string
        info_text = null;
    }

    /// <summary>
    /// Change terrain
    /// </summary>
    /// <param name="prototype"></param>
    public void Change_To(Tile prototype)
    {
        float old_appeal_effect = Terrain_Appeal_Effect;
        float old_appeal_range = Terrain_Appeal_Range;

        Terrain = prototype.Terrain;
        Texture = prototype.Texture;
        buildable = prototype.buildable;
        Appeal -= (Terrain_Appeal_Effect - prototype.Terrain_Appeal_Effect);

        Terrain_Appeal_Effect = prototype.Terrain_Appeal_Effect;
        Terrain_Appeal_Range = prototype.Terrain_Appeal_Range;
        Yields = new Dictionary<string, Dictionary<string, float>>();
        foreach (KeyValuePair<string, Dictionary<string, float>> yield in Yields) {
            Yields.Add(yield.Key, new Dictionary<string, float>());
            foreach (KeyValuePair<string, float> resource in yield.Value) {
                Yields[yield.Key].Add(resource.Key, resource.Value);
            }
        }
        Update_Appeal(old_appeal_effect, old_appeal_range);
        renderer.sprite = SpriteManager.Instance.Get_Sprite(Texture);
    }

    /// <summary>
    /// Tile constructor for prototypes
    /// </summary>
    /// <param name="terrain"></param>
    /// <param name="texture"></param>
    /// <param name="buildable"></param>
    /// <param name="appeal"></param>
    /// <param name="appeal_range"></param>
    public Tile(string terrain, string texture, bool buildable, float appeal, float appeal_range)
    {
        Terrain = terrain;
        Texture = texture;
        Appeal = appeal;
        Terrain_Appeal_Effect = appeal;
        Terrain_Appeal_Range = appeal_range;
        Yields = new Dictionary<string, Dictionary<string, float>>();
        Ore = new Dictionary<string, float>();
        this.buildable = buildable;
    }

    /// <summary>
    /// Color used to highlight this tile
    /// </summary>
    public Color Highlight
    {
        get {
            return highlight_color;
        }
        set {
            if(value != highlight_color) {
                highlight_color = value;
            } else {
                highlight_color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            }
            game_object.GetComponent<SpriteRenderer>().color = highlight_color;
        }
    }

    /// <summary>
    /// Turns off rendering of tile
    /// </summary>
    public bool No_Rendering {
        get {
            return no_rendering;
        }
        set {
            no_rendering = value;
            renderer.enabled = !no_rendering;
        }
    }


    /// <summary>
    /// Vector2 representation of this tile's position
    /// </summary>
    /// <returns></returns>
    public Vector2 Position
    {
        get {
            return new Vector2(X, Y);
        }
        private set {
            X = (int)value.x;
            Y = (int)value.y;
        }
    }

    /// <summary>
    /// Deletes GO if it is not visible and creates one if it is
    /// Also handles rendering of overlays
    /// </summary>
    public void Update_GO()
    {
        if(limited_rendering) {
            bool on_screen = On_Screen();
            if (game_object == null && on_screen) {
                //Create GO
                game_object = new GameObject();
                game_object.name = "Tile(" + X + "," + Y + ")";
                game_object.transform.position = new Vector3(X + 0.0f, Y - 0.5f, 0.0f);
                game_object.transform.parent = parent.transform;
                renderer = game_object.AddComponent<SpriteRenderer>();
                renderer.sprite = SpriteManager.Instance.Get_Sprite(Texture);
            } else if (game_object != null && !on_screen) {
                //Delete GO
                GameObject.Destroy(game_object);
                game_object = null;
            }
        }
        Update_Info();
    }

    /// <summary>
    /// Calculates distance to other tile
    /// </summary>
    /// <param name="tile"></param>
    /// <returns></returns>
    public float Distance(Tile tile)
    {
        return Mathf.Sqrt((X - tile.X) * (X - tile.X) + (Y - tile.Y) * (Y - tile.Y));
    }

    /// <summary>
    /// Is this tile on screen?
    /// </summary>
    /// <returns></returns>
    public bool On_Screen()
    {
        List<Vector2> screen = CameraManager.Instance.Get_Screen_Location();
        return (X > (screen[0].x * (1.0f - rendering_distance)) && X < (screen[2].x * (1.0f + rendering_distance)) && 
            Y > (screen[0].y * (1.0f - rendering_distance)) && Y < (screen[2].y * (1.0f + rendering_distance)));//TODO fix?
    }

    public bool Buildable {
        get {
            if(Building != null) {
                return false;
            }
            return buildable;
        }
        private set {
            buildable = value;
        }
    }

    public override string ToString()
    {
        return "Tile (" + X + "," + Y + ")";
    }

    /// <summary>
    /// Update appeal of surrouding tiles
    /// </summary>
    public void Update_Appeal(float old_appeal_effect, float old_appeal_range)
    {
        if (old_appeal_effect != 0.0f && old_appeal_range != 0.0f) {
            //Undo old effect
            foreach (Tile tile in Map.Get_Tiles_In_Circle(X, Y + 1, old_appeal_range)) {// Y + 1   WUT?
                if (tile.ToString() != ToString()) {
                    tile.Appeal -= ((old_appeal_effect + (old_appeal_effect * (1.0f - (Distance(tile) / old_appeal_range)))) / 2.0f);
                }
            }
        }

        //Apply new effect
        float effect = Terrain_Appeal_Effect;
        float range = Terrain_Appeal_Range;
        if (Building != null && Building.Appeal_Effect != 0.0f && Building.Is_Built) {
            effect = Building.Appeal_Effect;
            range = Building.Appeal_Range;
        }
        if(effect == 0.0f || range == 0.0f) {
            return;
        }
        foreach(Tile tile in Map.Get_Tiles_In_Circle(X, Y + 1, range)) {// Y + 1   WUT?
            if (tile.ToString() != ToString()) {
                tile.Appeal += ((effect + (effect * (1.0f - (Distance(tile) / range)))) / 2.0f);
            }
        }
    }

    /// <summary>
    /// Show info overlay
    /// </summary>
    /// <param name="overlay"></param>
    public void Show_Info(Map.Overlay overlay)
    {
        if(current_overlay == overlay) {
            if(info_text != null) {
                GameObject.Destroy(info_text);
            }
            info_text = null;
            current_overlay = Map.Overlay.No_Overlay;
            return;
        }
        current_overlay = overlay;
        Update_GO();
    }

    /// <summary>
    /// Update overlay info
    /// </summary>
    public void Update_Info()
    {
        if(!On_Screen() || current_overlay == Map.Overlay.No_Overlay) {
            if(info_text != null) {
                GameObject.Destroy(info_text);
            }
            return;
        }
        string text = "";
        switch (current_overlay) {
            case Map.Overlay.Appeal:
                text = "" + Math.Round(Appeal, 1);
                break;
            case Map.Overlay.Ore:
                if(Ore.Count != 0) {
                    foreach (KeyValuePair<string, float> ore in Ore) {
                        string char_s = ore.Key[0] + "";
                        text += (char_s.ToUpper() + Math.Round(ore.Value, 0));
                    }
                }
                break;
            case Map.Overlay.Build:
                if (Building != null && Building.Data.ContainsKey("connection_range")) {
                    text = "" + (int)Building.Data["connection_range"];
                }
                break;
        }
        if(text != "") {
            if (info_text == null) {
                info_text = GameObject.Instantiate(MenuManager.Instance.Tile_Info_Prefab);
            }
            info_text.gameObject.transform.SetParent(MenuManager.Instance.Canvas.transform);
            Vector3 position = game_object.transform.position;
            position.x += 0.5f;
            position.y -= 0.5f;
            info_text.transform.position = CameraManager.Instance.Camera.WorldToScreenPoint(position);
            info_text.gameObject.GetComponentInChildren<Text>().text = text;
        }
    }
}
