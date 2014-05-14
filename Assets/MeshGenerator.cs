using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class MeshGenerator : MonoBehaviour {

	GameObject meshObject;
	List<Vector3> baseVertices;
	int[] triangles;
	
	int[] currentFaces;

	int meshCount;
	
	List<VecPair> interpolationPoints;
	
	Vector3 RandomVert(Vector3 vert){
		float bound = .5f;
		return new Vector3(vert.x + Random.Range(-bound, bound),
		                   vert.y + Random.Range(-bound, bound),
		                   vert.z + Random.Range(-bound, bound));
	
	}
	
	public IEnumerator Init(Color color) {
		Random.seed = gameObject.GetInstanceID();
		meshCount = Random.Range(20,30);
		
		interpolationPoints = new List<VecPair>();
		
		baseVertices = new List<Vector3>();
		baseVertices.Add(new Vector3(transform.position.x - 1, transform.position.y + 0, transform.position.z - 1));
		interpolationPoints.Add (new VecPair(baseVertices[baseVertices.Count-1],
											 RandomVert(new Vector3(transform.position.x - 1, transform.position.y + 0, transform.position.z - 1))
											 )
								);
		
		baseVertices.Add(new Vector3(transform.position.x + 1, transform.position.y + 0, transform.position.z - 1));
		interpolationPoints.Add (new VecPair(baseVertices[baseVertices.Count-1],
										     RandomVert(new Vector3(transform.position.x + 1, transform.position.y + 0, transform.position.z - 1))
										     )
								);
		
		baseVertices.Add(new Vector3(transform.position.x + 0, transform.position.y + 0, transform.position.z + 1));
		interpolationPoints.Add (new VecPair(baseVertices[baseVertices.Count-1],
											 RandomVert(new Vector3(transform.position.x + 0, transform.position.y + 0, transform.position.z + 1))
											 )
								);
		
		baseVertices.Add(new Vector3(transform.position.x + 0, transform.position.y + 2, transform.position.z + 0));
		interpolationPoints.Add (new VecPair(baseVertices[baseVertices.Count-1],
		                                     RandomVert(new Vector3(transform.position.x + 0, transform.position.y + 2, transform.position.z + 0))
		                                     )
		                         );
		
		triangles = new int[12];
		
		triangles[0] = 0;
		triangles[1] = 3;
		triangles[2] = 2;
		
		triangles[3] = 0;
		triangles[4] = 1;
		triangles[5] = 3;
		
		triangles[6] = 1;
		triangles[7] = 2;
		triangles[8] = 3;
		
		triangles[9] = 0;
		triangles[10] = 1;
		triangles[11] = 2;
		
		currentFaces = new int[triangles.Length];
		System.Array.Copy(triangles, currentFaces, triangles.Length);
		
		List<Vector3> normals = new List<Vector3>();
		normals.Add(-Vector3.forward);
		normals.Add(-Vector3.forward);
		normals.Add(-Vector3.forward);
		normals.Add(-Vector3.forward);
		
		
		meshObject = new GameObject();
		meshObject.AddComponent("MeshFilter");
		meshObject.AddComponent("MeshRenderer");
		meshObject.AddComponent("MeshCollider");
		var mesh = meshObject.GetComponent<MeshFilter>().mesh;
		mesh.vertices = baseVertices.ToArray();
		mesh.triangles = triangles;
		mesh.normals = normals.ToArray();
		
		meshObject.renderer.material = new Material(Shader.Find ("Particles/Alpha Blended"));
		color.a = 0f;
		meshObject.renderer.material.SetColor("_TintColor", color);
		
		StartCoroutine(FadeIn(color));
		StartCoroutine(GenerateMesh());
		
		yield break;
	}
	
	IEnumerator FadeIn(Color color){
		while (color.a < .15f){
			color.a += .01f;
			meshObject.renderer.material.SetColor("_TintColor", color);
			yield return null;
		}
	
	}
	
	bool CheckIfPointHitsMesh(Vector3 face){
		Vector3 pointA = baseVertices[(int)face[0]],
				pointB = baseVertices[(int)face[1]],
				pointC = baseVertices[(int)face[2]];
		
		Vector3 midpoint = new Vector3(Avg(pointA.x, pointB.x, pointC.x),
		                               Avg(pointA.y, pointB.y, pointC.y),
		                               Avg(pointA.z, pointB.z, pointC.z));
		
		Vector3 perp = Vector3.Cross(pointC - pointA, pointB - pointA).normalized;
		
		Ray ray = new Ray(midpoint, perp);
		RaycastHit outHit;
		return !Physics.Raycast(ray, out outHit);
	}
	
	Vector3 PickFace(){
		List<int> faceCopy = currentFaces.ToList();
		
		bool viable = false;
		int face = 0;
		while (!viable){
			face = Random.Range (0,faceCopy.Count/3 - 1);
			viable = CheckIfPointHitsMesh(new Vector3(faceCopy[face*3], faceCopy[face*3+1], faceCopy[face*3+2]));
			if (!viable){
				faceCopy.RemoveAt(face*3);
				faceCopy.RemoveAt(face*3);
				faceCopy.RemoveAt(face*3);
			}
			if (faceCopy.Count <= 0 ){
				break;
			} 
		}
		
		int indexA = 0,
			indexB = 1,
			indexC = 2;
			
		if (faceCopy.Count > 1){
			indexA = currentFaces.ToList().IndexOf(faceCopy[face*3]);
			indexB = currentFaces.ToList().IndexOf(faceCopy[face*3+1]);
			indexC = currentFaces.ToList().IndexOf(faceCopy[face*3+2]);
		}
		
		return new Vector3(currentFaces[indexA], currentFaces[indexB], currentFaces[indexC]);
	}
	
	float Avg(float a, float b, float c){
		return (a + b + c) / 3;
	}
	
	IEnumerator GenerateNewPoint(Vector3 face){
		Vector3 pointA = baseVertices[(int)face[0]],
				pointB = baseVertices[(int)face[1]],
				pointC = baseVertices[(int)face[2]];
	
		Vector3 midpoint = new Vector3(Avg(pointA.x, pointB.x, pointC.x),
		                               Avg(pointA.y, pointB.y, pointC.y),
		                               Avg(pointA.z, pointB.z, pointC.z));
		                               
		Vector3 perp = Vector3.Cross(pointC - pointA, pointB - pointA).normalized;
		
		Vector3 pointD = (midpoint + perp) * Random.Range(.5f, 1.5f);
		Vector3 temp = midpoint;
		baseVertices.Add(temp);
		
		List<int> newTriangles = new List<int>();
		newTriangles.Add(baseVertices.IndexOf(pointA));
		newTriangles.Add(baseVertices.IndexOf(pointB));
		newTriangles.Add(baseVertices.IndexOf(temp));
		
		newTriangles.Add(baseVertices.IndexOf(pointA));
		newTriangles.Add(baseVertices.IndexOf(temp));
		newTriangles.Add(baseVertices.IndexOf(pointC));
		
		newTriangles.Add(baseVertices.IndexOf(pointC));
		newTriangles.Add(baseVertices.IndexOf(temp));
		newTriangles.Add(baseVertices.IndexOf(pointB));
		
		var tris = triangles.ToList();
		tris.AddRange(newTriangles);
		triangles = tris.ToArray();
		
		currentFaces = new int[9]{newTriangles[0],
								  newTriangles[1],		
								  newTriangles[2],
								  newTriangles[3],
								  newTriangles[4],
								  newTriangles[5],
								  newTriangles[6],
								  newTriangles[7],
								  newTriangles[8]};
		
		var mesh = meshObject.GetComponent<MeshFilter>().mesh;
		mesh.vertices = baseVertices.ToArray();
		mesh.triangles = triangles;
		
		float step = 0;
		
		while (step < 1){
			step+= .01f;
			var verts = mesh.vertices;
			for (int i = 0; i < verts.Length; i++){
				if (i == verts.Length-1){
					verts[i] = Vector3.Lerp(temp, pointD, step);
				}
				else{
					var sinVert = Vector3.Lerp(interpolationPoints[i].a, interpolationPoints[i].b, Mathf.Sin(interpolationPoints[i].Angle * Mathf.PI / 180));
					verts[i] = sinVert;
				}
			}
			
			mesh.vertices = verts;
			meshObject.GetComponent<MeshCollider>().sharedMesh = mesh;
			baseVertices = mesh.vertices.ToList();	
			yield return null;
		}
		
		interpolationPoints.Add (new VecPair(mesh.vertices[mesh.vertices.Length-1], mesh.vertices[mesh.vertices.Length-1]));
		
	}
	

	IEnumerator GenerateMesh(){
		while (true){
			var face = PickFace();
			yield return StartCoroutine(GenerateNewPoint(face));
			meshCount--;
			
			if (meshCount < 0){
				break;
			}
		}
		StartCoroutine(FadeThenDie());
	}
	
	IEnumerator FadeThenDie(){
		yield return new WaitForSeconds(5);
		Color currentColor = meshObject.renderer.material.GetColor("_TintColor");
		while (currentColor.a > 0){
			currentColor.a -= .001f;
			meshObject.renderer.material.SetColor("_TintColor", currentColor);
			yield return null;
		}
		
		Destroy (gameObject);
	}
	
}

public class VecPair{
	public Vector3 a,b;
	
	int angle;
	
	public int Angle{
		get{ angle++; if (angle > 180){angle = 0;} return angle;}
	}
	
	public VecPair(Vector3 avec, Vector3 bvec){
		a = avec;
		b = bvec;
		angle = 0;
	}

}


















