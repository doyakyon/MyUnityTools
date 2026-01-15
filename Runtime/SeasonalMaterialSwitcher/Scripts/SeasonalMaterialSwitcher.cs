using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using System;
using UnityEditor.Callbacks;

public class SeasonalMaterialSwitcher : UdonSharpBehaviour
{
    [Header("対象のオブジェクト")]
    [Tooltip("あらかじめシーンに配置した親オブジェクト")]
    public Transform objectParent;

    [Header("季節ごとのマテリアル設定")]
    public Material[] springMaterials;
    public Material[] summerMaterials;
    public Material[] autumnMaterials;
    public Material[] winterMaterials;

    [Header("デバッグ")]
    [Range(0,12)]
    public int debugMonth = 0;

    void Start()
    {
        // 親オブジェクトが指定されていない場合は何もしない
        if(objectParent == null)return;

        // 季節を決定してマテリアルを設定
        Material[] targetMaterials = DecideSeasonMaterials();

        if(targetMaterials == null || targetMaterials.Length == 0)return;

        // 親オブジェクトの下にあるすべてのMeshRenderを取得する
        MeshRenderer[] objectRenders = objectParent.GetComponentsInChildren<MeshRenderer>();

        // すべてのオブジェクトのマテリアルを差し替える
        foreach (MeshRenderer mr in objectRenders)
        {
            // 配列の中からランダムに選ぶ
            int randomIndex = UnityEngine.Random.Range(0, targetMaterials.Length);
            
            // GPU InstancingをUnity側の各マテリアル設定で有効化しておいてください
            mr.sharedMaterial = targetMaterials[randomIndex];
        }
    }

    private Material[] DecideSeasonMaterials()
    {
        int m = DateTime.Now.Month;
        // month != 0 ならdebug
        if(debugMonth != 0) m = debugMonth;
        
        // 3-5月: 春, 6-8月: 夏, 9-11月: 秋, 12-2月: 冬
        if (3 <= m && m <= 5)return springMaterials;
        else if(6 <= m && m <= 8)return summerMaterials;
        else if(9 <= m && m <= 11)return autumnMaterials;
        else return winterMaterials;
    }
}
