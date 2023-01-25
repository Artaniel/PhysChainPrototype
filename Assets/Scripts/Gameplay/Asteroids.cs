using UnityEngine;
using System.Collections;

public class Asteroids : MonoBehaviour {	
	public GameObject asteroidPrototype;
	public int maxNumberOfAsteroids = 100;
	public float maxDistFromPlayer = 1000; // distance of simulation of spawners
	public Vector3 normalSpeed; // asteroids will move with simular velocities, it is norm of this speed
	public float speedFluctuation;// diviation of speed
	private GameObject[] asteroidBelt;
	public int currentNumberOfAsteroids = 0;
	public GameObject playerChar;
	public float visualRange = 100; // radius of part of field blocked from dispawn by player's view
	public int sectorsNumber = 8; 
	public float dencityGrowMultiplier = 0.1f;//gradientt of expected dencity per meter by Y axis, in asteroids on segment
	public float beltStartY = 0f;
	public float beltStartDencity = 0f;//dencity on Y = BeltStartY
	public float minSize = 0.1f;
	public float beltStartMaxSize = 0.5f;
	public float maxSizeGrowMultiplier = 0.01f;//per meter by Y axis
	
	void Start () {
		asteroidBelt = new GameObject[maxNumberOfAsteroids];
	}
	
	void Update () {
		CheckDistance();
	}
	
	void CheckDistance(){
		Vector3 playerPos = playerChar.transform.position;
		int[,] sectorContent = new int[sectorsNumber,currentNumberOfAsteroids+1];//first index is sector, 0 is center;
		Vector3 currentAsteroidPosition;
		int currentSector;
		int i = 0;
        while (i < currentNumberOfAsteroids) { 
			currentAsteroidPosition = asteroidBelt[i].transform.position;
			if (Vector3.Distance(currentAsteroidPosition, playerPos) > maxDistFromPlayer) { 
				KillAsteroid(i); // of too far - dispawn
				i--;
			}else{
				if (Vector3.Distance(currentAsteroidPosition, playerPos) > visualRange) { 
					currentSector = (int)(Vector3.Angle(Vector3.up, currentAsteroidPosition-playerPos )/360*sectorsNumber); // sector starting frou up dirrection
					if (currentAsteroidPosition.x < playerPos.x) // exception for angles over 180
						currentSector = sectorsNumber - currentSector-1;
					sectorContent[currentSector,0]++; // 0 position for temporary array size
					sectorContent[currentSector,sectorContent[currentSector,0]] = i; 
				}
			}
			i++;
		}

		float dencityInCurrentSector;
		float sectorMidY;
		for (i=0; i<sectorsNumber; i++){
			sectorMidY = (maxDistFromPlayer + visualRange) / 2f * Mathf.Cos((i + 0.5f) * 2 * Mathf.PI / sectorsNumber) + playerPos.y;
			if (sectorMidY > beltStartY)
				dencityInCurrentSector = (sectorMidY - beltStartY) * dencityGrowMultiplier + beltStartDencity;
			else dencityInCurrentSector = 0;
			if ((sectorContent[i, 0] > dencityInCurrentSector) && (sectorContent[i, 0] != 0))
				KillAsteroid(sectorContent[i, 1]); // if too much - dispawn
			else {
				if (sectorContent[i, 0] < (dencityInCurrentSector - 1)) // if not enough - add
					if (currentNumberOfAsteroids<maxNumberOfAsteroids)
						MakeAsteroidInSector(visualRange, maxDistFromPlayer, i * 360 / sectorsNumber, (i + 1) * 360 / sectorsNumber, currentNumberOfAsteroids);		
			}
		}
	}
	
	void KillAsteroid(int index){
		Destroy(asteroidBelt[index]);
		asteroidBelt[index] = asteroidBelt[currentNumberOfAsteroids-1];
		asteroidBelt[index].name = "Asteroid"+index.ToString();
		asteroidBelt[currentNumberOfAsteroids-1] = null;
		currentNumberOfAsteroids--;
	}
	
	public void KillAsteroidExternal(GameObject target){//to delete asteroid by another source
		int index = int.Parse(target.name.Substring(8));//8 - position of number in name of gameObject
		KillAsteroid(index);
	}
	
	void MakeAsteroidInSector(float MinRange, float MaxRange, float MinAngle, float MaxAngle, int index){
		float range = Random.value*(MaxRange-MinRange) + MinRange;
		float angle = Random.value*(MaxAngle-MinAngle) + MinAngle;
		Vector3 playerPos = playerChar.transform.position;
		Vector3 newAsteroidPosition = playerPos + new Vector3(range*Mathf.Sin(angle/180*Mathf.PI),range*Mathf.Cos(angle/180*Mathf.PI),0);
		float size = Random.value*((newAsteroidPosition.y-beltStartY)*maxSizeGrowMultiplier+beltStartMaxSize);
		if (size<minSize) 
			size  = minSize;
		if (Physics.OverlapSphere(newAsteroidPosition,size).Length == 0){//check on space availability, if space is occupyed - do nothibg
			asteroidBelt[index] = Instantiate(asteroidPrototype) as GameObject;
			asteroidBelt[index].transform.position = newAsteroidPosition;
			asteroidBelt[index].transform.parent = transform;
			asteroidBelt[index].GetComponent<Rigidbody>().velocity = normalSpeed + new Vector3(2*(Random.value-0.5f)*speedFluctuation,2*(Random.value-0.5f)*speedFluctuation,0);
			asteroidBelt[index].name = "Asteroid"+index.ToString();
			asteroidBelt[index].transform.localScale = new Vector3(size,size,size);
			asteroidBelt[index].GetComponent<Rigidbody>().mass = size*size*size*100;
			currentNumberOfAsteroids++;
		}
	}
	
}
