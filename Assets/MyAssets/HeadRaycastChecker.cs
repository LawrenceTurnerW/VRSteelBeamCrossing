using UnityEngine;

public class HeadRaycastChecker : MonoBehaviour
{
    // 頭の位置を指定
    public Transform headTransform;

    // 判定する柱かつ落下時に削除するオブジェクトのタグ
    public string pillarTag = "Pillar";

    // 無視するレイヤー（複数選択可能）
    public LayerMask ignoreLayerMask;

    // 落下判定が一度でも有効になったかどうか
    private bool hasFallen = false;

    // Rayの判定距離
    public float checkDistance = 2.0f;

    // 落下時のイベントを通知するためのデリゲート
    public delegate void FallEvent();
    public event FallEvent OnPlayerFall;

    // 更新処理
    private void Update()
    {
        // すでに一度落下判定が発生していた場合は処理をスキップ
        if (hasFallen)
        {
            return;
        }

        // 頭の位置から下方向へのレイキャストで柱との接触を確認
        bool headRayHit = IsHeadAboveSurface();

        // レイキャストが何にも当たらなかった場合（空中に浮いている状態）
        if (!headRayHit)
        {
            // 落下フラグを設定
            hasFallen = true;
            Debug.Log("頭の下に何もありません。落下判定を有効にします。以降この判定は無効化されます。");

            // 落下イベントを通知
            if (OnPlayerFall != null)
            {
                OnPlayerFall.Invoke();
            }

            // 指定タグのオブジェクトをすべて削除
            DestroyObjectsWithTag(pillarTag);
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
            // ヒットしたオブジェクトが指定したタグを持っているか確認
            if (hit.collider.CompareTag(pillarTag))
            {
                return true; // 指定したタグを持つオブジェクトに当たった
            }
        }

        return false; // 何も当たらないか、指定したタグを持たないオブジェクトに当たった
    }

    // 特定のタグを持つすべてのオブジェクトを削除するメソッド
    private void DestroyObjectsWithTag(string tag)
    {
        GameObject[] objectsToDestroy = GameObject.FindGameObjectsWithTag(tag);

        foreach (GameObject obj in objectsToDestroy)
        {
            Debug.Log($"落下判定により '{tag}' タグのオブジェクトを削除: {obj.name}");
            Destroy(obj);
        }

        Debug.Log($"合計 {objectsToDestroy.Length} 個の '{tag}' タグのオブジェクトを削除しました");
    }
}