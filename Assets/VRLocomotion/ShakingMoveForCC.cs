using UnityEngine;

// このスクリプトは CharacterController がアタッチされていることを前提とします
[RequireComponent(typeof(CharacterController))]
public class BearLocomotionForCC : MonoBehaviour
{
    [Header("移動速度（低速・ペーシング）")]
    [Tooltip("左右交互に漕いだ時の移動速度倍率")]
    [SerializeField] private float pacingMoveSpeed = 1.5f;

    [Header("移動速度（高速・バウンド）")]
    [Tooltip("両手同時に漕いだ時の移動速度倍率")]
    [SerializeField] private float boundMoveSpeed = 4.0f;

    [Header("OVRカメラ（必須）")]
    [Tooltip("HMDの視点（OVRCameraRig内のCenterEyeAnchor）")]
    [SerializeField] private GameObject playerCamera;

    [Header("速度の閾値")]
    [Tooltip("このZ軸（前後）速度を超えたら移動として判定")]
    [SerializeField] private float speedThreshold = 0.1f;

    [Header("重力")]
    [Tooltip("キャラクターにかかる重力")]
    [SerializeField] private float gravity = -9.81f;

    private CharacterController characterController;
    private Vector3 verticalVelocity; // 重力計算用の速度

    private void Start()
    {
        characterController = GetComponent<CharacterController>();

        // 必須項目が設定されているかチェック
        if (playerCamera == null)
        {
            Debug.LogError("Mouse Camera（CenterEyeAnchorなど）が設定されていません。インスペクターから設定してください。", this);
        }
    }

    private void Update()
    {
        // 必須項目がなければ処理を中断
        if (playerCamera == null) return;

        // --- 1. 重力処理 ---
        ApplyGravity();

        // --- 2. 移動入力の計算 ---
        Vector3 moveDirection = Vector3.zero;

        // 右手と左手の（ローカル座標系での）Z軸速度（前後）を取得
        // ★★★ 修正点：.y (上下) ではなく .z (前後) を使用 ★★★
        float velocityR_Z = OVRInput.GetLocalControllerVelocity(OVRInput.Controller.RTouch).z;
        float velocityL_Z = OVRInput.GetLocalControllerVelocity(OVRInput.Controller.LTouch).z;

        // ★★★ 修正点：レポートに基づき、動作モードを判定 ★★★
        
        // 判定1：高速（バウンド）モード
        // 両手が同時に「前」に強く押し出されているか？
        bool isBounding = velocityR_Z > speedThreshold && velocityL_Z > speedThreshold;

        // 判定2：低速（ペーシング）モード
        // 両手が「逆方向」（交互）に動いているか？
        // (isBoundingではない前提で)
        bool isPacing = !isBounding && 
                        ( (velocityR_Z > speedThreshold && velocityL_Z < -speedThreshold) ||
                          (velocityR_Z < -speedThreshold && velocityL_Z > speedThreshold) );

        
        // --- 3. 移動ベクトルの計算 ---
        
        // 頭（カメラ）の向いている正面方向を取得（Y軸は無視）
        Vector3 forwardDirection = playerCamera.transform.forward;
        forwardDirection.y = 0;
        forwardDirection.Normalize();

        float currentSpeed = 0f;

        if (isBounding)
        {
            // 高速モード：両手のZ軸速度の平均で進む
            currentSpeed = ((velocityR_Z + velocityL_Z) / 2.0f) * boundMoveSpeed;
            moveDirection = forwardDirection * currentSpeed;
        }
        else if (isPacing)
        {
            // 低速モード：両手のZ軸速度の「絶対値」の平均で進む
            // (交互に動かす努力量を速度とする)
            currentSpeed = ((Mathf.Abs(velocityR_Z) + Mathf.Abs(velocityL_Z)) / 2.0f) * pacingMoveSpeed;
            moveDirection = forwardDirection * currentSpeed;
        }

        // --- 4. 移動の実行 (CharacterController.Move) ---
        // 水平移動(moveDirection) と 垂直移動(verticalVelocity) を合算してMoveに渡します
        characterController.Move((moveDirection + verticalVelocity) * Time.deltaTime);
    }

    /// <summary>
    /// 重力処理
    /// </summary>
    private void ApplyGravity()
    {
        if (characterController.isGrounded)
        {
            // 地面にいる時は重力速度をリセット
            verticalVelocity.y = -2f;
        }
        else
        {
            // 空中にいる時は重力を加算
            verticalVelocity.y += gravity * Time.deltaTime;
        }
    }
}