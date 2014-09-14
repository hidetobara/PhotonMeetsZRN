using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(MeshFilter))]
public class Polygons : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		MeshFilter filter = target as MeshFilter;
		string polygons = "Polygons: " + filter.sharedMesh.triangles.Length / 3;
		EditorGUILayout.LabelField(polygons);
	}
}