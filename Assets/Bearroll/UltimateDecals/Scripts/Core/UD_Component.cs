using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Bearroll.UltimateDecals {

	public class UD_Component: MonoBehaviour {
		
		Transform cachedTransform;
		bool isTransformCached;

		public new Transform transform {
			get {
				if (!isTransformCached) {
					cachedTransform = base.transform;
					isTransformCached = true;
				}
				return cachedTransform; 
			}
		}

	}

}