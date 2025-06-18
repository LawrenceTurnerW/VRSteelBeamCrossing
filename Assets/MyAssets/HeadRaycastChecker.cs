using UnityEngine;

public class HeadRaycastChecker : MonoBehaviour
{
    // 頭の位置を指定
    public Transform headTransform;

    // 無視するレイヤー（複数選択可能）
    public LayerMask ignoreLayerMask;

    // Rayの判定距離
    public float checkDistance = 2.0f;

    // 落下時のイベントを通知するためのデリゲート
    public delegate void FallEvent();
    public event FallEvent OnPlayerFall;

    // PillarControllerへの参照
    private PillarController pillarController;
    
    // TownMovingUpControllerへの参照
    private TownMovingUpController townMovingUpController;

    private void Start()
    {
        // PillarControllerの参照を取得
        pillarController = PillarController.Instance;
        if (pillarController == null)
        {
            Debug.LogError("PillarControllerが見つかりません。シーンに追加してください。");
        }
        
        // TownMovingUpControllerの参照を取得
        townMovingUpController = TownMovingUpController.Instance;
        if (townMovingUpController == null)
        {
            Debug.LogError("TownMovingUpControllerが見つかりません。シーンに追加してください。");
        }
    }

    // 更新処理
    private void Update()
    {
        // 頭の位置から下方向へのレイキャストで柱との接触を確認
        bool headRayHit = IsHeadAboveSurface();

        // レイキャストが何にも当たらなかった場合（柱から外れた）
        if (!headRayHit)
        {
            // 落下判定を有効化
            TriggerFall();
        }
    }

    // 落下する場合の処理
    private void TriggerFall()
    {
        // PillarControllerを通じて柱を無効化
        if (pillarController != null)
        {
            pillarController.DeactivatePillars();
        }
        // townMovingUpControllerを通じて柱を無効化
        if (townMovingUpController != null)
        {
            townMovingUpController.StartMovingUp();
        }
    }

    // 頭の下に地面や柱があるかを判定するメソッド
    private bool IsHeadAboveSurface()
    {
        if (headTransform == null)
        {
            Debug.LogError("頭のTransformが設定されていません！");
            return true;
        }

        // 下方向にRayを発射
        Ray ray = new Ray(headTransform.position, Vector3.down);

        // 無視するレイヤーを除外したレイヤーマスクを作成
        int layerMask = ~ignoreLayerMask.value; // ビット反転で指定レイヤーを除外

        // Raycastでヒットしたかどうかを確認（指定レイヤーは無視）
        if (Physics.Raycast(ray, out RaycastHit hit, checkDistance, layerMask))
        {
            // ヒットしたオブジェクトが柱かどうかを確認
            if (pillarController != null && hit.collider.CompareTag(pillarController.pillarTag))
            {
                return true; // 柱に当たった
            }
        }
        return false; // 何も当たらないか、柱以外のものに当たった
    }
}