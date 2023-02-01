using UnityEngine;
using System.Collections;

public class CloudController : MonoBehaviour {

	public float VisualRange = 50;
	public float SimulationRange = 200;
	public float Dencity = 1;
	public float MinDeapth = 20;
	public float MaxDeapth = 100;
	public GameObject CloudPrototype;
	public int MaxNumberOfClouds = 100;
	private int CurrentNumberOfClouds;
	private GameObject[] CloudsArray;
	private Vector3 CloudPosition;
	private Vector3 PlayerPos;
	private bool NeedToRespawn;
	private Vector3 RespawnPosition;
	
	void Start () {
		CloudsArray = new GameObject[MaxNumberOfClouds];
		CurrentNumberOfClouds = MaxNumberOfClouds;
		for (int i = 0; i<CurrentNumberOfClouds;i++){
			CloudPosition = new Vector3((Random.value*2-1)*SimulationRange,(Random.value*2-1)*SimulationRange,Random.value*(MaxDeapth-MinDeapth)+MinDeapth);
			CloudsArray[i] = Instantiate(CloudPrototype) as GameObject;
			CloudsArray[i].transform.position = CloudPosition;
			CloudsArray[i].transform.parent = transform;
		}
	}
	
	void Update () {
		PlayerPos = GameObject.Find("PlayerChar").transform.position;
		for (int i = 0; i<CurrentNumberOfClouds;i++){
			NeedToRespawn = false;
			if (CloudsArray[i].transform.position.x < PlayerPos.x - SimulationRange){
				NeedToRespawn = true;
				RespawnPosition = new Vector3( SimulationRange,(Random.value*2-1)*SimulationRange,Random.value*(MaxDeapth-MinDeapth)+MinDeapth);
			}
			else if (CloudsArray[i].transform.position.x > PlayerPos.x + SimulationRange){
					NeedToRespawn = true;
					RespawnPosition = new Vector3(- SimulationRange,(Random.value*2-1)*SimulationRange,Random.value*(MaxDeapth-MinDeapth)+MinDeapth);
				}
				else if (CloudsArray[i].transform.position.y < PlayerPos.y - SimulationRange){
						NeedToRespawn = true;
						RespawnPosition = new Vector3((Random.value*2-1)*SimulationRange, SimulationRange,Random.value*(MaxDeapth-MinDeapth)+MinDeapth);
					}
					else if (CloudsArray[i].transform.position.y > PlayerPos.y + SimulationRange){
							NeedToRespawn = true;
							RespawnPosition = new Vector3((Random.value*2-1)*SimulationRange, - SimulationRange,Random.value*(MaxDeapth-MinDeapth)+MinDeapth);
					}			
			if (NeedToRespawn){
				Destroy(CloudsArray[i]);
				CloudsArray[i] = Instantiate(CloudPrototype) as GameObject;
				CloudsArray[i].transform.position = PlayerPos + RespawnPosition;
				CloudsArray[i].transform.parent = transform;
			}
		}
	}
	
}
