using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainView : MonoBehaviour {

	public ScreenBreak screenBreak;
	
	public Camera maincamera;
	WebCamTexture webcamTexture;

	float org_x = 0;
	float org_y = 0;
	float org_z = 0;
	int wait_time = 0;

	void Start(){
		//MainViewを画面いっぱいに広げる
		float _h = maincamera.orthographicSize * 2;
		float _w = _h * maincamera.aspect;

		transform.localScale = new Vector3 (_h, _w, 1);
		transform.localRotation *= Quaternion.Euler(0,0,-90);

		//カメラのテクスチャをMainViewに載せる
		Renderer rend = GetComponent<Renderer>();

		if(WebCamTexture.devices.Length > 0){
			webcamTexture = new WebCamTexture ();
			rend.material.mainTexture = webcamTexture;
			webcamTexture.Play();
		}
	}

	void Update () {
		float x = Input.acceleration.x + 1;
		float y = Input.acceleration.y + 1;
		float z = Input.acceleration.z + 1;

		if (wait_time > 60 && (
			//Input.anyKeyDown ||
			Mathf.Abs(org_x - x) > 0.5f || Mathf.Abs(org_y - y) > 0.5f || Mathf.Abs(org_z - z) > 0.5f)) {

			screenBreak.Play();
		}
		else {
			wait_time++;
		}
		org_x = x;
		org_y = y;
		org_z = z;
	}

	void OnApplicationPause(bool pauseStatus) {
		wait_time = 0;
	}

}
