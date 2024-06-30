// 24/06/30

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Threading;


// NovLab.Base クラスライブラリなどの別プロジェクトから、このプロジェクトに必要な部分を抜粋したサブセットです。

// 名前空間以外は Windows フォーム版と同じです。
namespace Exam_WPF
{
    // ＜メモ＞NovLab.Base の XMutex.cs から抜粋
    //====================================================================================================
    /// <summary>
    /// 【Mutex 拡張メソッド】Mutex クラスに拡張メソッドを追加します。
    /// </summary>
    //====================================================================================================
    public static partial class XMutex
    {
        //--------------------------------------------------------------------------------
        /// <summary>
        /// 【ミューテックス所有権取得】ミューテックスの所有権を取得します。
        /// 別プロセスや別スレッドに所有権がある場合は、ミューテックスの所有権を取得できるか、タイムアウトするまで、現在のスレッドをブロックします。
        /// </summary>
        /// <param name="target">                         [in ]：対象ミューテックス</param>
        /// <param name="millisecondsTimeout">            [in ]：タイムアウト時間(ミリ秒)。無期限に待機する場合は <see cref="Timeout.Infinite"/>(-1)</param>
        /// <param name="abandonedMutexDetectionCallback">[in ]：放棄ミューテックス検出コールバック[null = ワーニングトレース出力]</param>
        /// <param name="callbackArg">                    [in ]：コールバック引数</param>
        /// <returns>
        /// 処理結果[true = 取得成功 / false = 取得失敗(タイムアウト時間内に所有権を取得できなかった)]
        /// </returns>
        /// <remarks>
        /// 補足<br/>
        /// ・<see cref="WaitHandle.WaitOne(int)"/> について、<see cref="AbandonedMutexException"/>例外への対応を簡略化したバージョンです。<br/>
        /// ・別プロセスや別スレッドが解放せずに終了することによって放棄されたミューテックスが
        ///   残っていたことを検出することはバグ発見に役立ちますが、多くの場合、検出した時点でできることは特段ありません。<br/>
        /// </remarks>
        //--------------------------------------------------------------------------------
        public static bool XWaitOne(this Mutex target, int millisecondsTimeout,
                                    Action<AbandonedMutexException, object> abandonedMutexDetectionCallback = null, object callbackArg = null)
        {
            //------------------------------------------------------------
            /// ミューテックスの所有権を取得する
            //------------------------------------------------------------
            try
            {                                                           //// try開始
                return target.WaitOne(millisecondsTimeout);             /////  ミューテックスの所有権を取得してその結果を戻り値とし、関数終了
            }
            catch (AbandonedMutexException ex)
            {                                                           //// catch：放棄されたミューテックス例外
                                                                        ////-(別プロセスや別スレッドが解放せずに終了することによって放棄されたミューテックスが残っており、その所有権を取得した場合)
                if (abandonedMutexDetectionCallback != null)
                {                                                       /////  放棄ミューテックス検出コールバックが指定されている場合
                    abandonedMutexDetectionCallback(ex, callbackArg);   //////   コールバック処理を行う
                }
                else
                {                                                       /////  放棄ミューテックス検出コールバックが指定されていない場合
                    M_Warning_AbandonedMutexDetected(ex);               //////   放棄ミューテックス検出ワーニング出力処理を行う
                }
                return true;                                            /////  戻り値 = true(取得成功) で関数終了
            }
        }


        //--------------------------------------------------------------------------------
        /// <summary>
        /// 【放棄ミューテックス検出ワーニング出力】
        /// 放棄されたミューテックスを検出したことを知らせるワーニングトレースを出力します。
        /// </summary>
        /// <param name="ex">[in ]：AbandonedMutexException例外</param>
        //--------------------------------------------------------------------------------
        private static void M_Warning_AbandonedMutexDetected(AbandonedMutexException ex)
        {
            Trace.TraceWarning($"Abandoned Mutex Detected:{ex.Message}");
        }

    } // class

} // namespace
