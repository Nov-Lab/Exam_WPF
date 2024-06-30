// @(h)SharedMemory.cs ver 0.00 ( '24.06.19 Nov-Lab ) 作成開始
// @(h)SharedMemory.cs ver 0.51 ( '24.06.24 Nov-Lab ) ベータ版完成
// @(h)SharedMemory.cs ver 0.51a( '24.06.27 Nov-Lab ) その他  ：コメント整理

// @(s)
// 　【共有メモリー】共有メモリーを操作するための機能を提供します。

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO;
using System.IO.MemoryMappedFiles;


// 名前空間とメモリマップトファイル名以外は Windows フォーム版と同じです。
// Windows フォーム版と同時実行して動作を比較できるように、メモリマップトファイル名を変えています。
namespace Exam_WPF
{
    //====================================================================================================
    /// <summary>
    /// 【共有メモリー】共有メモリーを操作するための機能を提供します。
    /// </summary>
    //====================================================================================================
    public static partial class SharedMemory
    {
        //====================================================================================================
        // 公開定数定義
        //====================================================================================================

        /// <summary>
        /// 【共有メモリー用メモリマップトファイル名】共有メモリーとして使用するメモリマップトファイルの名前です。
        /// </summary>
        /// <remarks>
        /// 補足<br/>
        /// ・共有メモリー用メモリマップトファイルの作成は、デバッグ対象プログラム側で行います。
        ///   デバッグ用プログラム側は、既存のメモリマップトファイルを開きます。<br/>
        /// </remarks>
        public const string MAPNAME = "SnowfallMonitoringDataWPF";

        /// <summary>
        /// 【メモリマップトファイルの最大容量】
        /// </summary>
        public const int CAPACTY = 1000 * 1000; // 1MB(余裕を持たせて大きめにしておく)


        //====================================================================================================
        // 内部定数定義
        //====================================================================================================

        /// <summary>
        /// 【排他アクセス制御用ミューテックス名】メモリマップトファイルの排他アクセスを制御する名前付きシステムミューテックスの名前です。
        /// </summary>
        private const string M_MUTEXNAME = MAPNAME + "Mutex";


        //====================================================================================================
        // static 公開メソッド
        //====================================================================================================

        //--------------------------------------------------------------------------------
        /// <summary>
        /// 【XML文字列読み取り】共有メモリーからXML文字列を読み取ります。
        /// </summary>
        /// <returns>
        /// XML文字列[null = 処理失敗、空文字列 = メモリマップトファイルが空っぽ(メモリマップトファイルは作成されているが、内容が未設定)]
        /// </returns>
        //--------------------------------------------------------------------------------
        public static string Read()
        {
            //------------------------------------------------------------
            /// 共有メモリーからXML文字列を読み取る
            //------------------------------------------------------------
            using (var mutex = new Mutex(false, M_MUTEXNAME))
            {                                                           //// 排他アクセス制御用ミューテックスを生成する
                bool mutexOwned = false;                                /////  ミューテックス所有フラグ = false に初期化する

                try
                {                                                       /////  try開始
                    mutexOwned = mutex.XWaitOne(10);                    //////   ミューテックスの所有権を取得する(タイムアウト時間 = 10ミリ秒)
                    if (mutexOwned == false)
                    {                                                   //////   所有権を取得できないままタイムアウト時間を経過した場合
                        return null;                                    ///////    戻り値 = null(処理失敗) で関数終了
                    }

                    using (var mmf =
                            MemoryMappedFile.OpenExisting(MAPNAME))     //////   using：既存のメモリマップトファイルを開く
                    using (var viewStream = mmf.CreateViewStream())     //////   using：メモリマップトファイルからビューストリームを生成する
                    using (var reader = new BinaryReader(viewStream))   //////   using：ビューストリームからバイナリーリーダーを生成する
                    {
                        var xmlString = reader.ReadString();            ///////    バイナリーリーダーからXML文字列を取得する(メモリマップトファイルが空っぽの場合は空文字列になる)
                        return xmlString;                               ///////    戻り値 = XML文字列 で関数終了
                    }
                }
                catch
                {                                                       /////  catch：すべての例外
                    return null;                                        //////   戻り値 = null(処理失敗) で関数終了
                }
                finally
                {                                                       /////  finally：後始末
                    if (mutexOwned)
                    {                                                   //////   ミューテックスを所有している場合
                        mutex.ReleaseMutex();                           ///////    ミューテックスを解放する
                    }
                }
            }
        }


        //--------------------------------------------------------------------------------
        /// <summary>
        /// 【XML文字列書き込み】共有メモリーにXML文字列を書き込みます。
        /// </summary>
        /// <param name="xmlString">[in ]：XML文字列</param>
        /// <returns>
        /// 処理結果[true = 正常終了 / false = 処理失敗(タイムアウト時間内に共有メモリーの排他ロックを取得できなかった)]
        /// </returns>
        //--------------------------------------------------------------------------------
        public static bool Write(string xmlString)
        {
            //------------------------------------------------------------
            /// 共有メモリーへXML文字列を書き込む
            //------------------------------------------------------------
            using (var mutex = new Mutex(false, M_MUTEXNAME))
            {                                                           //// 排他アクセス制御用ミューテックスを生成する
                bool mutexOwned = false;                                /////  ミューテックス所有フラグ = false に初期化する

                try
                {                                                       /////  try開始
                    mutexOwned = mutex.XWaitOne(10);                    //////   ミューテックスの所有権を取得する(タイムアウト時間 = 10ミリ秒)
                    if (mutexOwned == false)
                    {                                                   //////   所有権を取得できないままタイムアウト時間を経過した場合
                        return false;                                   ///////    戻り値 = false(処理失敗) で関数終了
                    }

                    using (var mmf =
                            MemoryMappedFile.OpenExisting(MAPNAME))     //////   using：既存のメモリマップトファイルを開く
                    using (var viewStream = mmf.CreateViewStream())     //////   using：メモリマップトファイルからビューストリームを生成する
                    using (var writer = new BinaryWriter(viewStream))   //////   using：ビューストリームからバイナリーライターを生成する
                    {
                        writer.Write(xmlString);                        ///////    バイナリライターへXML文字列を書き込む

                        Debug.Print($"書き込んだデータ長：{viewStream.Position / 1000f / 1000f} MB");
                    }

                    return true;                                        //////   戻り値 = true(正常終了) で関数終了
                }
                finally
                {                                                       /////  finally：後始末
                    if (mutexOwned)
                    {                                                   //////   ミューテックスを所有している場合
                        mutex.ReleaseMutex();                           ///////    ミューテックスを解放する
                    }
                }
            }
        }

    } // class

} // namespace
