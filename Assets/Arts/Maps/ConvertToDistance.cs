using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class OfflineDistanceFieldGenerator : EditorWindow
{
    public Texture2D landMask;
    public bool invertMask = false;
    public string outputFileName = "DistanceField.png";

    [MenuItem("Tools/Offline Distance Field Generator")]
    static void ShowWindow()
    {
        var window = GetWindow<OfflineDistanceFieldGenerator>("Distance Field Generator");
        window.Show();
    }

    void OnGUI()
    {
        landMask = (Texture2D)EditorGUILayout.ObjectField("Land Mask", landMask, typeof(Texture2D), false);
        invertMask = EditorGUILayout.Toggle("Invert Mask", invertMask);
        outputFileName = EditorGUILayout.TextField("Output File Name", outputFileName);

        if (GUILayout.Button("Generate Distance Field!!!"))
        {
            GenerateDistanceField();
        }
    }

    void GenerateDistanceField()
    {
        if (landMask == null)
        {
            Debug.LogError("Please assign a valid LandMask (black/white) first!");
            return;
        }

        int width = landMask.width;
        int height = landMask.height;
        Color[] pixels = landMask.GetPixels();

        int[] distances = new int[width * height];

        Queue<int> queue = new Queue<int>();

        int INF = 999999999;  // 标识“尚未更新”
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int idx = y * width + x;
                float val = pixels[idx].r; // 黑白图只关心R通道
                bool isLand = invertMask ? (val < 0.5f) : (val > 0.5f);
                if (isLand)
                {
                    distances[idx] = 0;
                    queue.Enqueue(idx); // 把陆地像素坐标压入队列
                }
                else
                {
                    distances[idx] = INF;
                }
            }
        }

        int[] offsetX = new int[] { 0, 1, 0, -1 };
        int[] offsetY = new int[] { -1, 0, 1, 0 };

        while (queue.Count > 0)
        {
            int idx = queue.Dequeue();
            int cx = idx % width;
            int cy = idx / width;
            int cDist = distances[idx];

            // 4方向邻居
            for (int i = 0; i < 4; i++)
            {
                int nx = cx + offsetX[i];
                int ny = cy + offsetY[i];
                if (nx < 0 || nx >= width || ny < 0 || ny >= height)
                    continue;

                int nIdx = ny * width + nx;
                int oldDist = distances[nIdx];
                int newDist = cDist + 1; // 邻居像素的距离=当前距离+1
                if (newDist < oldDist)
                {
                    distances[nIdx] = newDist;
                    queue.Enqueue(nIdx);
                }
            }
        }

        // 计算最大距离用来做归一化(把距离映射到0~1 区间)
        int maxDist = 0;
        for (int i = 0; i < distances.Length; i++)
        {
            if (distances[i] != INF && distances[i] > maxDist)
            {
                maxDist = distances[i];
            }
        }

        // 灰度图
        Texture2D distTex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int idx = y * width + x;
                int dist = distances[idx];
                float normDist = (dist == INF) ? 1.0f : (float)dist / (float)maxDist; 
                distTex.SetPixel(x, y, new Color(normDist, normDist, normDist, 1.0f));
            }
        }
        distTex.Apply();

        string folderPath = Path.Combine(Application.dataPath, "Arts/Maps/Textures");
        string savePath = Path.Combine(folderPath, outputFileName);

        byte[] pngData = distTex.EncodeToPNG();
        File.WriteAllBytes(savePath, pngData);
        AssetDatabase.Refresh();

        DestroyImmediate(distTex);

        Debug.Log("Distance Field generated and saved to: " + savePath);
    }
}
