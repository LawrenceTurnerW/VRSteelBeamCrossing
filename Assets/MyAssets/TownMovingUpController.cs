using UnityEngine;

public class TownMovingUpController : MonoBehaviour
{
    // シングルトンインスタンス
    private static TownMovingUpController _instance;
    public static TownMovingUpController Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("TownMovingUpControllerのインスタンスがありません");
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        _instance = this;
    }
    public Transform townTransform;
    public float gravityAcceleration = 9.81f;
    public float maxHeight = 30f;

    private float currentSpeed = 0f;
    private bool isMovingUp = false;
    private float fallStartTime;
    private float currentHeight = 0f;
    private bool hasStartedMovingUp = false;

    // 外部から呼び出して上昇開始
    public void StartMovingUp()
    {
        if (hasStartedMovingUp) return;
        hasStartedMovingUp = true;
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
            if (currentHeight >= maxHeight)
            {
                isMovingUp = false;
                Debug.Log($"最大高さ({maxHeight}m)に達したため上昇を停止します");
                return;
            }

            float timeElapsed = Time.time - fallStartTime;
            currentSpeed = gravityAcceleration * timeElapsed;
            float distanceThisFrame = currentSpeed * Time.deltaTime;
            float remainingDistance = maxHeight - currentHeight;
            if (distanceThisFrame > remainingDistance)
            {
                distanceThisFrame = remainingDistance;
            }
            townTransform.Translate(Vector3.up * distanceThisFrame, Space.World);
            currentHeight += distanceThisFrame;

            if (timeElapsed % 1 < Time.deltaTime)
            {
                Debug.Log($"町の上昇: 経過時間={timeElapsed:F2}秒, 速度={currentSpeed:F2}m/s, 高さ={currentHeight:F2}m");
            }
        }
    }
}