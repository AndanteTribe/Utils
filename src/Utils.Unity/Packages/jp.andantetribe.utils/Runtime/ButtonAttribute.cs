#nullable enable

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace AndanteTribe.Utils
{
    /// <summary>
    /// 指定したメソッドをUnityのInspector上に表示したボタンでテスト実行できるようになる属性.
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class ButtonAttribute : Attribute
    {
        /// <summary>
        /// 引数.
        /// </summary>
        public readonly object[] Parameters;

        /// <summary>
        /// ボタンの名前.
        /// </summary>
        public readonly string ButtonName;

        /// <summary>
        /// Initializes a new instance of the <see cref="ButtonAttribute"/> class.
        /// </summary>
        /// <param name="buttonName">ボタンの名前.デフォルト値は対象メソッド名.</param>
        /// <param name="parameters">引数.</param>
        public ButtonAttribute([CallerMemberName] string buttonName = "", params object[] parameters)
        {
            this.ButtonName = buttonName;
            this.Parameters = parameters;
        }
    }
}