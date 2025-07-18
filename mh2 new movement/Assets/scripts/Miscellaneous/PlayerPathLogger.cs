using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PlayerPathLogger : MonoBehaviour
{
    public float spacing = 0.5f;
    private Vector3 lastPos;
    private List<Vector3> pathPoints = new List<Vector3>();
    private string filePath;

    private string baseDir;

    void Start()
    {
        baseDir = Path.Combine(Application.dataPath, "Paths");
        if (!Directory.Exists(baseDir))
            Directory.CreateDirectory(baseDir);

        filePath = GetNewPathFile();
        Debug.Log("Saving path to: " + filePath);

        lastPos = transform.position;
        pathPoints.Add(lastPos);
    }

    void Update()
    {
        if (Vector3.Distance(transform.position, lastPos) > spacing)
        {
            lastPos = transform.position;
            pathPoints.Add(lastPos);
            File.AppendAllText(filePath, $"{lastPos.x},{lastPos.y},{lastPos.z}\n");
        }
    }

    string GetNewPathFile()
    {
        int i = 1;
        string path;
        do
        {
            path = Path.Combine(baseDir, $"player_path_{i}.txt");
            i++;
        } while (File.Exists(path));

        return path;
    }
}
