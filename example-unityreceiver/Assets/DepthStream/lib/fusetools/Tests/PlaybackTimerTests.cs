using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using UnityEngine.Events;

namespace FuseTools.Tests
{
    public class PlaybackTimerTests {
        [UnityTest]
        public IEnumerator AutoStopTest() {
          var entity = new GameObject("PlaybackTimer");
          var ptimer = entity.AddComponent<PlaybackTimer>();
          ptimer.PlaybackTimerData = new PlaybackTimerData();
          ptimer.AutoStopEvent = new UnityEvent();
          var autoStopCounter = new Test.EventCounter(ptimer.AutoStopEvent);
          ptimer.Duration = 0.5f;
          ptimer.PlaybackTimerData.Start();
          Assert.AreEqual(autoStopCounter.Count, 0);
          yield return new WaitForSeconds(0.2f);
          Assert.AreEqual(autoStopCounter.Count, 0);
          ptimer.PlaybackTimerData.Pause();
          Assert.AreEqual(autoStopCounter.Count, 0);
          yield return new WaitForSeconds(2.0f);
          Assert.AreEqual(autoStopCounter.Count, 0);
          ptimer.PlaybackTimerData.Resume();
          Assert.AreEqual(autoStopCounter.Count, 0);
          yield return new WaitForSeconds(0.2f);
          Assert.AreEqual(autoStopCounter.Count, 0);
          yield return new WaitForSeconds(0.2f);
          Assert.AreEqual(autoStopCounter.Count, 1);

          Object.Destroy(entity);
        }
    }

    public class PlaybackTimerDataTests {
        [UnityTest]
        public IEnumerator StartPauseResumeStopResetTest() {
          // init; not running yet
          var timer = new PlaybackTimerData();
          Assert.IsFalse(timer.IsRunning);
          Assert.IsFalse(timer.IsPaused);
          Assert.IsFalse(timer.IsStopped);
          Assert.IsTrue(timer.IsNotStarted);
          Assert.AreEqual(timer.Time, 0.0f, 0.001f);
          Assert.AreEqual(timer.FrameIndex, 0);
          yield return new WaitForSeconds(0.2f);
          Assert.AreEqual(timer.Time, 0.0f, 0.001f);
          Assert.AreEqual(timer.FrameIndex, 0);
          // start
          timer.Start();
          Assert.IsTrue(timer.IsRunning);
          Assert.IsFalse(timer.IsPaused);
          Assert.IsFalse(timer.IsStopped);
          Assert.IsFalse(timer.IsNotStarted);
          Assert.AreEqual(timer.Time, 0.0f, 0.001f);
          yield return new WaitForSeconds(0.2f);
          var t1 = timer.Time;
          Assert.AreEqual(t1, 0.2f, 0.04f);
          // pause
          timer.Pause();
          Assert.IsFalse(timer.IsRunning);
          Assert.IsTrue(timer.IsPaused);
          Assert.IsFalse(timer.IsStopped);
          Assert.IsFalse(timer.IsNotStarted);
          yield return new WaitForSeconds(2.0f);
          var t2 = timer.Time;
          Assert.AreEqual(t2, t1, 0.001f);
          // resume
          timer.Resume();
          Assert.IsTrue(timer.IsRunning);
          Assert.IsFalse(timer.IsPaused);
          Assert.IsFalse(timer.IsStopped);
          Assert.IsFalse(timer.IsNotStarted);
          yield return new WaitForSeconds(0.1f);
          Assert.IsTrue(timer.IsRunning);
          Assert.IsFalse(timer.IsPaused);
          Assert.IsFalse(timer.IsStopped);
          Assert.IsFalse(timer.IsNotStarted);
          Assert.AreEqual(timer.Time, t2+0.1f, 0.04f);
          yield return new WaitForSeconds(0.1f);
          var t3 = timer.Time;
          Assert.AreEqual(t3, t2+0.2f, 0.04f);
          // stop
          timer.Stop();
          Assert.IsFalse(timer.IsRunning);
          Assert.IsFalse(timer.IsPaused);
          Assert.IsTrue(timer.IsStopped);
          Assert.IsFalse(timer.IsNotStarted);
          yield return new WaitForSeconds(0.2f);
          var t4 = timer.Time;
          Assert.AreEqual(t4, t3, 0.001f);
          // reset
          timer.Reset();
          Assert.IsFalse(timer.IsRunning);
          Assert.IsFalse(timer.IsPaused);
          Assert.IsFalse(timer.IsStopped);
          Assert.IsTrue(timer.IsNotStarted);
          Assert.AreEqual(timer.Time, 0.0f, 0.0001f);
        }
    }
}
