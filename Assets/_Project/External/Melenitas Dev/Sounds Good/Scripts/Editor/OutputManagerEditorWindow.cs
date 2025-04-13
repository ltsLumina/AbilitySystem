#if UNITY_EDITOR
/*
 * All rights to the Sounds Good plugin, © Created by Melenitas Dev, are reserved.
 * Distribution of the standalone asset is strictly prohibited.
 */
#region
using System.Collections.Generic;
using MelenitasDev.SoundsGood.Domain;
using UnityEditor;
using UnityEngine;
#endregion

namespace MelenitasDev.SoundsGood.Editor
{
public class OutputManagerEditorWindow : EditorWindow
{
	OutputCollection outputCollection;

	Dictionary<OutputData, bool> lastRefreshedOutputsDict = new ();
	Vector2 scrollPosition;

	GUIStyle redBoxStyle;
	GUIStyle greenBoxStyle;

	[MenuItem("Tools/Melenitas Dev/Sounds Good/Output Manager")]
	public static void ShowWindow()
	{
		var window = (OutputManagerEditorWindow) GetWindow(typeof(OutputManagerEditorWindow));
		window.Show();
		window.titleContent = new ("Output Manager");
	}

	void OnEnable()
	{
		string path = "Melenitas Dev/Sounds Good/Outputs";
		outputCollection = Resources.Load<OutputCollection>($"{path}/OutputCollection");
		LoadOutputs();
	}

	void OnGUI()
	{
		redBoxStyle = new (GUI.skin.box);
		greenBoxStyle = new (GUI.skin.box);
		redBoxStyle.normal.background = MakeTex(2, 2, new (0.58f, 0.15f, 0.15f, 0.3f));
		greenBoxStyle.normal.background = MakeTex(2, 2, new (0.15f, 0.58f, 0.15f, 0.3f));

		EditorGUILayout.Space(20);

		CenterText("OUTPUTS MANAGER", 25, EditorWindowSharedTools.orange);

		GUILayout.Space(5);

		CenterText("Manage your audio outputs", 11, EditorWindowSharedTools.lightOrange);

		PlayModeMessage();

		GUI.enabled = !Application.isPlaying;

		GUILayout.Space(15);

		EditorGUI.BeginDisabledGroup(Application.isPlaying);
		if (GUILayout.Button("Reload Database", GUILayout.Height(45))) LoadOutputs();

		if (GUILayout.Button("Check Exposed Volumes", GUILayout.Height(24))) CheckExposedVolumes();

		if (outputCollection == null) return;

		GUILayout.Space(15);

		scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

		foreach (KeyValuePair<OutputData, bool> outputData in lastRefreshedOutputsDict)
		{
			bool exposed = outputData.Value;

			EditorGUILayout.BeginVertical(exposed ? greenBoxStyle : redBoxStyle);
			CenterText(outputData.Key.Name, 15, exposed ? Color.green : Color.red);
			string exposedText = exposed ? "Volume is exposed correctly!" : "Error: Volume is not exposed";
			CenterText(exposedText, 11, exposed ? Color.green : Color.red);
			GUILayout.Space(3);
			EditorGUILayout.EndVertical();
			GUILayout.Space(3);
		}

		EditorGUILayout.EndScrollView();
		EditorGUI.EndDisabledGroup();

		EditorWindowSharedTools.LogoBanner();
	}

	void CheckExposedVolumes()
	{
		lastRefreshedOutputsDict.Clear();

		foreach (OutputData outputData in outputCollection.Outputs)
		{
			bool exposed = outputData.Output.audioMixer.GetFloat(outputData.Name.Replace(" ", ""), out float value);
			lastRefreshedOutputsDict.Add(outputData, exposed);
		}
	}

	void LoadOutputs()
	{
		outputCollection = Resources.Load<OutputCollection>("Melenitas Dev/Sounds Good/Outputs/OutputCollection");

		if (outputCollection != null)
		{
			outputCollection.LoadOutputs();
			GenerateEnum();
			CheckExposedVolumes();
			EditorUtility.SetDirty(outputCollection);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			return;
		}

		string path = "Assets/Resources/Melenitas Dev/Sounds Good/Outputs";

		if (!AssetDatabase.IsValidFolder(path))
		{
			if (!AssetDatabase.IsValidFolder("Assets/Resources")) AssetDatabase.CreateFolder("Assets", "Resources");
			if (!AssetDatabase.IsValidFolder("Assets/Resources/Melenitas Dev")) AssetDatabase.CreateFolder("Assets/Resources", "Melenitas Dev");
			if (!AssetDatabase.IsValidFolder("Assets/Resources/Melenitas Dev/Sounds Good")) AssetDatabase.CreateFolder("Assets/Resources", "Sounds Good");
			AssetDatabase.CreateFolder("Assets/Resources/Melenitas Dev", "Outputs");
		}

		outputCollection = CreateInstance<OutputCollection>();
		AssetDatabase.CreateAsset(outputCollection, $"{path}/OutputCollection.asset");

		outputCollection.LoadOutputs();
		GenerateEnum();
		CheckExposedVolumes();
		EditorUtility.SetDirty(outputCollection);
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
	}

	void GenerateEnum()
	{
		string[] outputNames = new string[outputCollection.Outputs.Length];
		int i = 0;

		foreach (OutputData outputData in outputCollection.Outputs)
		{
			outputNames[i] = outputData.Name.Replace(" ", "");
			i++;
		}

		using (var enumGenerator = new EnumGenerator()) { enumGenerator.GenerateEnum("Output", outputNames); }
	}

	void PlayModeMessage()
	{
		if (!Application.isPlaying) return;
		EditorGUILayout.Space(10);
		CenterText("It cannot be used in Play mode", 13, new (0.65f, 0.25f, 0.25f));
	}

	void CenterText(string text, int fontSize, Color color)
	{
		var style = new GUIStyle(EditorStyles.label);
		style.normal.textColor = color;
		style.focused.textColor = color;
		style.hover.textColor = color;
		style.fontStyle = FontStyle.Bold;
		style.fontSize = fontSize;

		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.Label(text, style);
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
	}

	Texture2D MakeTex(int width, int height, Color color)
	{
		var pix = new Color[width * height];
		for (int i = 0; i < pix.Length; ++i) pix[i] = color;
		var result = new Texture2D(width, height);
		result.SetPixels(pix);
		result.Apply();
		return result;
	}
}
}
#endif
