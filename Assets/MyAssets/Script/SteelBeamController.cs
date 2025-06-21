using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteelBeamController : MonoBehaviour {
	// 柱のタグ
	public string steelBeamTag = "SteelBeam";

	// 無効化された柱のリスト（後で再有効化できるよう保持）
	private List<GameObject> deactivatedSteelBeams = new List<GameObject>();

	// 何度も削除を試行しないようにするフラグ
	private bool isDeactivate = false;

	// シングルトンインスタンス
	private static SteelBeamController _instance;

	public static SteelBeamController Instance {
		get {
			if (_instance == null) {
				Debug.LogError("SteelBeamControllerのインスタンスがありません");
			}

			return _instance;
		}
	}

	private void Awake() {
		// シングルトン設定
		if (_instance != null && _instance != this) {
			Destroy(this.gameObject);
			return;
		}

		_instance = this;
	}

	// 柱を無効化するメソッド
	public void DeactivateSteelBeams() {
		if (isDeactivate) return;
		isDeactivate = true;
		GameObject[] steelBeams = GameObject.FindGameObjectsWithTag(steelBeamTag);
		deactivatedSteelBeams.Clear();

		foreach (GameObject steelBeam in steelBeams) {
			Debug.Log($"柱を無効化: {steelBeam.name}");
			deactivatedSteelBeams.Add(steelBeam);
			steelBeam.SetActive(false);
		}

		Debug.Log($"合計 {deactivatedSteelBeams.Count} 個の柱を無効化しました");
	}

	// 柱を再有効化するメソッド
	public void ReactivateSteelBeams() {
		foreach (GameObject steelBeam in deactivatedSteelBeams) {
			if (steelBeam != null) {
				steelBeam.SetActive(true);
				isDeactivate = false;
				Debug.Log($"柱を再有効化: {steelBeam.name}");
			}
		}

		Debug.Log($"合計 {deactivatedSteelBeams.Count} 個の柱を再有効化しました");
		deactivatedSteelBeams.Clear();
	}

	// 特定の柱が活性状態かを確認
	public bool IsSteelBeamsActive(GameObject steelBeam) {
		return steelBeam.activeSelf && !deactivatedSteelBeams.Contains(steelBeam);
	}
}