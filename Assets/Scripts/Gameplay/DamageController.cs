using UnityEngine;
using System.Collections;
using TMPro;

public class DamageController : MonoBehaviour {
	
	public float maxEnginePower = 50f;
	public float hitPoints = 100f; // in %
	public float damageIgnoreMinimum = 1f;
	public float pulseToHPMultiplier = 0.1f;
	public float regenDelay = 1; // in second
	public float regenSpeed = 10; // hp/sec
	private float regenDelayTimer;
	private PlayerController player;
	public GameObject propeller = null;
	public TextMeshProUGUI hpText;
	
	void Start () {
		player = gameObject.GetComponent<PlayerController>();
		//maxEnginePower = playerController.EnginePower;
		
	}
	
	void Update () {
		if (regenDelayTimer < regenDelay){
			regenDelayTimer += Time.deltaTime;
		}else{
			hitPoints += regenSpeed * Time.deltaTime;
			if (hitPoints>100) hitPoints = 100;
		}
		player.enginePower = maxEnginePower * hitPoints / 100f;
		if (propeller)
			propeller.transform.Rotate(Vector3.forward, Time.deltaTime * hitPoints * 180f / (10f * Mathf.PI));
		hpText.text = $"{(int)hitPoints}%hp";
	}
	
	void OnCollisionEnter(Collision collision){
		Debug.DrawRay(transform.position, Vector3.Project(collision.relativeVelocity,collision.contacts[0].normal));
		if (collision.collider.gameObject.name.Contains("Asteroid")){
			//Debug.Log("Hit "+(Vector3.Project(collision.relativeVelocity,collision.contacts[0].normal).magnitude*collision.collider.rigidbody.mass).ToString());
			float Damage = (
				Vector3.Project(collision.relativeVelocity, collision.contacts[0].normal).magnitude * collision.gameObject.transform.localScale.x)
				* pulseToHPMultiplier - damageIgnoreMinimum;
			if (Damage < 0) Damage = 0;
			Debug.Log("Damage "+Damage.ToString());
			hitPoints -= Damage;
			if (hitPoints<0) hitPoints = 0;
			regenDelayTimer = 0;
		}
	}
}
