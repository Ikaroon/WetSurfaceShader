using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class FluidTextureController : MonoBehaviour
{
	static int s_fadePropertyID = Shader.PropertyToID("_Fade");

	[SerializeField]
	string m_fluidTextureName = "_Fluid_Texture";
	int m_fluidTextureID;
	[SerializeField]
	RenderTexture m_fluidTexture;
	[SerializeField, Range(0f, 0.1f)]
	float m_dotsFade = 0.005f;
	[SerializeField, Range(0f, 0.1f)]
	float m_dripsFade = 0f;
	[SerializeField, Range(0f, 0.1f)]
	float m_flowFade = 0.005f;

	RenderTexture m_fluidTargetTexture;

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

		m_fadeMaterial.SetVector(s_fadePropertyID, new Vector3(m_flowFade, m_dotsFade, m_dripsFade) * dt);

		Graphics.Blit(m_fluidTargetTexture, m_bufferTexture);
		Graphics.Blit(m_bufferTexture, m_fluidTargetTexture, m_fadeMaterial);
		Graphics.Blit(m_fluidTexture, m_fluidTargetTexture, m_transferMaterial);
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

		if (m_fluidTargetTexture != null)
			m_fluidTargetTexture.Release();
		m_fluidTargetTexture = null;
	}

	void RefreshTextures()
	{
		m_fluidTextureID = Shader.PropertyToID(m_fluidTextureName);

		ClearTextures();

		m_bufferTexture = new RenderTexture(m_fluidTexture)
		{
			name = $"{m_fluidTexture.name} Buffer",
			wrapMode = TextureWrapMode.Repeat,
		};

		m_fluidTargetTexture = new RenderTexture(m_fluidTexture)
		{
			name = $"{m_fluidTexture.name} Target",
			wrapMode = TextureWrapMode.Repeat,
		};

		Shader.SetGlobalTexture(m_fluidTextureID, m_fluidTargetTexture);
	}
}
