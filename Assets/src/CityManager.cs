using UnityEngine;

public class CityManager : MonoBehaviour {
    private static float go_update_intervals = 0.1f; // Seconds
    private static float go_update_cooldown = 0.0f;

    /// <summary>
    /// Initialization
    /// </summary>
    private void Start () { }
	
	/// <summary>
    /// Per frame update
    /// </summary>
	private void Update () {
        if(Game.Instance.State == Game.GameState.RUNNING) {
            City.Instance.Process(Time.deltaTime);
            //Check cooldown
            if (go_update_cooldown > 0.0f) {
                go_update_cooldown -= Time.deltaTime;
                return;
            }
            go_update_cooldown += go_update_intervals;
            Game.Instance.Map.Update_GOs();
        }
	}
}
