using UnityEngine;

public class FootRaycastChecker : MonoBehaviour {
	// 足の位置を指定
	public Transform foot1Transform;
	public Transform foot2Transform;

	// 無視するレイヤー（複数選択可能）
	public LayerMask ignoreLayerMask;

	// Rayの判定距離
	public float checkDistance = 2.0f;

	// 落下時のイベントを通知するためのデリゲート
	public delegate void FallEvent();

	public event FallEvent OnPlayerFall;

	// SteelBeamControllerへの参照
	private SteelBeamController _steelBeamController;

	// TownMovingUpControllerへの参照
	private TownMovingUpController townMovingUpController;

	private void Start() {
		// SteelBeamControllerの参照を取得
		_steelBeamController = SteelBeamController.Instance;
		if (_steelBeamController == null) {
			Debug.LogError("SteelBeamControllerが見つかりません。シーンに追加してください。");
		}

		// TownMovingUpControllerの参照を取得
		townMovingUpController = TownMovingUpController.Instance;
		if (townMovingUpController == null) {
			Debug.LogError("TownMovingUpControllerが見つかりません。シーンに追加してください。");
		}
	}

	// 更新処理
	private void Update() {
		// 両足が柱の上にあるか確認
		bool foot1OnPillar = IsOnSurface(foot1Transform);
		bool foot2OnPillar = IsOnSurface(foot2Transform);

		// 両足とも柱から外れた場合に落下を開始
		if (!foot1OnPillar && !foot2OnPillar) {
			TriggerFall();
		}
	}

	// 落下する場合の処理
	private void TriggerFall() {
		// SteelBeamControllerを通じて柱を無効化
		if (_steelBeamController != null) {
			_steelBeamController.DeactivateSteelBeams();
		}

		// townMovingUpControllerを通じて柱を無効化
		if (townMovingUpController != null) {
			townMovingUpController.StartMovingUp();
		}
	}

	// 指定されたTransformの下に柱があるかを判定するメソッド
	private bool IsOnSurface(Transform checkTransform) {
		if (checkTransform == null) {
			Debug.LogError("チェック用のTransformが設定されていません！");
			return true; // エラー時は落下しないようにtrueを返す
		}

		// 下方向にRayを発射
		Ray ray = new Ray(checkTransform.position, Vector3.down);

		// 無視するレイヤーを除外したレイヤーマスクを作成
		int layerMask = ~ignoreLayerMask.value;

		// Raycastでヒットしたかどうかを確認
		if (Physics.Raycast(ray, out RaycastHit hit, checkDistance, layerMask)) {
			// ヒットしたオブジェクトが柱かどうかを確認
			if (_steelBeamController != null && hit.collider.CompareTag(_steelBeamController.steelBeamTag)) {
				return true; // 柱に当たった
			}
		}

		return false; // 何も当たらないか、柱以外のものに当たった
	}
}