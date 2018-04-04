using System;
using System.Collections.Generic;
using UnityEngine;

public class City {
    private static readonly int CURRENT = 0;
    private static readonly int MAX = 1;

    private static City instance;

    public static Dictionary<string, Dictionary<string, float>> resource_data;

    public string Error_Message { get; private set; }
    public string Name { get; private set; }
    public float Cash { get; set; }
    public float Cash_Flow { get; set; }
    public float Time { get; private set; } //Days
    public float Grace_Time { get; set; } //Days
    public Dictionary<string, float> Resources { get; private set; }
    public Dictionary<string, float[]> Resource_Stats { get; private set; }
    private Dictionary<Building, Dictionary<string, float>> resources_on_queue;
    private Building town_hall;
    private List<Building> buildings;
    private List<Building> buildings_to_be_deleted;

    public Dictionary<Building.Resident, int[]> Residents { get; private set; }
    public Dictionary<Building.Resident, int> Required_Workers { get; private set; }
    public Dictionary<Building.Resident, float> Resident_Happiness { get; private set; }

    private City()
    {
        town_hall = null;
        Error_Message = "";
        Name = "xxx";
        Cash = 0.0f;
        Cash_Flow = 0.0f;
        Time = 0.0f;
        Grace_Time = 540.0f;
        buildings = new List<Building>();
        buildings_to_be_deleted = new List<Building>();
        Resources = new Dictionary<string, float>();
        resources_on_queue = new Dictionary<Building, Dictionary<string, float>>();
        Residents = new Dictionary<Building.Resident, int[]>();
        Resource_Stats = new Dictionary<string, float[]>();
        Required_Workers = new Dictionary<Building.Resident, int>();
        Resident_Happiness = new Dictionary<Building.Resident, float>();
        foreach (Building.Resident resident_type in Enum.GetValues(typeof(Building.Resident))) {
            Required_Workers.Add(resident_type, 0);
            Residents.Add(resident_type, new int[] { 0, 0 });
            Resident_Happiness.Add(resident_type, 0.0f);
        }
        resource_data = new Dictionary<string, Dictionary<string, float>>();

        Add_Food_Data("game", 1.0f, "meat");
        Add_Food_Data("berries", 1.15f, "vegetable");
        Add_Food_Data("mushrooms", 0.95f, "vegetable");
        Add_Food_Data("roots", 0.50f, "vegetable");
        Add_Food_Data("herbs", 10.0f, "vegetable");
        Add_Food_Data("potatoes", 0.75f, "vegetable");

        Add_Simple_Resource_Data("salt", 1.0f);
        Add_Simple_Resource_Data("alcohol", 2.5f);

        float price_mult = 4.00f;
        foreach(KeyValuePair<string, Dictionary < string, float>> resource in resource_data) {
            if (resource.Value.ContainsKey("price")) {
                resource.Value["price"] = price_mult * resource.Value["price"];
            }
        }
    }

    /// <summary>
    /// Accessor for singleton instance
    /// </summary>
    public static City Instance
    {
        get {
            if (instance == null) {
                instance = new City();
            }
            return instance;
        }
        private set {
            instance = value;
        }
    }

    /// <summary>
    /// Create a new city
    /// </summary>
    public void New(string name)
    {
        Name = name;
        MenuManager.Instance.Set_City_Name(Name);
        Cash = 15000.0f;
    }

    /// <summary>
    /// Build a new building on specified location
    /// </summary>
    /// <param name="prototype"></param>
    /// <param name="tile"></param>
    /// <returns></returns>
    public bool Build(Building prototype, Tile tile)
    {
        if(prototype == null) {
            Error_Message = "???";
            return false;
        }

        if (town_hall == null && prototype.Internal_Name != "town_hall") {
            Error_Message = "Start by building the Town Hall";
            return false;
        } else if(town_hall != null && prototype.Internal_Name == "town_hall") {
            Error_Message = "You already have Town Hall";
            return false;
        }

        //Check terrain
        List<Tile> tiles = Game.Instance.Map.Get_Tiles(tile.X, tile.Y, prototype.Width, prototype.Height);
        if(tiles == null) {
            Error_Message = "Invalid terrain";
            return false;
        }
        foreach(Tile building_tile in tiles) {
            if ((prototype.Valid_Terrain.Count == 0 && !building_tile.Buildable) ||
                (prototype.Valid_Terrain.Count != 0 && !prototype.Valid_Terrain.Contains(building_tile.Terrain))) {
                Error_Message = "Invalid terrain";
                return false;
            }
        }

        //Check cost
        foreach(KeyValuePair<string, float> resource_cost in prototype.Cost) {
            if(resource_cost.Key == "cash") {
                if (Cash < resource_cost.Value) {
                    Error_Message = "Not enough cash";
                    return false;
                }
            } else {
                if (Get_Resource_Amount(resource_cost.Key) < resource_cost.Value) {
                    Error_Message = "Not enough " + resource_cost.Key;
                    return false;
                }
            }
        }

        //Create building
        Building building = new Building(tiles, tile, prototype);
        if (building.Internal_Name == "town_hall") {
            town_hall = building;
        }
        buildings.Add(building);

        //Take cost
        foreach (KeyValuePair<string, float> resource_cost in prototype.Cost) {
            if (resource_cost.Key == "cash") {
                Cash -= (int)resource_cost.Value;
            } else {
                float amount_taken = 0;
                foreach(Building loop_building in buildings) {
                    if(loop_building.Attribute("storehouse") != 0.0f) {
                        amount_taken = loop_building.Take(resource_cost.Key, resource_cost.Value - amount_taken);
                    }
                    if(amount_taken == resource_cost.Value) {
                        break;
                    }
                }
                Resources[resource_cost.Key] -= resource_cost.Value;
            }
        }

        //Update menu
        Update_Resource_Menu();

        return true;
    }

    /// <summary>
    /// Instantly store resources in citys buildings
    /// Resources are added at the end of citys Process - function
    /// </summary>
    /// <param name="resource"></param>
    /// <param name="amount"></param>
    /// <returns>Overflow</returns>
    public void Queue_Store_Resource(Building building, string resource, float amount)
    {
        if (!resources_on_queue.ContainsKey(building)) {
            resources_on_queue.Add(building, new Dictionary<string, float>());
        }
        if (!resources_on_queue[building].ContainsKey(resource)) {
            resources_on_queue[building].Add(resource, amount);
        } else {
            resources_on_queue[building][resource] += amount;
        }
    }

    /// <summary>
    /// Instantly store resources in citys buildings
    /// </summary>
    /// <param name="resource"></param>
    /// <param name="amount"></param>
    /// <returns>Overflow</returns>
    public float Store_Resources(string resource, float amount)
    {
        if(resource == "cash") {
            Cash += amount;
            return 0.0f;
        }

        foreach(Building building in buildings) {
            if (building.Storage.ContainsKey(resource)) {
                amount = building.Add(resource, amount);
                if(amount <= 0.0f) {
                    return 0.0f;
                }
            }
        }

        return amount;
    }

    /// <summary>
    /// Deletes building from city
    /// </summary>
    /// <param name="building"></param>
    public void Delete_Building(Building building)
    {
        if (!buildings_to_be_deleted.Contains(building)) {
            buildings_to_be_deleted.Add(building);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="delta_time">Seconds since last update</param>
    public void Process(float delta_time)
    {
        Resources.Clear();
        Cash_Flow = 0.0f;
        Dictionary<Building.Resident, float> resident_happiness_temp = new Dictionary<Building.Resident, float>();
        Dictionary<Building.Resident, int> required_workers_temp = new Dictionary<Building.Resident, int>();
        foreach (Building.Resident resident_type in Enum.GetValues(typeof(Building.Resident))) {
            Residents[resident_type][CURRENT] = 0;
            Residents[resident_type][MAX] = 0;
            resident_happiness_temp[resident_type] = 0.0f;
            required_workers_temp.Add(resident_type, 0);
        }
        Dictionary<string, float[]> resource_stats_temp = new Dictionary<string, float[]>();

        float income = 0.0f;
        float expenditures = 0.0f;
        float food_current = 0.0f;
        float food_max = 0.0f;
        float food_produced = 0.0f;
        float food_consumed = 0.0f;
        List<int> food_storage_cheched = new List<int>();

        List<string> food_types = new List<string>();
        foreach(KeyValuePair<string, Dictionary<string, float>> resource in resource_data) {
            if (resource.Value.ContainsKey("food")) {
                food_types.Add(resource.Key);
            }
        }

        System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
        Time += (Game.Instance.Speed * delta_time);
        foreach (Building building in buildings) {
            //Process
            if (Game.Instance.Is_Paused) {
                building.Render_Alerts();
            } else {
                building.Process(Game.Instance.Speed * (delta_time + (stopwatch.ElapsedMilliseconds / 1000.0f)));
            }
            //Storage
            if (building.Is_Built) {
                if (building.Attribute("storehouse") != 0.0f) {
                    foreach (KeyValuePair<string, float[]> resource in building.Storage) {
                        if (!Resources.ContainsKey(resource.Key)) {
                            Resources.Add(resource.Key, resource.Value[CURRENT]);
                        } else {
                            Resources[resource.Key] += resource.Value[CURRENT];
                        }
                    }
                }
                //Resident count
                foreach (Building.Resident resident_type in Enum.GetValues(typeof(Building.Resident))) {
                    if (building.Population_Type == resident_type) {
                        Residents[resident_type][CURRENT] += building.Population;
                        Residents[resident_type][MAX] += building.Population_Max;
                        resident_happiness_temp[resident_type] += (building.Population_Happiness * building.Population);
                        break;
                    }
                }
                //Worker count
                foreach (KeyValuePair<Building.Resident, int[]> worker_type in building.Workers) {
                    required_workers_temp[worker_type.Key] += worker_type.Value[MAX];
                }
                if (building.Changeable_Workers_PC.Count != 0) {
                    if (building.Changeable_Workers_PC[2] == 0) {
                        required_workers_temp[Building.Resident.PEASANT] += building.Changeable_Workers_PC[MAX];
                    } else {
                        required_workers_temp[Building.Resident.CITIZEN] += building.Changeable_Workers_PC[MAX];
                    }
                }
                if (building.Changeable_Workers_CN.Count != 0) {
                    if (building.Changeable_Workers_CN[2] == 0) {
                        required_workers_temp[Building.Resident.CITIZEN] += building.Changeable_Workers_CN[MAX];
                    } else {
                        required_workers_temp[Building.Resident.NOBLE] += building.Changeable_Workers_CN[MAX];
                    }
                }
            }
            //Cash flow
            Cash_Flow -= building.Current_Upkeep;
            expenditures -= building.Current_Upkeep;
            Cash_Flow += building.Get_Statistic("income");
            income += building.Get_Statistic("income");
            //Food
            foreach (string food in food_types) {
                food_current += building.Resource_Amount(food);
                if (building.Storage.ContainsKey(food) && !food_storage_cheched.Contains(building.Id)) {
                    food_max += building.Storage[food][MAX];
                    food_storage_cheched.Add(building.Id);
                }
            }
            food_produced += building.Get_Statistic("food_produced");
            food_consumed += building.Get_Statistic("food_consumed");
            //Resources
            foreach (KeyValuePair<string, float> stat in building.Statistics) {
                if (stat.Key.StartsWith("produced_")) {
                    string resource_name = stat.Key.Substring(9);
                    float[] data = new float[3] { 0.0f, 0.0f, stat.Value };
                    if (!resource_stats_temp.ContainsKey(resource_name)) {
                        resource_stats_temp.Add(resource_name, data);
                    } else {
                        resource_stats_temp[resource_name][0] += data[0];
                        resource_stats_temp[resource_name][1] += data[1];
                        resource_stats_temp[resource_name][2] += data[2];
                    }
                } else if (stat.Key.StartsWith("consumed_")) {
                    string resource_name = stat.Key.Substring(9);
                    float[] data = new float[3] { 0, 0, -1.0f * stat.Value };
                    if (!resource_stats_temp.ContainsKey(resource_name)) {
                        resource_stats_temp.Add(resource_name, data);
                    } else {
                        resource_stats_temp[resource_name][0] += data[0];
                        resource_stats_temp[resource_name][1] += data[1];
                        resource_stats_temp[resource_name][2] += data[2];
                    }
                }
            }
            foreach (KeyValuePair<string, float[]> resource in building.Storage) {
                float[] data = new float[3] { resource.Value[CURRENT], resource.Value[MAX], 0.0f };
                if (!resource_stats_temp.ContainsKey(resource.Key)) {
                    resource_stats_temp.Add(resource.Key, data);
                } else {
                    resource_stats_temp[resource.Key][0] += data[0];
                    resource_stats_temp[resource.Key][1] += data[1];
                    resource_stats_temp[resource.Key][2] += data[2];
                }
            }
        }
        //Redistribute workers
        foreach(KeyValuePair<Building.Resident, int> temp_data in required_workers_temp) {
            Required_Workers[temp_data.Key] = temp_data.Value;
        }
        Dictionary<Building.Resident, int> available_workers = new Dictionary<Building.Resident, int>();
        Dictionary<Building.Resident, float> worker_ratios = new Dictionary<Building.Resident, float>();
        foreach (KeyValuePair<Building.Resident, int[]> resident in Residents) {
            available_workers.Add(resident.Key, resident.Value[CURRENT]);
            if(Required_Workers[resident.Key] == 0) {
                worker_ratios.Add(resident.Key, 0.0f);
            } else {
                worker_ratios.Add(resident.Key, resident.Value[CURRENT] / (float)Required_Workers[resident.Key]);
                if(worker_ratios[resident.Key] > 1.0f) {
                    worker_ratios[resident.Key] = 1.0f;
                }
            }
        }

        foreach(Building building in buildings) {
            if(building.Is_Built && (building.Workers.Count != 0 || building.Changeable_Workers_PC.Count != 0 || building.Changeable_Workers_CN.Count != 0)) {
                foreach(KeyValuePair<Building.Resident, int[]> worker_data in building.Workers) {
                    worker_data.Value[CURRENT] = (int)(worker_ratios[worker_data.Key] * worker_data.Value[MAX]);
                    available_workers[worker_data.Key] -= worker_data.Value[CURRENT];
                }
                if(building.Changeable_Workers_PC.Count != 0) {
                    Building.Resident type = Building.Resident.PEASANT;
                    if (building.Changeable_Workers_PC[2] == 1) {
                        type = Building.Resident.CITIZEN;
                    }
                    building.Changeable_Workers_PC[CURRENT] = (int)(worker_ratios[type] * building.Changeable_Workers_PC[MAX]);
                    available_workers[type] -= building.Changeable_Workers_PC[CURRENT];
                }
                if (building.Changeable_Workers_CN.Count != 0) {
                    Building.Resident type = Building.Resident.CITIZEN;
                    if (building.Changeable_Workers_CN[2] == 1) {
                        type = Building.Resident.NOBLE;
                    }
                    building.Changeable_Workers_CN[CURRENT] = (int)(worker_ratios[type] * building.Changeable_Workers_CN[MAX]);
                    available_workers[type] -= building.Changeable_Workers_CN[CURRENT];
                }
            }
        }

        //Average happiness
        foreach (Building.Resident resident_type in Enum.GetValues(typeof(Building.Resident))) {
            if(Residents[resident_type][CURRENT] == 0) {
                resident_happiness_temp[resident_type] = 0;
            } else {
                resident_happiness_temp[resident_type] /= Residents[resident_type][CURRENT];
            }
        }

        //Try to store resources
        foreach(KeyValuePair<Building, Dictionary<string, float>> store_data in resources_on_queue) {
            foreach(KeyValuePair<string, float> resource_data in store_data.Value) {
                float over_flow = Store_Resources(resource_data.Key, resource_data.Value);
                store_data.Key.Add(resource_data.Key, over_flow);
            }
        }
        resources_on_queue.Clear();

        //Delete
        foreach(Building b in buildings_to_be_deleted) {
            buildings.Remove(b);
        }
        buildings_to_be_deleted.Clear();

        //Update menu
        foreach(KeyValuePair<Building.Resident, float> pair in resident_happiness_temp) {
            Resident_Happiness[pair.Key] = pair.Value;
        }
        MenuManager.Instance.Set_Time(Time);
        Update_Resource_Menu();
        MenuManager.Instance.Set_Resident_Info(
            Residents[Building.Resident.PEASANT][CURRENT], Residents[Building.Resident.PEASANT][MAX], Mathf.RoundToInt(Resident_Happiness[Building.Resident.PEASANT] * 100.0f),
            Required_Workers[Building.Resident.PEASANT], Mathf.RoundToInt(100.0f * Get_Employment_Ratio(Building.Resident.PEASANT)),
            Residents[Building.Resident.CITIZEN][CURRENT], Residents[Building.Resident.CITIZEN][MAX], Mathf.RoundToInt(Resident_Happiness[Building.Resident.CITIZEN] * 100.0f),
            Required_Workers[Building.Resident.CITIZEN], Mathf.RoundToInt(100.0f * Get_Employment_Ratio(Building.Resident.CITIZEN)),
            Residents[Building.Resident.NOBLE][CURRENT], Residents[Building.Resident.NOBLE][MAX], Mathf.RoundToInt(Resident_Happiness[Building.Resident.NOBLE] * 100.0f),
            Required_Workers[Building.Resident.NOBLE], Mathf.RoundToInt(100.0f * Get_Employment_Ratio(Building.Resident.NOBLE))
        );

        //Update statistics
        StatisticsManager.Instance.Cash = Cash;
        StatisticsManager.Instance.Income = income;
        StatisticsManager.Instance.Expenditures = expenditures;
        StatisticsManager.Instance.Food_Current = food_current;
        StatisticsManager.Instance.Food_Max = food_max;
        StatisticsManager.Instance.Food_Produced = food_produced;
        StatisticsManager.Instance.Food_Consumed = food_consumed;

        Resource_Stats.Clear();
        foreach(KeyValuePair<string, float[]> resource_data_temp in resource_stats_temp) {
            Resource_Stats.Add(resource_data_temp.Key, resource_data_temp.Value);
        }
    }

    /// <summary>
    /// Returns list of buildings in city
    /// </summary>
    /// <returns></returns>
    public List<Building> Get_Buildings()
    {
        return buildings;
    }

    public float Get_Resource_Amount(string resource)
    {
        if (!Resources.ContainsKey(resource)) {
            return 0.0f;
        }
        return Resources[resource];
    }

    public List<string> Get_Food_Resource_Types()
    {
        List<string> list = new List<string>();

        foreach(KeyValuePair<string, Dictionary<string, float>> data in resource_data) {
            if (data.Value.ContainsKey("food")) {
                list.Add(data.Key);
            }
        }

        return list;
    }

    public Dictionary<string, float> Get_Resource_Data(string resource)
    {
        if (resource_data.ContainsKey(resource)) {
            return resource_data[resource];
        }
        Dictionary<string, float> placeholder_data = new Dictionary<string, float>();
        placeholder_data.Add("price", 0.0f);
        return placeholder_data;
    }

    public void Update_Resource_Menu()
    {
        MenuManager.Instance.Set_Top_Menu_Resources((int)Cash, Cash_Flow, (int)Get_Resource_Amount("wood"), (int)Get_Resource_Amount("lumber"),
            (int)Get_Resource_Amount("stone"), (int)Get_Resource_Amount("tools"));
    }

    /// <summary>
    /// Calculates employment ratio for type of resident
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public float Get_Employment_Ratio(Building.Resident type)
    {
        if(Required_Workers[type] == 0) {
            return 0.0f;
        }
        return ((float)Residents[type][CURRENT] / (float)Required_Workers[type]);
    }

    public bool Grace
    {
        get {
            return (Time < Grace_Time);
        }
        private set {
            return;
        }
    }

    public Building Get_Building(int id)
    {
        foreach(Building building in buildings) {
            if(building.Id == id) {
                return building;
            }
        }
        return null;
    }

    private void Add_Food_Data(string resource, float price, string type)
    {
        Dictionary<string, float> data = new Dictionary<string, float>();
        data.Add("price", price);
        data.Add("food", 1.0f);
        data.Add("food_type_" + type, 1.0f);
        resource_data.Add(resource, data);
    }

    private void Add_Simple_Resource_Data(string resource, float price)
    {
        Dictionary<string, float> data = new Dictionary<string, float>();
        data.Add("price", price);
        resource_data.Add(resource, data);
    }
}
