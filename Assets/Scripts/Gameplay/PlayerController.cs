using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour { // Скрипт для управления корабликом
	
	private bool moveEnabled = true;
	public float EnginePower = 1;
	public ChainController Chain;
	public Camera MainCamera;
	public GameObject Model;
	private Vector3 CameraStartPosition;
	private Vector3 InputPositionUniversal;
		
	void Awake(){
		CameraStartPosition = MainCamera.transform.localPosition;
		try{
			GameObject.Find("RealMessenger").GetComponent<Messenger>().GameInit();//пытаемся найти контейнер с установками иигры и пнуть его чтобы разложил переменные по объектам
		}catch{
			Debug.Log("Messenger not found. Run from wrong scene mb?");
		}
	}
		
	void Update () {
		MouseInputUpdate();
		//TouchInputUpdate();
		MainCamera.transform.localPosition =  CameraStartPosition+new Vector3(Mathf.Atan(GetComponent<Rigidbody>().velocity.x/10)*2,Mathf.Atan(GetComponent<Rigidbody>().velocity.y/10)*2,0);//тут залоджен эффект опережения камерой корабля игрока
		//MainCamera.transform.localRotation = new Quaternion(0,0,0,0);
		//MainCamera.transform.Rotate( new Vector3(-rigidbody.velocity.y*3,rigidbody.velocity.x*3,0));
		//Model.transform.LookAt(transform.position+rigidbody.velocity);
		//Quaternion ShipRotetion = new Quaternion(0,0,0,0);
		
//		Model.transform.localRotation = new Quaternion(0,0,0,0);
//		if (rigidbody.velocity.x != 0){
//			if (rigidbody.velocity.x >0){
//				Model.transform.RotateAroundLocal(Vector3.forward,-Mathf.Atan(-rigidbody.velocity.y/rigidbody.velocity.x));
//			}else{
//				Model.transform.RotateAroundLocal(Vector3.forward,Mathf.PI-Mathf.Atan(-rigidbody.velocity.y/rigidbody.velocity.x));
//			}
//		}		
	}
	
	void MouseInputUpdate(){// тут есть странные коэффициенты непонятной природы. Часть из них была подобрана вручную, выплывают гдето при переводе пикселов в метры
		if (Input.GetMouseButton(0) && moveEnabled) { // если можно ускорятся и кнопка нажата - ускоряемся
			GetComponent<Rigidbody>().AddForce((Input.mousePosition - new Vector3(Screen.width/2 ,Screen.height/2, 0))*EnginePower /100 + 40* new Vector3(Mathf.Atan(GetComponent<Rigidbody>().velocity.x/10)*2,Mathf.Atan(GetComponent<Rigidbody>().velocity.y/10)*2));//100 тут подобранный отфанаря параметр который позмоляет перевести из пикселов экрана в метры игрового пространства
		}
		if (!Input.GetMouseButton(0)&& !moveEnabled){// если мышку отпустили, а мы все еще натягиваем цепь
			moveEnabled = true;
			Chain.LaunchChain(Input.mousePosition - new Vector3(Screen.width/2, Screen.height/2, 0)+ 80 * new Vector3(Mathf.Atan(GetComponent<Rigidbody>().velocity.x/10)*2,Mathf.Atan(GetComponent<Rigidbody>().velocity.y/10)*2));
		}
		if (Input.GetMouseButton(0)){
			Debug.DrawRay(transform.position,(Input.mousePosition - new Vector3(Screen.width/2 ,Screen.height/2, 0)) /80+new Vector3(Mathf.Atan(GetComponent<Rigidbody>().velocity.x/10)*2,Mathf.Atan(GetComponent<Rigidbody>().velocity.y/10)*2) );
		}
	}
	
	void TouchInputUpdate(){
		InputPositionUniversal = new Vector3(Input.touches[0].position.x,Input.touches[0].position.y,0);
		if (Input.touchCount>0 && moveEnabled){			
			GetComponent<Rigidbody>().AddForce((InputPositionUniversal - new Vector3(Screen.width/2 ,Screen.height/2, 0))*EnginePower /100 + 40* new Vector3(Mathf.Atan(GetComponent<Rigidbody>().velocity.x/10)*2,Mathf.Atan(GetComponent<Rigidbody>().velocity.y/10)*2));//100 тут подобранный отфанаря параметр который позмоляет перевести из пикселов экрана в метры игрового пространства
		}
		if (Input.touchCount==0 && !moveEnabled){// если мышку отпустили, а мы все еще натягиваем цепь
			moveEnabled = true;
			Chain.LaunchChain(InputPositionUniversal - new Vector3(Screen.width/2, Screen.height/2, 0)+ 80 * new Vector3(Mathf.Atan(GetComponent<Rigidbody>().velocity.x/10)*2,Mathf.Atan(GetComponent<Rigidbody>().velocity.y/10)*2));
		}
		if (Input.touchCount>0){
			Debug.DrawRay(transform.position,(InputPositionUniversal - new Vector3(Screen.width/2 ,Screen.height/2, 0)) /80+new Vector3(Mathf.Atan(GetComponent<Rigidbody>().velocity.x/10)*2,Mathf.Atan(GetComponent<Rigidbody>().velocity.y/10)*2) );
		}
	}
	
	
	void OnMouseDown(){ // это клик непосредственно по объекту кораблю игрока
		if (Chain.status == "start"){
			moveEnabled = false; 
		}
		if ((Chain.status == "connected")||(Chain.status == "solid")){
			Chain.DisconnectHarpoon();
		}
	}

}