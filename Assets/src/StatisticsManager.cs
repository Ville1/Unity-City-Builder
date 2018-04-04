using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class StatisticsManager : MonoBehaviour {
    public static StatisticsManager Instance;
    private static float update_cooldown = 1.0f; //Seconds
    private static float time_since_update = 1.0f;

    private int resource_index;

    public GameObject Panel;
    public Text Cash_Text;
    public Text Food_Text;
    public Text Resources_Text;

    public float Cash;
    public float Income;
    public float Expenditures;

    public float Food_Current;
    public float Food_Max;
    public float Food_Produced;
    public float Food_Consumed;

    /// <summary>
    /// Initialization
    /// </summary>
	private void Start () {
		if(Instance != null) {
            Logger.Instance.Error("Start called multiple times");
        }
        Instance = this;
        Panel.SetActive(false);
        resource_index = 0;
	}
	
    /// <summary>
    /// Once per frame
    /// </summary>
	private void Update () {
        if (Panel.activeSelf) {
            time_since_update -= Time.deltaTime;
            if(time_since_update <= 0.0f) {
                Update_Panel();
                time_since_update += update_cooldown;
            }
        }
	}

    /// <summary>
    /// Open / close statistics panel
    /// </summary>
    public void Toggle_Panel()
    {
        if (Panel.activeSelf) {
            Close_Panel();
        } else {
            Open_Panel();
        }
    }

    /// <summary>
    /// Open statistics panel
    /// </summary>
    public void Open_Panel()
    {
        if (Panel.activeSelf || Game.Instance.State != Game.GameState.RUNNING) {
            return;
        }
        Update_Panel();
        Panel.SetActive(true);
    }

    /// <summary>
    /// Close statistics panel
    /// </summary>
    public void Close_Panel()
    {
        if (!Panel.activeSelf) {
            return;
        }
        Panel.SetActive(false);
    }

    /// <summary>
    /// Scrolls resource list up
    /// </summary>
    public void Resource_List_Up()
    {
        if (!Panel.activeSelf) {
            return;
        }
        resource_index--;
        if (resource_index < 0) {
            resource_index = 0;
        }
    }

    /// <summary>
    /// Scrolls resource list down
    /// </summary>
    public void Resource_List_Down()
    {
        if (!Panel.activeSelf) {
            return;
        }
        resource_index++;
    }

    /// <summary>
    /// Updates data shown on panel
    /// </summary>
    public void Update_Panel()
    {
        //Cash
        StringBuilder cash_text = new StringBuilder();
        float cash_flow = Income - (-1.0f * Expenditures);
        cash_text.Append(Math.Round(Cash, 1));
        if(cash_flow < 0.0f) {
            cash_text.Append(" ");
        } else {
            cash_text.Append(" +");
        }
        cash_text.Append(Math.Round(cash_flow, 1));
        cash_text.Append("\nIncome: ");
        cash_text.Append(Math.Round(Income, 1));
        cash_text.Append("\nExpend.: ");
        cash_text.Append(Math.Round(Expenditures, 1));
        Cash_Text.text = cash_text.ToString();

        //Food
        StringBuilder food_text = new StringBuilder();
        float food_balance = Food_Produced - Food_Consumed;
        food_text.Append(Math.Round(Food_Current, 1));
        food_text.Append(" / ");
        food_text.Append(Math.Round(Food_Max, 1));
        if (food_balance < 0.0f) {
            food_text.Append(" ");
        } else {
            food_text.Append(" +");
        }
        food_text.Append(Math.Round(food_balance, 1));
        food_text.Append("\nProduced: ");
        food_text.Append(Math.Round(Food_Produced, 1));
        food_text.Append("\nConsumed.: ");
        food_text.Append(Math.Round(Food_Consumed, 1));
        Food_Text.text = food_text.ToString();

        //Resources
        int max_rows = 14;
        int row = 0;
        StringBuilder data = new StringBuilder();
        foreach(KeyValuePair<string, float[]> resource_data in City.Instance.Resource_Stats) {
            if(row >= resource_index) {
                data.Append(resource_data.Key);
                data.Append(" ");
                data.Append(Math.Round(resource_data.Value[0]));
                data.Append(" / ");
                data.Append(resource_data.Value[1]);
                if (resource_data.Value[2] >= 0.0f) {
                    data.Append(" +");
                } else {
                    data.Append(" ");
                }
                data.Append(Math.Round(resource_data.Value[2], 2));
                data.Append("\n");
            }
            row++;
            if (row > max_rows + resource_index) {
                break;
            }
        }
        Resources_Text.text = data.ToString();
    }
}
