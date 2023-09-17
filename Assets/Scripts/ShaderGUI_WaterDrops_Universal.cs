using System;
using UnityEditor;
using UnityEngine;

public class ShaderGUI_WaterDrops_Universal : ShaderGUI
{
	static string[] s_surfaceTypes = new string[] { "Opaque", "Transparent" };

	public bool IsActive(Material material, string keyword)
	{
		return Array.IndexOf(material.shaderKeywords, keyword) != -1;
	}

	void SetTransparent(MaterialEditor materialEditor, MaterialProperty[] properties)
	{
		FindProperty("_SurfaceType", properties).floatValue = 1;

		FindProperty("_RenderQueueType", properties).floatValue = 4;
		FindProperty("_TransparentDepthPrepassEnable", properties).floatValue = 0;

		FindProperty("_DstBlend", properties).floatValue = 10;
		FindProperty("_AlphaSrcBlend", properties).floatValue = 1;
		FindProperty("_AlphaDstBlend", properties).floatValue = 10;

		FindProperty("_ZWrite", properties).floatValue = 0;
		FindProperty("_ZTestDepthEqualForOpaque", properties).floatValue = 4;

		FindProperty("_StencilRefDepth", properties).floatValue = 0;
		FindProperty("_StencilRefMV", properties).floatValue = 32;
		FindProperty("_StencilRefGBuffer", properties).floatValue = 2;

		Material targetMat = materialEditor.target as Material;
		targetMat.renderQueue = 3000;

		materialEditor.PropertiesChanged();
	}

	void SetOpaque(MaterialEditor materialEditor, MaterialProperty[] properties)
	{
		FindProperty("_SurfaceType", properties).floatValue = 0;

		FindProperty("_RenderQueueType", properties).floatValue = 1;
		FindProperty("_TransparentDepthPrepassEnable", properties).floatValue = 0;

		FindProperty("_DstBlend", properties).floatValue = 0;
		FindProperty("_AlphaSrcBlend", properties).floatValue = 1;
		FindProperty("_AlphaDstBlend", properties).floatValue = 0;

		FindProperty("_ZWrite", properties).floatValue = 1;
		FindProperty("_ZTestDepthEqualForOpaque", properties).floatValue = 3;

		FindProperty("_StencilRefDepth", properties).floatValue = 8;
		FindProperty("_StencilRefMV", properties).floatValue = 40;
		FindProperty("_StencilRefGBuffer", properties).floatValue = 10;

		Material targetMat = materialEditor.target as Material;
		targetMat.renderQueue = 2000;

		materialEditor.PropertiesChanged();
	}

	void RevertSurfaceType(Material targetMat)
	{
		if (targetMat.parent == null)
			return;

		targetMat.renderQueue = targetMat.parent.renderQueue;

		targetMat.RevertPropertyOverride("_SurfaceType");

		targetMat.RevertPropertyOverride("_RenderQueueType");
		targetMat.RevertPropertyOverride("_TransparentDepthPrepassEnable");

		targetMat.RevertPropertyOverride("_DstBlend");
		targetMat.RevertPropertyOverride("_AlphaSrcBlend");
		targetMat.RevertPropertyOverride("_AlphaDstBlend");

		targetMat.RevertPropertyOverride("_ZWrite");
		targetMat.RevertPropertyOverride("_ZTestDepthEqualForOpaque");

		targetMat.RevertPropertyOverride("_StencilRefDepth");
		targetMat.RevertPropertyOverride("_StencilRefMV");
		targetMat.RevertPropertyOverride("_StencilRefGBuffer");
	}

	public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
	{
		Material targetMat = materialEditor.target as Material;

		EditorGUILayout.BeginVertical(EditorStyles.helpBox);
		EditorGUILayout.LabelField("Surface", EditorStyles.boldLabel);

		var surfaceTypeProperty = FindProperty("_SurfaceType", properties);
		var surfaceType = (int)surfaceTypeProperty.floatValue;
		EditorGUI.BeginChangeCheck();
		surfaceType = EditorGUILayout.Popup("Type", surfaceType, s_surfaceTypes);

		if (targetMat.parent != null && targetMat.IsPropertyOverriden("_SurfaceType"))
		{
			var rect = GUILayoutUtility.GetLastRect();

			var overrideRect = rect;
			overrideRect.xMin = 0;
			overrideRect.xMax = 2;
			EditorGUI.DrawRect(overrideRect, Color.white);

			var labelRect = rect;
			var e = Event.current;
			if (e.type == EventType.MouseDown && e.button == 1)
			{
				var menu = new GenericMenu();
				menu.AddItem(new GUIContent("Revert"), false, () => { RevertSurfaceType(targetMat); });
				menu.ShowAsContext();
			}
		}
		else
		{
			var rect = GUILayoutUtility.GetLastRect();

			var labelRect = rect;
			var e = Event.current;
			if (e.type == EventType.MouseDown && e.button == 1)
			{
				var menu = new GenericMenu();
				menu.AddDisabledItem(new GUIContent("Revert"), false);
				menu.ShowAsContext();
			}
		}

		if (EditorGUI.EndChangeCheck())
		{
			switch (surfaceType)
			{
				case 0:
					SetOpaque(materialEditor, properties);
					break;
				case 1:
					SetTransparent(materialEditor, properties);
					break;
			}
		}

		EditorGUILayout.Space();
		materialEditor.ShaderProperty(FindProperty("_Diffuse", properties), "Diffuse");
		materialEditor.ShaderProperty(FindProperty("_Normal_Map", properties), "Normal");
		materialEditor.ShaderProperty(FindProperty("_Tint", properties), "Tint");
		materialEditor.ShaderProperty(FindProperty("_BaseSmoothness", properties), "Smoothness");
		EditorGUILayout.EndVertical();

		EditorGUILayout.BeginVertical(EditorStyles.helpBox);
		EditorGUILayout.LabelField("Fluid", EditorStyles.boldLabel);
		materialEditor.ShaderProperty(FindProperty("_FluidColor", properties), "Color");
		materialEditor.ShaderProperty(FindProperty("_Scale", properties), "Scale");
		materialEditor.ShaderProperty(FindProperty("_Fluid_Normal_Strength", properties), "Normal Strength");
		materialEditor.ShaderProperty(FindProperty("_FluidSmoothness", properties), "Smoothness");

		materialEditor.ShaderProperty(FindProperty("_Wetness", properties), "Wetness");

		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Refraction", EditorStyles.boldLabel);
		materialEditor.ShaderProperty(FindProperty("_Refraction", properties), "Refraction");
		materialEditor.ShaderProperty(FindProperty("_REFRACTSCENE", properties), "Refract Scene");

		EditorGUILayout.Space();
		materialEditor.ShaderProperty(FindProperty("_TOP_TEXTURE_TYPE", properties), "Top Texture Type");

		EditorGUILayout.Space();
		materialEditor.ShaderProperty(FindProperty("_MAPPING_METHOD", properties), "Mapping Method");

		if (IsActive(targetMat, "_MAPPING_METHOD_TRIPLANAR") || IsActive(targetMat, "_MAPPING_METHOD_BIPLANAR"))
			materialEditor.ShaderProperty(FindProperty("_Blending", properties), "Triplanar Blending");

		EditorGUILayout.EndVertical();
	}
}