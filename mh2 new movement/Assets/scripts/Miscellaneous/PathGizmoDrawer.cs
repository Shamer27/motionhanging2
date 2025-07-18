using System.Collections.Generic;
using System.IO;
using UnityEngine;

[ExecuteInEditMode]
public class PathGizmoDrawer : MonoBehaviour
{
    public string pathFolder = "Paths";
    public Color[] colors = new Color[] {
        Color.red, Color.green, Color.blue, Color.yellow, Color.cyan, Color.magenta, Color.white
    };

    private List<List<Vector3>> allPaths = new List<List<Vector3>>();

    void OnEnable()
    {
        LoadAllPaths();
    }

    void LoadAllPaths()
    {
        allPaths.Clear();
        string fullPath = Path.Combine(Application.dataPath, pathFolder);

        if (!Directory.Exists(fullPath))
        {
            Debug.LogWarning("Path folder not found: " + fullPath);
            return;
        }

        string[] files = Directory.GetFiles(fullPath, "player_path_*.txt");
        foreach (string file in files)
        {
            List<Vector3> path = new List<Vector3>();
            string[] lines = File.ReadAllLines(file);
            foreach (string line in lines)
            {
                string[] parts = line.Split(',');
                if (parts.Length == 3 &&
                    float.TryParse(parts[0], out float x) &&
                    float.TryParse(parts[1], out float y) &&
                    float.TryParse(parts[2], out float z))
                {
                    path.Add(new Vector3(x, y, z));
                }
            }
            allPaths.Add(path);
        }
    }

    void OnDrawGizmos()
    {
        LoadAllPaths(); // Refresh each frame for live view

        for (int i = 0; i < allPaths.Count; i++)
        {
            List<Vector3> path = allPaths[i];
            Gizmos.color = colors[i % colors.Length];
            for (int j = 0; j < path.Count - 1; j++)
            {
                Gizmos.DrawLine(path[j], path[j + 1]);
            }
        }
    }
}
