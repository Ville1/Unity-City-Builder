using System.Collections.Generic;

public class TilePrototypes {
    private static Dictionary<string, Tile> prototypes;

    /// <summary>
    /// Initializes list of prototypes
    /// </summary>
    private static void Initialize()
    {
        prototypes = new Dictionary<string, Tile>();

        prototypes.Add("undefined", new Tile("???", "tile_undefined", false, 0.0f, 0.0f));

        prototypes.Add("grass", new Tile("Grass", "tile_grass", true, 0.0f, 0.0f));

        prototypes.Add("forest", new Tile("Forest", "tile_forest", false, 1.5f, 2.0f));
        prototypes["forest"].Yields.Add("wood_cutters_lodge", new Dictionary<string, float>()); //TODO?
        prototypes["forest"].Yields["wood_cutters_lodge"].Add("wood", 0.1f);

        prototypes.Add("sparse_forest", new Tile("Sparse Forest", "tile_sparse_forest", false, 1.0f, 2.0f));
        prototypes["sparse_forest"].Yields.Add("wood_cutters_lodge", new Dictionary<string, float>());
        prototypes["sparse_forest"].Yields["wood_cutters_lodge"].Add("wood", 0.05f);

        prototypes.Add("fertile_ground", new Tile("Fertile Ground", "tile_fertile_ground", true, 0.5f, 0.0f));

        prototypes.Add("hill", new Tile("Hill", "tile_hill", false, 0.025f, 5.0f));
    }

    /// <summary>
    /// Get tile prototype of specific type
    /// </summary>
    /// <param name="tile"></param>
    /// <returns></returns>
    public static Tile Get(string tile)
    {
        if(prototypes == null) {
            Initialize();
        }
        if (!prototypes.ContainsKey(tile)) {
            return prototypes["undefined"];
        }
        return prototypes[tile];
    }
}
