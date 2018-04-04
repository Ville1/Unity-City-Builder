using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteManager : MonoBehaviour {
    public static SpriteManager Instance { get; private set; }

    private Dictionary<string, Sprite> sprites;


    /// <summary>
    /// Initialization
    /// </summary>
    void Start () {
        if (Instance != null) {
            Logger.Instance.Error("Start called multiple times");
        }
        Instance = this;

        sprites = new Dictionary<string, Sprite>();
        foreach (Sprite texture in Resources.LoadAll<Sprite>("images/buildings")) {
            sprites.Add("building_" + texture.name, texture);
        }

        foreach (Sprite texture in Resources.LoadAll<Sprite>("images/terrain")) {
            sprites.Add("tile_" + texture.name, texture);
        }

        foreach (Sprite texture in Resources.LoadAll<Sprite>("images/ui")) {
            sprites.Add("ui_" + texture.name, texture);
        }
    }

    /// <summary>
    /// Get sprite by name
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public Sprite Get_Sprite(string type)
    {
        if(type == "building_road") {
            type = "building_road_nesw";
        }

        if (sprites.ContainsKey(type)) {
            return sprites[type];
        }
        if(type.StartsWith("building_")) {
            return sprites["building_2x2_placeholder"];
        }
        Logger.Instance.Warning("SpriteManager: Sprite " + type + " does not exist!");
        return null;
    }
}
