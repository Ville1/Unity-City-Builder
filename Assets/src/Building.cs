using System.Collections.Generic;
using UnityEngine;

public class Building {
    public enum UI_Category { ADMIN, INFRASTRUCTURE, HOUSING, SERVICES, FORESTRY, AGRICULTURE, INDUSTRY }
    public enum Resident { PEASANT, CITIZEN, NOBLE }

    protected static int current_id = 0;
    protected static float population_cooldown_max = 10.0f; //days
    protected static float min_process_intervals = 0.5f; //days
    protected static float refound = 0.35f;
    protected static float tool_refound = 0.50f;
    protected static float deconstruct_speed = 10.0f;
    protected static float upkeep_while_paused = 0.25f;
    protected static float food_consumption = 0.02f;
    protected static readonly int CURRENT = 0;
    protected static readonly int MAX = 1;
    protected static readonly int AMOUNT = 0;
    protected static readonly int QUALITY = 1;

    public int Id { get; private set; }
    public string Texture { get; private set; }
    public List<string> Texture_Connects { get; set; }
    public string Current_Texture { get; private set; }
    public string Name { get; set; }
    public string Internal_Name { get; private set; }
    public UI_Category Category { get; private set; }
    public Tile Tile { get; private set; }
    public List<Tile> Tiles { get; private set; }
    public List<string> Valid_Terrain { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public float Construction_Time { get; private set; }
    public float Construction_Time_Left { get; private set; }
    protected bool deconstruct;

    public float Max_Storage { get; private set; }
    public Dictionary<string, float[]> Storage { get; private set; }
    public int Range { get; private set; }
    public float Transport_Speed { get; private set; } //Units / day
    public List<object[]> Storage_Links { get; private set; } //int id, string "Get" / "Give", int max amount, string resource

    public int Population { get; private set; }
    public int Population_Max { get; set; }
    public Resident Population_Type { get; set; }
    public float Population_Happiness { get; private set; }
    protected float population_cooldown;

    public float Appeal_Effect { get; set; }
    public float Appeal_Range { get; set; }

    public Dictionary<Resident, int[]> Workers { get; private set; }
    public List<int> Changeable_Workers_PC { get; private set; }//current; max, type 0 = p, 1 = c
    public List<int> Changeable_Workers_CN { get; private set; }//current; max, type 0 = c, 1 = n

    public bool Is_Road { get; set; }
    public bool Can_Be_Paused { get; set; }
    protected bool paused;

    public List<string> Produces { get; private set; }
    public List<string> Consumes { get; private set; }
    public float Upkeep { get; set; }
    public delegate void Process_Action(float delta_time, Building building);
    public Process_Action Action { get; set; }
    public delegate void Building_Action(Building building);

    public Building_Action On_Build { get; set; }
    public Building_Action On_Demolish { get; set; }
    public Building_Action Toggle_Select { get; set; }

    public Dictionary<string, float> Attributes { get; private set; }
    public Dictionary<string, float[]> Services { get; private set; }
    public Dictionary<string, object> Data { get; private set; }
    public Dictionary<string, float> Statistics { get; private set; }
    public Dictionary<string, float> Food_Stored { get; private set; }
    protected float max_food_storage = 10.0f;
    protected float food_quality;

    public Dictionary<string, float> Cost { get; private set; }

    protected GameObject game_object;
    protected SpriteRenderer renderer;
    protected GameObject alert_image;
    protected string alert_string;
    protected string new_alert_string;

    protected float process_cooldown;

    /// <summary>
    /// Building constructor
    /// </summary>
    /// <param name="tile"></param>
    public Building(List<Tile> tiles, Tile tile, Building prototype)
    {
        Id = current_id;
        current_id++;
        Tiles = tiles;
        foreach(Tile loop_tile in Tiles) {
            loop_tile.Building = this;
        }
        Tile = tile;
        Internal_Name = prototype.Internal_Name;
        Name = prototype.Name;
        Texture = prototype.Texture;
        Texture_Connects = prototype.Texture_Connects;
        Category = prototype.Category;
        Width = prototype.Width;
        Height = prototype.Height;

        Attributes = new Dictionary<string, float>();
        foreach (KeyValuePair<string, float> attribute in prototype.Attributes) {
            Attributes.Add(attribute.Key, attribute.Value);
        }
        Data = new Dictionary<string, object>();
        foreach (KeyValuePair<string, object> data in prototype.Data) {
            Data.Add(data.Key, data.Value);
        }

        Cost = new Dictionary<string, float>();
        foreach (KeyValuePair<string, float> resource in prototype.Cost) {
            Cost.Add(resource.Key, resource.Value);
        }

        Max_Storage = prototype.Max_Storage;
        Storage = new Dictionary<string, float[]>();
        foreach(KeyValuePair<string, float[]> resource in prototype.Storage) {
            Storage.Add(resource.Key, new float[2] { resource.Value[CURRENT], resource.Value[MAX] });
        }
        Range = prototype.Range;
        Transport_Speed = prototype.Transport_Speed;
        Storage_Links = new List<object[]>();

        Produces = new List<string>();
        foreach(string resource in prototype.Produces) {
            Produces.Add(resource);
        }
        Consumes = new List<string>();
        foreach(string resource in prototype.Consumes) {
            Consumes.Add(resource);
        }
        Upkeep = prototype.Upkeep;
        Statistics = new Dictionary<string, float>();

        Services = new Dictionary<string, float[]>();
        Food_Stored = new Dictionary<string, float>();

        Construction_Time = prototype.Construction_Time;
        Construction_Time_Left = Construction_Time;
        deconstruct = false;

        Workers = new Dictionary<Resident, int[]>();
        foreach(KeyValuePair<Resident, int[]> worker_data in prototype.Workers) {
            Workers.Add(worker_data.Key, worker_data.Value);
        }
        Changeable_Workers_PC = new List<int>();
        foreach (int worker_data in prototype.Changeable_Workers_PC) {
            Changeable_Workers_PC.Add(worker_data);
        }
        Changeable_Workers_CN = new List<int>();
        foreach (int worker_data in prototype.Changeable_Workers_CN) {
            Changeable_Workers_CN.Add(worker_data);
        }


        Valid_Terrain = prototype.Valid_Terrain;

        Population_Type = prototype.Population_Type;
        Population_Max = prototype.Population_Max;
        Population = 0;
        population_cooldown = min_process_intervals;
        process_cooldown = 0.0f;

        Appeal_Effect = prototype.Appeal_Effect;
        Appeal_Range = prototype.Appeal_Range;

        Action = prototype.Action;
        On_Build = prototype.On_Build;
        On_Demolish = prototype.On_Demolish;
        Toggle_Select = prototype.Toggle_Select;

        Is_Road = prototype.Is_Road;
        Is_Paused = false;
        Can_Be_Paused = prototype.Can_Be_Paused;
        alert_image = null;
        alert_string = "";
        new_alert_string = "";

        game_object = new GameObject();
        game_object.name = Internal_Name + "_id_" + Id;
        game_object.transform.position = new Vector3(tile.X, tile.Y, 0.0f);
        game_object.transform.parent = GameObject.Find("BuildingListObject").transform;
        renderer = game_object.AddComponent<SpriteRenderer>();
        if(Construction_Time_Left == 0.0f) {
            Set_Texture(Texture);
        } else {
            Set_Texture("building_construction_" + Width + "x" + Height + "_1");
        }
        renderer.sortingOrder = SortingLayer.layers[0].value + 1 + tile.Map.Height - tile.Y;
    }

    /// <summary>
    /// Prototype constructor
    /// </summary>
    /// <param name="name"></param>
    public Building(string name, int width, int height, float construction_time, int max_storage, int range, int transport_speed, Dictionary<string, float[]> storage,
        UI_Category category)
    {
        Internal_Name = name;
        Name = Helper.Parse_To_Human_Readable(Internal_Name);
        Texture = "building_" + Internal_Name;
        Texture_Connects = null;
        Category = category;
        Width = width;
        Height = height;

        Cost = new Dictionary<string, float>();
        Construction_Time = construction_time;
        Construction_Time_Left = 0.0f;

        Attributes = new Dictionary<string, float>();
        Data = new Dictionary<string, object>();
        Produces = new List<string>();
        Consumes = new List<string>();
        Upkeep = 0.0f;
        Workers = new Dictionary<Resident, int[]>();
        Changeable_Workers_PC = new List<int>();
        Changeable_Workers_CN = new List<int>();
        Can_Be_Paused = true;
        Valid_Terrain = new List<string>();

        Appeal_Effect = 0.0f;
        Appeal_Range = 0.0f;

        Max_Storage = max_storage;
        if(storage != null) {
            this.Storage = storage;
        } else {
            this.Storage = new Dictionary<string, float[]>();
        }
        Range = range;
        Transport_Speed = transport_speed;
    }

    /// <summary>
    /// Getter / setter for attributes
    /// </summary>
    /// <param name="name">Attribute's name</param>
    /// <param name="value">Attribute's value, if null this function is used as getter</param>
    /// <returns></returns>
    public float Attribute(string name, float? value = null)
    {
        if(value != null) {
            if (!Attributes.ContainsKey(name)) {
                Attributes.Add(name, (float)value);
            } else {
                Attributes[name] = (float)value;
            }
        }
        if (!Attributes.ContainsKey(name)) {
            return 0.0f;
        }
        return Attributes[name];
    }

    public bool Is_Paused {
        get {
            return paused;
        } set {
            if (Can_Be_Paused) {
                paused = value;
            }
        }
    }

    public bool Active
    {
        get {
            return (Is_Built && !Is_Paused && ((Workers.Count == 0 && Changeable_Workers_PC.Count == 0 && Changeable_Workers_CN.Count == 0)
                || Get_Worker_Efficiency() > 0.0f));
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="progress"></param>
    public void Build(float progress)
    {
        if(Construction_Time_Left <= 0.0f || deconstruct) {
            return;
        }
        Construction_Time_Left -= progress;
        string texture = Texture;
        if(Construction_Time_Left >= 0.66f * Construction_Time) {
            texture = "building_construction_" + Width + "x" + Height + "_1";
        } else if(Construction_Time_Left < 0.66f * Construction_Time && Construction_Time_Left >= 0.33f * Construction_Time) {
            texture = "building_construction_" + Width + "x" + Height + "_2";
        } else if(Construction_Time_Left > 0.0f) {
            texture = "building_construction_" + Width + "x" + Height + "_3";
        }

        if(Construction_Time_Left <= 0.0f) {
            if(On_Build != null) {
                On_Build(this);
            }
            foreach(Tile t in Tiles) {
                t.Update_Appeal(t.Terrain_Appeal_Effect, t.Terrain_Appeal_Range);
            }
        }

        if (texture != Texture) {
            Set_Texture(texture);
        } else {
            Update_Texture();
            List<Building> adjancent_buildings = Get_Adjancent_Buildings();
            foreach(Building adjancent_building in adjancent_buildings) {
                if(adjancent_building.Texture_Connects != null) {
                    adjancent_building.Update_Texture();
                }
            }
        }
    }

    /// <summary>
    /// Is this building built?
    /// </summary>
    /// <returns></returns>
    public bool Is_Built {
        get {
            return (Construction_Time_Left <= 0.0f && !deconstruct);
        }
    }

    /// <summary>
    /// Removes resources from buildings storage
    /// </summary>
    /// <param name="resource"></param>
    /// <param name="amount"></param>
    /// <returns>Amount taken</returns>
    public float Take(string resource, float amount)
    {
        if (!Storage.ContainsKey(resource)) {
            return 0;
        }
        if(Storage[resource][CURRENT] < amount) {
            float return_amount = Storage[resource][CURRENT];
            Storage[resource][CURRENT] = 0.0f;
            return return_amount;
        }
        Storage[resource][CURRENT] -= amount;
        return amount;
    }

    /// <summary>
    /// Add resources
    /// </summary>
    /// <param name="resource"></param>
    /// <param name="amount"></param>
    /// <returns>Overflow</returns>
    public float Add(string resource, float amount)
    {
        if (!Storage.ContainsKey(resource) || amount <= 0.0f || deconstruct) {
            return amount;
        }

        float free_space = Resource_Space(resource);
        if(free_space >= amount) {
            Storage[resource][CURRENT] += amount;
            return 0.0f;
        }
        Storage[resource][CURRENT] += free_space;
        return amount - free_space;
    }

    /// <summary>
    /// Updates buildings texture, for example road connections
    /// </summary>
    public void Update_Texture()
    {
        if (Texture_Connects == null) {
            Set_Texture(Texture);
            return;
        }
        //Connecting texture
        string texture = Texture + "_";
        Dictionary<Map.Direction, Tile> adjancent_tiles = Tile.Map.Get_Adjanced_Tiles(Tile);
        foreach(KeyValuePair<Map.Direction, Tile> pair in adjancent_tiles) {
            foreach (string connection_type in Texture_Connects) {
                if (pair.Value.Building != null && pair.Value.Building.Internal_Name == connection_type && !pair.Value.Building.deconstruct) {
                    switch (pair.Key) {
                        case Map.Direction.North:
                            texture += "n";
                            break;
                        case Map.Direction.East:
                            texture += "e";
                            break;
                        case Map.Direction.South:
                            texture += "s";
                            break;
                        case Map.Direction.West:
                            texture += "w";
                            break;
                    }
                }
            }
        }
        if (texture != Texture + "_") {
            Set_Texture(texture);
        } else {
            Set_Texture(Texture);
        }
    }

    /// <summary>
    /// Demolish this building
    /// </summary>
    public void Demolish()
    {
        if(Internal_Name == "town_hall") {
            return;
        }

        deconstruct = true;
        if (On_Demolish != null) {
            On_Demolish(this);
        }
        foreach(Tile t in Tiles) {
            t.Update_Appeal(Appeal_Effect, Appeal_Range);
        }
        Construction_Time_Left = Construction_Time - 1;
        if (alert_image != null) {
            GameObject.Destroy(alert_image);
        }
        if (Is_Road) {
            List<Building> adjancent_roads= Get_Adjancent_Buildings(1);
            foreach(Building adjancent_road in adjancent_roads) {
                adjancent_road.Update_Texture();
            }
        }
        Max_Storage = 99999;
        foreach (KeyValuePair<string, float> resource in Cost) {
            float actual_refound = refound;
            if(resource.Key == "tools") {
                actual_refound *= tool_refound;
            }
            City.Instance.Queue_Store_Resource(this, resource.Key, resource.Value * actual_refound);
        }
    }

    /// <summary>
    /// Returns list of buildings connected to this building in specified range, ignores roads
    /// </summary>
    /// <param name="range"></param>
    /// <param name="road">-1 = ignore roads, 1 = only roads, 0 = all</param>
    /// <param name="location"></param>
    /// <param name="show_range"></param>
    /// <returns></returns>
    public List<Building> Get_Connected_Buildings(int range, int roads = -1, Tile location = null, bool show_range = false)
    {
        List<Building> list = new List<Building>();
        Dictionary<int, int> road_ids = new Dictionary<int, int>();//ID, range
        List<int> ids = new List<int>();

        List<Building> road_starts = new List<Building>();
        if (location == null) {
            road_starts = Get_Adjancent_Buildings(1);
        } else {
            //Get adjancent tiles
            List<Tile> adjancent_tiles = new List<Tile>();
            //Top & bottom row
            for (int i = 0; i < Width; i++) {
                adjancent_tiles.Add(location.Map.Get_Tile_At(location.X + i, location.Y + 1));
                adjancent_tiles.Add(location.Map.Get_Tile_At(location.X + i, location.Y - Height));
            }
            //Left & right column
            for (int i = 0; i < Height; i++) {
                adjancent_tiles.Add(location.Map.Get_Tile_At(location.X - 1, location.Y - i));
                adjancent_tiles.Add(location.Map.Get_Tile_At(location.X + Width, location.Y - i));
            }

            //Get road starts
            foreach (Tile adjancent_tile in adjancent_tiles) {
                if (adjancent_tile != null && adjancent_tile.Building != null && adjancent_tile.Building.Is_Road) {
                    road_starts.Add(adjancent_tile.Building);
                }
            }
        }

        if(roads == 1 || roads == 0) {
            foreach (Building road_start in road_starts) {
                if (show_range) {
                    if (!road_start.Data.ContainsKey("connection_range")) {
                        road_start.Data.Add("connection_range", range);
                    } else {
                        road_start.Data["connection_range"] = range;
                    }
                }
            }
        }
        foreach(Building road_start in road_starts) {
            if(road_start.Is_Built) {
                Get_Connected_Buildings_Recursive(list, ids, road_ids, road_start, range, roads, show_range);
            }
        }

        return list;
    }

    /// <summary>
    /// Recursive function used by Get_Connected_Buildings
    /// </summary>
    /// <param name="list"></param>
    /// <param name="ids"></param>
    /// <param name="road"></param>
    /// <param name="range"></param>
    private void Get_Connected_Buildings_Recursive(List<Building> list, List<int> ids, Dictionary<int, int> road_ids, Building road, int range, int roads, bool show_range)
    {
        if (range <= 0.0f || (road_ids.ContainsKey(road.Id) && road_ids[road.Id] > range)) {
            return;
        }
        bool update = road_ids.ContainsKey(road.Id);
        if (!update) {
            road_ids.Add(road.Id, range);
        } else {
            road_ids[road.Id] = range;
        }
        if (roads == 1 || roads == 0) {
            if (!update) {
                list.Add(road);
                ids.Add(road.Id);
            }
            if (show_range) {
                if (!road.Data.ContainsKey("connection_range")) {
                    road.Data.Add("connection_range", range);
                } else {
                    road.Data["connection_range"] = range;
                }
            }
        }
        foreach (Building building in road.Get_Adjancent_Buildings()) {
            if (roads != 1 && !ids.Contains(building.Id) && !building.Is_Road) {
                //Building
                if (!update) {
                    list.Add(building);
                    ids.Add(building.Id);
                }
                if (show_range) {
                    if (!building.Data.ContainsKey("connection_range")) {
                        building.Data.Add("connection_range", range);
                    } else {
                        building.Data["connection_range"] = range;
                    }
                }
            }
            if (building.Is_Road && building.Is_Built) {
                //Road
                Get_Connected_Buildings_Recursive(list, ids, road_ids, building, range - 1, roads, show_range);
            }
        }
    }

    public void Clear_Range_Marker()
    {
        if (Data.ContainsKey("connection_range")) {
            Data.Remove("connection_range");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="road">-1 = ignore roads, 1 = only roads, 0 = all</param>
    /// <returns></returns>
    public List<Building> Get_Adjancent_Buildings(int road = 0)
    {
        List<Building> list = new List<Building>();
        List<int> ids = new List<int>();

        //Get adjancent tiles
        List<Tile> tiles = new List<Tile>();
        //Top & bottom row
        for(int i = 0; i < Width; i++) {
            tiles.Add(Tile.Map.Get_Tile_At(Tile.X + i, Tile.Y + 1));
            tiles.Add(Tile.Map.Get_Tile_At(Tile.X + i, Tile.Y - Height));
        }
        //Left & right column
        for (int i = 0; i < Height; i++) {
            tiles.Add(Tile.Map.Get_Tile_At(Tile.X - 1, Tile.Y - i));
            tiles.Add(Tile.Map.Get_Tile_At(Tile.X + Width, Tile.Y - i));
        }

        //Get buildings
        foreach(Tile tile in tiles) {
            if(tile != null && tile.Building != null && !ids.Contains(tile.Building.Id) &&
                (road == 0 || (road == 1 && tile.Building.Is_Road) || (road == -1 && !tile.Building.Is_Road))) {
                list.Add(tile.Building);
                ids.Add(tile.Building.Id);
            }
        }

        return list;
    }

    /// <summary>
    /// Returns total amount of resources stored in this building
    /// </summary>
    /// <returns></returns>
    public float Get_Total_Resources()
    {
        float total = 0;
        foreach(KeyValuePair<string, float[]> resource in Storage) {
            total += resource.Value[CURRENT];
        }
        return total;
    }

    /// <summary>
    /// Returns number of resources stored in this building
    /// </summary>
    /// <returns></returns>
    public float Resource_Amount(string resource)
    {
        if (!Storage.ContainsKey(resource)) {
            return 0.0f;
        }
        return Storage[resource][CURRENT];
    }

    /// <summary>
    /// Returns amount of free space for specific resource
    /// </summary>
    /// <param name="resource"></param>
    /// <returns></returns>
    public float Resource_Space(string resource)
    {
        if (!Storage.ContainsKey(resource)) {
            return 0.0f;
        }
        float max_free_space = Max_Storage - Get_Total_Resources();
        float resource_specific_free_space = Storage[resource][MAX] - Storage[resource][CURRENT];
        if(resource_specific_free_space < max_free_space) {
            return resource_specific_free_space;
        }
        return max_free_space;
    }

    /// <summary>
    /// Sets limit for resource storage
    /// </summary>
    /// <param name="resource"></param>
    /// <param name="limit"></param>
    public void Set_Storage_Limit(string resource, float limit)
    {
        if (!Storage.ContainsKey(resource)) {
            return;
        }
        if(limit > Max_Storage) {
            limit = Max_Storage;
        } else if(limit < 0.0f) {
            limit = 0.0f;
        }
        Storage[resource][MAX] = limit;
    }

    /// <summary>
    /// Does this building have storage limit for resource? Returns true if resource can't be stored here.
    /// </summary>
    /// <param name="resource"></param>
    /// <returns></returns>
    public bool Has_Storage_Limit(string resource)
    {
        if (!Storage.ContainsKey(resource)) {
            return true;
        }
        return Storage[resource][MAX] != Max_Storage;
    }

    /// <summary>
    /// Gets limit for resource storage
    /// </summary>
    /// <param name="resource"></param>
    /// <returns></returns>
    public float Get_Storage_Limit(string resource)
    {
        if (!Storage.ContainsKey(resource)) {
            return 0.0f;
        }
        return Storage[resource][MAX];
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    public List<Tile> Get_Tiles_In_Circle(float range)
    {
        int x = Tile.X;
        int y = Tile.Y;
        bool half_x = true;
        bool half_y = true;
        return Tile.Map.Get_Tiles_In_Circle(x, y, range, half_x, half_y);
    }

    public void Set_Statistic(string stat, float amount)
    {
        if(!Statistics.ContainsKey(stat)) {
            Statistics.Add(stat, amount);
        } else {
            Statistics[stat] = amount;
        }
    }

    public float Get_Statistic(string stat)
    {
        if (!Statistics.ContainsKey(stat)) {
            return 0.0f;
        } else {
            return Statistics[stat];
        }
    }

    /// <summary>
    /// Calculates buildings worker efficency
    /// </summary>
    /// <returns></returns>
    public float Get_Worker_Efficiency()
    {
        if(Internal_Name == "town_hall") {
            return 1.0f;
        }
        if(Workers.Count == 0 && Changeable_Workers_PC.Count == 0 && Changeable_Workers_CN.Count == 0) {
            return 0.0f;
        }
        int current_workers = 0;
        int max_workers = 0;
        float happiness = 0.0f;
        foreach(KeyValuePair<Resident, int[]> worker_data in Workers) {
            current_workers += worker_data.Value[CURRENT];
            max_workers += worker_data.Value[MAX];
            happiness += (worker_data.Value[CURRENT] * City.Instance.Resident_Happiness[worker_data.Key]);
        }
        if(Changeable_Workers_PC.Count != 0) {
            current_workers += Changeable_Workers_PC[CURRENT];
            max_workers += Changeable_Workers_PC[MAX];
            if(Changeable_Workers_PC[2] == 0) {
                happiness += (Changeable_Workers_PC[CURRENT] * City.Instance.Resident_Happiness[Resident.PEASANT]);
            } else {
                happiness += (Changeable_Workers_PC[CURRENT] * City.Instance.Resident_Happiness[Resident.CITIZEN]);
            }
        }
        if (Changeable_Workers_CN.Count != 0) {
            current_workers += Changeable_Workers_CN[CURRENT];
            max_workers += Changeable_Workers_CN[MAX];
            if (Changeable_Workers_CN[2] == 0) {
                happiness += (Changeable_Workers_CN[CURRENT] * City.Instance.Resident_Happiness[Resident.CITIZEN]);
            } else {
                happiness += (Changeable_Workers_CN[CURRENT] * City.Instance.Resident_Happiness[Resident.NOBLE]);
            }
        }
        if (max_workers == 0) {
            return 1.0f;
        }
        happiness /= (float)current_workers;
        float efficency = ((float)current_workers / (float)max_workers);
        if(happiness < 1.0f) {
            efficency *= ((1.0f + happiness) / 2.0f);
        } else if(happiness > 1.0f) {
            efficency *= (1.0f + (happiness / 10.0f));
        }
        return efficency;
    }

    public void Refine_Resources(string material, float material_amount, string product, float product_amount, float delta_time)
    {
        Dictionary<string, float> materials = new Dictionary<string, float>();
        Dictionary<string, float> products = new Dictionary<string, float>();
        materials.Add(material, material_amount);
        products.Add(product, product_amount);
        Refine_Resources(materials, products, delta_time);
    }

    public void Refine_Resources(Dictionary<string, float> materials, Dictionary<string, float> products, float delta_time)
    {
        float efficency = Get_Worker_Efficiency();
        if(Data.ContainsKey("alert_material")) {
            Data.Remove("alert_material");
        }
        if (Data.ContainsKey("alert_space")) {
            Data.Remove("alert_space");
        }
        //Check raw materials
        foreach (KeyValuePair<string, float> material in materials) {
            if (Resource_Amount(material.Key) < (material.Value * delta_time * efficency)) {
                //Not enough material
                Data.Add("alert_material", true);
                return;
            }
        }
        //Check space
        foreach (KeyValuePair<string, float> product in products) {
            if(Resource_Space(product.Key) < product.Value * delta_time * efficency) {
                //Not enouht space
                Data.Add("alert_space", true);
                return;
            }
        }
        //Refine
        //Take material
        foreach (KeyValuePair<string, float> material in materials) {
            Take(material.Key, material.Value * delta_time * efficency);
            Set_Statistic("consumed_" + material.Key, material.Value * efficency);
        }
        //Store products
        foreach (KeyValuePair<string, float> product in products) {
            Add(product.Key, product.Value * delta_time * efficency);
            Set_Statistic("produced_" + product.Key, product.Value * efficency);
        }
    }

    public bool Is_Residential
    {
        get {
            return Population_Max != 0;
        }
    }

    public float Current_Upkeep
    {
        get {
            if(Is_Paused) {
                return Upkeep * upkeep_while_paused;
            }
            return Upkeep;
        }
    }

    /// <summary>
    /// Give residential building service
    /// </summary>
    /// <param name="service"></param>
    /// <param name="amount"></param>
    /// <param name="quality"></param>
    /// <returns>Overflow</returns>
    public float Serve(string service, float amount, float quality)
    {
        if (!Is_Residential) {
            return amount;
        }
        float overflow;
        if (!Services.ContainsKey(service)) {
            Services.Add(service, new float[2] { amount, quality });
            if (Services[service][AMOUNT] > 1.0f) {
                overflow = Services[service][AMOUNT] - 1.0f;
                Services[service][AMOUNT] = 1.0f;
                return overflow;
            }
            return 0.0f;
        }
        float missing = 1.0f - Services[service][AMOUNT];
        overflow = amount - missing;
        if (overflow < 0.0f) {
            overflow = 0.0f;
        }
        amount -= overflow;

        float total_quality = (Services[service][AMOUNT] * Services[service][QUALITY]) + (amount * quality);
        float average_quality = total_quality / (Services[service][AMOUNT] + amount);
        Services[service][AMOUNT] += amount;
        Services[service][QUALITY] = average_quality;

        return overflow;
    }

    /// <summary>
    /// Consume services from building
    /// </summary>
    /// <param name="service"></param>
    /// <param name="amount"></param>
    /// <returns>Missing services</returns>
    public float Consume_Service(string service, float amount)
    {
        if (!Is_Residential) {
            return -1.0f;
        }
        if (!Services.ContainsKey(service)) {
            Services.Add(service, new float[2] { 0.0f, 0.0f });
        }
        Services[service][AMOUNT] -= amount;
        float missing = 0.0f;
        if (Services[service][AMOUNT] < 0.0f) {
            missing = (-1.0f * Services[service][AMOUNT]);
            Services[service][AMOUNT] = 0.0f;
        }
        if (Services[service][AMOUNT] == 0.0f) {
            Services[service][QUALITY] = 0.0f;
        }
        return missing;
    }

    /// <summary>
    /// Amount of service provided to this building
    /// </summary>
    /// <param name="service"></param>
    /// <returns></returns>
    public float Current_Service(string service)
    {
        if (!Is_Residential || !Services.ContainsKey(service)) {
            return 0.0f;
        }
        return Services[service][AMOUNT];
    }

    /// <summary>
    /// Amount of service missing from building
    /// </summary>
    /// <param name="service"></param>
    /// <returns></returns>
    public float Missing_Service(string service)
    {
        if (!Is_Residential) {
            return 0.0f;
        }
        if (!Services.ContainsKey(service)) {
            return 1.0f;
        }
        return 1.0f - Services[service][AMOUNT];
    }

    /// <summary>
    /// Returns current quality level of service provided to this building
    /// </summary>
    /// <param name="service"></param>
    /// <returns></returns>
    public float Service_Quality(string service)
    {
        if (!Is_Residential) {
            return 0.0f;
        }
        if (!Services.ContainsKey(service)) {
            return 0.0f;
        }
        return Services[service][QUALITY];
    }

    /// <summary>
    /// Calculates average appeal of building's tiles
    /// </summary>
    /// <returns></returns>
    public float Average_Appeal()
    {
        float average = 0.0f;
        foreach(Tile t in Tiles) {
            average += t.Appeal;
        }
        return (average / (1.0f * Tiles.Count));
    }

    /// <summary>
    /// Sell food to residence
    /// </summary>
    /// <param name="type"></param>
    /// <param name="amount"></param>
    /// <returns>Amount sold</returns>
    public float Sell_Food(string type, float amount)
    {
        if (!Is_Residential) {
            return 0.0f;
        }
        if (!Food_Stored.ContainsKey(type)) {
            Food_Stored.Add(type, 0.0f);
        }
        float amount_sold = amount;
        if (amount_sold > max_food_storage - Food_Stored[type]) {
            amount_sold = max_food_storage - Food_Stored[type];
        }
        Food_Stored[type] += amount_sold;
        return amount_sold;
    }

    /// <summary>
    /// Consume food
    /// </summary>
    /// <param name="delta_time"></param>
    private void Consume_Food(float delta_time)
    {
        if (!Is_Residential) {
            return;
        }
        float required_food = Population * food_consumption * delta_time;
        float food_consumed = 0.0f;
        Dictionary<string, float> food_types_consumed = new Dictionary<string, float>();
        float vegetables_consumed = 0.0f;
        float meat_consumed = 0.0f;
        int loop_index = 0;
        while (food_consumed < required_food && Food_Amount() > 0.0f) {
            //Search for lowest food amount
            float min_food = -1.0f;
            int food_types = 0;
            foreach (KeyValuePair<string, float> food_in_store in Food_Stored) {
                if (food_in_store.Value > 0.0f) {
                    if (min_food == -1.0f || food_in_store.Value < min_food) {
                        min_food = food_in_store.Value;
                    }
                    food_types++;
                }
            }

            if (food_types * min_food > required_food) {
                min_food = required_food / food_types;
            }

            //Consume food
            List<string> keys = new List<string>();
            foreach (KeyValuePair<string, float> food_in_store in Food_Stored) {
                if (food_in_store.Value > 0.0f) {
                    keys.Add(food_in_store.Key);
                }
            }
            foreach (string key in keys) {
                Food_Stored[key] -= min_food;
                food_consumed += min_food;
                if (!food_types_consumed.ContainsKey(key)) {
                    food_types_consumed.Add(key, min_food);
                } else {
                    food_types_consumed[key] += min_food;
                }
                if (City.Instance.Get_Resource_Data(key).ContainsKey("food_type_vegetable")) {
                    vegetables_consumed += min_food;
                } else if (City.Instance.Get_Resource_Data(key).ContainsKey("food_type_meat")) {
                    meat_consumed += min_food;
                }
            }
            loop_index++;

            //TODO: Fix this
            if (loop_index > 100) {
                Logger.Instance.Warning("Loop overflow! Food amount: " + Food_Amount() + " required: " + required_food + " consumed: " + food_consumed);
                break;
            }
        }
        //Calculate quality
        food_quality = 0.0f;
        if(food_consumed >= required_food && Population > 0) {
            float target_food_amount_per_type = required_food / (float)food_types_consumed.Count;
            foreach(KeyValuePair<string, float> food_type_consumed in food_types_consumed) {
                if(food_type_consumed.Value >= target_food_amount_per_type) {
                    food_quality += 1.0f;
                } else {
                    food_quality += (float)food_type_consumed.Value / target_food_amount_per_type;
                }
            }
            if(meat_consumed < 0.25f * food_consumed) {
                food_quality *= (meat_consumed / food_consumed) + 0.75f;
            }
            if(vegetables_consumed < 0.25f * food_consumed) {
                food_quality *= (vegetables_consumed / food_consumed) + 0.75f;
            }
        } else if(Population == 0 && Food_Amount() > 0.0f) {
            food_quality = 1.0f;
        }
    }

    /// <summary>
    /// Current quality of stored food
    /// </summary>
    /// <returns></returns>
    public float Food_Quality()
    {
        if (!Is_Residential) {
            return -1.0f;
        }
        /*List<string> food_types = new List<string>();
        float food_quality = 0.0f;
        foreach (KeyValuePair<string, float> food_in_store in Food_Stored) {
            if (food_in_store.Value != 0.0f) {
                food_quality += 1.0f;
                Dictionary<string, float> data = City.Instance.Get_Resource_Data(food_in_store.Key);
                if (data.ContainsKey("food_type_meat") && !food_types.Contains("meat")) {
                    food_types.Add("meat");
                } else if (data.ContainsKey("food_type_vegetable") && !food_types.Contains("vegetable")) {
                    food_types.Add("vegetable");
                }
            }
        }
        if (food_types.Count == 1) {
            food_quality /= 2.0f;
        }*/
        return food_quality;
    }

    /// <summary>
    /// Amount of food stored in this residence
    /// </summary>
    /// <returns></returns>
    public float Food_Amount()
    {
        if (!Is_Residential) {
            return -1.0f;
        }
        float amount = 0.0f;
        foreach (KeyValuePair<string, float> food_in_store in Food_Stored) {
            amount += food_in_store.Value;
        }
        return amount;
    }

    /// <summary>
    /// Returns empty tiles in specified range from building and tags them as being harvesed by this building
    /// Does not return tiles tagged as being harvested by other buildings
    /// </summary>
    /// <param name="range"></param>
    /// <param name="ignore_buildings"></param>
    /// <param name="mine"></param>
    /// <returns></returns>
    public List<Tile> Harvest(float range, bool ignore_buildings = false, bool mine = false)
    {
        List<Tile> tiles = new List<Tile>();
        string tag = "harvested_by";
        if (mine) {
            tag = "mined_by";
        }

        foreach (Tile tile in Get_Tiles_In_Circle(range)) {
            Building harvester = null;
            if (tile.Data.ContainsKey(tag)) {
                harvester = (Building)tile.Data[tag];
            }
            if((tile.Building == null || ignore_buildings) && (harvester == null || harvester.Id == Id)) {
                tiles.Add(tile);
                if (harvester == null) {
                    tile.Data.Add(tag, this);
                } else {
                    tile.Data[tag] = this;
                }
            } else if(harvester != null && harvester.Id == Id && tile.Building != null && !ignore_buildings) {
                tile.Data.Remove(tag);
            }
        }

        return tiles;
    }

    /// <summary>
    /// Release tiles harvested by this building
    /// </summary>
    /// <param name="range"></param>
    /// <param name="mine"></param>
    public void Release_Harvested_Tiles(float range, bool mine = false)
    {
        List<Tile> tiles = Get_Tiles_In_Circle(range);
        string tag = "harvested_by";
        if (mine) {
            tag = "mined_by";
        }
        foreach (Tile tile in tiles) {
            if (tile.Data.ContainsKey(tag)) {
                Building harvester = (Building)tile.Data[tag];
                if (harvester.Id == Id) {
                    tile.Data.Remove(tag);
                }
            }
        }
    }

    /// <summary>
    /// Converts resources to services
    /// </summary>
    /// <param name="resource"></param>
    /// <param name="service"></param>
    /// <param name="residential_buildings"></param>
    /// <param name="delta_t"></param>
    /// <param name="quality"></param>
    /// <param name="add_income"></param>
    /// <returns>Income</returns>
    public float Sell_Resources_As_Service(string resource, string service, List<Building> residential_buildings, float delta_t, float quality, bool add_income = true)
    {
        float min_resource = -1.0f;
        float price = City.Instance.Get_Resource_Data(resource)["price"];
        float sold = 0.0f;
        float income = 0.0f;
        int buildings_needing_service = 0;
        foreach (Building residence in residential_buildings) {
            if (min_resource == -1.0f || (residence.Missing_Service(service) > 0.0f && residence.Missing_Service(service) < min_resource)) {
                min_resource = residence.Missing_Service(service);
            }
            if (residence.Missing_Service(service) > 0.0f) {
                buildings_needing_service++;
            }
        }
        while (min_resource > 0.1f && Resource_Amount(resource) > 0.0f) {
            float use_ratio = Resource_Amount(resource) / (min_resource * buildings_needing_service);
            if (use_ratio > 1.0f) {
                use_ratio = 1.0f;
            }
            foreach (Building residence in residential_buildings) {
                if (residence.Missing_Service(service) > 0.0f) {
                    float resource_for_building = min_resource * use_ratio;
                    if (residence.Serve(service, resource_for_building, quality) > 0.0f) {
                        Logger.Instance.Warning(Internal_Name + " " + Id + ": " + resource + " overflow!");
                    }
                    float taken = Take(resource, resource_for_building);
                    if (taken > resource_for_building * 1.01f || taken < resource_for_building * 0.99f) {
                        Logger.Instance.Warning(Internal_Name + " " + Id + ": " + resource + " mismatch! " + taken + " <-> " + resource_for_building);
                    }
                    sold += resource_for_building;
                    income += (resource_for_building * price);
                }
            }
            min_resource = -1.0f;
            buildings_needing_service = 0;
            foreach (Building residence in residential_buildings) {
                if (min_resource == -1.0f || (residence.Missing_Service(service) > 0.0f && residence.Missing_Service(service) < min_resource)) {
                    min_resource = residence.Missing_Service(service);
                }
                if (residence.Missing_Service(service) > 0.0f) {
                    buildings_needing_service++;
                }
            }
        }

        //Stats and income
        if (add_income) {
            City.Instance.Cash += income;
            Set_Statistic("income", income / delta_t);
        }
        Set_Statistic(resource + "_sold", sold);
        return income;
    }

    /// <summary>
    /// 
    /// </summary>
    public void Render_Alerts()
    {
        new_alert_string = "";
        if (MenuManager.Instance.Show_Alerts && Is_Built) {
            bool no_raw_material = (Attribute("limited_raw_material_warnings") != 0.0f);
            if(Consumes.Count != 0) {
                foreach (string resource in Consumes) {
                    if(Attribute("limited_raw_material_warnings") == 0.0f) {
                        if (Resource_Amount(resource) == 0.0f) {
                            no_raw_material = true;
                            break;
                        }
                    } else {
                        if (Resource_Amount(resource) != 0.0f) {
                            no_raw_material = false;
                            break;
                        }
                    }
                }
            }
            bool no_room = false;
            foreach (string resource in Produces) {
                if (Resource_Space(resource) == 0.0f) {
                    no_room = true;
                    break;
                }
            }

            if (Is_Paused) {
                new_alert_string = "paused";
            } else if (Is_Residential && Population_Happiness < 0.33f) {
                new_alert_string = "low happiness";
            } else if (no_raw_material || Data.ContainsKey("alert_material")) {
                new_alert_string = "no raw material";
            } else if (no_room || Data.ContainsKey("alert_space")) {
                new_alert_string = "no room";
            } else if ((Workers.Count != 0 || Changeable_Workers_PC.Count != 0 || Changeable_Workers_CN.Count != 0) && Get_Worker_Efficiency() < 0.33f) {
                new_alert_string = "workers";
            }
        }

        if (new_alert_string != alert_string) {
            if (new_alert_string != "") {
                if(alert_image != null) {
                    GameObject.Destroy(alert_image);
                }
                if (new_alert_string == "low happiness") {
                    alert_image = GameObject.Instantiate(MenuManager.Instance.Alert_Unhappiness);
                } else if (new_alert_string == "workers") {
                    alert_image = GameObject.Instantiate(MenuManager.Instance.Alert_Workers);
                } else if (new_alert_string == "paused") {
                    alert_image = GameObject.Instantiate(MenuManager.Instance.Alert_Paused);
                } else if (new_alert_string == "no room") {
                    alert_image = GameObject.Instantiate(MenuManager.Instance.Alert_No_Room);
                } else if (new_alert_string == "no raw material") {
                    alert_image = GameObject.Instantiate(MenuManager.Instance.Alert_No_Resources);
                } else {
                    alert_image = GameObject.Instantiate(MenuManager.Instance.Alert_General);
                }
                alert_image.transform.SetParent(MenuManager.Instance.Canvas.transform);
                alert_image.transform.position = CameraManager.Instance.Camera.WorldToScreenPoint(game_object.transform.position);
                alert_string = new_alert_string;
            } else {
                GameObject.Destroy(alert_image);
                alert_string = new_alert_string;
                alert_image = null;
            }
        } else if (alert_string != "") {
            alert_image.transform.position = CameraManager.Instance.Camera.WorldToScreenPoint(game_object.transform.position);
        }
    }

    /// <summary>
    /// Main process function
    /// </summary>
    /// <param name="delta_time">Days</param>
    public void Process(float delta_time)
    {
        //Render alerts
        Render_Alerts();

        //Check cooldown
        if (process_cooldown > 0.0f) {
            process_cooldown -= delta_time;
            return;
        }
        delta_time = delta_time + min_process_intervals + (-1.0f * process_cooldown);

        //Deconstruct?
        if (deconstruct) {
            //Store overflow refound
            foreach(KeyValuePair<string, float[]> resource in Storage) {
                City.Instance.Queue_Store_Resource(this, resource.Key, resource.Value[CURRENT]);
            }
            //Deconstruct
            Construction_Time_Left -= (deconstruct_speed * delta_time);
            string texture = "building_construction_" + Width + "x" + Height + "_3";
            if (Construction_Time_Left < 0.33f * Construction_Time) {
                texture = "building_construction_" + Width + "x" + Height + "_1";
            } else if (Construction_Time_Left >= 0.33f * Construction_Time && Construction_Time_Left < 0.66f * Construction_Time) {
                texture = "building_construction_" + Width + "x" + Height + "_2";
            }
            Set_Texture(texture);
            if (Construction_Time_Left <= 0.0f) {
                foreach(Tile tile in Tiles) {
                    tile.Building = null;
                }
                GameObject.Destroy(game_object);
                City.Instance.Delete_Building(this);
            }
            //Cooldown
            process_cooldown = min_process_intervals;
            return;
        }

        //Build?
        if (Attribute("build_speed") != 0.0f && !Is_Paused) {
            List<Building> buildings = City.Instance.Get_Buildings();
            foreach (Building building in buildings) {
                if (!building.Is_Built && building.Id != Id) {
                    if (Attribute("build_range") >= Tile.Distance(building.Tile)) {
                        building.Build(delta_time * (Attribute("build_speed") * Get_Worker_Efficiency()));
                    } else {
                        float over_distance = Tile.Distance(building.Tile) - Attribute("build_range");
                        float penalized_speed_multiplier = 1.0f * Mathf.Pow(0.85f, over_distance);
                        building.Build(delta_time * (Attribute("build_speed") * Get_Worker_Efficiency() * penalized_speed_multiplier));
                    }
                }
            }
        }

        //Population
        if(Is_Residential) {
            Set_Statistic("food_consumed", Population * food_consumption);
            if (City.Instance.Time >= City.Instance.Grace_Time || Population_Type != Resident.PEASANT) {
                //Consume goods
                Consume_Food(delta_time);
                if(Service_Quality("salt") != 0.0f) {
                    Consume_Service("salt", Population * 0.01f * delta_time);
                    Set_Statistic("salt_consumed", Population * 0.01f);
                }
                if (Service_Quality("tavern") != 0.0f) {
                    Consume_Service("tavern", Population * 0.01f * delta_time);
                    Set_Statistic("alcohol_consumed", Population * 0.01f);
                }
                //Consume services
                Consume_Service("chapel", 0.1f * delta_time);
            }
            //Reduce tax effect
            if (Data.ContainsKey("tax_collected")) {
                ((float[])Data["tax_collected"])[1] -= delta_time;
                if(((float[])Data["tax_collected"])[1] <= 0.0f) {
                    Data.Remove("tax_collected");
                }
            }

            //Happiness
            Population_Happiness = 1.0f;
            if(Population_Type == Resident.PEASANT && !City.Instance.Grace) {
                if (Food_Quality() == 0.0f) {
                    Population_Happiness = 0.0f;
                } else {
                    //Food
                    float food_effect = 0.0f;
                    float food_quality = Food_Quality();
                    if (food_quality < 1.0f) {
                        food_effect = -1.0f * ((1.0f - food_quality) / 2.0f);
                    } else if (food_quality > 1.0f && food_quality <= 3.0f) {
                        food_effect = ((food_quality - 1.0f) * 0.075f);
                    } else if(food_quality > 3.0f) {
                        food_effect = 0.15f + ((food_quality - 3.0f) * 0.01f);
                    }
                    Population_Happiness += food_effect;
                    
                    //Appeal
                    float appeal_effect = 0.0f;
                    float average_appeal = Average_Appeal();
                    if (average_appeal < -1.0f) {
                        appeal_effect = -1.0f * Helper.Break_Point_Multiply(-1.0f * average_appeal, 1.0f, 3.0f, 5.0f, 0.005f, 0.020f);
                    } else if (average_appeal > 0.0f) {
                        appeal_effect = Helper.Break_Point_Multiply(average_appeal, 0.0f, 1.0f, 6.0f, 0.05f, 0.01f);
                    }
                    Population_Happiness += appeal_effect;
                    
                    //Salt
                    if (Service_Quality("salt") != 0.0f) {
                        Population_Happiness += 0.1f;
                    }

                    //Chapel
                    float chapel_effect = 0.0f;
                    if (Service_Quality("chapel") > 0.0f && Service_Quality("chapel") <= 1.0f) {
                        chapel_effect = Service_Quality("chapel") / 20.0f;
                    } else if (Service_Quality("chapel") > 1.0f && Service_Quality("chapel") <= 2.0f) {
                        chapel_effect = 0.05f + ((Service_Quality("chapel") - 1.0f) / 66.6666666f);
                    } else if (Service_Quality("chapel") > 2.0f) {
                        chapel_effect = 0.065f;
                    }
                    Population_Happiness += chapel_effect;

                    //Tavern
                    float tavern_effect = 0.0f;
                    if (Service_Quality("tavern") > 0.0f && Service_Quality("tavern") <= 1.0f) {
                        tavern_effect = Service_Quality("tavern") / 10.0f;
                    } else if (Service_Quality("tavern") > 1.0f && Service_Quality("tavern") <= 3.5f) {
                        tavern_effect = 0.10f + ((Service_Quality("tavern") - 1.0f) / 50.0f);
                    } else if (Service_Quality("tavern") > 3.5f) {
                        tavern_effect = 0.15f;
                    }
                    Population_Happiness += tavern_effect;

                    //Tax
                    if (Data.ContainsKey("tax_collected")) {
                        Population_Happiness -= 0.25f;
                    }

                    if(Population_Happiness < 0.0f) {
                        Population_Happiness = 0.0f;
                    }

                    //Employment - LAST!
                    float employment_multiplier = 1.0f;
                    if (City.Instance.Get_Employment_Ratio(Population_Type) > 1.1f) {
                        employment_multiplier = 1.0f / (City.Instance.Get_Employment_Ratio(Population_Type) - 0.1f);
                    }
                    Population_Happiness *= employment_multiplier;
                }
            } else if(Population_Type == Resident.CITIZEN) {
                if (Food_Quality() == 0.0f) {
                    Population_Happiness = 0.0f;
                } else {
                    //Food
                    float food_effect = 0.0f;
                    float food_quality = Food_Quality();
                    if (food_quality < 3.0f) {
                        food_effect = -1.0f * ((3.0f - food_quality) / 6.0f);
                    } else if (food_quality > 3.0f && food_quality <= 6.0f) {
                        food_effect = ((food_quality - 3.0f) * 0.075f);
                    } else if (food_quality > 6.0f) {
                        food_effect = 0.225f + ((food_quality - 6.0f) * 0.025f);
                    }
                    Population_Happiness += food_effect;

                    //Appeal
                    float appeal_effect = 0.0f;
                    float average_appeal = Average_Appeal();
                    if (average_appeal < 0.5f) {
                        appeal_effect = -1.0f * Helper.Break_Point_Multiply(-1.0f * average_appeal, 0.5f, 3.0f, 5.0f, 0.1f, 0.05f);
                    } else if (average_appeal > 0.5f && average_appeal <= 5.0f) {
                        appeal_effect = Helper.Break_Point_Multiply(average_appeal, 0.5f, 2.0f, 5.0f, 0.05f, 0.025f);
                    } else if(average_appeal > 5.0f) {
                        appeal_effect = 0.175f + ((average_appeal - 5.0f) * 0.01f);
                    }
                    Population_Happiness += appeal_effect;

                    //Salt
                    if (Service_Quality("salt") == 0.0f) {
                        Population_Happiness -= 0.25f;
                    }

                    //Chapel
                    float chapel_effect = -0.1f;
                    if (Service_Quality("chapel") > 0.0f && Service_Quality("chapel") < 1.0f) {
                        chapel_effect = -1.0f * ((1.0f - Service_Quality("chapel")) / 10.0f);
                    } else if (Service_Quality("chapel") >= 1.0f && Service_Quality("chapel") <= 2.0f) {
                        chapel_effect = ((Service_Quality("chapel") - 1.0f) / 10.0f);
                    } else if (Service_Quality("chapel") > 2.0f) {
                        chapel_effect = 0.10f + ((Service_Quality("chapel") - 2.0f) * 0.01f);
                    }
                    Population_Happiness += chapel_effect;

                    //Tavern
                    float tavern_effect = 0.0f;
                    if (Service_Quality("tavern") > 0.0f && Service_Quality("tavern") <= 1.0f) {
                        tavern_effect = Service_Quality("tavern") / 10.0f;
                    } else if (Service_Quality("tavern") > 1.0f && Service_Quality("tavern") <= 3.5f) {
                        tavern_effect = 0.10f + ((Service_Quality("tavern") - 1.0f) / 50.0f);
                    } else if (Service_Quality("tavern") > 3.5f) {
                        tavern_effect = 0.15f;
                    }
                    Population_Happiness += tavern_effect;

                    //Tax
                    if (Data.ContainsKey("tax_collected")) {
                        Population_Happiness -= 0.25f;
                    }

                    if (Population_Happiness < 0.0f) {
                        Population_Happiness = 0.0f;
                    }

                    //Employment - LAST!
                    float employment_multiplier = 1.0f;
                    if (City.Instance.Get_Employment_Ratio(Population_Type) > 1.1f) {
                        employment_multiplier = 1.0f / (City.Instance.Get_Employment_Ratio(Population_Type) - 0.1f);
                    }
                    Population_Happiness *= employment_multiplier;
                }
            }

            //Immigration
            //Cooldown from emigration
            if (Data.ContainsKey("emigration")) {
                Data["emigration"] = (float)Data["emigration"] - delta_time;
                if((float)Data["emigration"] <= 0.0f) {
                    Data.Remove("emigration");
                }
            }
            if (Population < Population_Max && Is_Built && (Population_Happiness >= 0.5f || (Population == 0 && Food_Amount() > 0.0f && !Data.ContainsKey("emigration")))) {
                if (population_cooldown <= 0.0f) {
                    Population++;
                    if(Population_Happiness < 0.85f) {
                        population_cooldown = population_cooldown_max * (3.0f - Population_Happiness);
                    } else {
                        population_cooldown = population_cooldown_max;
                    }
                } else {
                    population_cooldown -= delta_time;
                }
            }
            //Emigration
            if((Population_Happiness < 0.5f && Population > 0)) {
                if (!Data.ContainsKey("emigration")) {
                    Data.Add("emigration", 10.0f);
                } else {
                    Data["emigration"] = 10.0f;
                }
                if (population_cooldown <= 0.0f) {
                    Population--;
                    if (Population_Happiness > 0.15f) {
                        population_cooldown = population_cooldown_max * (2.0f + Population_Happiness);
                    } else {
                        population_cooldown = population_cooldown_max;
                    }
                } else {
                    population_cooldown -= delta_time;
                }
            }
            if (deconstruct) {
                Population = 0;
            }
        }

        //Transport
        if (Attribute("storehouse") != 0.0f && Active) {
            //Storehouse
            List<int> connected_ids = new List<int>();
            foreach (Building connected_building in Get_Connected_Buildings(Range, 0)) {
                if (connected_building.Is_Built && (!connected_building.Is_Road || connected_building.Consumes.Count != 0)) {
                    connected_ids.Add(connected_building.Id);
                    //Take resources
                    foreach (string resource in connected_building.Produces) {
                        if (Storage.ContainsKey(resource)) {
                            float free_space = Resource_Space(resource);
                            float take_amount = free_space;
                            if ((Transport_Speed * Get_Worker_Efficiency()) * delta_time < free_space) {
                                take_amount = (Transport_Speed * Get_Worker_Efficiency()) * delta_time;
                            }
                            Storage[resource][CURRENT] += connected_building.Take(resource, take_amount);
                        }
                    }
                    //Give resources
                    foreach (string resource in connected_building.Consumes) {
                        if (Storage.ContainsKey(resource)) {
                            float overflow = connected_building.Add(resource, Take(resource, (Transport_Speed * Get_Worker_Efficiency()) * delta_time));
                            Add(resource, overflow);
                        }
                    }
                }
            }
            //Links
            List<int> invalid_links = new List<int>();
            foreach (object[] link in Storage_Links) {
                Building linked_building = City.Instance.Get_Building((int)link[0]);
                if (linked_building == null || !connected_ids.Contains(linked_building.Id) || !Storage.ContainsKey((string)link[3]) || !linked_building.Storage.ContainsKey((string)link[3])) {
                    invalid_links.Add((int)link[0]);
                } else {
                    if ((string)link[1] == "Get" && Resource_Amount((string)link[3]) < (int)link[2]) {
                        //Take
                        float free_space = Resource_Space((string)link[3]);
                        float take_amount = free_space;
                        if ((Transport_Speed * Get_Worker_Efficiency()) * delta_time < free_space) {
                            take_amount = (Transport_Speed * Get_Worker_Efficiency()) * delta_time;
                        }
                        Storage[(string)link[3]][CURRENT] += linked_building.Take((string)link[3], take_amount);
                    } else if (linked_building.Resource_Amount((string)link[3]) < (int)link[2]) {
                        //Give
                        float overflow = linked_building.Add((string)link[3], Take((string)link[3], (Transport_Speed * Get_Worker_Efficiency()) * delta_time));
                        Add((string)link[3], overflow);
                    }
                }
            }
        } else if(Transport_Speed != 0) {
            //Non storehouse
            foreach (Building connected_building in Get_Connected_Buildings(Range, -1)) {
                if (connected_building.Is_Built) {
                    //Take resources
                    foreach (string resource in connected_building.Produces) {
                        if (Consumes.Contains(resource)) {
                            float free_space = Resource_Space(resource);
                            float take_amount = free_space;
                            if ((Transport_Speed * Get_Worker_Efficiency()) * delta_time < free_space) {
                                take_amount = (Transport_Speed * Get_Worker_Efficiency()) * delta_time;
                            }
                            Storage[resource][CURRENT] += connected_building.Take(resource, take_amount);
                        }
                    }
                }
            }
        }

        //Anonymous function
        if(Action != null) {
            Action(delta_time, this);
        }

        //Take upkeep
        City.Instance.Cash -= (Current_Upkeep * delta_time);

        //Cooldown
        process_cooldown = min_process_intervals;
    }

    private void Set_Texture(string texture)
    {
        Current_Texture = texture;
        renderer.sprite = SpriteManager.Instance.Get_Sprite(Current_Texture);
    }
}
