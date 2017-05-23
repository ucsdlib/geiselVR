using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;


public class TextRenderer : MonoBehaviour {

	// Use this for initialization
	void Start () {
		// Settings
		TextGenerationSettings settings = new TextGenerationSettings();
		settings.textAnchor = TextAnchor.MiddleCenter;
		settings.color = Color.black;
		settings.generationExtents = new Vector2(1, 1);
		settings.pivot = new Vector2(0.5f, 0.5f);
		settings.richText = true;
		settings.font = Font.CreateDynamicFontFromOSFont("Arial", 72);
		settings.fontSize = 11;
		settings.fontStyle = FontStyle.Normal;
		settings.verticalOverflow = VerticalWrapMode.Overflow;
		settings.horizontalOverflow = HorizontalWrapMode.Overflow;
		settings.lineSpacing = 1;
		settings.generateOutOfBounds = true;
		settings.resizeTextForBestFit = true;
		settings.scaleFactor = 1f;

		MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
		MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();

		TextGenerator generator = new TextGenerator();
		generator.Populate("HELLO WORLD", settings);
		Debug.Log("Generator made " + generator.vertexCount + " vertices.");

		Mesh mesh = new Mesh();
		GetMesh(generator, mesh);
		meshFilter.mesh = mesh;
		renderer.material = settings.font.material;
	}

	public void GetMesh(TextGenerator generator, Mesh mesh)
	{
		if (mesh == null) return;

		// Copy vertex data into mesh
		int vertSize = generator.vertexCount;
		Vector3[] tempVerts = new Vector3[vertSize];
		Color32[] tempColors = new Color32[vertSize];
		Vector2[] tempUvs = new Vector2[vertSize];
		IList<UIVertex> generatorVerts = generator.verts;
		for (int i = 0; i < vertSize; i++)
		{
			tempVerts[i] = generatorVerts[i].position;
			tempColors[i] = generatorVerts[i].color;
			tempUvs[i] = generatorVerts[i].uv0;
		}
		mesh.vertices = tempVerts;
		mesh.colors32 = tempColors;
		mesh.uv = tempUvs;

		// Map triangles to vertex data
		int charCount = vertSize / 4;
		int [] tempIndices = new int[charCount * 6];
		for (int i = 0; i < charCount; i++)
		{
			int vertIndexStart = i * 4;
			int trianglesIndexStart = i * 6;
			tempIndices[trianglesIndexStart++] = vertIndexStart;
			tempIndices[trianglesIndexStart++] = vertIndexStart + 1;
			tempIndices[trianglesIndexStart++] = vertIndexStart + 2;
			tempIndices[trianglesIndexStart++] = vertIndexStart;
			tempIndices[trianglesIndexStart++] = vertIndexStart + 2;
			tempIndices[trianglesIndexStart] = vertIndexStart + 3;
		}
		mesh.triangles = tempIndices;
		mesh.RecalculateBounds();
	}
	
}
