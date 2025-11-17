# VR Locomotion - コントローラーを振って移動するシステム

このプロジェクトは、Meta Quest（Oculus）コントローラーを振ることで、VR空間内を移動できるロコモーションシステムです。
両手のコントローラーを上下に振る動作を検出し、その速度に応じてキャラクターが前進します。

## 特徴

- **自然な移動感**: コントローラーを振る動作で移動するため、実際に歩いているような感覚を体験できます
- **CharacterController対応**: Unity標準のCharacterControllerを使用し、重力処理や地形との衝突判定に対応
- **視線方向への移動**: HMD（ヘッドマウントディスプレイ）が向いている方向に移動します
- **カスタマイズ可能**: 移動速度、感度の閾値、重力の強さを調整できます

## 技術仕様

### ShakingMoveForCC スクリプト

#### 主な機能

1. **コントローラー速度検出**
   - `OVRInput.GetLocalControllerVelocity()`を使用して、左右のコントローラーのローカル座標系でのY軸速度を取得
   - 上下に振る動作を検出し、その速度の絶対値を計算

2. **移動方向の決定**
   - HMDのカメラ（CenterEyeAnchor）の正面方向を取得
   - Y軸成分を0にして水平方向のみに正規化
   - これにより、上下を向いても水平移動のみが行われます

3. **重力処理**
   - CharacterControllerの`isGrounded`プロパティで接地判定
   - 空中では重力加速度を加算し、自然な落下を実現
   - 地面にいる時は重力速度をリセット

4. **移動の実行**
   - 水平移動ベクトルと垂直移動ベクトル（重力）を合算
   - `CharacterController.Move()`で移動を実行
   - `Time.deltaTime`を使用してフレームレート非依存

## Unity エディタでのセットアップ方法

### 前提条件

- Unity 2021.3以降（推奨）
- Meta XR SDK（Oculus Integration）がインポートされていること
- Universal Render Pipeline（URP）設定済み（オプション）

### ステップ1: プロジェクトの準備

1. Unity Hubで新規プロジェクトを作成、または既存のVRプロジェクトを開きます
2. Package Managerから以下のパッケージをインストール:
   - XR Plugin Management
   - Meta All in One SDK

### ステップ2: OVRCameraRig の設置

1. **Hierarchy**ウィンドウで右クリック → **XR** → **OVR Camera Rig** を選択
2. もしくは、**Assets/Oculus/VR/Prefabs/OVRCameraRig** プレハブをシーンにドラッグ&ドロップ

### ステップ3: プレイヤーオブジェクトの作成

1. Hierarchyで空のGameObjectを作成（右クリック → **Create Empty**）
2. 名前を「VRPlayer」などに変更
3. **OVRCameraRig**を「VRPlayer」の子オブジェクトにドラッグ

### ステップ4: CharacterController の追加

1. **VRPlayer**オブジェクトを選択
2. **Inspector**ウィンドウで **Add Component** をクリック
3. 「Character Controller」と入力して、**Character Controller**コンポーネントを追加
4. CharacterControllerのパラメータを調整:
   - **Center**: (0, 1, 0) - キャラクターの中心位置
   - **Radius**: 0.3 - カプセルの半径
   - **Height**: 1.8 - キャラクターの高さ
   - **Skin Width**: 0.08 - 衝突判定の余白

### ステップ5: ShakingMoveForCC スクリプトの追加

1. **VRPlayer**オブジェクトを選択（CharacterControllerと同じオブジェクト）
2. **Inspector**で **Add Component** をクリック
3. 「ShakingMoveForCC」と入力して、スクリプトを追加

### ステップ6: Meta Controllerの設定

1. **ShakingMoveForCC**コンポーネントのインスペクターを確認
2. パラメータを調整:
   - **Move Speed**: 2.0〜5.0（お好みで調整）
   - **Speed Threshold**: 0.1〜0.5（感度の調整）
   - **Gravity**: -9.81（通常の重力）

3. **重要**: スクリプト内の`playerCamera`フィールドに参照を設定する必要があります
   - Hierarchyで **OVRCameraRig/TrackingSpace/CenterEyeAnchor** を探します
   - ShakingMoveForCCスクリプトに`[SerializeField] private Transform playerCamera;`フィールドを追加（コード修正が必要）
   - または、コード内で`Camera.main.transform`を使用するように修正

### ステップ7: 動作確認用の地面を作成

1. Hierarchyで右クリック → **3D Object** → **Plane** を作成
2. Planeの位置を(0, 0, 0)に設定
3. スケールを(10, 1, 10)などに設定して広い床を作成

### ステップ8: ビルド設定

1. **File** → **Build Settings** を開く
2. **Platform**を**Android**に変更（Meta Questの場合）
3. **Switch Platform**をクリック
4. **Player Settings**を開き、以下を設定:
   - **XR Plug-in Management** → **Android** → **Oculus**にチェック
   - **Minimum API Level**: Android 10.0以上
   - **Install Location**: Auto

### ステップ9: テスト実行

1. Meta Questをケーブルで接続（またはAir Link/Quest Linkを設定）
2. Quest本体で開発者モードを有効化
3. Unity エディタで **Play**ボタンを押すか、ビルドしてデバイスにインストール
4. VR内でコントローラーを上下に振ると、視線方向に移動します

## 実装の詳細

### コントローラー速度の取得

```csharp
// 右手と左手の（ローカル座標系での）Y軸速度を取得
Vector3 velocityR = OVRInput.GetLocalControllerVelocity(OVRInput.Controller.RTouch);
Vector3 velocityL = OVRInput.GetLocalControllerVelocity(OVRInput.Controller.LTouch);

// Y軸方向の速度の絶対値を取得（上下に振る動きを検出）
float speedR = Mathf.Abs(velocityR.y);
float speedL = Mathf.Abs(velocityL.y);
```

### 移動方向の計算

```csharp
// 頭（カメラ）の向いている正面方向を取得
Transform headTransform = playerCamera.transform;
Vector3 forwardDirection = headTransform.forward;
forwardDirection.y = 0; // 水平移動のみ（上下を向いても前進する）
forwardDirection.Normalize();

// 移動ベクトルを計算
float totalSpeed = (speedR + speedL) * moveSpeed;
moveDirection = forwardDirection * totalSpeed;
```

### 重力と移動の統合

```csharp
// 空中にいる時は重力を加算
if (!characterController.isGrounded)
{
    verticalVelocity.y += gravity * Time.deltaTime;
}

// 水平移動と垂直移動を合算
characterController.Move((moveDirection + verticalVelocity) * Time.deltaTime);
```

## InspectorでのMeta Controllerセットアップ

### playerCameraフィールドの設定（重要）

1. **VRPlayer**オブジェクトの**ShakingMoveForCC**コンポーネントを選択
2. Inspectorで**Player Camera**フィールドを探す
3. Hierarchyから **OVRCameraRig → TrackingSpace → CenterEyeAnchor** をドラッグ&ドロップ

![設定イメージ]
```
VRPlayer
├─ Character Controller
├─ Shaking Move For CC
│   ├─ Move Speed: 2.0
│   ├─ Speed Threshold: 0.1
│   ├─ Gravity: -9.81
│   └─ Player Camera: CenterEyeAnchor  ← ここに設定
└─ OVRCameraRig
    └─ TrackingSpace
        └─ CenterEyeAnchor  ← これをドラッグ
```

### パラメータの調整例

- **遅い移動**: Move Speed = 1.0〜2.0
- **標準的な移動**: Move Speed = 2.0〜3.0
- **速い移動**: Move Speed = 4.0〜6.0

- **鈍感な検出**: Speed Threshold = 0.3〜0.5
- **標準的な検出**: Speed Threshold = 0.1〜0.2
- **敏感な検出**: Speed Threshold = 0.05〜0.1

## トラブルシューティング

### 移動しない場合

- コントローラーが正しく認識されているか確認
- Speed Thresholdの値を下げてみる（0.05など）
- Unityのコンソールでエラーメッセージを確認

### カメラのエラーが出る場合

- `playerCamera`フィールドにCenterEyeAnchorが設定されているか確認
- OVRCameraRigが正しくシーンに配置されているか確認

### 地面に埋まる・落下する場合

- CharacterControllerのCenter, Height, Radiusを調整
- 地面のColliderが正しく設定されているか確認
- OVRCameraRigのY位置を調整（通常は0）

## カスタマイズのヒント

### 移動速度を動的に変更

```csharp
// 走るボタンを押している間は速度を2倍に
if (OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch))
{
    totalSpeed *= 2.0f;
}
```

### 移動方向を制限

```csharp
// 前進のみ許可（後退しない）
if (Vector3.Dot(forwardDirection, headTransform.forward) < 0)
{
    moveDirection = Vector3.zero;
}
```

### エフェクトの追加

移動時にパーティクルエフェクトや足音を再生することで、より没入感を高められます。

## ライセンス

このプロジェクトはMIT Licenseのもとで公開されています。

## 参考リンク

- [Meta Quest Developer Documentation](https://developer.oculus.com/documentation/)
- [Unity CharacterController Reference](https://docs.unity3d.com/ScriptReference/CharacterController.html)
- [OVRInput API Reference](https://developer.oculus.com/documentation/unity/unity-ovrinput/)

---

**作成日**: 2025年11月17日

