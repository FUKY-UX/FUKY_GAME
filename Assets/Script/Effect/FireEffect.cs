using UnityEngine;
using UnityEngine.Serialization;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class FireEffect : MonoBehaviour
{
	// This will store all the settings for a specific set of renderers
	public float TilingX = 0.5f;
	public float TilingY = 0.5f;
	public float ScrollSpeedMin = -1.0f;
	public float ScrollSpeedMax = -0.5f;
	public enum Axis
	{
		X,
		Y
	}
	public Axis ScrollAxis = Axis.Y;
	public float TemperatureCoarse = 1.0f;
	public float TemperatureDetail = 1.0f;
	public float Opacity = 1.0f;

	// This will store a list of renderers that use specific settings
	public Renderer[] Renderers;

	// Internal working data
	class FireRendererData
	{
		public Vector4 ScrollTex_ST;
		public MaterialPropertyBlock ScrollBlock;
		public float ScrollSpeed;
	}
	List<FireRendererData> _RendererData;

	/// <summary>
	/// Call this to initialize the internal data
	/// </summary>
	void Initialize()
	{
		if (Renderers != null)
		{
			_RendererData = new List<FireRendererData>();
			for (int i = 0; i < Renderers.Length; ++i)
			{
				// Setup a new working set
				var data = new FireRendererData();

				// Pick random value for speed
				data.ScrollSpeed = Random.Range(ScrollSpeedMin, ScrollSpeedMax);

				// Assign tiling and offset
				float xoffset;
				float yoffset;
				if (ScrollAxis == Axis.Y)
				{
					xoffset = 0.5f - TilingX * 0.5f;
					yoffset = Random.Range(0.0f, 1.0f);
				}
				else
				{
					yoffset = 0.5f - TilingY * 0.5f;
					xoffset = Random.Range(0.0f, 1.0f);
				}
				data.ScrollTex_ST = new Vector4(TilingX, TilingY, xoffset, yoffset);

				// Push that to the property block
				data.ScrollBlock = new MaterialPropertyBlock();
				data.ScrollBlock.SetVector("_ScrollTex_ST", data.ScrollTex_ST);
				data.ScrollBlock.SetFloat("_OpacityCoarse", Opacity);
				data.ScrollBlock.SetFloat("_OpacityDetail", 1.0f);
				data.ScrollBlock.SetFloat("_TemperatureCoarse", TemperatureCoarse);
				data.ScrollBlock.SetFloat("_TemperatureDetail", TemperatureDetail);

				// Add to the working set list
				_RendererData.Add(data);
			}
		}
	}

	void Reset()
	{
		Renderers = GetComponentsInChildren<Renderer>();
	}

	// Use this for initialization
	void Start ()
	{
		Initialize();
	}

	void OnValidate()
	{
		Initialize();
		Update();
	}
	
	// Update is called once per frame
	void Update ()
	{
#if UNITY_EDITOR
		if (_RendererData == null)
		{
			Initialize();
		}
#endif

		if (_RendererData != null)
		{
			for (int i = 0; i < _RendererData.Count; ++i)
			{
				var data = _RendererData[i];
				var renderer = Renderers[i];
				if (renderer != null)
				{
					if (ScrollAxis == Axis.Y)
					{
						data.ScrollTex_ST.w += Time.deltaTime * data.ScrollSpeed;
					}
					else
					{
						data.ScrollTex_ST.z += Time.deltaTime * data.ScrollSpeed;
					}
					data.ScrollBlock.SetVector("_ScrollTex_ST", data.ScrollTex_ST);
					data.ScrollBlock.SetFloat("_OpacityCoarse", Opacity);
					data.ScrollBlock.SetFloat("_OpacityDetail", 1.0f);
					data.ScrollBlock.SetFloat("_TemperatureCoarse", TemperatureCoarse);
					data.ScrollBlock.SetFloat("_TemperatureDetail", TemperatureDetail);
					renderer.SetPropertyBlock(data.ScrollBlock);
				}
			}
		}
	}
}
