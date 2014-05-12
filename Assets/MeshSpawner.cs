using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeshSpawner : MonoBehaviour {

	public GameObject meshGen;
	
	List<Color> lastColors;
	Color[] colors;
	

	// Use this for initialization
	void Start () {
		lastColors = new List<Color>();
		colors = new Color[8] {Color.black,
							   Color.blue,
							   Color.red,
							   Color.cyan,
							   Color.green,
							   Color.magenta,
							   Color.white,
							   Color.yellow};
	
		StartCoroutine(SpawnGenerators());
	}
	
	IEnumerator SpawnGenerators(){
		while (true){
			Color color = colors[Random.Range (0, colors.Length-1)];
			while (lastColors.Contains(color)){
				color = colors[Random.Range (0, colors.Length-1)];
				yield return null;
			}
			
			lastColors.Add(color);
			if (lastColors.Count > 4){
				lastColors.RemoveAt(0);
			}
			
			Vector3 newPos = new Vector3(Random.Range(-4.0f, 4.0f),
			                             Random.Range(-2.0f, 4.0f),
			                             Random.Range(-1.0f, 4.2f));
			
			var newGen = GameObject.Instantiate(meshGen, newPos, transform.localRotation) as GameObject;
			newGen.GetComponent<MeshGenerator>().Init(color);
			
			yield return new WaitForSeconds(Random.Range(.2f,.7f));
		}
	}
	
}
