using System.Collections.Generic;
using UnityEngine;

public class BuildingPrototypes  {
    private static Dictionary<string, Building> prototypes;
    private static string currently_selected = null;

    /// <summary>
    /// Creates building prototypes
    /// </summary>
    private static void Initialize()
    {
        prototypes = new Dictionary<string, Building>();

        //Town hall
        Dictionary<string, float[]> storage = new Dictionary<string, float[]>();
        storage.Add("wood", new float[2] { 750.0f, 2500.0f });
        storage.Add("lumber", new float[2] { 0.0f, 2500.0f });
        storage.Add("stone", new float[2] { 750.0f, 2500.0f });
        storage.Add("tools", new float[2] { 750.0f, 2500.0f });
        Building town_hall = new Building("town_hall", 2, 2, 0.0f, 2500, 50, 5, storage, Building.UI_Category.ADMIN);
        town_hall.Attribute("build_speed", 5.0f);
        town_hall.Attribute("build_range", 15.0f);
        town_hall.Attribute("storehouse", 1.0f);
        town_hall.Attribute("highlight_tiles_during_build", 15.0f);
        /*town_hall.Toggle_Select =
        (Building building) => {
            //Highlight tiles
            foreach (Tile tile in building.Get_Tiles_In_Circle(15.0f)) {
                tile.Highlight = new Color(0.15f, 0.15f, 0.15f, 0.5f);
            }
        };*/
        prototypes.Add("town_hall", town_hall);

        //Road
        Building road = new Building("road", 1, 1, 10.0f, 0, 0, 0, null, Building.UI_Category.INFRASTRUCTURE);
        road.Is_Road = true;
        road.Texture_Connects = new List<string>();
        road.Texture_Connects.Add("road");
        road.Texture_Connects.Add("marketplace");
        road.Cost.Add("cash", 10.0f);
        road.Cost.Add("stone", 10.0f);
        road.Cost.Add("tools", 1.0f);
        prototypes.Add("road", road);

        //Chop forest
        storage = new Dictionary<string, float[]>();
        storage.Add("wood", new float[2] { 0.0f, 1000.0f });
        Building chop = new Building("axe", 1, 1, 15.0f, 1000, 0, 0, storage, Building.UI_Category.FORESTRY);
        chop.Name = "Chop Forest";
        chop.Cost.Add("cash", 5.0f);
        chop.Cost.Add("tools", 1.0f);
        chop.Valid_Terrain.Add("Forest");
        chop.Valid_Terrain.Add("Sparse Forest");
        chop.Upkeep = 0.0f;
        chop.Can_Be_Paused = false;
        chop.On_Build =
        (Building building) => {
            if(building.Tile.Terrain == "Forest") {
                building.Add("wood", 10.0f);
            } else if (building.Tile.Terrain == "Sparse Forest") {
                building.Add("wood", 3.0f);
            }
            building.Tile.Change_To(TilePrototypes.Get("grass"));
            building.Demolish();
        };
        prototypes.Add("axe", chop);

        //Cabin
        Building hut = new Building("hut", 2, 2, 115.0f, 0, 0, 0, null, Building.UI_Category.HOUSING);
        hut.Name = "Cabin";
        hut.Cost.Add("cash", 100.0f);
        hut.Cost.Add("wood", 100.0f);
        hut.Cost.Add("stone", 15.0f);
        hut.Cost.Add("tools", 10.0f);
        hut.Population_Type = Building.Resident.PEASANT;
        hut.Population_Max = 10;
        hut.Can_Be_Paused = false;
        prototypes.Add("hut", hut);

        //Wood cutter
        storage = new Dictionary<string, float[]>();
        storage.Add("wood", new float[2] { 0.0f, 100.0f });
        Building wood_cutter = new Building("wood_cutters_lodge", 2, 2, 85.0f, 100, 0, 0, storage, Building.UI_Category.FORESTRY);
        wood_cutter.Cost.Add("cash", 90);
        wood_cutter.Cost.Add("wood", 75);
        wood_cutter.Cost.Add("stone", 5);
        wood_cutter.Cost.Add("tools", 15);
        wood_cutter.Upkeep = 0.75f;
        wood_cutter.Workers.Add(Building.Resident.PEASANT, new int[] { 0, 5 });
        wood_cutter.Produces.Add("wood");
        wood_cutter.Attribute("highlight_tiles_during_build", 3.0f);
        wood_cutter.Action = 
        (float delta_time, Building building) => {
            if (!building.Is_Built || building.Is_Paused) {
                return;
            }
            //Check tiles
            float wood = 0.0f;
            foreach(Tile tile in building.Harvest(3.0f)) {
                if (tile.Terrain == "Forest") {
                    wood += 0.125f;
                } else if(tile.Terrain == "Sparse Forest") {
                    wood += 0.0375f;
                } else if (tile.Terrain == "Hill") {
                    wood += 0.00625f;
                }
            }
            wood *= delta_time;
            wood *= building.Get_Worker_Efficiency();

            building.Add("wood", wood);
            building.Set_Statistic("produced_wood", wood / delta_time);
        };
        wood_cutter.On_Demolish =
        (Building building) => {
            building.Release_Harvested_Tiles(3.0f);
        };
        wood_cutter.Toggle_Select =
        (Building building) => {
            //Highlight tiles
            foreach (Tile tile in building.Harvest(3.0f)) {
                tile.Highlight = new Color(0.1f, 0.5f, 0.1f, 0.5f);
            }
        };
        prototypes.Add("wood_cutters_lodge", wood_cutter);

        //Lumber mill
        storage = new Dictionary<string, float[]>();
        storage.Add("wood", new float[2] { 0.0f, 150.0f });
        storage.Add("lumber", new float[2] { 0.0f, 150.0f });
        Building lumber_mill = new Building("lumber_mill", 3, 3, 250.0f, 300, 10, 5, storage, Building.UI_Category.INDUSTRY);
        lumber_mill.Cost.Add("cash", 200);
        lumber_mill.Cost.Add("wood", 225);
        lumber_mill.Cost.Add("stone", 20);
        lumber_mill.Cost.Add("tools", 40);
        lumber_mill.Upkeep = 2.0f;
        lumber_mill.Workers.Add(Building.Resident.PEASANT, new int[] { 0, 10 });
        lumber_mill.Changeable_Workers_PC.Add(0);
        lumber_mill.Changeable_Workers_PC.Add(10);
        lumber_mill.Changeable_Workers_PC.Add(0);
        lumber_mill.Consumes.Add("wood");
        lumber_mill.Produces.Add("lumber");
        lumber_mill.Appeal_Effect = -0.5f;
        lumber_mill.Appeal_Range = 4.0f;
        lumber_mill.Action =
        (float delta_time, Building building) => {
            if (!building.Is_Built || building.Is_Paused) {
                return;
            }
            building.Refine_Resources("wood", 2.0f, "lumber", 2.0f, delta_time);
        };
        prototypes.Add("lumber_mill", lumber_mill);

        //Quarry
        storage = new Dictionary<string, float[]>();
        storage.Add("stone", new float[2] { 0.0f, 225.0f });
        Building quarry = new Building("quarry", 3, 3, 225.0f, 250, 0, 0, storage, Building.UI_Category.INDUSTRY);
        quarry.Cost.Add("cash", 175);
        quarry.Cost.Add("wood", 65);
        quarry.Cost.Add("lumber", 80);
        quarry.Cost.Add("tools", 45);
        quarry.Upkeep = 1.75f;
        quarry.Workers.Add(Building.Resident.PEASANT, new int[] { 0, 20 });
        quarry.Produces.Add("stone");
        quarry.Appeal_Effect = -0.75f;
        quarry.Appeal_Range = 5.0f;
        quarry.Action =
        (float delta_time, Building building) => {
            if (!building.Is_Built || building.Is_Paused) {
                return;
            }
            float stone = 2.5f * building.Get_Worker_Efficiency() * delta_time;
            building.Add("stone", stone);
            building.Set_Statistic("produced_stone", stone / delta_time);
        };
        prototypes.Add("quarry", quarry);

        //Hunter
        storage = new Dictionary<string, float[]>();
        storage.Add("game", new float[2] { 0.0f, 100.0f });
        Building hunter = new Building("hunting_lodge", 2, 2, 95.0f, 100, 0, 0, storage, Building.UI_Category.FORESTRY);
        hunter.Cost.Add("cash", 110);
        hunter.Cost.Add("wood", 85);
        hunter.Cost.Add("stone", 10);
        hunter.Cost.Add("tools", 10);
        hunter.Upkeep = 0.75f;
        hunter.Workers.Add(Building.Resident.PEASANT, new int[] { 0, 5 });
        hunter.Produces.Add("game");
        hunter.Attribute("highlight_tiles_during_build", 5.0f);
        hunter.Action =
        (float delta_time, Building building) => {
            if (!building.Is_Built || building.Is_Paused) {
                return;
            }
            //Add game
            float game = 0.0f;
            foreach (Tile tile in building.Harvest(5.0f)) {
                if (tile.Terrain == "Forest") {
                    game += 0.0675f;
                } else if (tile.Terrain == "Sparse Forest") {
                    game += 0.0550f;
                } else if (tile.Terrain == "Fertile Ground") {
                    game += 0.0250f;
                } else if (tile.Terrain == "Grass") {
                    game += 0.0110f;
                } else if (tile.Terrain == "Hill") {
                    game += 0.0335f;
                }
            }
            //Reduce game
            foreach (Tile tile in building.Get_Tiles_In_Circle(5.0f)) {
                if(tile.Building != null && tile.Building.Id != building.Id) {
                    if (tile.Building.Is_Road) {
                        game -= 0.005f;
                    } else {
                        game -= 0.05f;
                    }
                }
            }
            if (game < 0.0f) {
                game = 0.0f;
            }
            game *= delta_time;
            game *= building.Get_Worker_Efficiency();

            building.Add("game", game);
            building.Set_Statistic("produced_game", game / delta_time);
            building.Set_Statistic("food_produced", game / delta_time);
        };
        hunter.On_Demolish =
        (Building building) => {
            building.Release_Harvested_Tiles(5.0f);
        };
        hunter.Toggle_Select =
        (Building building) => {
            //Highlight tiles
            foreach (Tile tile in building.Harvest(5.0f)) {
                tile.Highlight = new Color(0.1f, 0.5f, 0.1f, 0.5f);
            }
        };
        prototypes.Add("hunting_lodge", hunter);

        //Cellar
        storage = new Dictionary<string, float[]>();
        foreach (string food in City.Instance.Get_Food_Resource_Types()) {
            storage.Add(food, new float[2] { 0.0f, 1000.0f });
        }
        storage.Add("salt", new float[2] { 0.0f, 1000.0f });
        storage.Add("alcohol", new float[2] { 0.0f, 1000.0f });
        Building cellar = new Building("cellar", 1, 1, 110.0f, 1000, 15, 10, storage, Building.UI_Category.INFRASTRUCTURE);
        cellar.Cost.Add("cash", 100);
        cellar.Cost.Add("wood", 15);
        cellar.Cost.Add("lumber", 50);
        cellar.Cost.Add("stone", 50);
        cellar.Cost.Add("tools", 10);
        cellar.Upkeep = 0.5f;
        cellar.Attribute("storehouse", 1.0f);
        cellar.Workers.Add(Building.Resident.PEASANT, new int[] { 0, 5 });
        prototypes.Add("cellar", cellar);

        //Market
        storage = new Dictionary<string, float[]>();
        foreach(string food in City.Instance.Get_Food_Resource_Types()) {
            storage.Add(food, new float[2] { 0.0f, 10.0f });
        }
        storage.Add("salt", new float[2] { 0.0f, 10.0f });
        Building market = new Building("marketplace", 3, 3, 110.0f, 100, 7, 0, storage, Building.UI_Category.SERVICES);
        market.Is_Road = true;
        market.Cost.Add("cash", 110);
        market.Cost.Add("lumber", 20);
        market.Cost.Add("stone", 90);
        market.Cost.Add("tools", 10);
        market.Attribute("limited_raw_material_warnings", 1.0f);
        market.Attribute("limited_storage_settings", 1.0f);
        market.Upkeep = 1.0f;
        market.Appeal_Effect = 0.1f;
        market.Appeal_Range = 5.0f;
        market.Changeable_Workers_PC.Add(0);
        market.Changeable_Workers_PC.Add(10);
        market.Changeable_Workers_PC.Add(0);
        foreach (string food in City.Instance.Get_Food_Resource_Types()) {
            market.Consumes.Add(food);
        }
        market.Consumes.Add("salt");
        market.Action =
        (float delta_t, Building building) => {
            if (!building.Active) {
                return;
            }
            List<Building> residential_buildings = new List<Building>();
            //Search for residential buildings
            foreach (Building b in building.Get_Connected_Buildings(building.Range, -1)) {
                if (b.Is_Residential && b.Is_Built) {
                    residential_buildings.Add(b);
                }
            }
            if(residential_buildings.Count == 0) {
                //No residences
                return;
            }
            //Stats
            float income = 0.0f;
            Dictionary<string, float> food_sold_stats = new Dictionary<string, float>();
            List<string> food_types = City.Instance.Get_Food_Resource_Types();
            foreach (string food_type in food_types) {
                food_sold_stats.Add(food_type, 0.0f);
                if(building.Statistics.ContainsKey(food_type + "_sold")) {
                    building.Statistics.Remove(food_type + "_sold");
                }
            }
            //Serve food
            foreach (string food_type in food_types) {
                if(building.Resource_Amount(food_type) != 0.0f) {
                    float food_per_residence = building.Resource_Amount(food_type) / residential_buildings.Count;
                    float food_served = 0.0f;
                    foreach(Building residence in residential_buildings) {
                        food_served += residence.Sell_Food(food_type, food_per_residence);
                    }
                    //Substract and stats
                    food_served = building.Take(food_type, food_served);
                    food_sold_stats[food_type] = food_served;
                    income += food_served * City.Instance.Get_Resource_Data(food_type)["price"];
                }
            }
            //Serve salt
            income += building.Sell_Resources_As_Service("salt", "salt", residential_buildings, delta_t, 1.0f, false);

            //Stats and income
            City.Instance.Cash += income;
            building.Set_Statistic("income", income / delta_t);
            foreach(KeyValuePair<string, float> stat in food_sold_stats) {
                if(stat.Value != 0.0f) {
                    building.Set_Statistic(stat.Key + "_sold", stat.Value / delta_t);
                }
            }
        };
        prototypes.Add("marketplace", market);

        //Storehouse
        storage = new Dictionary<string, float[]>();
        storage.Add("wood", new float[2] { 0.0f, 2000.0f });
        storage.Add("lumber", new float[2] { 0.0f, 2000.0f });
        storage.Add("stone", new float[2] { 0.0f, 2000.0f });
        storage.Add("tools", new float[2] { 0.0f, 2000.0f });
        storage.Add("coal", new float[2] { 0.0f, 2000.0f });
        storage.Add("iron_ore", new float[2] { 0.0f, 2000.0f });
        storage.Add("iron_bars", new float[2] { 0.0f, 2000.0f });
        Building storehouse = new Building("storehouse", 2, 2, 250.0f, 2000, 10, 20, storage, Building.UI_Category.INFRASTRUCTURE);
        storehouse.Cost.Add("cash", 255.0f);
        storehouse.Cost.Add("lumber", 275.0f);
        storehouse.Cost.Add("stone", 30.0f);
        storehouse.Cost.Add("tools", 25.0f);
        storehouse.Upkeep = 1.0f;
        storehouse.Workers.Add(Building.Resident.PEASANT, new int[2] { 0, 10 });
        storehouse.Attribute("storehouse", 1.0f);
        prototypes.Add("storehouse", storehouse);

        //Gatherer's hut
        storage = new Dictionary<string, float[]>();
        storage.Add("berries", new float[2] { 0.0f, 100.0f });
        storage.Add("mushrooms", new float[2] { 0.0f, 100.0f });
        storage.Add("roots", new float[2] { 0.0f, 100.0f });
        storage.Add("herbs", new float[2] { 0.0f, 100.0f });
        Building gatherers_lodge = new Building("gatherers_lodge", 2, 2, 95.0f, 100, 0, 0, storage, Building.UI_Category.FORESTRY);
        gatherers_lodge.Cost.Add("cash", 100);
        gatherers_lodge.Cost.Add("wood", 85);
        gatherers_lodge.Cost.Add("stone", 10);
        gatherers_lodge.Cost.Add("tools", 10);
        gatherers_lodge.Upkeep = 0.75f;
        gatherers_lodge.Workers.Add(Building.Resident.PEASANT, new int[] { 0, 5 });
        gatherers_lodge.Produces.Add("berries");
        gatherers_lodge.Produces.Add("mushrooms");
        gatherers_lodge.Produces.Add("roots");
        gatherers_lodge.Produces.Add("herbs");
        gatherers_lodge.Attribute("highlight_tiles_during_build", 3.0f);
        gatherers_lodge.Action =
        (float delta_time, Building building) => {
            if (!building.Is_Built || building.Is_Paused) {
                return;
            }
            //Add food
            float berries = 0.0f;
            float mushrooms = 0.0f;
            float roots = 0.0f;
            float herbs = 0.0f;
            foreach (Tile tile in building.Harvest(3.0f)) {
                if (tile.Terrain == "Forest") {
                    berries   += 0.0085f;
                    mushrooms += 0.0075f;
                    roots     += 0.0045f;
                    herbs     += 0.0006f;
                } else if (tile.Terrain == "Sparse Forest") {
                    berries   += 0.0075f;
                    mushrooms += 0.0050f;
                    roots     += 0.0040f;
                    herbs     += 0.0005f;
                } else if (tile.Terrain == "Fertile Ground") {
                    berries   += 0.0025f;
                    mushrooms += 0.0005f;
                    roots     += 0.0050f;
                    herbs     += 0.0010f;
                } else if (tile.Terrain == "Grass") {
                    berries   += 0.0020f;
                    mushrooms += 0.0005f;
                    roots     += 0.0040f;
                    herbs     += 0.0004f;
                } else if (tile.Terrain == "Hill") {
                    berries   += 0.0020f;
                    mushrooms += 0.0010f;
                    roots     += 0.0045f;
                    herbs     += 0.0003f;
                }
            }

            float efficiency = building.Get_Worker_Efficiency();
            berries = (berries * delta_time) * efficiency;
            mushrooms = (mushrooms * delta_time) * efficiency;
            roots = (roots * delta_time) * efficiency;
            herbs = (herbs * delta_time) * efficiency;

            building.Set_Statistic("food_produced", (berries + mushrooms + roots + herbs) / delta_time);
            building.Add("berries", berries);
            building.Set_Statistic("produced_berries", berries / delta_time);
            building.Add("mushrooms", mushrooms);
            building.Set_Statistic("produced_mushrooms", mushrooms / delta_time);
            building.Add("roots", roots);
            building.Set_Statistic("produced_roots", roots / delta_time);
            building.Add("herbs", herbs);
            building.Set_Statistic("produced_herbs", herbs / delta_time);
        };
        gatherers_lodge.On_Demolish =
        (Building building) => {
            building.Release_Harvested_Tiles(3.0f);
        };
        gatherers_lodge.Toggle_Select =
        (Building building) => {
            //Highlight tiles
            foreach (Tile tile in building.Harvest(3.0f)) {
                tile.Highlight = new Color(0.1f, 0.5f, 0.1f, 0.5f);
            }
        };
        prototypes.Add("gatherers_lodge", gatherers_lodge);

        //Decorative tree
        Building decorative_tree = new Building("decorative_tree", 1, 1, 50.0f, 0, 0, 0, new Dictionary<string, float[]>(), Building.UI_Category.SERVICES);
        decorative_tree.Cost.Add("cash", 25.0f);
        decorative_tree.Cost.Add("stone", 5.0f);
        decorative_tree.Cost.Add("tools", 1.0f);
        decorative_tree.Appeal_Effect = 2.0f;
        decorative_tree.Appeal_Range = 2.0f;
        decorative_tree.Can_Be_Paused = false;
        decorative_tree.Attribute("highlight_tiles_during_build", 2.0f);
        prototypes.Add("decorative_tree", decorative_tree);

        //Mine
        storage = new Dictionary<string, float[]>();
        storage.Add("iron_ore", new float[2] { 0.0f, 100.0f });
        storage.Add("coal", new float[2] { 0.0f, 100.0f });
        storage.Add("salt", new float[2] { 0.0f, 100.0f });
        Building mine = new Building("mine", 2, 2, 250.0f, 100, 0, 0, storage, Building.UI_Category.INDUSTRY);
        mine.Cost.Add("cash", 250);
        mine.Cost.Add("wood", 90);
        mine.Cost.Add("lumber", 90);
        mine.Cost.Add("stone", 15);
        mine.Cost.Add("tools", 40);
        mine.Upkeep = 2.00f;
        mine.Workers.Add(Building.Resident.PEASANT, new int[] { 0, 15 });
        mine.Produces.Add("iron_ore");
        mine.Produces.Add("coal");
        mine.Produces.Add("salt");
        mine.Appeal_Effect = -1.25f;
        mine.Appeal_Range = 5.0f;
        mine.Attribute("highlight_tiles_during_build", 5.0f);
        mine.Action =
        (float delta_time, Building building) => {
            if (!building.Active) {
                return;
            }
            float iron_ore = 0.0f;
            float coal = 0.0f;
            float salt = 0.0f;
            foreach (Tile tile in building.Harvest(3.0f, false, true)) {
                if (tile.Ore.ContainsKey("iron")) {
                    iron_ore += tile.Ore["iron"];
                }
                if (tile.Ore.ContainsKey("coal")) {
                    coal += tile.Ore["coal"];
                }
                if (tile.Ore.ContainsKey("salt")) {
                    salt += tile.Ore["salt"];
                }
            }

            float efficiency = building.Get_Worker_Efficiency();
            iron_ore = (iron_ore * delta_time) * efficiency;
            coal = (coal * delta_time) * efficiency;
            salt = (salt * delta_time) * efficiency;

            building.Add("iron_ore", iron_ore);
            building.Set_Statistic("produced_iron_ore", iron_ore / delta_time);
            building.Add("coal", coal);
            building.Set_Statistic("produced_coal", coal / delta_time);
            building.Add("salt", salt);
            building.Set_Statistic("produced_salt", salt / delta_time);
        };
        mine.On_Demolish =
        (Building building) => {
            building.Release_Harvested_Tiles(5.0f, true);
        };
        mine.Toggle_Select =
        (Building building) => {
            //Highlight tiles
            foreach (Tile tile in building.Harvest(5.0f, true, true)) {
                tile.Highlight = new Color(0.1f, 0.5f, 0.1f, 0.5f);
            }
        };
        prototypes.Add("mine", mine);

        //Charcoal burner
        storage = new Dictionary<string, float[]>();
        storage.Add("wood", new float[2] { 0.0f, 50.0f });
        storage.Add("coal", new float[2] { 0.0f, 50.0f });
        Building charcoal_burner = new Building("charcoal_burner", 2, 2, 50.0f, 100, 5, 2, storage, Building.UI_Category.FORESTRY);
        charcoal_burner.Cost.Add("cash", 45);
        charcoal_burner.Cost.Add("lumber", 50);
        charcoal_burner.Cost.Add("stone", 5);
        charcoal_burner.Cost.Add("tools", 5);
        charcoal_burner.Upkeep = 0.5f;
        charcoal_burner.Workers.Add(Building.Resident.PEASANT, new int[] { 0, 5 });
        charcoal_burner.Consumes.Add("wood");
        charcoal_burner.Produces.Add("coal");
        charcoal_burner.Appeal_Effect = -1.25f;
        charcoal_burner.Appeal_Range = 6.0f;
        charcoal_burner.Action =
        (float delta_time, Building building) => {
            if (!building.Active) {
                return;
            }
            building.Refine_Resources("wood", 0.5f, "coal", 0.5f, delta_time);
        };
        prototypes.Add("charcoal_burner", charcoal_burner);

        //Foundry
        storage = new Dictionary<string, float[]>();
        storage.Add("coal", new float[2] { 0.0f, 50.0f });
        storage.Add("iron_ore", new float[2] { 0.0f, 100.0f });
        storage.Add("iron_bars", new float[2] { 0.0f, 50.0f });
        Building foundry = new Building("foundry", 2, 2, 205.0f, 200, 5, 5, storage, Building.UI_Category.INDUSTRY);
        foundry.Cost.Add("cash", 160);
        foundry.Cost.Add("lumber", 75);
        foundry.Cost.Add("stone", 130);
        foundry.Cost.Add("tools", 20);
        foundry.Upkeep = 1.0f;
        foundry.Workers.Add(Building.Resident.PEASANT, new int[] { 0, 10 });
        foundry.Consumes.Add("coal");
        foundry.Consumes.Add("iron_ore");
        foundry.Produces.Add("iron_bars");
        foundry.Appeal_Effect = -1.25f;
        foundry.Appeal_Range = 6.0f;
        foundry.Action =
        (float delta_time, Building building) => {
            if (!building.Active) {
                return;
            }
            Dictionary<string, float> raw_material = new Dictionary<string, float>();
            raw_material.Add("coal", 1.0f);
            raw_material.Add("iron_ore", 2.0f);
            Dictionary<string, float> produce = new Dictionary<string, float>();
            produce.Add("iron_bars", 1.0f);
            building.Refine_Resources(raw_material, produce, delta_time);
        };
        prototypes.Add("foundry", foundry);

        //Smithy
        storage = new Dictionary<string, float[]>();
        storage.Add("coal", new float[2] { 0.0f, 50.0f });
        storage.Add("iron_bars", new float[2] { 0.0f, 100.0f });
        storage.Add("tools", new float[2] { 0.0f, 50.0f });
        Building smithy = new Building("smithy", 2, 2, 165.0f, 200, 5, 5, storage, Building.UI_Category.INDUSTRY);
        smithy.Cost.Add("cash", 150);
        smithy.Cost.Add("lumber", 65);
        smithy.Cost.Add("stone", 100);
        smithy.Cost.Add("tools", 30);
        smithy.Upkeep = 2.0f;
        smithy.Changeable_Workers_PC.Add(0);
        smithy.Changeable_Workers_PC.Add(5);
        smithy.Changeable_Workers_PC.Add(0);
        smithy.Consumes.Add("coal");
        smithy.Consumes.Add("iron_bars");
        smithy.Produces.Add("tools");
        smithy.Appeal_Effect = -0.75f;
        smithy.Appeal_Range = 4.0f;
        smithy.Action =
        (float delta_time, Building building) => {
            if (!building.Active) {
                return;
            }
            Dictionary<string, float> raw_material = new Dictionary<string, float>();
            raw_material.Add("coal", 0.5f);
            raw_material.Add("iron_bars", 1.0f);
            Dictionary<string, float> produce = new Dictionary<string, float>();
            produce.Add("tools", 0.5f);
            building.Refine_Resources(raw_material, produce, delta_time);
        };
        prototypes.Add("smithy", smithy);

        //Small farm
        storage = new Dictionary<string, float[]>();
        storage.Add("potatoes", new float[2] { 0.0f, 200.0f });
        Building small_farm = new Building("small_farm", 2, 2, 135.0f, 200, 0, 0, storage, Building.UI_Category.AGRICULTURE);
        small_farm.Cost.Add("cash", 100);
        small_farm.Cost.Add("lumber", 110);
        small_farm.Cost.Add("stone", 15);
        small_farm.Cost.Add("tools", 10);
        small_farm.Upkeep = 1.00f;
        small_farm.Workers.Add(Building.Resident.PEASANT, new int[] { 0, 10 });
        small_farm.Produces.Add("potatoes");
        small_farm.Attribute("highlight_tiles_during_build", 4.0f);
        small_farm.Action =
        (float delta_time, Building building) => {
            if (!building.Active) {
                return;
            }

            float potatoes = 0.0f;
            foreach (Tile tile in building.Harvest(4.0f, true)) {
                if (tile.Building != null && tile.Building.Internal_Name == "potato_field") {
                    if(tile.Terrain == "Fertile Ground") {
                        potatoes += 0.090f;
                    } else {
                        potatoes += 0.045f;
                    }
                }
            }

            potatoes *= delta_time;
            potatoes *= building.Get_Worker_Efficiency();

            building.Add("potatoes", potatoes);
            building.Set_Statistic("produced_potatoes", potatoes / delta_time);
            building.Set_Statistic("food_produced", potatoes / delta_time);
        };
        small_farm.On_Demolish =
        (Building building) => {
            building.Release_Harvested_Tiles(4.0f);
        };
        small_farm.Toggle_Select =
        (Building building) => {
            //Highlight tiles
            foreach (Tile tile in building.Harvest(4.0f, true)) {
                tile.Highlight = new Color(0.1f, 0.5f, 0.1f, 0.5f);
            }
        };
        prototypes.Add("small_farm", small_farm);

        //Potato field
        Building potato_field = new Building("potato_field", 1, 1, 20.0f, 0, 0, 0, new Dictionary<string, float[]>(), Building.UI_Category.AGRICULTURE);
        potato_field.Cost.Add("cash", 10.0f);
        potato_field.Cost.Add("tools", 1.0f);
        potato_field.Appeal_Effect = -0.05f;
        potato_field.Appeal_Range = 2.0f;
        potato_field.Can_Be_Paused = false;
        prototypes.Add("potato_field", potato_field);

        //Chapel
        Building chapel = new Building("chapel", 2, 2, 235.0f, 0, 10, 0, new Dictionary<string, float[]>(), Building.UI_Category.SERVICES);
        chapel.Cost.Add("cash", 210);
        chapel.Cost.Add("lumber", 60);
        chapel.Cost.Add("stone", 175);
        chapel.Cost.Add("tools", 20);
        chapel.Upkeep = 2.0f;
        chapel.Appeal_Effect = 0.2f;
        chapel.Appeal_Range = 5.0f;
        chapel.Changeable_Workers_PC.Add(0);
        chapel.Changeable_Workers_PC.Add(5);
        chapel.Changeable_Workers_PC.Add(0);
        chapel.Action =
        (float delta_t, Building building) => {
            if (!building.Active) {
                return;
            }
            List<Building> residential_buildings = new List<Building>();
            //Search for residential buildings in need of services
            foreach (Building b in building.Get_Connected_Buildings(building.Range, -1)) {
                if (b.Is_Residential && b.Is_Built && b.Missing_Service("chapel") >= 0.05f) {
                    residential_buildings.Add(b);
                }
            }
            //Serve buildings
            float quality = ((building.Get_Worker_Efficiency() + 1.0f) / 2.0f);
            foreach (Building residence in residential_buildings) {
                //Serve
                residence.Serve("chapel", 1.0f, quality);
            }
        };
        prototypes.Add("chapel", chapel);

        //Brewery
        storage = new Dictionary<string, float[]>();
        storage.Add("potatoes", new float[2] { 0.0f, 50.0f });
        storage.Add("alcohol", new float[2] { 0.0f, 50.0f });
        Building brewery = new Building("brewery", 2, 2, 135.0f, 100, 5, 5, storage, Building.UI_Category.AGRICULTURE);
        brewery.Cost.Add("cash", 110);
        brewery.Cost.Add("lumber", 120);
        brewery.Cost.Add("stone", 15);
        brewery.Cost.Add("tools", 15);
        brewery.Upkeep = 1.0f;
        brewery.Changeable_Workers_PC.Add(0);
        brewery.Changeable_Workers_PC.Add(5);
        brewery.Changeable_Workers_PC.Add(0);
        brewery.Consumes.Add("potatoes");
        brewery.Produces.Add("alcohol");
        brewery.Appeal_Effect = -0.25f;
        brewery.Appeal_Range = 2.0f;
        brewery.Action =
        (float delta_time, Building building) => {
            if (!building.Active) {
                return;
            }
            building.Refine_Resources("potatoes", 1.0f, "alcohol", 0.5f, delta_time);
        };
        prototypes.Add("brewery", brewery);

        //Tavern
        storage = new Dictionary<string, float[]>();
        storage.Add("alcohol", new float[2] { 0.0f, 100.0f });
        Building tavern = new Building("tavern", 2, 2, 350.0f, 100, 5, 5, storage, Building.UI_Category.SERVICES);
        tavern.Cost.Add("cash", 205);
        tavern.Cost.Add("lumber", 175);
        tavern.Cost.Add("stone", 175);
        tavern.Cost.Add("tools", 15);
        tavern.Upkeep = 1.5f;
        tavern.Changeable_Workers_PC.Add(0);
        tavern.Changeable_Workers_PC.Add(10);
        tavern.Changeable_Workers_PC.Add(0);
        tavern.Consumes.Add("alcohol");
        tavern.Action =
        (float delta_time, Building building) => {
            if (!building.Active) {
                return;
            }
            List<Building> residential_buildings = new List<Building>();
            //Search for residential buildings in need of services
            foreach (Building b in building.Get_Connected_Buildings(building.Range, -1)) {
                if (b.Is_Residential && b.Is_Built && b.Missing_Service("tavern") >= 0.05f) {
                    residential_buildings.Add(b);
                }
            }
            //Serve
            building.Sell_Resources_As_Service("alcohol", "tavern", residential_buildings, delta_time, (building.Get_Worker_Efficiency() + 0.5f) / 1.5f);
        };
        prototypes.Add("tavern", tavern);

        //Abode
        Building abode = new Building("abode", 2, 2, 125.0f, 0, 0, 0, null, Building.UI_Category.HOUSING);
        abode.Cost.Add("cash", 150.0f);
        abode.Cost.Add("lumber", 50.0f);
        abode.Cost.Add("stone", 75.0f);
        abode.Cost.Add("tools", 10.0f);
        abode.Population_Type = Building.Resident.CITIZEN;
        abode.Population_Max = 5;
        abode.Can_Be_Paused = false;
        prototypes.Add("abode", abode);

        //Tax office
        Building tax_office = new Building("tax_office", 2, 2, 180.0f, 0, 10, 0, new Dictionary<string, float[]>(), Building.UI_Category.ADMIN);
        tax_office.Cost.Add("cash", 225);
        tax_office.Cost.Add("lumber", 90);
        tax_office.Cost.Add("stone", 90);
        tax_office.Cost.Add("tools", 10);
        tax_office.Upkeep = 1.5f;
        tax_office.Workers.Add(Building.Resident.CITIZEN, new int[] { 0, 5 });
        tax_office.Action =
        (float delta_time, Building building) => {
            if (!building.Active) {
                return;
            }
            List<Building> residential_buildings = new List<Building>();
            //Search for residential buildings
            foreach (Building b in building.Get_Connected_Buildings(building.Range, -1)) {
                if (b.Is_Residential && b.Is_Built) {
                    residential_buildings.Add(b);
                }
            }
            //Collect tax
            float income = 0.0f;
            foreach(Building residence in residential_buildings) {
                if(!residence.Data.ContainsKey("tax_collected") || (int)(((float[])residence.Data["tax_collected"])[0]) == building.Id) {
                    if (!residence.Data.ContainsKey("tax_collected")) {
                        residence.Data.Add("tax_collected", new float[2]{ building.Id, 1.0f });
                    } else {
                        ((float[])residence.Data["tax_collected"])[1] = 1.0f;
                    }
                    switch (residence.Population_Type) {
                        case Building.Resident.PEASANT:
                            income += 0.05f * residence.Population;
                            break;
                        case Building.Resident.CITIZEN:
                            income += 0.25f * residence.Population;
                            break;
                        case Building.Resident.NOBLE:
                            income += 5.0f * residence.Population;
                            break;
                    }
                }
            }
            income *= building.Get_Worker_Efficiency();
            building.Set_Statistic("income", income);
            income *= delta_time;
            City.Instance.Cash += income;
        };
        prototypes.Add("tax_office", tax_office);

        //Builder's hall
        Building builders_hall = new Building("builders_hall", 2, 2, 275.0f, 0, 0, 0, new Dictionary<string, float[]>(), Building.UI_Category.INFRASTRUCTURE);
        builders_hall.Name = "Builder's Hall";
        builders_hall.Cost.Add("cash", 260);
        builders_hall.Cost.Add("lumber", 85);
        builders_hall.Cost.Add("stone", 190);
        builders_hall.Cost.Add("tools", 35);
        builders_hall.Upkeep = 1.25f;
        builders_hall.Changeable_Workers_PC.Add(0);
        builders_hall.Changeable_Workers_PC.Add(10);
        builders_hall.Changeable_Workers_PC.Add(0);
        builders_hall.Attribute("build_speed", 10.0f);
        builders_hall.Attribute("build_range", 10.0f);
        builders_hall.Attribute("highlight_tiles_during_build", 10.0f);
        prototypes.Add("builders_hall", builders_hall);
    }

    /// <summary>
    /// Get building prototype
    /// </summary>
    /// <param name="building"></param>
    /// <returns></returns>
    public static Building Get(string building = null)
    {
        if (prototypes == null) {
            Initialize();
        }
        if(building == null) {
            building = Currently_Selected;
        }
        if(building == null) {
            return null;
        }
        if (!prototypes.ContainsKey(building)) {
            return null;
        }
        return prototypes[building];
    }

    /// <summary>
    /// Returns list of building belonging to specified category
    /// </summary>
    /// <param name="category"></param>
    /// <returns></returns>
    public static List<Building> Get_By_Category(Building.UI_Category category)
    {
        if (prototypes == null) {
            Initialize();
        }
        List<Building> list = new List<Building>();
        foreach(KeyValuePair<string, Building> pair in prototypes) {
            if(pair.Value.Category == category) {
                list.Add(pair.Value);
            }
        }
        return list;
    }

    public static string Currently_Selected {
        get {
            return currently_selected;
        }
        set {
            currently_selected = value;
            if (Get(currently_selected) != null) {
                MouseListener.Instance.Transparent_Building.SetActive(true);
                SpriteRenderer renderer = MouseListener.Instance.Transparent_Building.GetComponent<SpriteRenderer>();
                renderer.sprite = SpriteManager.Instance.Get_Sprite(Get(currently_selected).Texture + "_trans");
                renderer.sortingOrder = SortingLayer.layers[0].value + Game.Instance.Map.Height + 1;
                MenuManager.Instance.Show_Building_Info();
                Game.Instance.Map.Set_Overlay(Map.Overlay.Build);
            } else {
                MouseListener.Instance.Transparent_Building.SetActive(false);
                MenuManager.Instance.Hide_Building_Info();
                Game.Instance.Map.Set_Overlay(Map.Overlay.No_Overlay);
            }
        }
    }
}
