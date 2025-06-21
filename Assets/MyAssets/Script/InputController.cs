using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour {
	[Tooltip("操作対象のSteelBeamGroupController")]
	public ToggleDebugSteelBeamController toggleDebugSteelBeamController;

	[Tooltip("操作対象のTownMovingUpController")]
	public TownMovingUpController townMovingUpController;
	
	[Tooltip("操作対象のSteelBeamController")]
	public SteelBeamController steelBeamController;

	[Tooltip("柱の消去に使用するコントローラーのボタン")] public OVRInput.Button steelBeamTargetButton = OVRInput.Button.One;
	[Tooltip("位置リセットに使用するコントローラーのボタン")] public OVRInput.Button ressetTargetButton = OVRInput.Button.Two;

	void Update() {
		// 指定されたボタンが押された瞬間を検知
		if (OVRInput.GetDown(steelBeamTargetButton)) {
			if (toggleDebugSteelBeamController != null) {
				toggleDebugSteelBeamController.ToggleSteelBeamsActivity();
			}
		}

		// 指定されたボタンが押された瞬間を検知
		if (OVRInput.GetDown(ressetTargetButton)) {
			if (townMovingUpController != null) {
				townMovingUpController.ResetPosition();
			}
			if (steelBeamController != null) {
				steelBeamController.ReactivateSteelBeams();
			}
		}
	}
}