﻿
using UnityEngine;
using UnityEngine.EventSystems;

using Core.Components;

using ExerComps.Windows;

namespace ExerComps.Controls.InputFields {

    /// <summary>
    /// 日期选择器
    /// </summary>
    public class DateTimePickersPlane : ToggleWindow {

		/// <summary>
		/// 外部组件设置
		/// </summary>

		/// <summary>
		/// 内部变量设置
		/// </summary>
		DateTimeField field;

        #region 启动窗口

        /// <summary>
        /// 启动窗口
        /// </summary>
        /// <param name="field">绑定输入域</param>
        public void activate(DateTimeField field) {
            Debug.Log("cur field:" + field.name);
            activate();
            this.field = field;
        }

        #endregion
		
		#region 事件控制

		/// <summary>
		/// 取消回调
		/// </summary>
		protected override void onCancel() {
			field?.endSelect();
		}

		#endregion
	}
}