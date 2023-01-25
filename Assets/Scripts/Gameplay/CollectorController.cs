using UnityEngine;
using System.Collections;

public class CollectorController : MonoBehaviour {
	
	//ToDo:
	//1. Надо перепродумать систему на более гибкую
	//2. Расширить и углубять. Астероиды не влазят самые жирные
	//3. Научить не прикусывать гарпун.
	
	public GameObject PlayerChar;
	public string status = "start";
	//"start" - закрыто
	//"opening" - закрыто
	//"open" - закрыто
	//"closing" - закрыто	
	public GameObject LeftDoor,RightDoor;
	public float OpeningRange;
	private float OpeningStage = 0f;//[0..1]
	public float OpeningStep = 0.01f;//[0..1] на каую долю дверь откроется за frame
	private Vector3 LeftDoorStartLocPos;
	public float CloseDist;//расстояние которое надо пройти двери чтобы закрыться
	private bool MoveEnabled = true;
	private GameObject NearAsteroid=null;// Астероид помеченный как тот который сейчас будем перерабатывать.
	public Asteroids AsteroidsController;
	public ScoreController scoreController;
	public GameObject GoHomeButton;
	
	void Start(){
		LeftDoorStartLocPos = LeftDoor.transform.localPosition;
	}
	
	void Update(){
		if (MoveEnabled){MoveUnderPlayer();}else{MoveSlowDown();}
		if (status == "start"){CheckDist();}
		if (status == "opening"){OperingUpdate();}
		if (status == "open") {CloseCheck();}
		if (status == "closing") {ClosingUpdate();}
		GoHomeButton.transform.position = transform.position + new Vector3(0,-2,-0.6f);
	}
	
	void MoveUnderPlayer(){
		GetComponent<Rigidbody>().velocity=new Vector3( PlayerChar.transform.position.x-transform.position.x ,0,0);
	}
	void MoveSlowDown(){
		GetComponent<Rigidbody>().velocity = 0.8f*GetComponent<Rigidbody>().velocity;
	}
	
	void CheckDist(){
		if((PlayerChar.transform.position.y-transform.position.y)<OpeningRange){
			status = "opening";
		}
	}
	
	void OperingUpdate(){
		if (OpeningStage >= 1){
			status = "open";
			OpeningStage = 1;
		}else{
			OpeningStage += OpeningStep;
		}
		LeftDoor.transform.localPosition = LeftDoorStartLocPos - new Vector3(CloseDist,0,0) * OpeningStage;//вообще кривой метод, сферичность стремится к нулю, надо будет переписать както
		RightDoor.transform.localPosition = LeftDoor.transform.localPosition - new Vector3(2* LeftDoor.transform.localPosition.x ,0,0);	//основано на том что LeftDoorX = - RightDoorX
	}
	
	void CloseCheck(){
		RaycastHit HitInfo;
		bool AsteroidDetected = false;
		for (int i = 0; i<7; i++){//срамной хардкод, стыдно должно быть, стукните меня по пальцам если это увидите прежде чем я его заменю на чтото более гибкое, фуфуфу
			Physics.Raycast(transform.position+2*Vector3.down+(i-3)*Vector3.right, Vector3.up, out HitInfo,8f);//константы надо вынести наружу
			Debug.DrawLine(transform.position+2*Vector3.down+(i-3)*Vector3.right, transform.position+2*Vector3.down+(i-3)*Vector3.right+8*Vector3.up);
			if (HitInfo.collider != null){// Нельзя смотреть с кем столкнулись пока не убедились что столкнулись
				if (HitInfo.collider.gameObject.name.Contains("Asteroid")){
					AsteroidDetected = true;//Это значит что один из наших лучей добра уткнулся в астероид. Значит он над коллектором и пора коллектор стопорить.
					NearAsteroid = HitInfo.collider.gameObject;
				}
			}
		}
		if(AsteroidDetected){MoveEnabled = false;}
		if (NearAsteroid!= null){
			Vector3 NearAsteroidPosition = NearAsteroid.transform.position;
			Vector3 PlayerPos = PlayerChar.transform.position;
			
			
			if (
					(NearAsteroidPosition.x>(transform.position.x-3f))&&
					(NearAsteroidPosition.x<(transform.position.x+3f))&&
					(NearAsteroidPosition.y<(transform.position.y+0.5f))&&
					(NearAsteroidPosition.y>(transform.position.y-1.5f))&&//вобщем если астероид внутри коробки. Могут быть проблемы изза касания крышки коробки, да и вообще размеров астероида
					(PlayerChar.GetComponent<ChainController>().Harpoon.GetComponent<CharacterJoint>()==null)&&//Надо упростить бы както... Вобщем чек не присоединен ли гарпун к комуто
					(//ну и чек нет ли игрока в коробке
						(PlayerPos.x<(transform.position.x-3f))||
						(PlayerPos.x>(transform.position.x+3f))||
						(PlayerPos.y>(transform.position.y+2f))||//добавлю сверху единицу чтобы не прищемляло крышкой
						(PlayerPos.y<(transform.position.y-1.5f))
					)
				){
				status = "closing";
				OpeningStage = 1;
			}
		}
		if((PlayerChar.transform.position.y-transform.position.y)>OpeningRange){
			status = "closing";
			OpeningStage = 1;
		}
	}
	
	void ClosingUpdate(){
		if (OpeningStage <= 0){ 
			status = "start";
			OpeningStage = 0;	
			MoveEnabled = true;
			//Destroy(NearAsteroid);
			if (NearAsteroid != null){
				scoreController.AddScore(NearAsteroid.GetComponent<Rigidbody>().mass);
				Vector3 NearAsteroidPosition = NearAsteroid.transform.position;
				if (
					(NearAsteroidPosition.x>(transform.position.x-3f))&&
					(NearAsteroidPosition.x<(transform.position.x+3f))&&
					(NearAsteroidPosition.y<(transform.position.y+1f))&&
					(NearAsteroidPosition.y>(transform.position.y-1.5f))
				){
					AsteroidsController.KillAsteroidExternal(NearAsteroid);
				}
			}
		}else{
			OpeningStage -= OpeningStep;
		}
		LeftDoor.transform.localPosition = LeftDoorStartLocPos - new Vector3(CloseDist,0,0) * OpeningStage;
		RightDoor.transform.localPosition = LeftDoor.transform.localPosition - new Vector3(2* LeftDoor.transform.localPosition.x ,0,0);
	}
	
}