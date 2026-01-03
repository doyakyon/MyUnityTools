using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using System;

public class SeasonalTreeManager : UdonSharpBehaviour
{
    [Header("木の種類設定")]
    public GameObject[] springPrefabs;
    public GameObject[] summerPrefabs;
    public GameObject[] autumnPrefabs;
    public GameObject[] winterPrefabs;

    [Header("配置設定")]
    [Tooltip("地面と判定するレイヤー（DefaultやEnvironmentなど）")]
    public LayerMask groundLayer;

    [Tooltip("木を植える場所（マーカー）の親オブジェクトを指定")]
    public Transform markersParent; 
    
    [Tooltip("1フレームあたりに生成する木の数（30〜50推奨）")]
    public int treesPerFrame = 50;

    [Tooltip("埋め込み深さ")]
    public float sinkDepth = 0.1f;

    [Header("デバッグ")]
    [Range(0, 12)]
    public int debugMonth = 0;

    private int currentMarkerIndex = 0;
    private int totalMarkers = 0;
    private GameObject prefabToSpawn;
    
    // 生成した木を管理する配列（後で消すため）
    private GameObject[] spawnedTrees;

    void Start()
    {
        if (markersParent == null) return;

        totalMarkers = markersParent.childCount;
        spawnedTrees = new GameObject[totalMarkers];

        // 季節の決定
        DecideSeasonPrefab();

        // 生成プロセスの開始
        SendCustomEventDelayedFrames(nameof(SpawnBatch), 1);
    }

    public void DecideSeasonPrefab()
    {
        int m = DateTime.Now.Month;
        if (debugMonth > 0) m = debugMonth;

        int seasonType = 3; // 冬
        if (m >= 3 && m <= 5) seasonType = 0;
        else if (m >= 6 && m <= 8) seasonType = 1;
        else if (m >= 9 && m <= 11) seasonType = 2;

        if (seasonType == 0) prefabToSpawn = GetRandom(springPrefabs);
        else if (seasonType == 1) prefabToSpawn = GetRandom(summerPrefabs);
        else if (seasonType == 2) prefabToSpawn = GetRandom(autumnPrefabs);
        else prefabToSpawn = GetRandom(winterPrefabs);
    }

    // ★ここが重要：少しずつ生成するループ処理
    public void SpawnBatch()
    {
        if (prefabToSpawn == null) return;

        // 今回のフレームで植える本数
        int count = 0;

        while (count < treesPerFrame && currentMarkerIndex < totalMarkers)
        {
            Transform marker = markersParent.GetChild(currentMarkerIndex);
            
            // 位置決定ロジック (Raycast)
            Vector3 spawnPos = marker.position;
            RaycastHit hit;
            // マーカーの上空からRayを撃つ
            if (Physics.Raycast(marker.position + Vector3.up * 10.0f, Vector3.down, out hit, 50.0f, groundLayer))
            {
                spawnPos = hit.point;
            }

            // 生成
            GameObject tree = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
            
            // 高さ調整
            Vector3 finalPos = spawnPos;
            finalPos.y -= sinkDepth;
            tree.transform.position = finalPos;

            // 回転をランダムにする等の処理があればここへ
            tree.transform.rotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0);
            
            // 親を設定（整理のため）
            tree.transform.SetParent(transform);

            // 配列に保存
            spawnedTrees[currentMarkerIndex] = tree;

            currentMarkerIndex++;
            count++;
        }

        // まだ全部植え終わっていなければ、次のフレームで続きをやる
        if (currentMarkerIndex < totalMarkers)
        {
            SendCustomEventDelayedFrames(nameof(SpawnBatch), 1);
        }
    }

    private GameObject GetRandom(GameObject[] list)
    {
        if (list == null || list.Length == 0) return null;
        return list[UnityEngine.Random.Range(0, list.Length)];
    }
}