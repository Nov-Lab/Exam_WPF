// @(h)WinSnowfallApp.xaml.cs ver 0.00 ( '24.06.30 Nov-Lab ) 作成開始
// @(h)WinSnowfallApp.xaml.cs ver 0.51 ( '24.07.02 Nov-Lab ) ベータ版完成
// @(h)WinSnowfallApp.xaml.cs ver 0.51a( '24.07.03 Nov-Lab ) その他  ：コメント整理

// @(s)
// 　【雪景色ビューアー画面】雪景色のような風景を描くビューアーアプリのメイン画面です。

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

using System.IO.MemoryMappedFiles;
using System.Threading;

using NovLab;


namespace Exam_WPF
{
    //====================================================================================================
    /// <summary>
    /// 【雪景色ビューアー画面】雪景色のような風景を描くビューアーアプリのメイン画面です。
    /// </summary>
    /// <remarks>
    /// 画面構成について<br/>
    /// ・<see cref="WinSnowfallApp"/>：デバッグ対象プログラムを想定した雪景色ビューアーメイン画面です。<br/>
    /// ・<see cref="WinSnowfallDebugger"/>：デバッグ用プログラムを想定した雪景色デバッガー画面です。<br/>
    /// ※本来は、デバッグ対象プログラムとデバッグ用プログラムはプロジェクトを分けて別々に作るべきですが、
    ///   本サンプルでは簡素化のため１つのプロジェクトにまとめてあり、コマンドライン引数で起動モードを切り替えるようにしています。<br/>
    /// </remarks>
    //====================================================================================================
    public partial class WinSnowfallApp : Window
    {
        //====================================================================================================
        // 内部フィールド
        //====================================================================================================

#if DEBUG
        /// <summary>
        /// 【共有メモリー用メモリマップトファイル】共有メモリーとして使用するメモリマップトファイルです。
        /// (DEBUGビルドのみ)
        /// </summary>
        protected MemoryMappedFile m_memoryMappedFile;
#endif

        /// <summary>
        /// 【アニメーションタイマー】アニメーション動作を進行させるためのタイマーです。
        /// </summary>
        protected readonly DispatcherTimer m_tmrAnimation = new DispatcherTimer(DispatcherPriority.Normal);


        //====================================================================================================
        // フォームイベント
        //====================================================================================================

        //--------------------------------------------------------------------------------
        /// <summary>
        /// 【コンストラクター】雪景色ビューアー画面の新しいインスタンスを生成します。
        /// (自動生成された内容のままです。)
        /// </summary>
        //--------------------------------------------------------------------------------
        public WinSnowfallApp()
        {
            InitializeComponent();
        }


        //--------------------------------------------------------------------------------
        /// <summary>
        /// 【ウィンドウ_Loaded イベント】本フォームを初期化します。<br/>
        /// ・<see cref="FrameworkElement.Loaded"/> のイベントハンドラーです。<br/>
        /// </summary>
        //--------------------------------------------------------------------------------
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //------------------------------------------------------------
            /// (DEBUGビルドの場合のみ)共有メモリーを初期化する
            //------------------------------------------------------------
#if DEBUG
            try
            {                                                           //// try開始
                m_memoryMappedFile =                                    /////  共有メモリー用メモリマップトファイルを生成する
                    MemoryMappedFile.CreateNew(SharedMemory.MAPNAME, SharedMemory.CAPACTY);
            }
            catch (Exception ex)
            {                                                           //// catch：すべての例外(二重起動など)

                MessageBox.Show($"{ex.GetType().Name}:{ex.Message}:0x{ex.HResult.ToString("x")}",
                    this.Title,
                    MessageBoxButton.OK, MessageBoxImage.Error);        /////  エラーメッセージボックスを表示する
                Close();                                                /////  本フォームを閉じる
            }
#endif


            // ＜メモ＞
            // ・デバッグ用プログラムは複数起動可能だが、この処理では１つも動作していない場合にだけ自動的に起動する。
            //------------------------------------------------------------
            /// (DEBUGビルドの場合のみ)デバッグ用プログラムが動作していない場合は起動する
            //------------------------------------------------------------
#if DEBUG
            var debuggerRunning = Mutex.TryOpenExisting(
                AppBase.MUTEXNAME_DEBUGGER_RUNNING, out Mutex mutex);   //// デバッガー動作中ミューテックスをオープン試行する
            if (debuggerRunning)
            {                                                           //// オープンできた場合(デバッグ用プログラムが動作中の場合)
                mutex.Dispose();                                        /////  オープンしたミューテックスを廃棄する
            }
            else
            {                                                           //// オープンできなかった場合(デバッグ用プログラムが動作中でない場合)
                AppBase.LaunchDebugger();                               /////  デバッガー起動処理を行う
            }
#endif


            //------------------------------------------------------------
            /// 本フォームを初期化する
            //------------------------------------------------------------
            M_InitializeAnimation();                                    //// アニメーション情報初期化処理を行う

            SceneRenderBox.DoRender = M_DoRender;                       //// 風景描画ボックスに描画実行コールバックを設定する
            SceneRenderBox.InvalidateVisual();                          //// 風景描画ボックスを無効化して再描画する(初回の描画を行う)

            m_tmrAnimation.Interval = TimeSpan.FromMilliseconds(15);    //// アニメーションタイマーを初期化して開始する
            m_tmrAnimation.Tick += TmrAnimation_Tick;
            m_tmrAnimation.Start();


#if DEBUG                                                               //// DEBUGビルドの場合
            MnuDebug.Visibility = Visibility.Visible;                   /////  デバッグメニューを表示する
#else                                                                   //// DEBUGビルドでない場合
            MnuDebug.Visibility = Visibility.Hidden;                    /////  デバッグメニューを非表示にする
#endif
        }


        //--------------------------------------------------------------------------------
        /// <summary>
        /// 【ウィンドウ_Closed】本ウィンドウの後始末をします。
        /// </summary>
        //--------------------------------------------------------------------------------
        private void Window_Closed(object sender, EventArgs e)
        {
            //------------------------------------------------------------
            /// 本ウィンドウの後始末をする
            //------------------------------------------------------------
            m_tmrAnimation.Stop();                                      //// アニメーションタイマーを停止する


            //------------------------------------------------------------
            /// (DEBUGビルドの場合のみ)共有メモリーを破棄する
            //------------------------------------------------------------
#if DEBUG
            if (m_memoryMappedFile != null)
            {                                                           //// 共有メモリー用メモリマップトファイルを生成してある場合
                m_memoryMappedFile.Dispose();                           /////  共有メモリー用メモリマップトファイルを破棄する
                m_memoryMappedFile = null;                              /////  共有メモリー用メモリマップトファイル = null にクリアする
            }
#endif
        }


        //--------------------------------------------------------------------------------
        /// <summary>
        /// 【デバッグ - デバッグ用プログラムを起動メニュー_Click】
        /// デバッグ用プログラムを起動します。
        /// </summary>
        //--------------------------------------------------------------------------------
        private void MnuDebug_LaunchDebugger_Click(object sender, RoutedEventArgs e)
        {
            AppBase.LaunchDebugger();
        }


        //====================================================================================================
        // DEBUGビルド用内部メソッド
        //====================================================================================================

#if DEBUG
        //--------------------------------------------------------------------------------
        /// <summary>
        /// 【モニタリングデータ更新】共有メモリー内のモニタリングデータを更新します。
        /// </summary>
        //--------------------------------------------------------------------------------
        protected void M_UpdateMonitoringData()
        {
            //------------------------------------------------------------
            /// 共有メモリー内のモニタリングデータを更新する
            //------------------------------------------------------------
            var monitoringData =
                new MonitoringData(m_frameCount, m_snowflakes);         //// モニタリングデータを生成する
            var xmlString = XmlUtil.ToXmlString(monitoringData);        //// モニタリングデータからXML文字列を作成する
            SharedMemory.Write(xmlString);                              //// XML文字列を共有メモリーに書き込む
        }
#endif


        //====================================================================================================
        // 雪景色アニメーション関連
        //====================================================================================================

        /// <summary>
        /// 【経過フレーム数】アプリケーションを起動してからの経過フレーム数です。
        /// </summary>
        protected int m_frameCount = 0;

        /// <summary>
        /// 【雪片アニメーション配列】
        /// </summary>
        protected SnowflakeAnimation[] m_snowflakes = new SnowflakeAnimation[30];


        //--------------------------------------------------------------------------------
        /// <summary>
        /// 【アニメーション情報初期化】アニメーション情報を初期化します。
        /// </summary>
        //--------------------------------------------------------------------------------
        protected void M_InitializeAnimation()
        {
            //------------------------------------------------------------
            /// アニメーション情報を初期化する
            //------------------------------------------------------------
            for (int tmpIndex = 0; tmpIndex < m_snowflakes.Length; tmpIndex++)
            {                                                           //// 雪片アニメーション配列を繰り返す
                m_snowflakes[tmpIndex] = new SnowflakeAnimation();      /////  インスタンスを生成する
            }
        }


        //--------------------------------------------------------------------------------
        /// <summary>
        /// 【アニメーションタイマー_Tick】雪景色アニメーションを進行させます。
        /// </summary>
        //--------------------------------------------------------------------------------
        private void TmrAnimation_Tick(object sender, EventArgs e)
        {
            //------------------------------------------------------------
            /// 経過フレーム数を進める
            //------------------------------------------------------------
            if (m_frameCount == int.MaxValue)
            {                                                           //// 経過フレーム数 = 最大値 の場合
                m_frameCount = 0;                                       /////  経過フレーム数 = 0 にリセットする
            }
            else
            {                                                           //// 経過フレーム数 = 最大値 でない場合
                m_frameCount++;                                         /////  経過フレーム数に１加算する
            }


            //------------------------------------------------------------
            /// 降雪風景アニメーションを進行させる
            //------------------------------------------------------------
            foreach (var tmpItem in m_snowflakes)
            {                                                           //// 雪片アニメーション配列を繰り返す
                tmpItem.MoveAnimation();                                /////  アニメーション動作処理を行う
            }

            SceneRenderBox.InvalidateVisual();                          //// 風景描画ボックスを無効化して再描画する


            //------------------------------------------------------------
            /// (DEBUGビルドの場合のみ)モニタリングデータ更新処理を行う
            //------------------------------------------------------------
#if DEBUG
            M_UpdateMonitoringData();
#endif
        }


        //--------------------------------------------------------------------------------
        /// <summary>
        /// 【描画実行】雪景色を描画します。
        /// </summary>
        /// <param name="dc">        <inheritdoc cref="CtlRenderBox.DoRenderCallback" path="/param[@name='dc']"/></param>
        /// <param name="renderSize"><inheritdoc cref="CtlRenderBox.DoRenderCallback" path="/param[@name='renderSize']"/></param>
        //--------------------------------------------------------------------------------
        protected void M_DoRender(DrawingContext dc, Size renderSize)
        {
            //------------------------------------------------------------
            /// 雪景色を描画する
            //------------------------------------------------------------
            foreach (var tmpItem in m_snowflakes)
            {                                                           //// 雪片アニメーション配列を繰り返す
                var x = (renderSize.Width * tmpItem.xPos / 100)
                      - (tmpItem.diameter / 2);                         /////  描画領域内でのX座標を算出する
                var y = (renderSize.Height * tmpItem.yPos / 100)
                      - (tmpItem.diameter / 2);                         /////  描画領域内でのY座標を算出する

                dc.DrawEllipse(Brushes.White, null, new Point(x, y),
                        tmpItem.diameter / 2, tmpItem.diameter / 2);    /////  雪片を描画する
            }
        }

    } // class

} // namespace
