// @(h)App.xaml.cs ver 0.00 ( '24.06.30 Nov-Lab ) 作成開始
// @(h)App.xaml.cs ver 0.51 ( '24.07.01 Nov-Lab ) ベータ版完成
// @(h)App.xaml.cs ver 0.51a( '24.07.02 Nov-Lab ) その他  ：コメント整理

// @(s)
// 　【アプリケーション】WPFアプリケーションの本体部分です。

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Threading;
using System.Windows;


namespace Exam_WPF
{
    //====================================================================================================
    /// <summary>
    /// 【アプリケーション】WPFアプリケーションの本体部分です。
    /// </summary>
    //====================================================================================================
    public partial class App : Application
    {
        //--------------------------------------------------------------------------------
        /// <summary>
        /// 【アプリケーション_Startup イベント】アプリケーションを開始します。<br/>
        /// ・<see cref="Application.Startup"/> のイベントハンドラーです。<br/>
        /// </summary>
        //--------------------------------------------------------------------------------
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            //------------------------------------------------------------
            /// 起動モードに対応する画面を実行する
            //------------------------------------------------------------
            if (AppBase.IsDebuggerMode(e.Args))
            {                                                           //// 起動モード = デバッガーモード の場合
                var debugWindow = new WinSnowfallDebugger();            /////  雪景色デバッガー画面を実行する
                debugWindow.Show();
            }
            else
            {                                                           //// 起動モード = デバッガーモード でない場合(通常モードの場合)
                var appWindow = new WinSnowfallApp();                   /////  雪景色ビューアーメイン画面を実行する
                appWindow.Show();
            }
        }

    } // class

} // namespace
