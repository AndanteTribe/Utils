#if ENABLE_UNITASK
#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using UnityEngine;

namespace AndanteTribe.Utils.Unity.Tasks
{
    /// <summary>
    /// 進捗率表示ロードのUIを実装するプレゼンター.
    /// </summary>
    /// <remarks>
    /// モックなどで活用できる.
    /// </remarks>
    public class LoadProgressPresenter : IProgress<float>, IInitializable
    {
        private readonly AsyncGUITrigger _trigger;

        private float _progressValue;

        /// <summary>
        /// Initialize a new instance of <see cref="LoadProgressPresenter"/>.
        /// </summary>
        /// <param name="gameObject"><see cref="AsyncGUITrigger"/>を取得するためのゲームオブジェクト.</param>
        public LoadProgressPresenter(GameObject gameObject) => _trigger = gameObject.GetAsyncGUITrigger();

        /// <inheritdoc />
        public async ValueTask InitializeAsync(CancellationToken cancellationToken)
        {
            await foreach (var _ in _trigger.WithCancellation(cancellationToken))
            {
                if (_progressValue >= 1f)
                {
                    break;
                }

                var bgStyle = new GUIStyle(GUI.skin.box)
                {
                    normal = { background = Texture2D.grayTexture }
                };

                var width = Screen.width * 0.8f;
                var height = Screen.height * 0.8f;
                var x = (Screen.width - width) / 2f;
                var y = (Screen.height - height) / 2f;
                GUI.Box(new Rect(x, y, width, height), GUIContent.none, bgStyle);

                var labelStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 50,
                    normal = { textColor = Color.white }
                };
                GUI.Label(new Rect(x, y, width, height), $"ロード中 {_progressValue * 100}% ※後でロード画面作る必要あり", labelStyle);
            }
        }

        /// <inheritdoc />
        public void Report(float value) => _progressValue = Math.Clamp(value, 0f, 1f);
    }
}

#endif