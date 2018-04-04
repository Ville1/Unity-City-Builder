using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class represents map city is build on
/// </summary>
public class Map {
    public enum Direction { North, North_East, East, South_East, South, South_West, West, North_West }
    public enum Overlay { No_Overlay, Appeal, Ore, Build }

    private List<List<Tile>> tiles;
    private System.Random rng;
    public int Width { get; private set; }
    public int Height { get; private set; }
    public Overlay Current_Overlay { get; private set; }

    public Map(int width, int height)
    {
        Width = width;
        Height = height;
        rng = new System.Random();
        tiles = new List<List<Tile>>();

        //Generate
        Dictionary<int, Tile> seed = new Dictionary<int, Tile>();
        seed.Add(1000, TilePrototypes.Get("grass"));
        seed.Add(1010, TilePrototypes.Get("forest"));
        seed.Add(1060, TilePrototypes.Get("sparse_forest"));
        seed.Add(1080, TilePrototypes.Get("fertile_ground"));
        int max = 1080;

        for (int x = 0; x < width; x++) {
            tiles.Add(new List<Tile>());
            for (int y = 0; y < height; y++) {
                int random = rng.Next(max);
                Tile tile = null;
                foreach (KeyValuePair<int, Tile> seed_pair in seed) {
                    if (seed_pair.Key >= random) {
                        tile = seed_pair.Value;
                        break;
                    }
                }
                tiles[x].Add(new Tile(this, x, y, tile));
            }
        }

        //Update appeal
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                tiles[x][y].Update_Appeal(0.0f, 0.0f);
            }
        }

        //Spawn hills
        int hill_change = 10; //Promiles
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                if (rng.Next(1000) < hill_change) {
                    //Get squares under new hill
                    List<Tile> hill_tiles = new List<Tile>();
                    bool invalid_tile = false;
                    for (int delta_x = 0; delta_x < 3 && !invalid_tile; delta_x++) {
                        for (int delta_y = 0; delta_y < 3 && !invalid_tile; delta_y++) {
                            Tile t = Get_Tile_At(x + delta_x, y + delta_y);
                            if (t == null || t.Terrain == "Hill") {
                                invalid_tile = true;
                            }
                            hill_tiles.Add(t);
                        }
                    }
                    if (invalid_tile) {
                        continue;
                    }
                    for(int i = 0; i < hill_tiles.Count; i++) {
                        hill_tiles[i].Change_To(TilePrototypes.Get("hill"));
                        if (i != 2) {
                            hill_tiles[i].No_Rendering = true;
                        }
                    }
                }
            }
        }

        //Spread terrain
        Dictionary<string, Dictionary<Tile, float[]>> spread = new Dictionary<string, Dictionary<Tile, float[]>>();
        //Forest
        spread.Add("Forest", new Dictionary<Tile, float[]>());
        spread["Forest"].Add(TilePrototypes.Get("sparse_forest"), new float[2] { 50.0f, 2.0f });//Change, range
        spread["Forest"].Add(TilePrototypes.Get("forest"), new float[2] { 25.0f, 2.0f });
        spread.Add("Sparse Forest", new Dictionary<Tile, float[]>());
        spread["Sparse Forest"].Add(TilePrototypes.Get("sparse_forest"), new float[2] { 1.0f, 5.0f });
        spread.Add("Hill", new Dictionary<Tile, float[]>());
        spread["Hill"].Add(TilePrototypes.Get("sparse_forest"), new float[2] { 5.0f, 4.0f });
        spread["Hill"].Add(TilePrototypes.Get("forest"), new float[2] { 3.0f, 2.0f });
        spread.Add("Fertile Ground", new Dictionary<Tile, float[]>());
        spread["Fertile Ground"].Add(TilePrototypes.Get("sparse_forest"), new float[2] { 20.0f, 1.0f });
        spread["Fertile Ground"].Add(TilePrototypes.Get("fertile_ground"), new float[2] { 1.0f, 2.0f });


        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                if (spread.ContainsKey(tiles[x][y].Terrain)) {
                    foreach (KeyValuePair<Tile, float[]> spread_data in spread[tiles[x][y].Terrain]) {
                        List<Tile> affected_tiles = Get_Tiles_In_Circle(x, y, spread_data.Value[1]);
                        foreach (Tile tile in affected_tiles) {
                            if (tile.Terrain != "Hill" && rng.Next(100) < spread_data.Value[0] / (tile.Distance(tiles[x][y]))) {
                                tile.Change_To(spread_data.Key);
                            }
                        }
                    }
                }
            }
        }

        //Spawn ore
        Dictionary<int, string> ore_seed = new Dictionary<int, string>();
        ore_seed.Add(1000, "iron");
        ore_seed.Add(2200, "coal");
        ore_seed.Add(3250, "salt");
        max = 3250;
        int base_ore_spawn_change = 3; //Promiles

        Dictionary<string, int[]> ore_data = new Dictionary<string, int[]>();
        ore_data.Add("iron", new int[2] { 25, 200 });// / 100
        ore_data.Add("coal", new int[2] { 25, 200 });
        ore_data.Add("salt", new int[2] { 25, 200 });

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                int random = rng.Next(1000);
                int ore_spawn_change = base_ore_spawn_change;
                if(tiles[x][y].Terrain == "Hill") {
                    ore_spawn_change *= 10;
                }
                if(random < ore_spawn_change) {
                    //Spawn vein
                    random = rng.Next(max);
                    string ore_name = "???";
                    float ore_amount = -1.0f;
                    foreach (KeyValuePair<int, string> seed_pair in ore_seed) {
                        if (seed_pair.Key >= random) {
                            ore_name = seed_pair.Value;
                            ore_amount = (rng.Next(ore_data[ore_name][1] - ore_data[ore_name][0]) + ore_data[ore_name][0]) / 100.0f;
                            break;
                        }
                    }
                    tiles[x][y].Ore.Add(ore_name, ore_amount);
                }
            }
        }

        //Spread ore
        Dictionary<string, float[]> ore_spread = new Dictionary<string, float[]>();
        ore_spread.Add("iron", new float[2] { 15.0f, 2.0f });
        ore_spread.Add("coal", new float[2] { 8.0f, 2.5f });
        ore_spread.Add("salt", new float[2] { 19.0f, 1.5f });

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                foreach(KeyValuePair<string, float> ore in tiles[x][y].Ore) {
                    if(ore_spread.ContainsKey(ore.Key)) {
                        List<Tile> affected_tiles = Get_Tiles_In_Circle(x, y, ore_spread[ore.Key][1]);
                        foreach (Tile tile in affected_tiles) {
                            if(!tile.Ore.ContainsKey(ore.Key)) {
                                int random = rng.Next(100);
                                int ore_spread_change = Mathf.RoundToInt(ore_spread[ore.Key][0] / (tile.Distance(tiles[x][y])));
                                if(tile.Terrain == "Hill") {
                                    ore_spread_change *= 3;
                                }
                                if (random < ore_spread_change) {
                                    tile.Ore.Add(ore.Key, (rng.Next(ore_data[ore.Key][1] - ore_data[ore.Key][0]) + ore_data[ore.Key][0]) / 100.0f);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Returns tile at coordinates
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public Tile Get_Tile_At(int x, int y, Direction? offset = null)
    {
        if (offset == null) {
            if (x < 0 || y < 0 || x >= Width || y >= Height) {
                return null;
            }
            return tiles[x][y];
        }
        switch (offset) {
            case Direction.North:
                y++;
                break;
            case Direction.North_East:
                y++;
                x++;
                break;
            case Direction.East:
                x++;
                break;
            case Direction.South_East:
                y--;
                x++;
                break;
            case Direction.South:
                y--;
                break;
            case Direction.South_West:
                y--;
                x--;
                break;
            case Direction.West:
                x--;
                break;
            case Direction.North_West:
                y++;
                x--;
                break;
        }
        if (x < 0 || y < 0 || x >= Width || y >= Height) {
            return null;
        }
        return tiles[x][y];
    }

    /// <summary>
    /// Returns list of tiles in the rectangle
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    public List<Tile> Get_Tiles(int x, int y, int width, int height)
    {
        List<Tile> tiles = new List<Tile>();
        for (int x_i = x; x_i < width + x; x_i++) {
            for (int y_i = y; y_i > y - height; y_i--) {
                Tile tile = Get_Tile_At(x_i, y_i);
                if(tile == null) {
                    return null;
                }
                tiles.Add(tile);
            }
        }
        return tiles;
    }

    /// <summary>
    /// Returns list of tiles adjancent to tile
    /// </summary>
    /// <param name="tile"></param>
    /// <returns></returns>
    public Dictionary<Direction, Tile> Get_Adjanced_Tiles(Tile tile, bool diagonal = false)
    {
        Dictionary<Direction, Tile> tiles = new Dictionary<Direction, Tile>();
        Dictionary<Direction, Tile> possible_tiles = new Dictionary<Direction, Tile>();
        if (!diagonal) {
            possible_tiles.Add(Direction.North, Get_Tile_At(tile.X, tile.Y, Direction.North));
            possible_tiles.Add(Direction.East, Get_Tile_At(tile.X, tile.Y, Direction.East));
            possible_tiles.Add(Direction.South, Get_Tile_At(tile.X, tile.Y, Direction.South));
            possible_tiles.Add(Direction.West, Get_Tile_At(tile.X, tile.Y, Direction.West));
        } else {
            foreach(Direction direction in Enum.GetValues(typeof(Direction))) {
                possible_tiles.Add(direction, Get_Tile_At(tile.X, tile.Y, direction));
            }
        }
        foreach(KeyValuePair<Direction, Tile> possible_tile in possible_tiles) {
            if(possible_tile.Value != null) {
                tiles.Add(possible_tile.Key, possible_tile.Value);
            }
        }
        return tiles;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public List<Tile> Get_Tiles_In_Circle(int x_p, int y_p, float range, bool plus_half_x = false, bool plus_half_y = false)
    {
        List<Tile> list = new List<Tile>();

        bool searching = true;
        int loop_index = 0;
        y_p -= 1;
        while (searching) {
            bool insert = false;

            int loop_x = x_p - 1 - loop_index;
            int loop_x_max = x_p + 1 + loop_index;
            if (plus_half_x) {
                loop_x = x_p - loop_index;
                loop_x_max = x_p + 1 + loop_index;
            }
            int loop_y = y_p - 1 - loop_index;
            int loop_y_max = y_p + 1 + loop_index;
            if (plus_half_y) {
                loop_y = y_p - loop_index;
                loop_y_max = y_p + 1 + loop_index;
            }
            float x_p_2 = x_p + (Convert.ToInt32(plus_half_x) * 0.5f);
            float y_p_2 = y_p + (Convert.ToInt32(plus_half_y) * 0.5f);

            for (int x = loop_x; x <= loop_x_max; x++) {
                for(int y = loop_y; y <= loop_y_max; y++) {
                    if (x == loop_x || x == loop_x_max || y == loop_y || y == loop_y_max) {
                        Tile tile = Get_Tile_At(x, y);
                        if (tile != null && range >= Mathf.Sqrt((x_p_2 - tile.X) * (x_p_2 - tile.X) + (y_p_2 - tile.Y) * (y_p_2 - tile.Y))) {
                            list.Add(tile);
                            insert = true;
                        }
                    }
                }
            }

            loop_index++;
            if (!insert) {
                searching = false;
            }
        }

        return list;
    }

    /// <summary>
    /// Deletes tile GameObjects that are not visible and creates ones, that are
    /// </summary>
    public void Update_GOs()
    {
        for (int x = 0; x < Width; x++) {
            for (int y = 0; y < Height; y++) {
                tiles[x][y].Update_GO();
            }
        }
    }

    /// <summary>
    /// Set overlay. If same overlay is selected, overlay is hidden
    /// </summary>
    /// <param name="overlay"></param>
    public void Set_Overlay(Overlay overlay)
    {
        for (int x = 0; x < Width; x++) {
            for (int y = 0; y < Height; y++) {
                tiles[x][y].Show_Info(overlay);
            }
        }
        Current_Overlay = overlay;
    }

    /// <summary>
    /// Clears all highlights, should be used to clear highlights caused by bugs
    /// </summary>
    public void Clear_Highlights()
    {
        Color white = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        for (int x = 0; x < Width; x++) {
            for (int y = 0; y < Height; y++) {
                if(tiles[x][y].Highlight != white) {
                    tiles[x][y].Highlight = white;
                }
            }
        }
    }
}
