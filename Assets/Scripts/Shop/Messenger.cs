using UnityEngine;
using System.Collections;

public class Messenger : MonoBehaviour {//это контейнер для переноса переменных между сценами с механихмом распихивания этих самых переменных по объектам.

	
	public int EngineLvl;
	public int ArmorLvl;
	public int AvionicLvl;
	public int BeltNumber;
	public float Money;
	private GameObject PlayerChar;
	private DamageController damageController;
	private Asteroids asteroidSpawner;
	
	void Awake (){
		if (GameObject.Find("RealMessenger")== null){
			gameObject.name = "RealMessenger";
			DontDestroyOnLoad(gameObject);
		}else{
			GameObject.Find("RealMessenger").GetComponent<Messenger>().ReInitBase();// Мы вернулись на базу, пора раскладывать переменные по объектам
			Destroy(gameObject);// Если нашли настоящего, то я фальшивка и должен покончить с собой for the great justice
		}
	}
	
	public void GameInit(){
		PlayerChar = GameObject.Find("PlayerChar");
		damageController = PlayerChar.GetComponent<DamageController>();
		asteroidSpawner = GameObject.Find("AsteroidSpawner").GetComponent<Asteroids>();
		GameObject.Find("MoneyController").GetComponent<ScoreController>().PlayerScore = Money;
		SetupShip();
		SetAsteroidSpawner();
	}
	
	void SetupShip(){
		damageController.MaxEnginePower = 50+ 25* EngineLvl;//пока костыль, потом придумаю как вынести таблицу из кода чтобы редактировать ее без прблем
		PlayerChar.GetComponent<Rigidbody>().drag = 0.5f / (AvionicLvl+1);
		//damageController.DamageIgnoreMinimum = 10+10*ArmorLvl;
		damageController.PulseToHPMultiplier = 30f/(1+ArmorLvl);
	}
	
	void SetAsteroidSpawner(){
		if (BeltNumber == 0) {
			asteroidSpawner.NormalSpeed = new Vector3(1,0,0);
			asteroidSpawner.SpeedFluctuation = 0.2f;			
		}
		if (BeltNumber == 1){			
			asteroidSpawner.NormalSpeed = new Vector3(5,0,0);
			asteroidSpawner.SpeedFluctuation = 1f;	
		}
		if (BeltNumber == 2){
			
		}
	}
	
	public void GoHome(){
		Money = GameObject.Find("MoneyController").GetComponent<ScoreController>().PlayerScore;
		gameObject.name = "RealMessenger";//пока будем отличать настоящего от неочень по имени, может потом переделаю умнее както
	}
	
	public void ReInitBase(){
		GameObject.Find("WindowController").GetComponent<WindowControl>().messenger = gameObject.GetComponent<Messenger>();
		GameObject.Find("MoneyController").GetComponent<ScoreController>().AddScore(Money);
	}
	
}
