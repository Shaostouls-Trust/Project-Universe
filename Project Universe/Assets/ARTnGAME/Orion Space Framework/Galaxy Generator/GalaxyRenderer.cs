﻿// ----------------------------------------------------------------
// Galaxia
// ©2016 Simeon Radivoev
// Written by Simeon Radivoev (simeonradivoev@gmail.com)
// ----------------------------------------------------------------

using UnityEngine;

namespace Artngame.Orion.Galaxia
{
	public abstract class GalaxyRenderer : MonoBehaviour
	{
		[SerializeField] private Galaxy galaxy;

		protected virtual void OnEnable()
		{
			galaxy?.OnPreRender.RemoveListener(OnGalaxyPreRender);
			galaxy?.OnPreRender.AddListener(OnGalaxyPreRender);
        }

		/// <summary>
		/// Sets the galaxy for the Renderer
		/// </summary>
		/// <param name="galaxy">The galaxy to render</param>
		public void SetGalaxy(Galaxy galaxy)
		{
			if (this.galaxy != galaxy)
			{
				this.galaxy.OnPreRender.RemoveListener(OnGalaxyPreRender);
			}
			galaxy.OnPreRender.AddListener(OnGalaxyPreRender);
        }

		protected virtual void OnGalaxyPreRender(Particles particles,Galaxy.RenderEvent eEvent)
		{
			eEvent.Used = true;
		}

		public Galaxy Galaxy
		{
			get { return galaxy; }
		}

		protected virtual void OnDisable()
		{
			galaxy?.OnPreRender.RemoveListener(OnGalaxyPreRender);
		}
	}
}