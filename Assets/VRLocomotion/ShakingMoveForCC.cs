using UnityEngine;

// このスクリプトは CharacterController がアタッチされていることを前提とします
[RequireComponent(typeof(CharacterController))]
public class ShakingMoveForCC : MonoBehaviour
{
    [Header("移動速度")]
    [Tooltip("コントローラーを振った際の移動速度の倍率")]
    [SerializeField] private float moveSpeed = 2.0f;

    [Header("速度の閾値")]
    [Tooltip("このY軸速度を超えたら移動として判定")]
    [SerializeField] private float speedThreshold = 0.1f;

    [Header("重力")]
    [Tooltip("キャラクターにかかる重力")]
    [SerializeField] private float gravity = -9.81f;

    [Header("カメラ参照")]
    [Tooltip("CenterEyeAnchorなどのカメラTransformを設定")]
    [SerializeField] private Transform playerCamera;

    private CharacterController characterController;
    private Vector3 verticalVelocity; // 重力計算用の速度

    private void Start()
    {
        characterController = GetComponent<CharacterController>();

        // 必須項目が設定されているかチェック
        if (playerCamera == null)
        {
            Debug.LogError("Player Camera（CenterEyeAnchorなど）が設定されていません。インスペクターから設定してください。", this);
        }
    }

    private void Update()
    {
        // 必須項目がなければ処理を中断
        if (playerCamera == null) return; // ← OVRCameraRigのチェックを削除


        // ---1. 重力処理 ---
        if (characterController.isGrounded)
        {
            // 地面にいる時は重力速度をリセット（蓄積させない）
            verticalVelocity.y = -2f; // わずかに下向きの力をかけておくと安定します
        }
        else
        {
            // 空中にいる時は重力を加算
            verticalVelocity.y += gravity * Time.deltaTime;
        }

        // --- 2. 移動入力の計算 ---
        Vector3 moveDirection = Vector3.zero;

        // 右手と左手の（ローカル座標系での）Y軸速度を取得
        Vector3 velocityR = OVRInput.GetLocalControllerVelocity(OVRInput.Controller.RTouch);
        Vector3 velocityL = OVRInput.GetLocalControllerVelocity(OVRInput.Controller.LTouch);

        // Y軸方向の速度の絶対値を取得（上下に振る動きを検出）
        float speedR = Mathf.Abs(velocityR.y);
        float speedL = Mathf.Abs(velocityL.y);

        // どちらかの手の速度が閾値を超えた場合のみ
        if (speedR > speedThreshold || speedL > speedThreshold)
        {
            // 両手の速度を合計して移動速度を計算
            float totalSpeed = (speedR + speedL) * moveSpeed;

            // 頭（カメラ）の向いている正面方向を取得
            Transform headTransform = playerCamera.transform;
            Vector3 forwardDirection = headTransform.forward;

            forwardDirection.y = 0; // 水平移動のみ（上下を向いても前進する）
            forwardDirection.Normalize();

            // 移動ベクトルを計算
            moveDirection = forwardDirection * totalSpeed;
        }

        // --- 3. 移動の実行 (CharacterController.Move) ---
        // 水平移動(moveDirection) と 垂直移動(verticalVelocity) を合算してMoveに渡します
        // Time.deltaTimeを掛けてフレームレートに依存しない移動にします
        characterController.Move((moveDirection + verticalVelocity) * Time.deltaTime);
    }
}
