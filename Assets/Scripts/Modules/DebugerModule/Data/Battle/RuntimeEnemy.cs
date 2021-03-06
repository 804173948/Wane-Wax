﻿using System;
using System.Collections.Generic;

using UnityEngine;

using Core.Data;

using BattleModule;
using BattleModule.Data;

namespace DebugerModule.Data {

	/// <summary>
	/// 敌人
	/// </summary>
	public class RuntimeEnemy : RuntimeBattler {

		/// <summary>
		/// 敌人
		/// </summary>
		public override bool isEnemy => true;

		/// <summary>
		/// 下一位置
		/// </summary>
		public override int nextY => y - nextWall; // wall 大于 2 会修改direction

		/// <summary>
		/// 初始位置
		/// </summary>
		public override Vector2 initPos => new Vector2(mapX / 2, mapY / 2 - 1);
		public override bool initDir => false;

		/// <summary>
		/// 跳跃计算
		/// </summary>
		protected override float jumpY => -base.jumpY;
		protected override float fallY => -base.fallY;

		/// <summary>
		/// 更新
		/// </summary>
		protected override void updateOthers() {
			base.updateOthers();
			updateScore();
		}

		/// <summary>
		/// 更新积分
		/// </summary>
		void updateScore() {
			var deltaHP = this.deltaHP;
			if (!map.judgePosBelong(x, y, belong)) debugSer.score += 5;
			//if (deltaHP != null && deltaHP.value < 0) debugSer.score += 10;
		}

		/// <summary>
		/// 初始化
		/// </summary>
		public RuntimeEnemy() { }
		public RuntimeEnemy(Map map) : base(map) { }
		public RuntimeEnemy(Map map, int x, int y) : base(map, x, y) { }

	}
}
