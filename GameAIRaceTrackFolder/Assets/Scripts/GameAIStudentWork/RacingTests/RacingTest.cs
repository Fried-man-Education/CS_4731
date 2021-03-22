
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

        const int timeScale = 5; // how fast to run the game. Running fast doesn't necessarily
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
    
            var sceneName = "RaceTrackFZ";

            SceneManager.LoadScene(sceneName);

            var waitForScene = new WaitForSceneLoaded(sceneName);
            yield return waitForScene;

            Assert.IsFalse(waitForScene.TimedOut, "Scene " + sceneName + " was never loaded");

            yield return new WaitForSeconds(5f * 60f);

            var gm = GameManager.Instance;

            Assert.That(gm.KpHLTA, Is.GreaterThanOrEqualTo(40f));

            Assert.That(gm.Wipeouts, Is.LessThanOrEqualTo(1));
           

        }

    }

}

