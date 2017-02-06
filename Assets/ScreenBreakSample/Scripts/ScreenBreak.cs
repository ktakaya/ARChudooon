using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// スクリーンが割れるようなトランジションを表示.
/// </summary>
public class ScreenBreak : MonoBehaviour {
	[SerializeField] private Animation breakAnim;
	[SerializeField] private List<Camera> cameras;
	[SerializeField] private RenderTexture renderTex;
	/// <summary>
	/// グレースケール色変化演出を使う場合、GrayScaleParametricシェーダーをセットする必要あり
	/// </summary>
	[SerializeField] private Material renderMaterial;
	[SerializeField] private Camera targetCamera;

	/// <summary>
	/// 破壊アニメーション名.
	/// </summary>
	public string breakAnimationName = "Break";
	/// <summary>
	/// 位置リセットアニメーション名.
	/// </summary>
	public string resetAnimationName = "Reset";
	/// <summary>
	/// 破壊アニメーションの再生速度.
	/// </summary>
	public float animationSpeed = 1f;
	/// <summary>
	/// 再生中かどうか.(true=再生中)
	/// </summary>
	public bool IsPlaying { get { return _playFlag; } }


	private bool _playFlag;
	private bool _targetCamEnabled;


	private Vector3 originPosition;
	private float shake_time;


	private AudioSource audioSource;
	public AudioClip sound01; 
	public AudioClip sound02; 


	void Awake () {
		if (renderTex == null) {
			renderTex = new RenderTexture(
				(int)(Screen.width),
				(int)(Screen.height), 24);
			renderTex.enableRandomWrite = false;
		}
		_playFlag = false;

		// グレースケールを戻す.
		if (renderMaterial.HasProperty("_Amount")) {
			renderMaterial.SetFloat("_Amount", 0f);
		}

		audioSource = gameObject.GetComponent<AudioSource>();

		gameObject.SetActive(false);
	}

	void Update () {
		if (shake_time > 0) {	
			breakAnim.transform.position = originPosition + UnityEngine.Random.insideUnitSphere;
			shake_time--;  
			if (shake_time == 0) {
				breakAnim.transform.position = originPosition;
			}
		}
	}

	/// <summary>
	/// スクリーン破壊演出の開始.
	/// </summary>
	public void Play(bool autoCollectCamera = true)
	{
		if (_playFlag) {
			return;
		}
		_playFlag = true;

		// 画面一杯に伸長する
		transform.localScale = new Vector3 (GetComponent<Camera>().aspect * (2 / 1.13f), 1, 1);

		// 画面を揺らす前の元座標を保存
		originPosition = breakAnim.transform.position;

		// カメラの自動取得
		if (autoCollectCamera) {
			cameras = new List<Camera>(FindObjectsOfType<Camera>());
		}

		// Depthを更新(最前面に表示)
		if (cameras != null)
		{
			float max_depth = 0f;
			foreach (var cam in cameras)
			{
				if (cam.depth > max_depth)
					max_depth = cam.depth;
			}
			targetCamera.depth = max_depth + 1f;
		}

		gameObject.SetActive(true);
		StartCoroutine(co_Play());
	}


	IEnumerator co_Play()
	{
		if (_playFlag == false) {
			yield break;
		}

		// カメラにレンダラテクスチャをセット.
		foreach (var cam in cameras) {
			cam.targetTexture = renderTex;
		}

		// 表示ON
		renderMaterial.mainTexture = renderTex;
		_targetCamEnabled = targetCamera.enabled;
		targetCamera.enabled = true;
		breakAnim.gameObject.SetActive(true);

		audioSource.PlayOneShot( sound01 );


		// 表示リセット.
		breakAnim[resetAnimationName].speed = 10f;
		breakAnim.Play(resetAnimationName);

		// 待機
		yield return new WaitForSeconds(0.5f);

		// 画面を揺らす
		shake_time = 120;
		yield return new WaitForSeconds(1f);

		// 音と共に再生
		audioSource.PlayOneShot( sound02 );
		breakAnim[breakAnimationName].speed = animationSpeed;
		breakAnim.Play(breakAnimationName);

		// 待機.
		yield return new WaitForSeconds(2.0f);

		// 後処理
		foreach (var cam in cameras) {
			cam.targetTexture = null;
		}

		_playFlag = false;
		gameObject.SetActive(false);
		targetCamera.enabled = _targetCamEnabled;
	}
}
