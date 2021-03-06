﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

using Core.Systems;

using GameModule.Services;

using ExerComps.Controls.SystemExtensions.QuestionText;

namespace Core.Components.Utils {

	using SceneFramework;

    /// <summary>
    /// 场景工具类
    /// </summary>
    public static class SceneUtils {

        /// <summary>
        /// 常量设定
        /// </summary>
        public const string AlertWindowKey = "AlertWindow";
        public const string LoadingWindowKey = "LoadingWindow";
		public const string AudioSourceKey = "AudioSource";
		public const string RebuildControllerKey = "RebuildController";

		public const string CurrentSceneKey = "Scene";

		#region 快捷访问

		/// <summary>
		/// 提示窗口（脚本）
		/// </summary>
		public static AlertWindow alertWindow {
            get {
                return getSceneObject(AlertWindowKey) as AlertWindow;
            }
            set {
                depositSceneObject(AlertWindowKey, value);
            }
        }

        /// <summary>
        /// 加载窗口（脚本）
        /// </summary>
        public static LoadingWindow loadingWindow {
            get {
                return getSceneObject(LoadingWindowKey) as LoadingWindow;
            }
            set {
                depositSceneObject(LoadingWindowKey, value);
            }
		}

		/// <summary>
		/// 界面重构控制器（脚本）
		/// </summary>
		public static RebuildController rebuildController {
			get {
				return getSceneObject(RebuildControllerKey) as RebuildController;
			}
			set {
				depositSceneObject(RebuildControllerKey, value);
			}
		}

		/// <summary>
		/// 获取/设置当前场景（脚本）
		/// </summary>
		public static T getCurrentScene<T>() where T : BaseScene {
			return getSceneObject(CurrentSceneKey) as T;
		}
		public static BaseScene getCurrentScene() {
			return getSceneObject(CurrentSceneKey) as BaseScene;
		}
		public static void setCurrentScene(BaseScene scene) {
            depositSceneObject(CurrentSceneKey, scene);
        }

		#endregion

		/// <summary>
		/// BGM播放源
		/// </summary>
		public static AudioSource audioSource;

		/// <summary>
		/// 提示文本缓存
		/// </summary>
		public static string alertText { get; set; } = "";

        /// <summary>
        /// 场景物体托管
        /// </summary>
        static Dictionary<string, UnityEngine.Object> sceneObjectDeposit =
            new Dictionary<string, UnityEngine.Object>();

        /// <summary>
        /// 外部系统
        /// </summary>
        static GameSystem gameSys = null;
        static SceneSystem sceneSys = null;
        static GameService gameSer = null;

        /// <summary>
        /// 初始化界面工具
        /// </summary>
        /// <param name="scene">当前场景</param>
        /// <param name="alertWindow">当前场景的提示弹窗</param>
        /// <param name="loadingWindow">当前场景的加载窗口</param>
        public static void initialize(BaseScene scene, AudioSource audioSource = null) {
            Debug.Log("initialize Scene: " + scene);
            initializeSystems();
            initializeScene(scene, audioSource);
        }

        /// <summary>
        /// 初始化外部系统
        /// </summary>
        static void initializeSystems() {
            gameSys = gameSys ?? GameSystem.Get();
            sceneSys = sceneSys ?? SceneSystem.Get();
            gameSer = gameSer ?? GameService.Get();
        }

        /// <summary>
        /// 初始化场景
        /// </summary>
        /// <param name="scene">当前场景</param>
        /// <param name="alertWindow">当前场景的提示弹窗</param>
        /// <param name="loadingWindow">当前场景的加载窗口</param>
        static void initializeScene(BaseScene scene, AudioSource audioSource = null) {

            var sceneIndex = scene.sceneType;
            if (sceneSys.currentScene() != sceneIndex)
                sceneSys.gotoScene(sceneIndex);

            setCurrentScene(scene);
			setupAduioSource(scene, audioSource);
			setupOthers(scene);
		}

		/// <summary>
		/// 配置音乐源
		/// </summary>
		/// <param name="audioSource"></param>
		static void setupAduioSource(BaseScene scene, AudioSource audioSource) {
			if (SceneUtils.audioSource == null && audioSource != null) {
				SceneUtils.audioSource = audioSource;
				UnityEngine.Object.DontDestroyOnLoad(audioSource);
			}
			if (SceneUtils.audioSource) {
				SceneUtils.audioSource.loop = true;
				if (scene.bgmClip && SceneUtils.audioSource.clip?.name != scene.bgmClip.name) {
					SceneUtils.audioSource.clip = scene.bgmClip;
					SceneUtils.audioSource.Play();
				}
			}
		}

		/// <summary>
		/// 配置其他要素
		/// </summary>
		static void setupOthers(BaseScene scene) {
			alertWindow = scene.promptLayer.alertWindow;
			loadingWindow = scene.promptLayer.loadingWindow;
			rebuildController = scene.promptLayer.rebuildController;
		}

		#region 场景管理

		/// <summary>
		/// 托管场景物体（键每个单词首字母必须为大写）
		/// </summary>
		/// <param name="key">键</param>
		/// <param name="obj">场景物体</param>
		/// <return>场景物体</return>
		public static UnityEngine.Object depositSceneObject(string key, UnityEngine.Object obj) {
            //if (key == AlertWindowKey) alertWindow = (AlertWindow)obj;
            //if (key == LoadingWindowKey) loadingWindow = (LoadingWindow)obj;
            Debug.Log("depositSceneObject: " + key + ", " + obj);
            if (sceneObjectDeposit.ContainsKey(key))
                sceneObjectDeposit[key] = obj;
            else
                sceneObjectDeposit.Add(key, obj);
            return obj;
        }

        /// <summary>
        /// 获取托管的场景物体
        /// </summary>
        /// <param name="key">键</param>
        /// <return>场景物体</return>
        public static UnityEngine.Object getSceneObject(string key) {
            return hasSceneObject(key) ? sceneObjectDeposit[key] : null;
        }

        /// <summary>
        /// 是否存在托管的场景物体
        /// </summary>
        /// <param name="key">键</param>
        /// <return>场是否存在</return>
        public static bool hasSceneObject(string key) {
            return sceneObjectDeposit.ContainsKey(key) && sceneObjectDeposit[key] != null;
        }

        /// <summary>
        /// 清空场景物体托管
        /// </summary>
        public static void clearSceneObjects() {
            sceneObjectDeposit.Clear();
        }

        #endregion

        #region 公用组件管理

        /// <summary>
        /// 显示提示
        /// </summary>
        /// <param name="msg">提示信息</param>
        /// <param name="btns">按键文本</param>
        /// <param name="actions">按键回调</param>
        static void alert(string text,
            AlertWindow.Type type = AlertWindow.Type.Notice,
            UnityAction onOK = null, UnityAction onCancel = null,
            float duration = AlertWindow.DefaultDuration) {
            Debug.Log("alert: " + text + ":" + alertWindow);
            if (alertWindow) alertWindow.activate(text, type, onOK, onCancel, duration);
            // 若未设置提示窗口，储存这个信息
            else {
                alertText = text;
                onOK?.Invoke();
            }
        }

		/// <summary>
		/// 处理提示请求
		/// </summary>
		/// <param name="req">弹窗请求</param>
		static void processAlertRequest(GameSystem.AlertRequest req) {
            alert(req.text, req.type, req.onOK, req.onCancel, req.duration);
        }

        /// <summary>
        /// 开始加载窗口
        /// </summary>
        /// <param name="tips">加载界面文本</param>
        static void startLoadingWindow(string tips = "") {
            if (loadingWindow) loadingWindow.activate(tips);
        }

        /// <summary>
        /// 设置加载进度
        /// </summary>
        /// <param name="tips">加载界面文本</param>
        static void setupLoadingProgress(double progress = -1) {
            if (loadingWindow) loadingWindow.setProgress(progress);
        }

        /// <summary>
        /// 结束加载窗口
        /// </summary>
        static void endLoadingWindow() {
            if (loadingWindow) loadingWindow.deactivate();
        }

        /// <summary>
        /// 设置加载窗口进度
        /// </summary>
        /// <param name="rate">进度</param>
        static void setLoadingProgress(float rate) {
            if (loadingWindow) loadingWindow.setProgress(rate);
        }

        /// <summary>
        /// 处理加载窗口
        /// </summary>
        /// <param name="tips">加载界面文本</param>
        public static void processLoadingRequest(GameSystem.LoadingRequest req) {
            if (req.start)
                if (req.setProgress)
                    setupLoadingProgress(req.progress);
                else startLoadingWindow(req.text);
            else endLoadingWindow();
        }

        /// <summary>
        /// 申请重建布局
        /// </summary>
        /// <param name="rect"></param>
        public static void registerUpdateLayout(RectTransform rect) {
            rebuildController?.registerUpdateLayout(rect);
        }

        #endregion

        #region 更新管理

        /// <summary>
        /// 更新
        /// </summary>
        public static void update() {
            updateGameSystem();
        }

        /// <summary>
        /// 更新GameSystem
        /// </summary>
        public static void updateGameSystem() {
            updateAlert();
            updateLoading();
            gameSys.update();
        }

        /// <summary>
        /// 更新Alert
        /// </summary>
        static void updateAlert() {
			if (alertWindow && alertWindow.shown) return;
			gameSys.invokeAlertRequest(processAlertRequest);
        }

        /// <summary>
        /// 更新Loading
        /// </summary>
        static void updateLoading() {
			gameSys.invokeLoadingRequest(processLoadingRequest);
        }

		#endregion

		#region Find, GetComponent的封装

		/// <summary>
		/// 获取组件
		/// </summary>
		/// <typeparam name="T">组件类型</typeparam>
		/// <param name="comp">组件对象</param>
		/// <remarks>
		/// 取代 comp.GetComponent<T> 的写法：SceneUtils.get<T>(comp)
		/// </remarks>
		/// <returns>返回获取的组件</returns>
		public static T get<T>(Component comp) {
			return comp == null ? default : comp.GetComponent<T>();
		}
		public static Component get(Component comp, Type type) {
			return comp == null ? default : comp.GetComponent(type);
		}
		/// <summary>
		/// 获取组件
		/// </summary>
		/// <typeparam name="T">组件类型</typeparam>
		/// <param name="obj">物体对象</param>
		/// <remarks>
		/// 取代 obj.GetComponent<T> 的写法：SceneUtils.get<T>(obj)
		/// </remarks>
		/// <returns>返回获取的组件</returns>
		public static T get<T>(GameObject obj) {
			return obj == null ? default : obj.GetComponent<T>();
		}
		public static Component get(GameObject obj, Type type) {
			return obj == null ? default : obj.GetComponent(type);
		}
		
		/// <summary>
		/// 获取组件
		/// </summary>
		/// <typeparam name="T">组件类型</typeparam>
		/// <param name="comp">组件对象</param>
		/// <remarks>
		/// 取代 comp.GetComponent<T> 的写法：SceneUtils.get<T>(comp)
		/// </remarks>
		/// <returns>返回获取的组件</returns>
		public static T[] gets<T>(Component comp) {
			return comp == null ? default : comp.GetComponents<T>();
		}
		public static Component[] gets(Component comp, Type type) {
			return comp == null ? default : comp.GetComponents(type);
		}
		/// <summary>
		/// 获取组件
		/// </summary>
		/// <typeparam name="T">组件类型</typeparam>
		/// <param name="obj">物体对象</param>
		/// <remarks>
		/// 取代 obj.GetComponent<T> 的写法：SceneUtils.get<T>(obj)
		/// </remarks>
		/// <returns>返回获取的组件</returns>
		public static T[] gets<T>(GameObject obj) {
			return obj == null ? default : obj.GetComponents<T>();
		}
		public static Component[] gets(GameObject obj, Type type) {
			return obj == null ? default : obj.GetComponents(type);
		}

		/// <summary>
		/// 获取子物体下的组件
		/// </summary>
		/// <remarks>
		/// 取代 parent.Find(obj).GetComponent<T> 的写法：
		/// SceneUtils.find<T>(obj)
		/// </remarks>
		/// <typeparam name="T">组件类型</typeparam>
		/// <param name="parent">父物体变换对象</param>
		/// <param name="path">寻找路径</param>
		/// <returns>返回查找到的组件</returns>
		public static T find<T>(Transform parent, string path) {
			return get<T>(parent.Find(path));
		}
		public static Component find(
			Transform parent, Type type, string path) {
			return get(parent.Find(path), type);
		}
		/// <summary>
		/// 获取子物体下的组件
		/// </summary>
		/// <remarks>
		/// 取代 parent.Find(path).GetComponent<T> 的写法：
		/// SceneUtils.find<T>(parent, path)
		/// </remarks>
		/// <typeparam name="T">组件类型</typeparam>
		/// <param name="parent">父物体对象</param>
		/// <param name="path">寻找路径</param>
		/// <returns>返回查找到的组件</returns>
		public static T find<T>(GameObject parent, string path) {
			return get<T>(parent.transform.Find(path));
		}
		public static Component find(
			GameObject parent, Type type, string path) {
			return get(parent.transform.Find(path), type);
		}
		/// <summary>
		/// 获取子物体下的组件
		/// </summary>
		/// <remarks>
		/// 取代 comp.transform.Find(path).GetComponent<T> 的写法：
		/// SceneUtils.find<T>(comp, path)
		/// </remarks>
		/// <typeparam name="T">组件类型</typeparam>
		/// <param name="comp">父物体组件对象</param>
		/// <param name="path">寻找路径</param>
		/// <returns>返回查找到的组件</returns>
		public static T find<T>(Component comp, string path) {
			return find<T>(comp.transform, path);
		}
		public static Component find(
			Component comp, Type type, string path) {
			return find(comp.transform, type, path);
		}
		/// <summary>
		/// 获取子物体下的 GameObject
		/// </summary>
		/// <remarks>
		/// 取代 parent.Find(obj).gameObject 的写法：
		/// SceneUtils.find(obj)
		/// </remarks>
		/// <param name="parent">父物体对象</param>
		/// <param name="path">寻找路径</param>
		/// <returns>返回查找到的物体</returns>
		public static GameObject find(Transform parent, string obj) {
            return parent.Find(obj).gameObject;
		}
		/// <summary>
		/// 获取子物体下的 GameObject
		/// </summary>
		/// <remarks>
		/// 取代 parent.transform.Find(obj).gameObject 的写法：
		/// SceneUtils.find(obj)
		/// </remarks>
		/// <param name="parent">父物体对象</param>
		/// <param name="path">寻找路径</param>
		/// <returns>返回查找到的物体</returns>
		public static GameObject find(GameObject parent, string obj) {
			return parent.transform.Find(obj).gameObject;
		}
		/// <summary>
		/// 获取子物体下的 GameObject
		/// </summary>
		/// <remarks>
		/// 取代 parent.transform.Find(obj).gameObject 的写法：
		/// SceneUtils.find(obj)
		/// </remarks>
		/// <param name="comp">父物体组件对象</param>
		/// <param name="path">寻找路径</param>
		/// <returns>返回查找到的物体</returns>
		public static GameObject find(Component comp, string obj) {
			return find(comp.transform, obj);
		}

		/// <summary>
		/// 获取父物体下的 GameObject
		/// </summary>
		/// <remarks>
		/// </remarks>
		/// <typeparam name="T">父物体包含的组件类型</typeparam>
		/// <param name="self">当前物体组件对象</param>
		/// <returns>返回查找到的物体</returns>
		public static T findParent<T>(GameObject self) {
			return findParent<T>(self.transform);
		}
		public static T findParent<T>(Transform self) {
			var parent = self.parent;
			if (parent == null) return default;

			var res = get<T>(parent);
			if (res != null) return res;
			return findParent<T>(parent);
		}
		public static T findParent<T>(Component self) {
			return findParent<T>(self.transform);
		}
		/// <param name="type">父物体包含的组件类型</param>
		public static Component findParent(GameObject self, Type type) {
			return findParent(self.transform, type);
		}
		public static Component findParent(Transform self, Type type) {
			var parent = self.parent;
			if (parent == null) return default;

			var res = get(parent, type);
			if (res != null) return res;
			return findParent(parent, type);
		}
		public static Component findParent(Component self, Type type) {
			return findParent(self.transform, type);
		}
		public static GameObject findParent(GameObject self) {
			return findParent(self.transform);
		}
		public static GameObject findParent(Transform self) {
			var parent = self.parent;
			if (parent == null) return default;

			var res = parent.gameObject;
			if (res != null) return res;
			return findParent(parent);
		}
		public static GameObject findParent(Component self) {
			return findParent(self.transform);
		}

		/// <summary>
		/// 快速获取 Text 组件
		/// </summary>
		/// <param name="t">物体变换对象</param>
		/// <returns>返回该对象的 Text 组件</returns>
		public static Text text(Transform t) {
            return get<Text>(t);
        }
        /// <summary>
        /// 快速获取 Text 组件
        /// </summary>
        /// <param name="obj">物体对象</param>
        /// <returns>返回该对象的 Text 组件</returns>
        public static Text text(GameObject obj) {
            return get<Text>(obj);
        }
        /// <summary>
        /// 快速获取 Button 组件
        /// </summary>
        /// <param name="t">物体变换对象</param>
        /// <returns>返回该对象的 Button 组件</returns>
        public static Button button(Transform t) {
            return get<Button>(t);
        }
        /// <summary>
        /// 快速获取 Button 组件
        /// </summary>
        /// <param name="obj">物体对象</param>
        /// <returns>返回该对象的 Button 组件</returns>
        public static Button button(GameObject obj) {
            return get<Button>(obj);
        }
        /// <summary>
        /// 快速获取 Image 组件
        /// </summary>
        /// <param name="t">物体变换对象</param>
        /// <returns>返回该对象的 Image 组件</returns>
        public static Image image(Transform t) {
            return get<Image>(t);
        }
        /// <summary>
        /// 快速获取 Image 组件
        /// </summary>
        /// <param name="obj">物体对象</param>
        /// <returns>返回该对象的 Image 组件</returns>
        public static Image image(GameObject obj) {
            return get<Image>(obj);
        }
        /// <summary>
        /// 快速获取 Animation 组件
        /// </summary>
        /// <param name="t">物体变换对象</param>
        /// <returns>返回该对象的 Animation 组件</returns>
        public static Animation ani(Transform t) {
            return get<Animation>(t);
        }
        /// <summary>
        /// 快速获取 Animation 组件
        /// </summary>
        /// <param name="obj">物体对象</param>
        /// <returns>返回该对象的 Animation 组件</returns>
        public static Animation ani(GameObject obj) {
            return get<Animation>(obj);
        }

        #endregion

        #region RectTransform操作封装
        /// <summary>
        /// 设置RectTransform的宽度
        /// </summary>
        /// <param name="rt">RectTransform实例</param>
        /// <param name="w">要设置的宽度</param>
        public static void setRectWidth(RectTransform rt, float w) {
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);
        }
        /// <summary>
        /// 设置RectTransform的高度
        /// </summary>
        /// <param name="rt">RectTransform实例</param>
        /// <param name="h">要设置的高度</param>
        public static void setRectHeight(RectTransform rt, float h) {
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);
        }
        #endregion

        #region 时间文本转换封装

        /// <summary>
        /// TimeSpan转字符串（00:00 格式）
        /// </summary>
        /// <param name="span">TimeSpan实例</param>
        /// <returns>转换后字符串</returns>
        public static string time2StrWithDay(TimeSpan span) {
            return string.Format("{0}天{1:00}时{2:00}分{3:00}秒", span.Days, span.Hours, span.Minutes, span.Seconds);
        }

        /// <summary>
        /// TimeSpan转字符串（00:00 格式）
        /// </summary>
        /// <param name="span">TimeSpan实例</param>
        /// <returns>转换后字符串</returns>
        public static string time2Str(TimeSpan span) {
            return string.Format("{0:00}:{1:00}", Math.Floor(span.TotalMinutes), span.Seconds);
        }
        /// <summary>
        /// 秒数转字符串（00:00 格式）
        /// </summary>
        /// <param name="span">TimeSpan实例</param>
        /// <returns>转换后字符串</returns>
        public static string time2Str(int sec) {
            return string.Format("{0:00}:{1:00}", sec / 60, sec % 60);
        }
        public static string time2Str(double sec) {
            sec = Math.Round(sec, 2);
            return string.Format("{0:00}:{1:00.00}", (int)sec / 60, sec % 60);
        }

        /// <summary>
        /// TimeSpan转字符串（00:00:00 格式）
        /// </summary>
        /// <param name="span">TimeSpan实例</param>
        /// <returns>转换后字符串</returns>
        public static string time2StrWithHour(TimeSpan span) {
            return string.Format("{0}:{1:00}:{2:00}", Math.Floor(span.TotalHours), span.Minutes, span.Seconds);
        }
        /// <summary>
        /// 秒数转字符串（00:00:00 格式）
        /// </summary>
        /// <param name="span">TimeSpan实例</param>
        /// <returns>转换后字符串</returns>
        public static string time2StrWithHour(int sec) {
            return string.Format("{0}:{1:00}:{2:00}", sec / 60 / 60, sec / 60 % 60, sec % 60);
        }
        public static string time2StrWithHour(double sec) {
            sec = Math.Round(sec, 2);
            return string.Format("{0}:{1:00}:{2:00}", (int)sec / 60 / 60,
                (int)sec / 60 % 60, sec % 60);
        }

        #endregion

        #region 浮点数转换操作

        /// <summary>
        /// 浮点数转化为字符串
        /// </summary>
        /// <param name="value">浮点数</param>
        /// <returns>字符串</returns>
        public static string double2Str(double value, bool intAdj = false) {
            if (intAdj && value == (int)value)
                return string.Format("{0}", (int)value);
            return string.Format("{0:0.00}", value);
        }

        /// <summary>
        /// 浮点数转化为百分数
        /// </summary>
        /// <param name="value">浮点数</param>
        /// <returns>百分数字符串</returns>
        public static string double2Perc(double value, bool intAdj = false) {
            if (intAdj && value == (int)value)
                return string.Format("{0}%", (int)value * 100);
            return string.Format("{0:0.00}%", value * 100);
        }

        /// <summary>
        /// 浮点数转化为百分数（四舍五入）
        /// </summary>
        /// <param name="value">浮点数</param>
        /// <returns>百分数（四舍五入）字符串</returns>
        public static string double2RoundedPerc(double value) {
            return string.Format("{0:0}%", value * 100);
        }

        #endregion

        #region 颜色转化

        /// <summary>
        /// 十六进制字符串转颜色
        /// </summary>
        /// <param name="str">十六进制字符串，形如“#ABCDEF”</param>
        /// <returns>颜色</returns>
        public static Color str2Color(string str) {
            int index = 0;
            float[] c = { 0, 0, 0, 1 };
            str = str.ToUpperInvariant();
            string reg = @"[0-9A-F][0-9A-F]";
            foreach (Match match in Regex.Matches(str, reg))
                c[index++] = Convert.ToInt32(match.Value, 16) / 255.0f;
            return new Color(c[0], c[1], c[2], c[3]);
        }

        /// <summary>
        /// 十六进制字符串转颜色
        /// </summary>
        /// <param name="str">十六进制字符串，形如“#ABCDEF”</param>
        /// <returns>颜色</returns>
        public static string color2Str(Color c) {
            var r = (int)(c.r * 255);
            var g = (int)(c.g * 255);
            var b = (int)(c.b * 255);
            return string.Format("#{0:X}{1:X}{2:X}", r, g, b);
        }

        #endregion

        #region 其他文本操作

        /// <summary>
        /// 获取JSON解码后文本
        /// </summary>
        /// <param name="oriText">原文本</param>
        /// <returns>解码后文本</returns>
        public static string decodedJsonText(string oriText, int maxLength, bool adjuest = true) {
            var decoded = Regex.Unescape(oriText);
            if (decoded.Length > maxLength)
                decoded = decoded.Substring(0, maxLength) + "...";
            return adjuest ? adjustText(decoded) : decoded;
        }

        /// <summary>
        /// 数值转化为增量数值
        /// </summary>
        /// <param name="value">数值</param>
        /// <returns>增量数值字符串</returns>
        public static string int2Incr(int value) {
            if (value >= 0) return "+" + value;
            return value.ToString();
        }

        /// <summary>
        /// 数值转化为增量数值
        /// </summary>
        /// <param name="value">数值</param>
        /// <returns>增量数值字符串</returns>
        public static string double2Incr(double value) {
            if (value >= 0) return "+" + double2Str(value);
            return double2Str(value);
        }

        /// <summary>
        /// 文本调整（替换掉自动换行的空格符）
        /// </summary>
        /// <param name="text">文本</param>
        /// <returns>调整后的文本</returns>
        public static string adjustText(string text) {
            text = Regex.Replace(text, @"(?<=<.*?) (?=.*?>)", QuestionText.SpaceIdentifier);
            text = text.Replace(" ", QuestionText.SpaceEncode);
            text = text.Replace(QuestionText.SpaceIdentifier, " ");
            return text.ToString();
        }

		#endregion

		#region 坐标转换

		/// <summary>
		/// 屏幕坐标到本地坐标
		/// </summary>
		/// <param name="pos"></param>
		/// <param name="rt"></param>
		/// <returns></returns>
		public static Vector2 screen2Local(
			Vector2 pos, RectTransform rt, Camera camera = null) {
			Vector2 res; if (camera == null) camera = Camera.main;
			Debug.Log("camera: " + camera + ", Camera.main: " + Camera.main + ", Camera.current: " + Camera.current);

			if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
				rt, pos, camera, out res)) return res;
			return pos;
		}

		#endregion
	}
}