using System.IO;
using UnityEngine;

namespace ArtificialLife
{
    public static class SaveSystem
    {
        public static string Path =>
            System.IO.Path.Combine(Application.persistentDataPath, "population.json");

        public static void Save(PopulationSave data)
        {
            File.WriteAllText(Path, JsonUtility.ToJson(data, true));
            Debug.Log($"[Save] generation {data.generation}, {data.seedGenomes.Count} genomes -> {Path}");
        }

        public static PopulationSave Load()
        {
            if (!File.Exists(Path)) return null;
            var data = JsonUtility.FromJson<PopulationSave>(File.ReadAllText(Path));
            Debug.Log($"[Save] resumed generation {data.generation} from {Path}");
            return data;
        }

        public static void Delete()
        {
            if (File.Exists(Path)) File.Delete(Path);
        }
    }
}
