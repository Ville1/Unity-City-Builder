using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour {
    public static MenuManager Instance { get; private set; }

    private float message_panel_open_time;
    private static float message_panel_time = 5.0f; //seconds
    private float info_update_cooldown;
    private static float info_update_intervals = 1.0f; //seconds

    private bool menu_is_open;
    private bool city_menu_is_open;
    private bool overlays_menu_is_open;
    private Building.UI_Category? open_building_tab;
    private Dictionary<Building.UI_Category, Button> tab_buttons;
    private Dictionary<string, Button> building_buttons;
    private int max_storage_settings = 11;
    private int max_storage_links = 9;
    private Dictionary<string, InputField> storage_settings_fields;
    private Dictionary<string, InputField> storage_links_inputs;
    private Dictionary<string, Button> storage_links_buttons;
    private Dictionary<string, int> storage_links_building_ids;

    private bool select_link_building_mode;
    private int new_storage_link_building_id;
    private string new_storage_link_building_button;

    public GameObject Canvas;

    public GameObject Tile_Info_Prefab;

    public GameObject New_Game_Button;
    public GameObject Save_Game_Button;
    public GameObject Load_Game_Button;
    public GameObject Exit_Game_Button;

    public GameObject City_Menu_Button;
    public GameObject Overlays_Button;
    public GameObject Statistics_Button;

    public Button No_Overlay_Button;
    public Button Ore_Overlay_Button;
    public Button Appeal_Overlay_Button;

    public GameObject Cash_Text;
    public GameObject Wood_Text;
    public GameObject Lumber_Text;
    public GameObject Stone_Text;
    public GameObject Tools_Text;

    public Button Button_Prefab;
    public GameObject Bottom_Menu;

    public GameObject Top_Menu;

    public GameObject Peasant_Info;
    public GameObject Citizen_Info;
    public GameObject Noble_Info;

    public GameObject Alert_General;
    public GameObject Alert_No_Resources;
    public GameObject Alert_No_Room;
    public GameObject Alert_Paused;
    public GameObject Alert_Road;
    public GameObject Alert_Unhappiness;
    public GameObject Alert_Workers;

    public GameObject RightPanel;
    public Building selected_building;

    public GameObject StorageSettingsPanel;
    public GameObject StorageSettingsButton;
    public GameObject StorageLinksButton;
    public GameObject StorageSettingsHeader;

    public GameObject StorageLinksPanel;

    public GameObject Message_Panel;

    private RectTransform bottom_menu_rectangle_transform;
    private RectTransform top_menu_rectangle_transform;
    private float bottom_menu_width;
    private float bottom_menu_height;

    private List<Tile> highlighted_road_tiles;
    private Color road_color;
    private List<Tile> highlighted_building_tiles;
    private Color building_color;

    private bool show_alerts;
    private bool show_building_ranges;

    // Use this for initialization
    private void Start () {
        if(Instance != null) {
            Logger.Instance.Error("Start called multiple times");
        }
        Instance = this;
        show_alerts = true;
        show_building_ranges = true;
        info_update_cooldown = 0;

        StorageSettingsPanel.SetActive(false);
        storage_settings_fields = new Dictionary<string, InputField>();
        StorageLinksPanel.SetActive(false);
        storage_links_inputs = new Dictionary<string, InputField>();
        storage_links_buttons = new Dictionary<string, Button>();
        storage_links_building_ids = new Dictionary<string, int>();
        select_link_building_mode = false;
        new_storage_link_building_id = -1;

        highlighted_road_tiles = new List<Tile>();
        road_color = new Color(0.5f, 0.2f, 0.0f, 0.5f);
        highlighted_building_tiles = new List<Tile>();
        building_color = new Color(0.2f, 0.5f, 0.0f, 0.5f);

        menu_is_open = true;
        city_menu_is_open = true;
        overlays_menu_is_open = true;
        tab_buttons = new Dictionary<Building.UI_Category, Button>();
        building_buttons = new Dictionary<string, Button>();
        open_building_tab = null;
        Close_Menus();
        City_Menu_Enabled = false;

        //Initialize bottom menu
        int index = 0;
        //Resize
        bottom_menu_rectangle_transform = Bottom_Menu.GetComponent<RectTransform>();
        bottom_menu_rectangle_transform.sizeDelta = new Vector2(Screen.width, bottom_menu_rectangle_transform.sizeDelta.y);
        //Calculate width & height of bottom menu
        Vector3[] bottom_menu_corners = new Vector3[4];
        bottom_menu_rectangle_transform.GetWorldCorners(bottom_menu_corners);
        bottom_menu_width = bottom_menu_corners[2].x - bottom_menu_corners[0].x;
        bottom_menu_height = bottom_menu_corners[2].y - bottom_menu_corners[0].y;
        //Calculate width & height of buttons
        RectTransform button_rectangle_transform = Button_Prefab.GetComponent<RectTransform>();
        Vector3[] corners = new Vector3[4];
        button_rectangle_transform.GetWorldCorners(corners);
        float button_width = corners[2].x - corners[0].x;
        float button_height = corners[2].y - corners[0].y;

        foreach (Building.UI_Category category in Enum.GetValues(typeof(Building.UI_Category))) {
            //Add button for category
            Button button = Instantiate(Button_Prefab);
            button.name = category.ToString() + "_tab_button";
            button.GetComponentInChildren<Text>().text = category.ToString();
            button.transform.SetParent(Bottom_Menu.transform);
            button.transform.position = new Vector3(Bottom_Menu.transform.position.x + (index * button_width) + (button_width / 2.0f) - (bottom_menu_width / 2.0f),
                Bottom_Menu.transform.position.y + (bottom_menu_height / 2.0f) + (button_height / 2.0f));
            Button.ButtonClickedEvent on_click_event = new Button.ButtonClickedEvent();
            on_click_event.AddListener(new UnityEngine.Events.UnityAction(delegate() { Open_Tab(category); }));
            button.onClick = on_click_event;
            index++;

            tab_buttons.Add(category, button);

            //Create buttons for buildings
            int index_2 = 0;
            foreach(Building building in BuildingPrototypes.Get_By_Category(category)) {
                Button building_button = Instantiate(Button_Prefab);
                building_button.GetComponentInChildren<Text>().text = building.Name;
                building_button.transform.SetParent(Bottom_Menu.transform);
                building_button.gameObject.SetActive(false);
                Image image = building_button.GetComponent<Image>();
                image.sprite = SpriteManager.Instance.Get_Sprite(building.Texture);
                image.rectTransform.sizeDelta = new Vector2(image.rectTransform.sizeDelta.x / 2.0f, image.rectTransform.sizeDelta.y * 3.0f);
                building_button.transform.position = new Vector3(Bottom_Menu.transform.position.x + (index_2 * image.rectTransform.sizeDelta.x) + (image.rectTransform.sizeDelta.x / 2.0f) - (bottom_menu_width / 2.0f),
                    Bottom_Menu.transform.position.y + (bottom_menu_height / 2.0f) - (image.rectTransform.sizeDelta.y / 2.0f));
                Button.ButtonClickedEvent building_on_click_event = new Button.ButtonClickedEvent();
                building_on_click_event.AddListener(new UnityEngine.Events.UnityAction(delegate () {
                    if (BuildingPrototypes.Currently_Selected == building.Internal_Name) {
                        Hide_Building_Info();
                        BuildingPrototypes.Currently_Selected = null;
                    } else {
                        BuildingPrototypes.Currently_Selected = building.Internal_Name;
                    }
                }));
                building_button.onClick = building_on_click_event;

                building_buttons.Add(building.Name, building_button);
                index_2++;
            }
        }

        //Hide bottom menu
        bottom_menu_rectangle_transform.Translate(new Vector3(0.0f, -bottom_menu_height, 0.0f));

        //Hide message panel
        Message_Panel.SetActive(false);
        message_panel_open_time = 0.0f;

        //Initialize top menu
        top_menu_rectangle_transform = Top_Menu.GetComponent<RectTransform>();
        top_menu_rectangle_transform.sizeDelta = new Vector2(Screen.width, top_menu_rectangle_transform.sizeDelta.y);

        //Right Menu
        RightPanel.SetActive(false);
        StorageSettingsButton.SetActive(false);
        StorageSettingsHeader.SetActive(false);
        StorageLinksButton.SetActive(false);
        selected_building = null;
    }
	
	// Update is called once per frame
	private void Update () {
		if(message_panel_open_time > 0.0f) {
            message_panel_open_time -= Time.deltaTime;
            if(message_panel_open_time <= 0.0f) {
                Hide_Message();
            }
        }
        if(selected_building != null) {
            info_update_cooldown -= Time.deltaTime;
            if(info_update_cooldown <= 0.0f) {
                Update_Selected_Building_Panel();
                info_update_cooldown += info_update_intervals;
            }
        }
	}

    public void Select_Building_At(int index)
    {
        if(Open_Building_Tab == null) {
            return;
        }
        List<Building> buildings_in_category = BuildingPrototypes.Get_By_Category(Open_Building_Tab.Value);
        if(index < 0 || index >= buildings_in_category.Count) {
            return;
        }
        if(BuildingPrototypes.Currently_Selected == buildings_in_category[index].Internal_Name) {
            BuildingPrototypes.Currently_Selected = null;
        } else {
            BuildingPrototypes.Currently_Selected = buildings_in_category[index].Internal_Name;
        }
    }

    public bool Show_Alerts
    {
        get {
            return show_alerts;
        }
        set {
            show_alerts = value;
            if (show_alerts) {
                Show_Message("Show alerts");
            } else {
                Show_Message("Hide alerts");
            }
        }
    }

    public bool Show_Building_Ranges
    {
        get {
            return show_building_ranges;
        }
        set {
            show_building_ranges = value;
            if (show_building_ranges) {
                Show_Message("Show highlights");
            } else {
                Show_Message("Hide highlights");
            }
        }
    }

    public bool City_Menu_Enabled
    {
        get {
            return City_Menu_Button.GetComponent<Button>().interactable;
        }
        set {
            City_Menu_Button.GetComponent<Button>().interactable = value;
        }
    }

    /// <summary>
    /// Get currently selected building
    /// </summary>
    /// <returns></returns>
    public Building Get_Selected_Building()
    {
        return selected_building;
    }

    /// <summary>
    /// Opens and closes game menu
    /// </summary>
    public void Toggle_Menu()
    {
        if (!menu_is_open) {
            New_Game_Button.SetActive(true);
            Save_Game_Button.SetActive(true);
            Load_Game_Button.SetActive(true);
            Exit_Game_Button.SetActive(true);
        } else {
            New_Game_Button.SetActive(false);
            Save_Game_Button.SetActive(false);
            Load_Game_Button.SetActive(false);
            Exit_Game_Button.SetActive(false);
        }
        menu_is_open = !menu_is_open;
    }

    /// <summary>
    /// Closes menu, if it is open 
    /// </summary>
    public void Close_Menu()
    {
        if (menu_is_open) {
            New_Game_Button.SetActive(false);
            Save_Game_Button.SetActive(false);
            Load_Game_Button.SetActive(false);
            Exit_Game_Button.SetActive(false);
            menu_is_open = false;
        }
    }

    public void On_New_Game_Click()
    {
        Game.Instance.New();
    }

    public void On_Save_Game_Click()
    {
        //TODO
    }

    public void On_Load_Game_Click()
    {
        //TODO
    }

    public void On_Exit_Game_Click()
    {
        Game.Instance.Quit();
    }

    /// <summary>
    /// Opens and closes city menu
    /// </summary>
    public void Toggle_City_Menu()
    {
        if (city_menu_is_open) {
            Overlays_Button.SetActive(false);
            Statistics_Button.SetActive(false);
            Close_Overlays_Menu();
            city_menu_is_open = false;
        } else {
            Overlays_Button.SetActive(true);
            Statistics_Button.SetActive(true);
            city_menu_is_open = true;
        }
    }

    /// <summary>
    /// Closes menu, if it is open 
    /// </summary>
    public void Close_City_Menu()
    {
        if (city_menu_is_open) {
            Overlays_Button.SetActive(false);
            Statistics_Button.SetActive(false);
            city_menu_is_open = false;
            Close_Overlays_Menu();
        }
    }

    public void On_Overlays_Click()
    {
        Toggle_Overlays_Menu();
    }

    public void On_Statistic_Click()
    {
        StatisticsManager.Instance.Open_Panel();
    }

    public void Toggle_Overlays_Menu()
    {
        if (overlays_menu_is_open) {
            No_Overlay_Button.gameObject.SetActive(false);
            Ore_Overlay_Button.gameObject.SetActive(false);
            Appeal_Overlay_Button.gameObject.SetActive(false);
            overlays_menu_is_open = false;
        } else {
            No_Overlay_Button.gameObject.SetActive(true);
            Ore_Overlay_Button.gameObject.SetActive(true);
            Appeal_Overlay_Button.gameObject.SetActive(true);
            Update_Overlays_Menu();
            overlays_menu_is_open = true;
        }
    }

    public void Close_Overlays_Menu()
    {
        if (overlays_menu_is_open) {
            No_Overlay_Button.gameObject.SetActive(false);
            Ore_Overlay_Button.gameObject.SetActive(false);
            Appeal_Overlay_Button.gameObject.SetActive(false);
            overlays_menu_is_open = false;
        }
    }

    public void On_No_Overlay_Click()
    {
        Game.Instance.Map.Set_Overlay(Map.Overlay.No_Overlay);
        Update_Overlays_Menu();
    }

    public void On_Ore_Overlay_Click()
    {
        Game.Instance.Map.Set_Overlay(Map.Overlay.Ore);
        Update_Overlays_Menu();
    }

    public void On_Appeal_Overlay_Click()
    {
        Game.Instance.Map.Set_Overlay(Map.Overlay.Appeal);
        Update_Overlays_Menu();
    }

    public void Update_Overlays_Menu()
    {
        GameObject.Find(No_Overlay_Button.gameObject.name + "/SelectedText").GetComponent<Text>().text = Game.Instance.Map.Current_Overlay == Map.Overlay.No_Overlay ? ">" : "";
        GameObject.Find(Ore_Overlay_Button.gameObject.name + "/SelectedText").GetComponent<Text>().text = Game.Instance.Map.Current_Overlay == Map.Overlay.Ore ? ">" : "";
        GameObject.Find(Appeal_Overlay_Button.gameObject.name + "/SelectedText").GetComponent<Text>().text = Game.Instance.Map.Current_Overlay == Map.Overlay.Appeal ? ">" : "";
    }

    public void Close_Menus()
    {
        Close_Menu();
        Close_City_Menu();
        Close_Overlays_Menu();
    }

    /// <summary>
    /// Opens a building tab on bottom menu
    /// </summary>
    /// <param name="tab"></param>
    public void Open_Tab(Building.UI_Category tab)
    {
        if(Game.Instance.State != Game.GameState.RUNNING) {
            //No building menu access if game is not running
            Show_Message("Start new game from menu: Game -> New");
            return;
        }
        if(tab == open_building_tab) {
            //Same tab -> hide menu
            bottom_menu_rectangle_transform.Translate(new Vector3(0.0f, -bottom_menu_height, 0.0f));
            tab_buttons[(Building.UI_Category)open_building_tab].GetComponent<Image>().color = Color.white;
            foreach (Building building in BuildingPrototypes.Get_By_Category((Building.UI_Category)open_building_tab)) {
                building_buttons[building.Name].gameObject.SetActive(false);
            }
            open_building_tab = null;
            Hide_Building_Info();
            BuildingPrototypes.Currently_Selected = null;
            return;
        } else if(open_building_tab == null) {
            //Show menu
            bottom_menu_rectangle_transform.Translate(new Vector3(0.0f, bottom_menu_height, 0.0f));
        }

        if(open_building_tab != null) {
            //Hide buildings in old tab
            tab_buttons[(Building.UI_Category)open_building_tab].GetComponent<Image>().color = Color.white;
            foreach (Building building in BuildingPrototypes.Get_By_Category((Building.UI_Category)open_building_tab)) {
                building_buttons[building.Name].gameObject.SetActive(false);
            }
        }
        open_building_tab = tab;
        //Show buildings in new tab
        foreach (Building building in BuildingPrototypes.Get_By_Category(tab)) {
            building_buttons[building.Name].gameObject.SetActive(true);
        }
        tab_buttons[(Building.UI_Category)open_building_tab].GetComponent<Image>().color = Color.blue;
    }

    public Building.UI_Category? Open_Building_Tab
    {
        get {
            return open_building_tab;
        }
    }

    /// <summary>
    /// Close building tab, if one is open
    /// </summary>
    public void Close_Tab()
    {
        if(open_building_tab != null) {
            Open_Tab((Building.UI_Category)open_building_tab);
            Hide_Building_Info();
        }
    }

    /// <summary>
    /// Shows a message to player, that disappears after predetermined time
    /// </summary>
    /// <param name="message"></param>
    public void Show_Message(string message)
    {
        if(message_panel_open_time > 0.0f && Message_Panel.GetComponentInChildren<Text>().text == message) {
            Hide_Message(); //TODO: Fix this
            return;
        }
        message_panel_open_time = message_panel_time;
        Message_Panel.SetActive(true);
        Message_Panel.GetComponentInChildren<Text>().text = message;
    }

    /// <summary>
    /// Hides current message
    /// </summary>
    public void Hide_Message()
    {
        if(Message_Panel.activeSelf) {
            message_panel_open_time = 0.0f;
            Message_Panel.SetActive(false);
        }
    }

    /// <summary>
    /// Set city name
    /// </summary>
    /// <param name="name"></param>
    public void Set_City_Name(string name)
    {
        GameObject text_go = GameObject.Find("CityNameText");
        Text text = text_go.GetComponent<Text>();
        text.text = name;
    }

    /// <summary>
    /// Set time
    /// </summary>
    /// <param name="days"></param>
    public void Set_Time(float days)
    {
        GameObject text_go = GameObject.Find("TimeText");
        Text text = text_go.GetComponent<Text>();
        int years = (int)(days / 360.0f);
        days -= (int)(years * 360.0f);
        int months = (int)(days / 30.0f);
        days -= (int)(months * 30.0f);
        text.text = "D: " + Front_Zeroes((int)days + 1) + " M: " + Front_Zeroes(months + 1) + " Y: " + Front_Zeroes(years + 1);
    }

    /// <summary>
    /// Set game speed display
    /// </summary>
    /// <param name="index"></param>
    public void Set_Speed(int index)
    {
        GameObject text_go = GameObject.Find("SpeedText");
        Text text = text_go.GetComponent<Text>();
        if(index == 0) {
            text.text = "||";
        } else {
            string arrows = "";
            for (int i = 0; i < index; i++) {
                arrows += ">";
            }
            text.text = arrows;
        }
    }

    /// <summary>
    /// Set building material amounts to top menu
    /// </summary>
    /// <param name="cash"></param>
    /// <param name="cash_flow"></param>
    /// <param name="wood"></param>
    /// <param name="lumber"></param>
    /// <param name="stone"></param>
    /// <param name="tools"></param>
    public void Set_Top_Menu_Resources(int cash, float cash_flow, int wood, int lumber, int stone, int tools)
    {
        if(cash_flow >= 0.0f) {
            Cash_Text.GetComponent<Text>().text = "" + cash + " +" + Math.Round(cash_flow, 1);
        } else {
            Cash_Text.GetComponent<Text>().text = "" + cash + " " + Math.Round(cash_flow, 1);
        }
        if(wood < 10000) {
            Wood_Text.GetComponent<Text>().text = "" + wood;
        } else {
            Wood_Text.GetComponent<Text>().text = "9999";
        }
        if (lumber < 10000) {
            Lumber_Text.GetComponent<Text>().text = "" + lumber;
        } else {
            Lumber_Text.GetComponent<Text>().text = "9999";
        }
        if (stone < 10000) {
            Stone_Text.GetComponent<Text>().text = "" + stone;
        } else {
            Stone_Text.GetComponent<Text>().text = "9999";
        }
        if (tools < 10000) {
            Tools_Text.GetComponent<Text>().text = "" + tools;
        } else {
            Tools_Text.GetComponent<Text>().text = "9999";
        }
    }

    /// <summary>
    /// Set resident info of top panel
    /// </summary>
    /// <param name="peasants"></param>
    /// <param name="peasant_max"></param>
    /// <param name="peasant_happiness"></param>
    /// <param name="peasant_jobs"></param>
    /// <param name="peasant_employment"></param>
    /// <param name="citizens"></param>
    /// <param name="citizen_max"></param>
    /// <param name="citizen_happiness"></param>
    /// <param name="citizen_jobs"></param>
    /// <param name="citizen_employment"></param>
    /// <param name="nobles"></param>
    /// <param name="nobles_max"></param>
    /// <param name="noble_happiness"></param>
    /// <param name="noble_jobs"></param>
    /// <param name="noble_employment"></param>
    public void Set_Resident_Info(int peasants, int peasant_max, int peasant_happiness, int peasant_jobs, int peasant_employment,
        int citizens, int citizen_max, int citizen_happiness, int citizen_jobs, int citizen_employment,
        int nobles, int nobles_max, int noble_happiness, int noble_jobs, int noble_employment)
    {
        Peasant_Info.GetComponent<Text>().text = peasants + " / " + peasant_max + "  " + peasant_employment + "%\n" + peasant_happiness + "      " + peasant_jobs;
        Citizen_Info.GetComponent<Text>().text = citizens + " / " + citizen_max + "  " + citizen_employment + "%\n" + citizen_happiness + "      " + citizen_jobs;
        Noble_Info.GetComponent<Text>().text = nobles + " / " + nobles_max + "  " + noble_employment + "%\n" + noble_happiness + "      " + noble_jobs;
    }

    /// <summary>
    /// If building is selected, it will get unselected
    /// </summary>
    public void Unselect_Building()
    {
        if(selected_building != null) {
            Select_Building(selected_building);
        }
    }

    /// <summary>
    /// Sets selected building and shows it's info on right panel, selecting same building again will hide panel
    /// </summary>
    /// <param name="building"></param>
    public void Select_Building(Building building)
    {
        if (select_link_building_mode && selected_building != null) {
            //Set new link
            //Check connection
            List<Building> connected_buildings = selected_building.Get_Connected_Buildings(selected_building.Range);
            bool found = false;
            foreach(Building connected_building in connected_buildings) {
                if(connected_building.Id == building.Id) {
                    found = true;
                    break;
                }
            }
            if (!found) {
                Show_Message("Not connected! (press Esc to cancel)");
                return;
            }
            if(building.Attribute("storehouse") == 0.0f) {
                Show_Message("Invalid building! (press Esc to cancel)");
                return;
            }
            //Set link
            new_storage_link_building_id = building.Id;
            select_link_building_mode = false;
            StorageLinksPanel.SetActive(true);
            if (storage_links_building_ids.ContainsKey(new_storage_link_building_button)) {
                storage_links_building_ids[new_storage_link_building_button] = new_storage_link_building_id;
            } else {
                storage_links_building_ids.Add(new_storage_link_building_button, new_storage_link_building_id);
            }
            storage_links_buttons[new_storage_link_building_button].GetComponentInChildren<Text>().text = building.Name;
            return;
        }

        if(selected_building != null && selected_building.Id == building.Id) {
            if (StorageSettingsPanel.activeSelf) {
                return;
            }
            //Hide
            Game.Instance.Map.Set_Overlay(Map.Overlay.No_Overlay);
            RightPanel.SetActive(false);
            StorageSettingsButton.SetActive(false);
            Hide_Storage_Settings();
            Hide_Storage_Links();
            if(selected_building.Toggle_Select != null) {
                selected_building.Toggle_Select(selected_building);
            }
            selected_building = null;
            foreach(Tile t in highlighted_road_tiles) {
                t.Highlight = road_color;
                if(t.Building != null) {
                    t.Building.Clear_Range_Marker();
                }
            }
            foreach (Tile t in highlighted_building_tiles) {
                t.Highlight = building_color;
                if (t.Building != null) {
                    t.Building.Clear_Range_Marker();
                }
            }
            highlighted_road_tiles.Clear();
            highlighted_building_tiles.Clear();
        } else {
            //Show
            Unselect_Building();
            Game.Instance.Map.Set_Overlay(Map.Overlay.Build);
            RightPanel.SetActive(true);
            if(selected_building != null && selected_building.Toggle_Select != null) {
                selected_building.Toggle_Select(selected_building);
            }
            selected_building = building;
            if (selected_building.Toggle_Select != null) {
                selected_building.Toggle_Select(selected_building);
            }
            Hide_Storage_Settings();
            Hide_Storage_Links();
            //Highlight
            if (selected_building.Range != 0 && Show_Building_Ranges) {
                List<Building> connected_buildings = selected_building.Get_Connected_Buildings(selected_building.Range, 0, null, true);
                foreach (Building connected_building in connected_buildings) {
                    foreach (Tile t in connected_building.Tiles) {
                        if (connected_building.Is_Road) {
                            t.Highlight = road_color;
                            highlighted_road_tiles.Add(t);
                        } else {
                            t.Highlight = building_color;
                            highlighted_building_tiles.Add(t);
                        }
                    }
                }
            }
            RightPanel.transform.Find("NameText").GetComponent<Text>().text = building.Name;
            Update_Selected_Building_Panel();
        }
    }

    /// <summary>
    /// If there is building selected, update its info on right panel
    /// </summary>
    public void Update_Selected_Building_Panel()
    {
        if(selected_building == null) {
            return;
        }
        Building building = selected_building;
        //Collect data
        StringBuilder data = new StringBuilder();

        //Pause
        if (selected_building.Is_Paused) {
            data.Append("\nPaused\n");
        }

        if (selected_building.Is_Residential) {
            //Population
            data.Append("\nPopulation:\n");
            data.Append(selected_building.Population + " / " + selected_building.Population_Max + "\n" +
                Helper.Parse_To_Human_Readable(selected_building.Population_Type.ToString()) + "s");

            //Happiness
            data.Append("\nHappiness: ");
            data.Append(Mathf.RoundToInt(building.Population_Happiness * 100.0f));
            if (building.Population_Type == Building.Resident.PEASANT && City.Instance.Grace) {
                data.Append(" (nc:");
                data.Append(Math.Round(City.Instance.Grace_Time - City.Instance.Time, 0));
                data.Append(")");
            }
            data.Append("\n");

            //Appeal
            data.Append("\nAppeal: ");
            data.Append(Math.Round(building.Average_Appeal(), 1));
            data.Append("\n");

            //Services
            if (selected_building.Is_Residential) {
                data.Append("\nFood");
                data.Append("\nAmount: ");
                data.Append(Math.Round(selected_building.Food_Amount(), 1));
                data.Append("\nQuality: ");
                data.Append(Math.Round(selected_building.Food_Quality(), 1));
                data.Append("\n\nServices & Goods\n");
                if (selected_building.Services.Count == 0) {
                    data.Append("-");
                } else {
                    foreach (KeyValuePair<string, float[]> service in selected_building.Services) {
                        data.Append(Helper.Parse_To_Human_Readable(service.Key));
                        data.Append(" (");
                        data.Append(Math.Round(selected_building.Service_Quality(service.Key), 3));
                        data.Append("): ");
                        data.Append(Mathf.RoundToInt(selected_building.Current_Service(service.Key) * 100.0f));
                        data.Append("%\n");
                    }
                }
            }
        }

        //Workers
        if (selected_building.Workers.Count != 0 || selected_building.Changeable_Workers_PC.Count != 0 || selected_building.Changeable_Workers_CN.Count != 0) {
            data.Append("\nWorkers\n");
            foreach (KeyValuePair<Building.Resident, int[]> worker_data in selected_building.Workers) {
                data.Append(worker_data.Value[0] + " / " + worker_data.Value[1] + " " + Helper.Parse_To_Human_Readable(worker_data.Key.ToString()) + "\n");
            }
            if(selected_building.Changeable_Workers_PC.Count != 0) {
                data.Append(selected_building.Changeable_Workers_PC[0] + " / " + selected_building.Changeable_Workers_PC[1]);
                if(selected_building.Changeable_Workers_PC[2] == 0) {
                    data.Append(" [P]/C\n");
                } else {
                    data.Append(" P/[C]\n");
                }
            }
            if (selected_building.Changeable_Workers_CN.Count != 0) {
                data.Append(selected_building.Changeable_Workers_CN[0] + " / " + selected_building.Changeable_Workers_CN[1]);
                if (selected_building.Changeable_Workers_CN[2] == 0) {
                    data.Append(" [C]/N\n");
                } else {
                    data.Append(" C/[N]\n");
                }
            }
            data.Append("Efficiency: " + Mathf.RoundToInt(selected_building.Get_Worker_Efficiency() * 100.0f) + "%\n");
        }

        //Storage
        if (selected_building.Max_Storage != 0) {
            data.Append("\nStorage:\n");
            data.Append(Mathf.RoundToInt(selected_building.Get_Total_Resources()) + " / " + selected_building.Max_Storage + "\n");
            foreach (KeyValuePair<string, float[]> resource in selected_building.Storage) {
                data.Append(Helper.Parse_To_Human_Readable(resource.Key) + ": " + Mathf.RoundToInt(resource.Value[0]));
                if (selected_building.Has_Storage_Limit(resource.Key)) {
                    data.Append(" / " + selected_building.Get_Storage_Limit(resource.Key) + "\n");
                } else {
                    data.Append("\n");
                }
            }
        }

        //Stats
        data.Append("\nStats:\n");
        data.Append("Upkeep: ");
        data.Append(Math.Round(selected_building.Current_Upkeep, 2));
        data.Append("\n");
        foreach (KeyValuePair<string, float> stat in building.Statistics) {
            data.Append(stat.Key + ": " + Math.Round(stat.Value, 1) + "\n");
        }

        //Storage settings
        StorageSettingsButton.SetActive(selected_building.Attribute("storehouse") != 0.0f || selected_building.Attribute("limited_storage_settings") != 0.0f);
        StorageLinksButton.SetActive(selected_building.Attribute("storehouse") != 0.0f);
        StorageSettingsHeader.SetActive(selected_building.Attribute("storehouse") != 0.0f || selected_building.Attribute("limited_storage_settings") != 0.0f);
        RightPanel.transform.Find("DataText").gameObject.GetComponent<Text>().text = data.ToString();
    }

    /// <summary>
    /// Shows info about building that is selected to be built
    /// </summary>
    public void Show_Building_Info()
    {
        if(BuildingPrototypes.Currently_Selected == null) {
            return;
        }
        Unselect_Building();
        RightPanel.SetActive(true);

        //Collect data
        StringBuilder data = new StringBuilder();
        Building proto = BuildingPrototypes.Get();

        RightPanel.transform.Find("NameText").gameObject.GetComponent<Text>().text = proto.Name;

        data.Append("\nCost\n");
        bool has_cost = false;
        foreach(KeyValuePair<string, float> cost in proto.Cost) {
            data.Append(Helper.Parse_To_Human_Readable(cost.Key));
            data.Append(": ");
            data.Append(cost.Value);
            data.Append("\n");
            has_cost = true;
        }
        if(!has_cost) {
            data.Append("free\n");
        }

        data.Append("\nBuilding time: ");
        data.Append(proto.Construction_Time);
        data.Append("\nUpkeep: ");
        data.Append(proto.Upkeep);
        
        if(proto.Appeal_Effect != 0.0f) {
            float actual_effect = proto.Appeal_Effect * (proto.Width * proto.Height);
            data.Append("\n\nAppeal effect: ");
            data.Append(Math.Round(actual_effect, 2));
            data.Append("\nAppeal range: ");
            data.Append(proto.Appeal_Range);
        }

        data.Append("\nStorage: ");
        data.Append(proto.Max_Storage);
        if(proto.Range != 0 && proto.Transport_Speed != 0) {
            data.Append("\nTransport range: ");
            data.Append(proto.Range);
            data.Append("\nTransport speed: ");
            data.Append(proto.Transport_Speed);
        }

        //Render
        RightPanel.transform.Find("DataText").gameObject.GetComponent<Text>().text = data.ToString();
    }

    /// <summary>
    /// Hides info about building that is selected to be built
    /// </summary>
    public void Hide_Building_Info()
    {
        if(selected_building != null || !RightPanel.activeSelf) {
            return;
        }
        RightPanel.SetActive(false);
    }

    /// <summary>
    /// Deconstructs selected building
    /// </summary>
    public void Delete_Building()
    {
        if(selected_building == null || !selected_building.Is_Built) {
            return;
        }
        selected_building.Demolish();
    }

    /// <summary>
    /// Toggles pause status of selected building
    /// </summary>
    public void Toggle_Pause()
    {
        if (selected_building == null || !selected_building.Is_Built || !selected_building.Can_Be_Paused) {
            return;
        }
        selected_building.Is_Paused = !selected_building.Is_Paused;
    }

    /// <summary>
    /// Show storage settings panel
    /// </summary>
    public void Show_Storage_Settings()
    {
        if(selected_building == null || (selected_building.Attribute("storehouse") == 0.0f && selected_building.Attribute("limited_storage_settings") == 0.0f)) {
            return;
        }
        StorageSettingsPanel.SetActive(true);

        //Find all texts & reset
        Text[] texts = StorageSettingsPanel.GetComponentsInChildren<Text>();
        Dictionary<string, Text> text_dictionary = new Dictionary<string, Text>();
        foreach(Text text in texts) {
            if(text.name != "StorageSettingsTitle" && text.name != "Placeholder" && text.name != "Text") {
                if (text.name.Contains("Amount")) {
                    text.text = "0 /";
                } else {
                    text.text = "-";
                }
                text_dictionary.Add(text.name, text);
            }
        }

        //Set resource names, amounts, limits and get links
        storage_settings_fields.Clear();
        int index = 1;
        foreach(KeyValuePair<string, float[]> resource in selected_building.Storage) {
            //TODO: remove these
            if(!text_dictionary.ContainsKey("ResourceText" + index)) {
                Debug.Log("text_dictionary is missing element: " + "ResourceText" + index);
            }
            if (!text_dictionary.ContainsKey("ResourceAmountText" + index)) {
                Debug.Log("text_dictionary is missing element: " + "ResourceAmountText" + index);
            }
            if (!text_dictionary.ContainsKey("ResourceInputText" + index)) {
                Debug.Log("text_dictionary is missing element: " + "ResourceInputText" + index);
            }
            //Name, amount, limit
            text_dictionary["ResourceText" + index].text = Helper.Parse_To_Human_Readable(resource.Key);
            text_dictionary["ResourceAmountText" + index].text = "" + Math.Round(resource.Value[0], 1) + " /";
            InputField field = text_dictionary["ResourceInputText" + index].transform.parent.gameObject.GetComponent<InputField>();
            storage_settings_fields.Add(resource.Key, field);
            if (selected_building.Has_Storage_Limit(resource.Key)) {
                field.text = "" + selected_building.Get_Storage_Limit(resource.Key);
            } else {
                field.text = "";
            }

            //Index
            index++;
            if(index > max_storage_settings) {
                break;
            }
        }
    }

    /// <summary>
    /// Hide storage settings panel
    /// </summary>
    public void Hide_Storage_Settings()
    {
        if (!StorageSettingsPanel.activeSelf || selected_building == null) {
            return;
        }

        //Save data
        foreach(KeyValuePair<string, InputField> field_data in storage_settings_fields) {
            if(field_data.Value.text != "") {
                int value = 0;
                int.TryParse(field_data.Value.text, out value);
                selected_building.Set_Storage_Limit(field_data.Key, value);
            } else {
                selected_building.Set_Storage_Limit(field_data.Key, selected_building.Max_Storage);
            }
        }

        //Unactivate
        StorageSettingsPanel.SetActive(false);
    }

    /// <summary>
    /// Sets selected building to accept all resources to storage
    /// </summary>
    public void Storage_Settings_Accept_All()
    {
        if (!StorageSettingsPanel.activeSelf || selected_building == null) {
            return;
        }
        foreach (KeyValuePair<string, InputField> field_data in storage_settings_fields) {
            field_data.Value.text = "";
        }
    }
    /// <summary>
    /// Sets selected building to not accept anything to its storage
    /// </summary>
    public void Storage_Settings_Block_All()
    {
        if (!StorageSettingsPanel.activeSelf || selected_building == null) {
            return;
        }
        foreach (KeyValuePair<string, InputField> field_data in storage_settings_fields) {
            field_data.Value.text = "0";
        }
    }

    /// <summary>
    /// Show storage links panel
    /// </summary>
    public void Show_Storage_Links()
    {
        if (StorageLinksPanel.activeSelf || selected_building == null || selected_building.Attribute("storehouse") == 0.0f) {
            return;
        }
        StorageLinksPanel.SetActive(true);

        //Find all inputs & reset
        InputField[] inputs = StorageLinksPanel.GetComponentsInChildren<InputField>();
        storage_links_inputs.Clear();
        foreach (InputField input in inputs) {
            input.text = "";
            storage_links_inputs.Add(input.name, input);
        }

        //Find all buttons & reset
        storage_links_building_ids.Clear();
        Button[] buttons = StorageLinksPanel.GetComponentsInChildren<Button>();
        storage_links_buttons.Clear();
        foreach (Button button in buttons) {
            if (button.name.StartsWith("Action")) {
                Button.ButtonClickedEvent action_on_click_event = new Button.ButtonClickedEvent();
                button.GetComponentInChildren<Text>().text = "Get";
                action_on_click_event.AddListener(new UnityEngine.Events.UnityAction(delegate () {
                    if(button.GetComponentInChildren<Text>().text == "Get") {
                        button.GetComponentInChildren<Text>().text = "Give";
                    } else {
                        button.GetComponentInChildren<Text>().text = "Get";
                    }
                }));
                button.onClick = action_on_click_event;
                storage_links_buttons.Add(button.name, button);
            } else if (button.name.StartsWith("Building")) {
                button.GetComponentInChildren<Text>().text = "Set";
                Button.ButtonClickedEvent building_on_click_event = new Button.ButtonClickedEvent();
                building_on_click_event.AddListener(new UnityEngine.Events.UnityAction(delegate () {
                    Show_Message("Select building (press Esc to cancel)");
                    select_link_building_mode = true;
                    new_storage_link_building_button = button.name;
                    StorageLinksPanel.SetActive(false);
                }));
                button.onClick = building_on_click_event;
                storage_links_buttons.Add(button.name, button);
                storage_links_building_ids.Add(button.name, -1);
            }
        }

        //Set data
        int index = 1;
        foreach (object[] link in selected_building.Storage_Links) {
            if((string)link[1] == "Get") {
                storage_links_buttons["ActionButton" + index].GetComponentInChildren<Text>().text = "Get";
            } else {
                storage_links_buttons["ActionButton" + index].GetComponentInChildren<Text>().text = "Give";
            }
            storage_links_inputs["AmountInput" + index].text = "" + (int)link[2];
            storage_links_inputs["ResourceInput" + index].text = (string)link[3];
            Building link_building = City.Instance.Get_Building((int)link[0]);
            if(link_building == null) {
                storage_links_buttons["BuildingButton" + index].GetComponentInChildren<Text>().text = "Broken";
                storage_links_building_ids["BuildingButton" + index] = -1;
            } else {
                storage_links_buttons["BuildingButton" + index].GetComponentInChildren<Text>().text = link_building.Name;
                storage_links_building_ids["BuildingButton" + index] = link_building.Id;
            }
            index++;
        }
    }

    /// <summary>
    /// Cancels out of selecting of a new building to link to
    /// </summary>
    public void Cancel_New_Storage_Link_Building()
    {
        if (!select_link_building_mode) {
            return;
        }
        select_link_building_mode = false;
        StorageLinksPanel.SetActive(true);
    }

    public bool Select_Link_Building_Mode
    {
        get {
            return select_link_building_mode;
        }
        private set {
            select_link_building_mode = value;
        }
    }

    /// <summary>
    /// Hides storage links panel
    /// </summary>
    public void Hide_Storage_Links()
    {
        if (!StorageLinksPanel.activeSelf) {
            return;
        }
        StorageLinksPanel.SetActive(false);
        if(selected_building == null || selected_building.Attribute("storehouse") == 0.0f) {
            return;
        }
        //Save changes
        //Clear old
        selected_building.Storage_Links.Clear();
        //Save new
        for(int i = 1; i <= max_storage_links; i++) {
            string action = storage_links_buttons["ActionButton" + i].GetComponentInChildren<Text>().text;
            int amount = 0;
            int.TryParse(storage_links_inputs["AmountInput" + i].text, out amount);
            string resource = storage_links_inputs["ResourceInput" + i].text;
            int building = storage_links_building_ids["BuildingButton" + i];
            if (amount > 0 && resource != "" && building != -1) {
                selected_building.Storage_Links.Add(new object[4] { building, action, amount, resource });
            }
        }
    }

    /// <summary>
    /// Does menu manager have an open panel witch takes kayboard inputs?
    /// </summary>
    public bool Takes_Keyboard_Inputs {
        get {
            return StorageLinksPanel.activeSelf || StorageSettingsPanel.activeSelf;
        }
        private set {
            //Abc
        }
    }

    /// <summary>
    /// Toggle worker type options for selected building
    /// </summary>
    public void Toggle_Workers_PC()
    {
        if (selected_building == null || selected_building.Changeable_Workers_PC.Count == 0) {
            return;
        }
        if(selected_building.Changeable_Workers_PC[2] == 0) {
            selected_building.Changeable_Workers_PC[2] = 1;
        } else {
            selected_building.Changeable_Workers_PC[2] = 0;
        }
    }

    /// <summary>
    /// Toggle worker type options for selected building
    /// </summary>
    public void Toggle_Workers_CN()
    {
        if (selected_building == null || selected_building.Changeable_Workers_CN.Count == 0) {
            return;
        }
        if (selected_building.Changeable_Workers_CN[2] == 0) {
            selected_building.Changeable_Workers_CN[2] = 1;
        } else {
            selected_building.Changeable_Workers_CN[2] = 0;
        }
    }

    /// <summary>
    /// Returns number with zero on front if it is < 10
    /// </summary>
    /// <param name="number"></param>
    /// <returns></returns>
    private string Front_Zeroes(int number)
    {
        if(number <= 9) {
            return "0" + number;
        }
        return "" + number;
    }
}
