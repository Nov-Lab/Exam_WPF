// @(h)SnowflakeInfoItem.cs ver 0.00 ( '24.07.01 Nov-Lab ) 作成開始
// @(h)SnowflakeInfoItem.cs ver 0.51 ( '24.07.02 Nov-Lab ) ベータ版完成
// @(h)SnowflakeInfoItem.cs ver 0.51a( '24.07.03 Nov-Lab ) その他  ：コメント整理

// @(s)
// 　【雪片情報リストビュー項目】雪片１つ分のモニタリングデータをリストビューに表示するためのバインディングソースです。

using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Windows.Controls;


namespace Exam_WPF
{
    //====================================================================================================
    /// <summary>
    /// 【雪片情報リストビュー項目】雪片１つ分のモニタリングデータをリストビューに表示するためのバインディングソースです。
    /// </summary>
    /// <remarks>
    /// 補足<br/>
    /// ・<see cref="ListView"/> コントロールの <see cref="ItemsControl.ItemsSource"/> に設定するコレクションの要素として使います。<br/>
    /// ・プロパティーを変更するとバインディング先であるリストビューにも反映されます。<br/>
    /// </remarks>
    //====================================================================================================
    public partial class SnowflakeInfoItem : INotifyPropertyChanged
    {
        //====================================================================================================
        // 公開プロパティー
        //====================================================================================================

        /// <summary>
        /// 【インデックス】
        /// </summary>
        /// <remarks>
        /// 補足<br/>
        /// ・このプロパティーを変更すると、バインディング先にも通知・反映されます。<br/>
        /// </remarks>
        public int Index
        {
            get => bf_index;
            set => M_SetProperty(nameof(Index), ref bf_index, value);
        }
        protected int bf_index;


        // 【X位置】
        /// <summary>
        /// <inheritdoc cref="SnowflakeAnimation.xPos"/>
        /// </summary>
        /// <remarks><inheritdoc cref="Index"/></remarks>
        public float XPos
        {
            get => bf_xPos;
            set => M_SetProperty(nameof(XPos), ref bf_xPos, value);
        }
        protected float bf_xPos;


        // 【Y位置】
        /// <summary>
        /// <inheritdoc cref="SnowflakeAnimation.yPos"/>
        /// </summary>
        /// <remarks><inheritdoc cref="Index"/></remarks>
        public float YPos
        {
            get => bf_yPos;
            set => M_SetProperty(nameof(YPos), ref bf_yPos, value);
        }
        protected float bf_yPos;


        // 【角度値】
        /// <summary>
        /// <inheritdoc cref="SnowflakeAnimation.degree"/>
        /// </summary>
        /// <remarks><inheritdoc cref="Index"/></remarks>
        public float Degree
        {
            get => bf_degree;
            set => M_SetProperty(nameof(Degree), ref bf_degree, value);
        }
        protected float bf_degree;


        //====================================================================================================
        // INotifyPropertyChanged I/F の実装
        //====================================================================================================

        /// <summary>
        /// 【PropertyChanged イベント】プロパティ値が変更されたときに発生します。
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;


        //====================================================================================================
        // 内部メソッド
        //====================================================================================================

        //--------------------------------------------------------------------------------
        /// <summary>
        /// 【プロパティ値設定】プロパティに値を設定します。
        /// 現在値と同じ場合は何もせず、現在値と異なる場合は値を変更してプロパティ変更通知を行い、バインディング先に変更を反映します。
        /// </summary>
        /// <typeparam name="T">プロパティ値の型</typeparam>
        /// <param name="propertyName">[in ]：プロパティ名(nameof(＜プロパティ＞))</param>
        /// <param name="backingField">[ref]：プロパティ値を格納するバッキングフィールド</param>
        /// <param name="value">       [in ]：設定する値</param>
        //--------------------------------------------------------------------------------
        protected void M_SetProperty<T>(string propertyName, ref T backingField, T value)
        {
            //------------------------------------------------------------
            /// プロパティに値を設定する
            //------------------------------------------------------------
            if (backingField.Equals(value) == false)
            {                                                           //// 現在値と異なる場合
                backingField = value;                                   /////  プロパティ値を変更する
                M_NotifyPropertyChanged(propertyName);                  /////  プロパティ変更通知処理を行う
            }
        }


        //--------------------------------------------------------------------------------
        /// <summary>
        /// 【プロパティー変更通知】
        /// <see cref="INotifyPropertyChanged.PropertyChanged"/> イベントを発生させて、バインディング先に変更を反映します。
        /// </summary>
        /// <param name="propertyName">[in ]：変更のあったプロパティー名[null または 空文字列 = すべてのプロパティーが変更された]</param>
        //--------------------------------------------------------------------------------
        protected void M_NotifyPropertyChanged(string propertyName)
        {
            //------------------------------------------------------------
            /// PropertyChanged イベントを発生させる
            //------------------------------------------------------------
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    } // class

} // namespace
