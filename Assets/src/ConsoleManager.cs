using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ConsoleManager : MonoBehaviour {
    private static int max_lines = 10;
    private static char echo_command_start = '[';
    private static char echo_command_end = ']';

    public static ConsoleManager Instance;
    public GameObject Panel;
    public Text Output;
    public InputField Input;

    private List<string> output_log;
    private int scroll_position;
    private delegate string Command(string[] arguments);
    private Dictionary<string, Command> commands;
    private List<string> command_history;
    private int command_history_index;

    // Use this for initialization
    private void Start () {
        if(Instance != null) {
            Logger.Instance.Error("Start called multiple times");
        }
        Instance = this;

        output_log = new List<string>();
        output_log.Add("Console!");
        scroll_position = 0;
        command_history = new List<string>();
        command_history_index = 0;

        commands = new Dictionary<string, Command>();
        commands.Add("exit", (string[] arguments) => {
            Close_Console();
            return "";
        });
        commands.Add("echo", (string[] arguments) => {
            if(arguments.Length == 1) {
                return "Missing argument";
            }
            StringBuilder builder = new StringBuilder();
            for(int i = 1; i < arguments.Length; i++) {
                builder.Append(arguments[i]);
                builder.Append(" ");
            }
            string full_input = builder.ToString();
            builder = new StringBuilder();
            StringBuilder command_builder = new StringBuilder();
            bool in_command = false;
            for(int i = 0; i < full_input.Length; i++) {
                if(full_input[i] == echo_command_start && !in_command) {
                    in_command = true;
                } else if(full_input[i] == echo_command_end && in_command) {
                    in_command = false;
                    if(command_builder.Length != 0) {
                        string[] parts = command_builder.ToString().Split(' ');
                        if (commands.ContainsKey(parts[0])) {
                            builder.Append(commands[parts[0]](parts));
                        } else {
                            builder.Append("Invalid command!");
                        }
                    }
                    command_builder = new StringBuilder();
                } else {
                    if (in_command) {
                        command_builder.Append(full_input[i]);
                    } else {
                        builder.Append(full_input[i]);
                    }
                }
            }

            return builder.ToString();
        });
        commands.Add("version", (string[] arguments) => {
            return ("Game: " + Game.VERSION + " Unity: " + Application.unityVersion);
        });
        commands.Add("kill", (string[] arguments) => {
            Application.Quit();
            return "";
        });
        commands.Add("add_resources_city", (string[] arguments) => {
            if(Game.Instance.State != Game.GameState.RUNNING) {
                return "Invalid game state: " + Game.Instance.State.ToString();
            }
            if (arguments.Length != 3) {
                return "Invalid number of arguments";
            }
            float amount = 0.0f;
            float.TryParse(arguments[1], out amount);
            float overflow = City.Instance.Store_Resources(arguments[2], amount);
            return "Added " + (amount - overflow) + " " + arguments[2];
        });
        commands.Add("add_resources_building", (string[] arguments) => {
            if (Game.Instance.State != Game.GameState.RUNNING) {
                return "Invalid game state: " + Game.Instance.State.ToString();
            }
            if (arguments.Length != 3) {
                return "Invalid number of arguments";
            }
            if(MenuManager.Instance.Get_Selected_Building() == null) {
                return "No selected building";
            }
            float amount = 0.0f;
            float.TryParse(arguments[1], out amount);
            float overflow = MenuManager.Instance.Get_Selected_Building().Add(arguments[2], amount);
            return "Added " + (amount - overflow) + " " + arguments[2];
        });
        commands.Add("set_grace_time", (string[] arguments) => {
            if (Game.Instance.State != Game.GameState.RUNNING) {
                return "Invalid game state: " + Game.Instance.State.ToString();
            }
            if (arguments.Length != 2) {
                return "Invalid number of arguments";
            }
            float time = 0.0f;
            float.TryParse(arguments[1], out time);
            City.Instance.Grace_Time = time;
            return "Grace time set to " + City.Instance.Grace_Time + " days";
        });
        commands.Add("get_grace_time", (string[] arguments) => {
            if (Game.Instance.State != Game.GameState.RUNNING) {
                return "Invalid game state: " + Game.Instance.State.ToString();
            }
            return "Grace time: " + City.Instance.Grace_Time + " Left: " + Mathf.Round(City.Instance.Grace_Time - City.Instance.Time);
        });
        commands.Add("add_service", (string[] arguments) => {
            if (Game.Instance.State != Game.GameState.RUNNING) {
                return "Invalid game state: " + Game.Instance.State.ToString();
            }
            if (arguments.Length != 4) {
                return "Invalid number of arguments";
            }
            if (MenuManager.Instance.Get_Selected_Building() == null) {
                return "No selected building";
            }
            float amount = 0.0f;
            float.TryParse(arguments[1], out amount);
            float quality = 0.0f;
            float.TryParse(arguments[2], out quality);
            float overflow = MenuManager.Instance.Get_Selected_Building().Serve(arguments[3], amount, quality);
            return "Type: " + arguments[3] + " amount: " + (amount - overflow) + " quality: " + quality;
        });
        commands.Add("clear_highlights", (string[] arguments) => {
            Game.Instance.Map.Clear_Highlights();
            return "Highlights cleared";
        });


        Update_Output();
        Panel.SetActive(false);
	}
	
	// Update is called once per frame
	private void Update () {
		
	}

    /// <summary>
    /// Opens console
    /// </summary>
    public void Open_Console()
    {
        if (Panel.activeSelf) {
            return;
        }
        Panel.SetActive(true);
        EventSystem.current.SetSelectedGameObject(Input.gameObject, null);
        Input.OnPointerClick(new PointerEventData(EventSystem.current));
    }

    /// <summary>
    /// Close console
    /// </summary>
    public void Close_Console()
    {
        if(Panel.activeSelf == false) {
            return;
        }
        Panel.SetActive(false);
    }

    /// <summary>
    /// Open / close console
    /// </summary>
    public void Toggle_Console()
    {
        if (Panel.activeSelf) {
            Close_Console();
        } else {
            Open_Console();
        }
    }

    /// <summary>
    /// Run command from program
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    public bool Run_Command(string command)
    {
        Input.text = command;
        return Run_Command(false);
    }

    /// <summary>
    /// Runs a command currently typed into Input
    /// </summary>
    /// <param name="add_to_history"></param>
    /// <returns></returns>
    public bool Run_Command(bool add_to_history = true)
    {
        if (Input.text == "") {
            return false;
        }
        string[] parts = Input.text.Split(' ');
        if (!commands.ContainsKey(parts[0])) {
            output_log.Add("Invalid command!");
        } else {
            if(add_to_history) {
                command_history.Insert(0, Input.text);
            }
            string log = commands[parts[0]](parts);
            if(log != "") {
                output_log.Add(log);
            }
            Input.text = "";
        }
        Update_Output();
        EventSystem.current.SetSelectedGameObject(Input.gameObject, null);
        Input.OnPointerClick(new PointerEventData(EventSystem.current));
        return true;
    }

    /// <summary>
    /// Scroll up
    /// </summary>
    /// <returns></returns>
    public bool Scroll_Up()
    {
        if (!Panel.activeSelf) {
            return false;
        }
        scroll_position++;
        if (output_log.Count - max_lines - 2 - scroll_position < 0) {
            scroll_position--;
        }
        Update_Output();
        return true;
    }

    /// <summary>
    /// Scroll down
    /// </summary>
    /// <returns></returns>
    public bool Scroll_Down()
    {
        if (!Panel.activeSelf) {
            return false;
        }
        scroll_position--;
        if(scroll_position < 0) {
            scroll_position = 0;
        }
        Update_Output();
        return true;
    }

    /// <summary>
    /// Scroll command history up
    /// </summary>
    /// <returns></returns>
    public bool Command_History_Up()
    {
        if (!Panel.activeSelf) {
            return false;
        }
        if(command_history.Count == 0) {
            return true;
        }
        command_history_index++;
        if(command_history_index > command_history.Count + 1) {
            command_history_index = command_history.Count + 1;
        }
        if(command_history.Count > command_history_index - 1) {
            Input.text = command_history[command_history_index - 1];
        } else {
            Input.text = "";
        }
        return true;
    }

    /// <summary>
    /// Scroll command history down
    /// </summary>
    /// <returns></returns>
    public bool Command_History_Down()
    {
        if (!Panel.activeSelf) {
            return false;
        }
        if (command_history.Count == 0) {
            return true;
        }
        command_history_index--;
        if (command_history_index < 0) {
            command_history_index = 0;
        }
        if (command_history.Count > command_history_index - 1 && command_history_index > 0) {
            Input.text = command_history[command_history_index - 1];
        } else {
            Input.text = "";
        }
        return true;
    }

    /// <summary>
    /// Try to autocomplete command name
    /// </summary>
    /// <returns></returns>
    public bool Auto_Complete()
    {
        if (!Panel.activeSelf) {
            return false;
        }
        if(Input.text == "") {
            return true;
        }

        string complete = "";
        foreach(KeyValuePair<string, Command> command in commands) {
            if (command.Key.StartsWith(Input.text)) {
                complete = command.Key;
                break;
            }
        }

        if (complete != "") {
            Input.text = complete;
        }

        return true;
    }

    /// <summary>
    /// Is console open?
    /// </summary>
    public bool Is_Open()
    {
        return Panel.activeSelf;
    }

    /// <summary>
    /// Updates console output field
    /// </summary>
    private void Update_Output()
    {
        StringBuilder builder = new StringBuilder();
        for(int i = 0; i < max_lines; i++) {
            if(output_log.Count > i - scroll_position) {
                builder.Insert(0, output_log[output_log.Count - i - 1 - scroll_position] + "\n");
            }
        }
        Output.text = builder.ToString();
        command_history_index = 0;
    }
}
