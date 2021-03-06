﻿using System;

using UnityEngine;

namespace Core.Components.SceneFramework {

	/// <summary>
	/// 提示层
	/// </summary>
	public class PromptLayer : CanvasComponent {

		/// <summary>
		/// 外部组件设置
		/// </summary>
		public AlertWindow alertWindow; // 提示窗口
		public LoadingWindow loadingWindow; // 加载窗口

		public RebuildController rebuildController; // 布局重建器

	}
}
