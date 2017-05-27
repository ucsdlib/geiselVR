using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;


public class TextRenderer : MonoBehaviour
{
    public float Scale;

    void Start()
    {
        gameObject.AddComponent<MeshFilter>();
        gameObject.AddComponent<MeshRenderer>();
        gameObject.transform.localScale = Vector3.one * Scale / 10000f;
        GenerateText("HELLO WORLD");
    }

    public void GenerateText(string text)
    {
        MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
        MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();

        TextGenerationSettings settings = GetDefSettings();
        TextGenerator generator = new TextGenerator();
        generator.Populate(text, settings);

        Mesh mesh = new Mesh();
        GetMesh(generator, mesh);
        meshFilter.mesh = mesh;
        meshRenderer.material = settings.font.material;
    }


    private TextGenerationSettings GetDefSettings()
    {
        return new TextGenerationSettings
        {
            textAnchor = TextAnchor.MiddleCenter,
            color = Color.black,
            generationExtents = new Vector2(10, 10),
            pivot = new Vector2(0f, 0f),
            richText = true,
            font = Font.CreateDynamicFontFromOSFont("Arial", 72),
            fontSize = 24,
            fontStyle = FontStyle.Normal,
            verticalOverflow = VerticalWrapMode.Overflow,
            horizontalOverflow = HorizontalWrapMode.Overflow,
            lineSpacing = 1,
            generateOutOfBounds = true,
            resizeTextForBestFit = true,
            scaleFactor = 1f
        };
    }

    private void GetMesh(TextGenerator generator, Mesh mesh)
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
        int[] tempIndices = new int[charCount * 6];
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