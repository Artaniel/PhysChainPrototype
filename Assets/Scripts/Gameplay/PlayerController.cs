using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour { 
	
	private bool moveEnabled = true;
	public float enginePower = 1;
	public ChainController chain;
	private Vector3 cameraStartPosition;
	private Vector3 InputPositionUniversal;

	public static PlayerController instance = null;

	void Awake(){
		instance = this;
		cameraStartPosition = Camera.main.transform.localPosition;
	}
		
	void Update () {
		MouseInputUpdate();
		//TouchInputUpdate();
		Camera.main.transform.localPosition =  //camera following player with showing some forward space
			cameraStartPosition+new Vector3(Mathf.Atan(GetComponent<Rigidbody>().velocity.x/10)*2,Mathf.Atan(GetComponent<Rigidbody>().velocity.y/10)*2,0);
	}
	
	void MouseInputUpdate()
	{
		Vector3 deltaPosition = Input.mousePosition - new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
		if (Input.GetMouseButton(0) && moveEnabled) { 
			GetComponent<Rigidbody>().AddForce(deltaPosition * enginePower);
		}
		if (!Input.GetMouseButton(0)&& !moveEnabled) {
			moveEnabled = true;
			chain.LaunchChain(deltaPosition/20f);
		}
		if (Input.GetMouseButton(0)){
			Debug.DrawRay(transform.position, deltaPosition/100f);
		}
	}
	
	void TouchInputUpdate(){ // needs rework
		InputPositionUniversal = new Vector3(Input.touches[0].position.x,Input.touches[0].position.y,0);
		if (Input.touchCount>0 && moveEnabled){			
			GetComponent<Rigidbody>().AddForce((InputPositionUniversal - new Vector3(Screen.width/2 ,Screen.height/2, 0))*enginePower /100 + 40* new Vector3(Mathf.Atan(GetComponent<Rigidbody>().velocity.x/10)*2,Mathf.Atan(GetComponent<Rigidbody>().velocity.y/10)*2));//100 тут подобранный отфанаря параметр который позмоляет перевести из пикселов экрана в метры игрового пространства
		}
		if (Input.touchCount==0 && !moveEnabled){
			moveEnabled = true;
			chain.LaunchChain(InputPositionUniversal - new Vector3(Screen.width/2, Screen.height/2, 0)+ 80 * new Vector3(Mathf.Atan(GetComponent<Rigidbody>().velocity.x/10)*2,Mathf.Atan(GetComponent<Rigidbody>().velocity.y/10)*2));
		}
		if (Input.touchCount>0){
			Debug.DrawRay(transform.position,(InputPositionUniversal - new Vector3(Screen.width/2 ,Screen.height/2, 0)) /80+new Vector3(Mathf.Atan(GetComponent<Rigidbody>().velocity.x/10)*2,Mathf.Atan(GetComponent<Rigidbody>().velocity.y/10)*2) );
		}
	}
	
	
	void OnMouseDown(){ 
		if (chain.status == "start"){
			moveEnabled = false; 
		}
		if ((chain.status == "connected")||(chain.status == "solid")){
			chain.DisconnectHarpoon();
		}
	}

}