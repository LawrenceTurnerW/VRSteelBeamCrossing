using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour {
	[Tooltip("操作対象のScaffoldGroupController")]
	public ScaffoldGroupController scaffoldGroupController;

	[Tooltip("使用するコントローラーのボタン")] public OVRInput.Button targetButton = OVRInput.Button.One;

	void Update() {
		// 指定されたボタンが押された瞬間を検知
		if (OVRInput.GetDown(targetButton)) {
			if (scaffoldGroupController != null) {
				scaffoldGroupController.ToggleScaffoldsActivity();
			}
			else {
				Debug.LogWarning("ScaffoldGroupControllerが設定されていません。");
			}
		}
	}
}　