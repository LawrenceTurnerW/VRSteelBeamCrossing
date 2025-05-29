using UnityEngine;

public class MoveUp : MonoBehaviour
{
    // 上昇させる町のオブジェクト
    public Transform townTransform;

    // 落下判定のスクリプト参照
    public HeadRaycastChecker headRaycastChecker;

    // 重力加速度（m/s²）- 物理的な重力加速度は約9.81
    public float gravityAcceleration = 9.81f;

    // 最大上昇高さ (m)
    public float maxHeight = 30f;

    // 上昇速度
    private float currentSpeed = 0f;

    // 上昇中かどうか
    private bool isMovingUp = false;

    // 上昇を開始した時間
    private float fallStartTime;

    // 現在の上昇高さ
    private float currentHeight = 0f;

    private void Start()
    {
        // HeadRaycastCheckerがない場合は探す
        if (headRaycastChecker == null)
        {
            headRaycastChecker = FindObjectOfType<HeadRaycastChecker>();
            if (headRaycastChecker == null)
            {
                Debug.LogError("HeadRaycastCheckerが見つかりません。手動で設定してください。");
                return;
            }
        }

        // 落下イベントを購読
        headRaycastChecker.OnPlayerFall += StartMovingUp;

        // すでに落下済みの場合は即座に上昇を開始
        if (headRaycastChecker.HasFallen)
        {
            StartMovingUp();
        }
    }

    private void OnDestroy()
    {
        // イベント購読を解除
        if (headRaycastChecker != null)
        {
            headRaycastChecker.OnPlayerFall -= StartMovingUp;
        }
    }

    private void StartMovingUp()
    {
        if (!isMovingUp && townTransform != null)
        {
            isMovingUp = true;
            currentSpeed = 0f;
            currentHeight = 0f;
            fallStartTime = Time.time;
            Debug.Log("町の上昇を開始します");
        }
    }

    private void Update()
    {
        if (isMovingUp && townTransform != null)
        {
            // 最大高さに達したら停止
            if (currentHeight >= maxHeight)
            {
                isMovingUp = false;
                Debug.Log($"最大高さ({maxHeight}m)に達したため上昇を停止します");
                return;
            }

            // 経過時間に基づいて加速
            float timeElapsed = Time.time - fallStartTime;

            // 重力加速度に基づいて上昇速度を計算（v = g * t）
            currentSpeed = gravityAcceleration * timeElapsed;

            // 1フレームあたりの移動距離を計算（距離 = 速度 * 時間）
            float distanceThisFrame = currentSpeed * Time.deltaTime;

            // 最大高さを超えないように調整
            float remainingDistance = maxHeight - currentHeight;
            if (distanceThisFrame > remainingDistance)
            {
                distanceThisFrame = remainingDistance;
            }

            // 町を上に移動させる
            townTransform.Translate(Vector3.up * distanceThisFrame, Space.World);

            // 累積高さを更新
            currentHeight += distanceThisFrame;

            // デバッグ情報
            if (timeElapsed % 1 < Time.deltaTime) // 約1秒ごとに表示
            {
                Debug.Log($"町の上昇: 経過時間={timeElapsed:F2}秒, 速度={currentSpeed:F2}m/s, 高さ={currentHeight:F2}m");
            }
        }
    }
}