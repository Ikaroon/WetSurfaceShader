using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;

class PreRenderVFXGraph : CustomPass
{
	[SerializeField]
	string m_globalTextureName = "_VFXTexture";
	int m_globalTextureID;

	[SerializeField]
	RenderTexture m_sourceTexture;

	[SerializeField]
	bool m_useFade = true;

	[SerializeField, Range(0f, 0.1f)]
	float m_fade = 0.005f;

	Material m_fadeMaterial;
	Material m_transferMaterial;

	RenderTexture m_bufferTexture;
	RenderTexture m_targetTexture;

	// It can be used to configure render targets and their clear state. Also to create temporary render target textures.
	// When empty this render pass will render to the active camera render target.
	// You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
	// The render pipeline will ensure target setup and clearing happens in an performance manner.
	protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
	{
		m_globalTextureID = Shader.PropertyToID(m_globalTextureName);

		if (!m_useFade)
		{
			Shader.SetGlobalTexture(m_globalTextureID, m_sourceTexture);
			return;
		}

		var transferShader = Shader.Find("Hidden/Shader_Transfer");
		m_transferMaterial = CoreUtils.CreateEngineMaterial(transferShader);

		var fadeShader = Shader.Find("Hidden/Shader_Fade");
		m_fadeMaterial = CoreUtils.CreateEngineMaterial(fadeShader);
		m_fadeMaterial.SetFloat("_Fade", m_fade);

		m_bufferTexture = new RenderTexture(m_sourceTexture)
			{
				name = $"{m_sourceTexture.name} Buffer",
				wrapMode = TextureWrapMode.Repeat,
			};
		m_targetTexture = new RenderTexture(m_sourceTexture)
			{
				name = $"{m_sourceTexture.name} Target",
				wrapMode = TextureWrapMode.Repeat,
			};

		Shader.SetGlobalTexture(m_globalTextureID, m_targetTexture);
	}

	protected override void Execute(CustomPassContext ctx)
	{
		if (!m_useFade)
		{
			Shader.SetGlobalTexture(m_globalTextureID, m_sourceTexture);
			return;
		}

		m_fadeMaterial.SetFloat("_Fade", m_fade);

		ctx.cmd.Blit(m_targetTexture, m_bufferTexture);
		ctx.cmd.Blit(m_bufferTexture, m_targetTexture, m_fadeMaterial);
		ctx.cmd.Blit(m_sourceTexture, m_targetTexture, m_transferMaterial);
	}

	protected override void Cleanup()
	{
		if (!m_useFade)
			return;

		m_bufferTexture.Release();
		m_targetTexture.Release();

		CoreUtils.Destroy(m_fadeMaterial);
		CoreUtils.Destroy(m_transferMaterial);
	}
}