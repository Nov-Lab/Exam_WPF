// @(h)WinSnowfallDebugger.xaml.cs ver 0.00 ( '24.07.01 Nov-Lab ) 作成開始
// @(h)WinSnowfallDebugger.xaml.cs ver 0.51 ( '24.07.02 Nov-Lab ) ベータ版完成
// @(h)WinSnowfallDebugger.xaml.cs ver 0.51a( '24.07.03 Nov-Lab ) その他  ：コメント整理

// @(s)
// 　【雪景色デバッガー画面】雪景色ビューアーのデバッグ用画面です。

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Collections.ObjectModel;

using NovLab;


namespace Exam_WPF
{
    //====================================================================================================
    /// <summary>
    /// 【雪景色デバッガー画面】雪景色ビューアーのデバッグ用画面です。
    /// 雪景色ビューアーの動作状況をリアルタイムにモニタリングすることができます。
    /// </summary>
    /// <remarks><inheritdoc cref="WinSnowfallApp"/></remarks>
    //====================================================================================================
    public partial class WinSnowfallDebugger : Window
    {
        //====================================================================================================
        // 内部フィールド
        //====================================================================================================

        /// <summary>
        /// 【デバッガー動作中ミューテックス】
        /// </summary>
        /// <remarks>
        /// <inheritdoc cref="AppBase.MUTEXNAME_DEBUGGER_RUNNING"/>
        /// </remarks>
        protected Mutex m_runningMutex;

        /// <summary>
        /// 【更新タイマー】モニタリングデータを更新するためのタイマーです。
        /// </summary>
        protected readonly DispatcherTimer m_tmrRefresh = new DispatcherTimer(DispatcherPriority.Normal);

        /// <summary>
        /// 【経過フレーム数最終値】モニタリング情報を最後に更新したときの経過フレーム数です。
        /// </summary>
        /// <remarks>
        /// 補足<br/>
        /// ・前回の更新時から内容が変化しているかどうかの判定に使用します。<br/>
        /// </remarks>
        protected int m_lastFrameCount = 0;

        /// <summary>
        /// 【雪片情報コレクション】複数の雪片情報をリストビューに表示するためのコレクションです。<br/>
        /// ・<see cref="ListView"/> コントロールの <see cref="ItemsControl.ItemsSource"/> に設定するバインディングソースです。<br/>
        /// </summary>
        protected readonly ObservableCollection<SnowflakeInfoItem> m_snowflakeInfos = new ObservableCollection<SnowflakeInfoItem>();


        //====================================================================================================
        // フォームイベント
        //====================================================================================================

        //--------------------------------------------------------------------------------
        /// <summary>
        /// 【コンストラクター】雪景色デバッガー画面の新しいインスタンスを生成します。
        /// (自動生成された内容のままです。)
        /// </summary>
        //--------------------------------------------------------------------------------
        public WinSnowfallDebugger()
        {
            InitializeComponent();
        }


        //--------------------------------------------------------------------------------
        /// <summary>
        /// 【ウィンドウ_Loaded】本ウィンドウを初期化します。<br/>
        /// ・<see cref="FrameworkElement.Loaded"/> のイベントハンドラーです。<br/>
        /// </summary>
        //--------------------------------------------------------------------------------
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //------------------------------------------------------------
            /// デバッガー動作中ミューテックスを生成する
            //------------------------------------------------------------
            m_runningMutex =
                new Mutex(false, AppBase.MUTEXNAME_DEBUGGER_RUNNING);


            //------------------------------------------------------------
            /// 本ウィンドウを初期化する
            //------------------------------------------------------------
            LvwSnowflakeInfo.ItemsSource = m_snowflakeInfos;            //// 雪片情報コレクションをリストビューのバインディングソースに設定する

            m_tmrRefresh.Interval = TimeSpan.FromMilliseconds(15);      //// 更新タイマーを初期化して開始する
            m_tmrRefresh.Tick += M_tmrRefresh_Tick;
            m_tmrRefresh.Start();
        }


        //--------------------------------------------------------------------------------
        /// <summary>
        /// 【ウィンドウ_Closed】本ウィンドウの後始末をします。<br/>
        /// ・<see cref="Window.Closed"/> のイベントハンドラーです。<br/>
        /// </summary>
        //--------------------------------------------------------------------------------
        private void Window_Closed(object sender, EventArgs e)
        {
            //------------------------------------------------------------
            /// 本ウィンドウの後始末をする
            //------------------------------------------------------------
            m_tmrRefresh.Stop();                                        //// 更新タイマーを停止する


            //------------------------------------------------------------
            /// デバッガー動作中ミューテックスを破棄する
            //------------------------------------------------------------
            if (m_runningMutex != null)
            {                                                           //// デバッガー動作中ミューテックスを生成してある場合
                m_runningMutex.Close();                                 /////  デバッガー動作中ミューテックスを閉じる
                m_runningMutex = null;                                  /////  デバッガー動作中ミューテックス = null にクリアする
            }
        }


        //--------------------------------------------------------------------------------
        /// <summary>
        /// 【更新タイマー_Tick】モニタリングデータを更新します。
        /// </summary>
        //--------------------------------------------------------------------------------
        private void M_tmrRefresh_Tick(object sender, EventArgs e)
        {
            //------------------------------------------------------------
            /// モニタリングデータを更新する
            //------------------------------------------------------------
            if (MnuMonitoring_Pause.IsChecked == false)
            {                                                           //// モニタリング-一時停止メニューがチェック状態でない場合
                M_Refresh();                                            /////  モニタリングデータ更新処理を行う
            }
        }


        //====================================================================================================
        // 内部メソッド
        //====================================================================================================

        //--------------------------------------------------------------------------------
        /// <summary>
        /// 【モニタリングデータ更新】共有メモリーから最新のモニタリングデータを取得して画面に表示します。
        /// </summary>
        //--------------------------------------------------------------------------------
        protected void M_Refresh()
        {
            //------------------------------------------------------------
            /// 共有メモリーから最新のモニタリングデータを取得する
            //------------------------------------------------------------
            MonitoringData monitoringData = null;                       //// モニタリングデータ = null に初期化する


            var xmlString = SharedMemory.Read();                        //// 共有メモリーからXML文字列を読み取る
            if (xmlString != null)
            {                                                           //// 読み取り成功の場合
                if (xmlString != string.Empty)
                {                                                       /////  XML文字列が空文字列でない場合(メモリマップトファイルが空っぽでない場合)
                    monitoringData =                                    //////   XML文字列を解析してモニタリングデータを取得する(解析失敗時はnullになる)
                        XmlUtil.ParseXmlString<MonitoringData>(xmlString);
                }
            }


            if (monitoringData == null)
            {                                                           //// モニタリングデータ = null の場合(読み取り失敗か、空っぽだったか、内容が正しくない場合)
                TxtFrameCount.Text =
                    "モニタリングデータを取得できません。";             /////  エラーメッセージ(モニタリングデータ取得失敗)を表示する
                return;                                                 /////  関数終了
            }


            //------------------------------------------------------------
            /// モニタリングデータを画面に表示する
            //------------------------------------------------------------
            if (monitoringData.frameCount == m_lastFrameCount)
            {                                                           //// 経過フレーム数が変化していない場合
                return;                                                 /////  画面を更新せずに関数終了
            }
            m_lastFrameCount = monitoringData.frameCount;               //// 経過フレーム数最終値 = 経過フレーム数 に更新する


            TxtFrameCount.Text = m_lastFrameCount.ToString();           //// 経過フレーム数を画面に表示する


            //----------------------------------------
            // 雪片の増減をバインディングソースに反映
            //----------------------------------------
            //[-] 保留：削除パターンは未テスト(雪片の数が減少するパターン自体が未実装なので)
            while (m_snowflakeInfos.Count > monitoringData.snowflakes.Length)
            {                                                           //// 雪片情報コレクションの要素数が過剰な間、繰り返す
                m_snowflakeInfos.RemoveAt(m_snowflakeInfos.Count - 1);  /////  末尾の要素を削除する
            }

            while (m_snowflakeInfos.Count < monitoringData.snowflakes.Length)
            {                                                           //// 雪片情報コレクションの要素数が不足している間、繰り返す
                var newItem = new SnowflakeInfoItem();                  /////  雪片情報リストビュー項目を生成する
                newItem.Index = m_snowflakeInfos.Count;                 /////  インデックスを設定する

                m_snowflakeInfos.Add(newItem);                          /////  雪片情報コレクションに追加する
            }


            //----------------------------------------
            // 雪片の現在値をバインディングソースに反映
            //----------------------------------------
            var tmpIndex = 0;                                           /////  インデックス = 0 に初期化する
            foreach (var snowflake in monitoringData.snowflakes)
            {                                                           /////  雪片アニメーション配列を繰り返す
                m_snowflakeInfos[tmpIndex].XPos = snowflake.xPos;       //////   X位置の値を更新する
                m_snowflakeInfos[tmpIndex].YPos = snowflake.yPos;       //////   Y位置の値を更新する
                m_snowflakeInfos[tmpIndex].Degree = snowflake.degree;   //////   角度値を更新する

                tmpIndex++;                                             //////   インデックスに１加算する
            }
        }

    } // class

} // namespace
