
using System.IO;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

using GameAI;

namespace Tests
{

    public class RacingTest
    {

        const int timeScale = 1; // how fast to run the game. Running fast doesn't necessarily
                                 // give accurate results.

        const int PlayMatchTimeOutMS = int.MaxValue; // don't mess with this; add it to new tests
                                                     // as [Timeout(PlayMatchTimeOutMS)] (see below for
                                                     // example) It stops early default timeout

        public RacingTest()
        {

        }


        [UnityTest]
        [Timeout(PlayMatchTimeOutMS)]
        public IEnumerator TestFuzzyRace()
        {
            return _TestFuzzyRace();
        }


        [Timeout(PlayMatchTimeOutMS)]
        public IEnumerator _TestFuzzyRace()
        {
            Time.timeScale = timeScale;

            Time.fixedDeltaTime = Time.fixedDeltaTime / timeScale;

            Application.targetFrameRate = 60 * timeScale;

            var sceneName = "RaceTrackFZ";

            SceneManager.LoadScene(sceneName);

            var waitForScene = new WaitForSceneLoaded(sceneName);
            yield return waitForScene;

            Assert.IsFalse(waitForScene.TimedOut, "Scene " + sceneName + " was never loaded");

            yield return new WaitForSeconds(5f * 60f);

            var gm = GameManager.Instance;

            Debug.Log($"Km/H LTA: {gm.KpHLTA} Num wipeouts: {gm.Wipeouts} meters: {gm.MetersTravelled}");


            var targetSpeed = 45f;
            var extraCreditSpeed = 58f;
            var minAllowedSpeed = 30f;

            var speedScoreWeight = 0.6f;
            var wipeoutScoreWeight = 0.4f;
            var extraCreditWeight = 0.05f;

            var speedPenalty = Mathf.Lerp(speedScoreWeight, 0f,

                Power(
                    Mathf.InverseLerp(minAllowedSpeed, targetSpeed, gm.KpHLTA)
                    , 0.5f)

                    );


            var maxAllowedWipeouts = 1;
            var maxPartialPenaltyWipeouts = 10;


            var wipeoutPenalty = Mathf.Lerp(wipeoutScoreWeight, 0f,
                    1f - Mathf.InverseLerp(maxAllowedWipeouts, maxPartialPenaltyWipeouts, gm.Wipeouts)
                );


            float extraCredit = 0f;

            if (gm.Wipeouts <= 0)
            {
                extraCredit = Mathf.Lerp(0f, extraCreditWeight,

                     Power(
                         Mathf.InverseLerp(targetSpeed, extraCreditSpeed, gm.KpHLTA)
                         , 0.5f)

                         );

                Debug.Log($"Extra credit earned: {extraCredit}");
            }
            else
            {
                Debug.Log($"Extra credit only earned if no wipeouts!");
            }

            var totalScore = (speedScoreWeight - speedPenalty) +
                (wipeoutScoreWeight - wipeoutPenalty) +
                extraCredit;

            Debug.Log($"Estimated Total Score: {totalScore * 100}%");

        }

        float Power(float t, float strength)
        {
            return Mathf.Pow(t, strength);
        }


    }

}

