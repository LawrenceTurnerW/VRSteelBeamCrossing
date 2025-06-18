using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PillarController : MonoBehaviour
{
    // 柱のタグ
    public string pillarTag = "Pillar";

    // 無効化された柱のリスト（後で再有効化できるよう保持）
    private List<GameObject> deactivatedPillars = new List<GameObject>();
    // 何度も削除を試行しないようにするフラグ
    private bool isDeactivate = false;

    // シングルトンインスタンス
    private static PillarController _instance;
    public static PillarController Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("PillarControllerのインスタンスがありません");
            }
            return _instance;
        }
    }

    private void Awake()
    {
        // シングルトン設定
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        _instance = this;
    }

    // 柱を無効化するメソッド
    public void DeactivatePillars()
    {
        if (isDeactivate) return;
        isDeactivate = true;
        GameObject[] pillars = GameObject.FindGameObjectsWithTag(pillarTag);
        deactivatedPillars.Clear();

        foreach (GameObject pillar in pillars)
        {
            Debug.Log($"柱を無効化: {pillar.name}");
            deactivatedPillars.Add(pillar);
            pillar.SetActive(false);
        }

        Debug.Log($"合計 {deactivatedPillars.Count} 個の柱を無効化しました");
    }

    // 柱を再有効化するメソッド
    public void ReactivatePillars()
    {
        foreach (GameObject pillar in deactivatedPillars)
        {
            if (pillar != null)
            {
                pillar.SetActive(true);
                Debug.Log($"柱を再有効化: {pillar.name}");
            }
        }

        Debug.Log($"合計 {deactivatedPillars.Count} 個の柱を再有効化しました");
        deactivatedPillars.Clear();
    }

    // 特定の柱が活性状態かを確認
    public bool IsPillarActive(GameObject pillar)
    {
        return pillar.activeSelf && !deactivatedPillars.Contains(pillar);
    }
}
