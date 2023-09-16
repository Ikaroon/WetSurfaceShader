using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class FluidTextureController : MonoBehaviour
{
	static int s_fadePropertyID = Shader.PropertyToID("_Fade");

	[Header("Flow")]
	[SerializeField]
	string m_flowTextureName = "_Fluid_Flow_Texture";
	int m_flowTextureID;
	[SerializeField]
	RenderTexture m_flowTexture;
	[SerializeField, Range(0f, 0.1f)]
	float m_flowFade = 0.005f;

	RenderTexture m_flowTargetTexture;

	[Header("Drips")]
	[SerializeField]
	string m_dripsTextureName = "_Fluid_Drips_Texture";
	int m_dripsTextureID;
	[SerializeField]
	RenderTexture m_dripsTexture;

	[Header("Dots")]
	[SerializeField]
	string m_dotsTextureName = "_Fluid_Dots_Texture";
	int m_dotsTextureID;
	[SerializeField]
	RenderTexture m_dotsTexture;
	[SerializeField, Range(0f, 0.1f)]
	float m_dotsFade = 0.005f;

	RenderTexture m_dotsTargetTexture;

	// System
	Material m_fadeMaterial;
	Material m_transferMaterial;

	RenderTexture m_bufferTexture;

	void OnEnable()
	{
		var transferShader = Shader.Find("Hidden/Shader_Transfer");
		m_transferMaterial = CoreUtils.CreateEngineMaterial(transferShader);

		var fadeShader = Shader.Find("Hidden/Shader_Fade");
		m_fadeMaterial = CoreUtils.CreateEngineMaterial(fadeShader);

		RefreshTextures();
	}

	void OnDisable()
	{
		ClearTextures();

		DestroyImmediate(m_fadeMaterial);
		DestroyImmediate(m_transferMaterial);
	}

	void Update()
	{
		var dt = Time.deltaTime;

		// Flow Blits
		m_fadeMaterial.SetFloat(s_fadePropertyID, m_flowFade * dt);

		Graphics.Blit(m_flowTargetTexture, m_bufferTexture);
		Graphics.Blit(m_bufferTexture, m_flowTargetTexture, m_fadeMaterial);
		Graphics.Blit(m_flowTexture, m_flowTargetTexture, m_transferMaterial);

		// Dots Blits
		m_fadeMaterial.SetFloat(s_fadePropertyID, m_dotsFade * dt);

		Graphics.Blit(m_dotsTargetTexture, m_bufferTexture);
		Graphics.Blit(m_bufferTexture, m_dotsTargetTexture, m_fadeMaterial);
		Graphics.Blit(m_dotsTexture, m_dotsTargetTexture, m_transferMaterial);
	}

	void OnValidate()
	{
		RefreshTextures();
	}

	void ClearTextures()
	{
		if (m_bufferTexture != null)
			m_bufferTexture.Release();
		m_bufferTexture = null;

		if (m_flowTargetTexture != null)
			m_flowTargetTexture.Release();
		m_flowTargetTexture = null;

		if (m_dotsTargetTexture != null)
			m_dotsTargetTexture.Release();
		m_dotsTargetTexture = null;
	}

	void RefreshTextures()
	{
		m_flowTextureID = Shader.PropertyToID(m_flowTextureName);
		m_dripsTextureID = Shader.PropertyToID(m_dripsTextureName);
		m_dotsTextureID = Shader.PropertyToID(m_dotsTextureName);

		ClearTextures();

		m_bufferTexture = new RenderTexture(m_flowTexture)
		{
			name = $"{m_flowTexture.name} Buffer",
			wrapMode = TextureWrapMode.Repeat,
		};

		m_flowTargetTexture = new RenderTexture(m_flowTexture)
		{
			name = $"{m_flowTexture.name} Target",
			wrapMode = TextureWrapMode.Repeat,
		};

		m_dotsTargetTexture = new RenderTexture(m_dotsTexture)
		{
			name = $"{m_dotsTexture.name} Target",
			wrapMode = TextureWrapMode.Repeat,
		};

		Shader.SetGlobalTexture(m_flowTextureID, m_flowTargetTexture);
		Shader.SetGlobalTexture(m_dripsTextureID, m_dripsTexture);
		Shader.SetGlobalTexture(m_dotsTextureID, m_dotsTargetTexture);
	}
}
