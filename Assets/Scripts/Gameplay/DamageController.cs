using UnityEngine;
using System.Collections;

public class DamageController : MonoBehaviour {
	
	public float MaxEnginePower = 50f;
	public float HitPoints = 100f;// в %
	public float DamageIgnoreMinimum = 1f;// потом будет зависить от брони, тот уровень урона который полностью поглощается
	public float PulseToHPMultiplier = 0.1f;// Коэффициент перевода относительного импульса (вообще там сложнее) в проценты hp
	public float RegenDelay = 1;//в сек
	public float RegenSpeed = 10;// hp/sec
	private float RegenDelayTimer;
	private PlayerController playerController;
	public GameObject Veslo;//чтобы греб быстрее... Вобщем пока примитивное отображение состояния движка, чем быстрее вертитися тем живее
	
	void Start () {
		playerController = gameObject.GetComponent<PlayerController>();
		//MaxEnginePower = playerController.EnginePower;// это только для начального значения
	}
	
	void Update () {
		if (RegenDelayTimer < RegenDelay){
			RegenDelayTimer += Time.deltaTime;
		}else{
			HitPoints += RegenSpeed * Time.deltaTime;
			if (HitPoints>100) HitPoints = 100;
		}
		playerController.enginePower = MaxEnginePower * HitPoints/100;
		Veslo.transform.Rotate(Vector3.forward, Time.deltaTime*HitPoints*180/(10*Mathf.PI));
	}
	
	void OnGUI(){
		GUI.Label(new Rect(5,5,100,20),((int)HitPoints).ToString()+"% hp");		
	}
	
	void OnCollisionEnter(Collision collision){
		Debug.DrawRay(transform.position, Vector3.Project(collision.relativeVelocity,collision.contacts[0].normal));
		if (collision.collider.gameObject.name.Contains("Asteroid")){
			//Debug.Log("Hit "+(Vector3.Project(collision.relativeVelocity,collision.contacts[0].normal).magnitude*collision.collider.rigidbody.mass).ToString());
			float Damage = (Vector3.Project(collision.relativeVelocity,collision.contacts[0].normal).magnitude*collision.gameObject.transform.localScale.x)*PulseToHPMultiplier-DamageIgnoreMinimum;
			if (Damage<0) Damage = 0;
			Debug.Log("Damage "+Damage.ToString());
			HitPoints -= Damage;
			if (HitPoints<0) HitPoints = 0;//может взрыв? подом подумаю
			RegenDelayTimer = 0;
		}
	}
}
