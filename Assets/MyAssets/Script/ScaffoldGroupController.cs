// 足場のリストと切り替えロジックを保持する

using System.Collections.Generic;
using UnityEngine;

public class ScaffoldGroupController : MonoBehaviour
{
	[Tooltip("このコントローラーで有効/無効を切り替える足場のリスト")]
	public List<GameObject> targetScaffolds;

	// 登録された足場の有効/無効を切り替える
	public void ToggleScaffoldsActivity()
	{
		if (targetScaffolds == null || targetScaffolds.Count == 0)
		{
			Debug.LogWarning("対象の足場が設定されていません。");
			return;
		}

		// リストの最初の足場の状態を基準に、すべての足場の状態を統一して切り替える
		bool newState = !targetScaffolds[0].activeSelf;

		foreach (var scaffold in targetScaffolds)
		{
			if (scaffold != null)
			{
				scaffold.SetActive(newState);
			}
		}
	}
}