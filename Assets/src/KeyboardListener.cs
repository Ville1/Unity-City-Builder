using UnityEngine;

/// <summary>
/// Class responsible of listening to keyboard inputs from user
/// </summary>
public class KeyboardListener : MonoBehaviour {
	/// <summary>
    /// Initialization
    /// </summary>
	private void Start () { }
	
	/// <summary>
    /// Per frame update
    /// TODO: Messy code
    /// </summary>
	private void Update () {
        //Close stuff
		if(Input.GetButtonDown("Cancel")) {
            if (MenuManager.Instance.Select_Link_Building_Mode) {
                MenuManager.Instance.Cancel_New_Storage_Link_Building();
                MenuManager.Instance.Hide_Message();
            } else {
                MenuManager.Instance.Hide_Building_Info();
                MenuManager.Instance.Hide_Message();
                MenuManager.Instance.Hide_Storage_Settings();
                MenuManager.Instance.Close_Menus();
                MenuManager.Instance.Close_Tab();
                MenuManager.Instance.Unselect_Building();
                MenuManager.Instance.Hide_Storage_Links();
                ConsoleManager.Instance.Close_Console();
                StatisticsManager.Instance.Close_Panel();
            }
        }

        //Camera movement
        if(!ConsoleManager.Instance.Is_Open() && !MenuManager.Instance.Takes_Keyboard_Inputs) {
            if (Input.GetAxis("Vertical") > 0.0f) {
                CameraManager.Instance.Move_Camera(Map.Direction.North);
            }
            if (Input.GetAxis("Horizontal") < 0.0f) {
                CameraManager.Instance.Move_Camera(Map.Direction.West);
            }
            if (Input.GetAxis("Vertical") < 0.0f) {
                CameraManager.Instance.Move_Camera(Map.Direction.South);
            }
            if (Input.GetAxis("Horizontal") > 0.0f) {
                CameraManager.Instance.Move_Camera(Map.Direction.East);
            }
        }
        if(Input.GetAxis("Mouse ScrollWheel") > 0.0f) {
            if(!ConsoleManager.Instance.Scroll_Up()) {
                CameraManager.Instance.Zoom_Camera(CameraManager.Zoom.Out);
            }
        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0.0f) {
            if (!ConsoleManager.Instance.Scroll_Down()) {
                CameraManager.Instance.Zoom_Camera(CameraManager.Zoom.In);
            }
        }
        if (Input.GetButtonDown("Reset Zoom")) {
            CameraManager.Instance.Reset_Zoom();
        }
        //Game speed
        if (Input.GetButtonDown("Speed Up")) {
            Game.Instance.Speed_Up();
        }
        if (Input.GetButtonDown("Speed Down")) {
            Game.Instance.Speed_Down();
        }
        if (Input.GetButtonDown("Pause")) {
            Game.Instance.Toggle_Pause();
        }
        //If shift is raised remove selected building
        if(Input.GetButtonUp("Build Multiple") && !MenuManager.Instance.Takes_Keyboard_Inputs) {
            BuildingPrototypes.Currently_Selected = null;
        }
        //Toggle alerts
        if (Input.GetButtonDown("Toggle Alerts")) {
            MenuManager.Instance.Show_Alerts = !MenuManager.Instance.Show_Alerts;
        }
        //Toggle highlights
        if (Input.GetButtonDown("Toggle Highlights")) {
            MenuManager.Instance.Show_Building_Ranges = !MenuManager.Instance.Show_Building_Ranges;
        }
        //Delete building
        if (Input.GetButtonDown("Demolish") && !ConsoleManager.Instance.Is_Open() && !MenuManager.Instance.Takes_Keyboard_Inputs) {
            MenuManager.Instance.Delete_Building();
        }
        //Toggle building pause
        if (Input.GetButtonDown("Pause Building") && !ConsoleManager.Instance.Is_Open() && !MenuManager.Instance.Takes_Keyboard_Inputs) {
            MenuManager.Instance.Toggle_Pause();
        }
        //Storage settings
        if (Input.GetButtonDown("Storage Settings") && !ConsoleManager.Instance.Is_Open() && !MenuManager.Instance.Takes_Keyboard_Inputs) {
            MenuManager.Instance.Show_Storage_Settings();
        }
        //Toggle workers
        if (Input.GetButtonDown("Toggle Workers P/C") && !ConsoleManager.Instance.Is_Open() && !MenuManager.Instance.Takes_Keyboard_Inputs) {
            MenuManager.Instance.Toggle_Workers_PC();
        }
        //Toggle workers
        if (Input.GetButtonDown("Toggle Workers C/N") && !ConsoleManager.Instance.Is_Open() && !MenuManager.Instance.Takes_Keyboard_Inputs) {
            MenuManager.Instance.Toggle_Workers_CN();
        }

        //Console
        if (Input.GetButtonDown("Statistics")) {
            StatisticsManager.Instance.Toggle_Panel();
        }
        if (Input.GetButtonDown("Console")) {
            ConsoleManager.Instance.Toggle_Console();
        }
        if (Input.GetButtonDown("Submit")) {
            ConsoleManager.Instance.Run_Command();
        }
        if (Input.GetButtonDown("Console History Up")) {
            ConsoleManager.Instance.Command_History_Up();
        }
        if (Input.GetButtonDown("Console History Down")) {
            ConsoleManager.Instance.Command_History_Down();
        }
        if (Input.GetButtonDown("Console Auto Complete")) {
            ConsoleManager.Instance.Auto_Complete();
        }
        if (Input.GetButtonDown("Console Scroll Up")) {
            ConsoleManager.Instance.Scroll_Up();
        }
        if (Input.GetButtonDown("Console Scroll Down")) {
            ConsoleManager.Instance.Scroll_Down();
        }
        
        if(!ConsoleManager.Instance.Is_Open() && !MenuManager.Instance.Takes_Keyboard_Inputs && Game.Instance.State == Game.GameState.RUNNING) {
            //Building Tabs
            if (Input.GetButtonDown("Admin Tab")) {
                MenuManager.Instance.Open_Tab(Building.UI_Category.ADMIN);
            }
            if (Input.GetButtonDown("Infrastructure Tab")) {
                MenuManager.Instance.Open_Tab(Building.UI_Category.INFRASTRUCTURE);
            }
            if (Input.GetButtonDown("Housing Tab")) {
                MenuManager.Instance.Open_Tab(Building.UI_Category.HOUSING);
            }
            if (Input.GetButtonDown("Services Tab")) {
                MenuManager.Instance.Open_Tab(Building.UI_Category.SERVICES);
            }
            if (Input.GetButtonDown("Forestry Tab")) {
                MenuManager.Instance.Open_Tab(Building.UI_Category.FORESTRY);
            }
            if (Input.GetButtonDown("Agriculture Tab")) {
                MenuManager.Instance.Open_Tab(Building.UI_Category.AGRICULTURE);
            }
            if (Input.GetButtonDown("Industry Tab")) {
                MenuManager.Instance.Open_Tab(Building.UI_Category.INDUSTRY);
            }
            //Buildings
            if(MenuManager.Instance.Open_Building_Tab != null) {
                if (Input.GetKeyDown(KeyCode.Alpha1)) {
                    MenuManager.Instance.Select_Building_At(0);
                }
                if (Input.GetKeyDown(KeyCode.Alpha2)) {
                    MenuManager.Instance.Select_Building_At(1);
                }
                if (Input.GetKeyDown(KeyCode.Alpha3)) {
                    MenuManager.Instance.Select_Building_At(2);
                }
                if (Input.GetKeyDown(KeyCode.Alpha4)) {
                    MenuManager.Instance.Select_Building_At(3);
                }
                if (Input.GetKeyDown(KeyCode.Alpha5)) {
                    MenuManager.Instance.Select_Building_At(4);
                }
                if (Input.GetKeyDown(KeyCode.Alpha6)) {
                    MenuManager.Instance.Select_Building_At(5);
                }
                if (Input.GetKeyDown(KeyCode.Alpha7)) {
                    MenuManager.Instance.Select_Building_At(6);
                }
                if (Input.GetKeyDown(KeyCode.Alpha8)) {
                    MenuManager.Instance.Select_Building_At(7);
                }
                if (Input.GetKeyDown(KeyCode.Alpha9)) {
                    MenuManager.Instance.Select_Building_At(8);
                }
                if (Input.GetKeyDown(KeyCode.Alpha0)) {
                    MenuManager.Instance.Select_Building_At(9);
                }
            }
        }

        //Overlays
        if (Input.GetButtonDown("Appeal Overlay")) {
            Game.Instance.Map.Set_Overlay(Map.Overlay.Appeal);
        }
        if (Input.GetButtonDown("Ore Overlay")) {
            Game.Instance.Map.Set_Overlay(Map.Overlay.Ore);
        }
    }
}
