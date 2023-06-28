
using System.IO;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using System.Text.RegularExpressions;


namespace Tests
{

    public class DodgeballTests
    {

        const int matchLenSec = 120;
        const int numMatches = 10;

        const int PlayMatchTimeOutMS = int.MaxValue; // don't mess with this; add it to new tests
                                                     // as [Timeout(PlayMatchTimeOutMS)] (see below for
                                                     // examples) It stops early default timeout

        public DodgeballTests()
        {

        }

        void CheckName(string s, string fname)
        {       
            Assert.False(s.Contains("George P. Burdell"), $"You forgot to change your name in {fname}");
        }

        [UnityTest]
        [Timeout(PlayMatchTimeOutMS)]
        public IEnumerator CheckName()
        {
            CheckName(GameAIStudent.ThrowMethods.StudentName, "HW5/HW6 - ThrowMethods");
            CheckName(GameAIStudent.ShotSelection.StudentName, "HW5/HW6 - ShotSelection");
            CheckName(GameAIStudent.MinionStateMachine.StudentName, "HW5 - MinionStateMachine");

            return null;
        }


        [UnityTest]
        [Timeout(PlayMatchTimeOutMS)]
        public IEnumerator HeadToHead_4v4_3b()
        {
            // This is just one possible matchup scenario. Make more for testing!

            return TestMatch(4, 3);
        }


        [Timeout(PlayMatchTimeOutMS)]
        public IEnumerator TestMatch(
            int teamSize,
            int ballsPerTeam
            )
        {
      
            int numWins = 0;
            int numLosses = 0;
            int numTies = 0;

            Debug.Log($"TESTING dodgeball: teamSize={teamSize} ballsPerTeam={ballsPerTeam}");
            Debug.Log($"Total matches for test: {numMatches} Match Len Sec: {matchLenSec}");

            // Pick your agents for the matchup

            // You can set up other opponents. You might A/B test against your own designs.
            // Just duplicate folder "GameAIStudentWork" with a new name but remove the tests folders.
            // Then rename the assembly and edit that file to change name.
            // The new reference will look something like:
            // "GameAIStudent.MinionStateMachine, AltOpponent, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";

            var playerTeamName = "GameAIStudent.MinionStateMachine, GameAIStudentWork, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";
            var opponentTeamName = "GlassJoeAI.MinionStateMachine, GlassJoeAI, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";

            for (int i = 0; i < numMatches; ++i)
            {
                var sceneName = "PrisonBall";

                bool isTeamAThePlayer = i % 2 == 0;

                PrisonDodgeballManager.OverrideConfiguration = true;
                PrisonDodgeballManager.Override_TeamAAssemblyQualifiedName = isTeamAThePlayer ? playerTeamName : opponentTeamName;
                PrisonDodgeballManager.Override_TeamBAssemblyQualifiedName = isTeamAThePlayer ? opponentTeamName : playerTeamName;
                PrisonDodgeballManager.Override_teamSize = teamSize;
                PrisonDodgeballManager.Override_ballsPerTeam = ballsPerTeam;
                PrisonDodgeballManager.Override_matchLengthSec = matchLenSec;
                PrisonDodgeballManager.Override_dodgeballSimulationMode = PrisonDodgeballManager.DodgeballSimulationMode.FPS_60_1X_SimTime;

                SceneManager.LoadScene(sceneName);

                var waitForScene = new WaitForSceneLoaded(sceneName);
                yield return waitForScene;

                Assert.IsFalse(waitForScene.TimedOut, "Scene " + sceneName + " was never loaded");

                PrisonDodgeballManager mgr = PrisonDodgeballManager.Instance;

                var waitForMatchEnd = new WaitForCondition(() => mgr.IsGameOver, PrisonDodgeballManager.Override_matchLengthSec + 5);

                yield return waitForMatchEnd;

                Assert.IsFalse(waitForMatchEnd.TimedOut, "Match never ended");

                PrisonDodgeballManager.Team playersTeam = isTeamAThePlayer ? PrisonDodgeballManager.Team.TeamA : PrisonDodgeballManager.Team.TeamB;

                string status = "lost";
                if (mgr.IsTie)
                {
                    status = "tied";
                    numTies += 1;
                }
                else if (mgr.IsWinner(playersTeam))
                {
                    status = "won";
                    numWins += 1;
                }
                else
                {
                    status = "lost";
                    numLosses += 1;
                }

                Debug.Log($"Player's team {status}! Now at Win-Loss-Tie: {numWins}-{numLosses}-{numTies}");

            } //for

            var winRatio = numWins / (float)(numWins + numLosses + numTies);
            var winTarget = 2f / 3f;
            Assert.That(winRatio, Is.GreaterThanOrEqualTo(winTarget));

        }



        [UnityTest]
        [Timeout(PlayMatchTimeOutMS)]
        public IEnumerator TestShotSelection( )
        {
  
            //QualitySettings.vSyncCount = 0;
            //Application.targetFrameRate = 60;

            float CurrentElapsedTime = 0f;
     
            long firstFrameNum = 0L;
            float startTime = 0f;

            // The MinionThrowTester calls your ShotSelection logic. That state machine is a bare bones AI Agent that just stands in one
            // spot and requests a ball for throwing whenever he doesn't have one (allowed in special throw testing mode).

            var playerTeamName = "GameAIStudent.MinionThrowTester, GameAIStudentWork, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";
            var opponentTeamName = "GlassJoeAI.MinionMovingTargetDrone, GlassJoeAI, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";

            var sceneName = "AdvancedMinionTestThrowScenario";

            const int ShotSelectionTimeoutS = 60 * 20;


            PrisonDodgeballManager.OverrideConfiguration = true;
            PrisonDodgeballManager.Override_TeamAAssemblyQualifiedName = playerTeamName;
            PrisonDodgeballManager.Override_TeamBAssemblyQualifiedName = opponentTeamName;
            PrisonDodgeballManager.Override_teamSize = 1;
            PrisonDodgeballManager.Override_ballsPerTeam = 3;
            PrisonDodgeballManager.Override_matchLengthSec = ShotSelectionTimeoutS;
            PrisonDodgeballManager.Override_dodgeballSimulationMode = PrisonDodgeballManager.DodgeballSimulationMode.FPS_60_1X_SimTime;
    
            SceneManager.LoadScene(sceneName);

            var waitForScene = new WaitForSceneLoaded(sceneName);
            yield return waitForScene;

            Assert.IsFalse(waitForScene.TimedOut, "Scene " + sceneName + " was never loaded");

            firstFrameNum = Time.frameCount;
            startTime = Time.time;

            PrisonDodgeballManager mgr = PrisonDodgeballManager.Instance;


            var waitForMatchEnd = new WaitForSeconds(ShotSelectionTimeoutS);
     
            yield return waitForMatchEnd;

            CurrentElapsedTime += Time.time - startTime;


            //Debug.Log($"Results: {mgr.HitCount} {mgr.MissCount}");

            // This scoring logic is the same as the autograder

            float bottomAcc = 0.2f;
            float minAcc = 0.3f;
            float goodAcc = 0.8f;
            float maxAcc = 0.9f;

            float lowAccScore = 0.6f;
            float highAccScore = 0.95f;
            float maxBonusAccScore = .1f;

            float accScore = 0f;

            int totalShots = mgr.MissCount + mgr.HitCount;

            float rawAcc = mgr.HitCount / (float)(totalShots);

            if (rawAcc < minAcc)
            {
                accScore = Mathf.Lerp(0f, lowAccScore, Mathf.InverseLerp(bottomAcc, minAcc, rawAcc));
            }
            else if (rawAcc < goodAcc)
            {
                accScore = Mathf.Lerp(lowAccScore, highAccScore, Mathf.InverseLerp(minAcc, goodAcc, rawAcc));
            }
            else
            {
                accScore = highAccScore + Mathf.Lerp(0, maxBonusAccScore, Mathf.InverseLerp(goodAcc, maxAcc, rawAcc));
            }


            float minSPM = 90f;
            float goodSPM = 135f;
            float maxSPM = 165f;

            float lowSPMScore = 0.6f;
            float highSPMScore = 0.95f;
            float maxBonusSPMScore = .1f;

            float rawSPM = totalShots / (CurrentElapsedTime / 60f);

            float spmScore = 0;

            if (rawSPM < minSPM)
            {
                spmScore = Mathf.Lerp(0f, lowSPMScore, Mathf.InverseLerp(0f, minSPM, rawSPM));
            }
            else if (rawSPM < goodSPM)
            {
                spmScore = Mathf.Lerp(lowSPMScore, highAccScore, Mathf.InverseLerp(minSPM, goodSPM, rawSPM));
            }
            else
            {
                spmScore = highSPMScore + Mathf.Lerp(0, maxBonusSPMScore, Mathf.InverseLerp(goodSPM, maxSPM, rawSPM));
            }


            float accWeight = 3f;
            float spmWeight = 1f;

            float overallScore = (accScore * accWeight + spmScore * spmWeight) / (accWeight + spmWeight);

            Debug.Log($"Shot Selection: rawAcc: {rawAcc} rawSPM: {rawSPM} accScore: {accScore} spmScore: {spmScore} overallScore: {overallScore}");

        }


 

        [UnityTest]
        [Timeout(PlayMatchTimeOutMS)]
        public IEnumerator TestShootingRange()
        {

            //QualitySettings.vSyncCount = 0;
            //Application.targetFrameRate = 60;

            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = -1;
            Time.captureFramerate = 60;

            float CurrentElapsedTime = 0f;


            float startTime = 0f;


            var sceneName = "ShootingRange";

            SceneManager.LoadScene(sceneName);

            var waitForScene = new WaitForSceneLoaded(sceneName);
            yield return waitForScene;

            Assert.IsFalse(waitForScene.TimedOut, "Scene " + sceneName + " was never loaded");

            ShootingRange srange = GameObject.FindObjectOfType<ShootingRange>();

            Assert.IsNotNull(srange, "SETUP ERROR: shooting range not found");

            srange.INTERNAL_setShootingEnabled(false);

            var waitForInit = new WaitForEndOfFrame();

            yield return waitForInit;

            startTime = Time.time;

            srange.INTERNAL_ClearAimMethods();

            srange.INTERNAL_AddAimMethod(new ShootingRange.AimMethod(GameAIStudent.MinionStateMachine.StudentName,
                GameAIStudent.ThrowMethods.PredictThrow,
                Physics.gravity
                ));

            srange.INTERNAL_ResetStats();

            srange.INTERNAL_setShootingEnabled(true);


            const int ShootingRangeTestTimeOutS = 60 * 20;

            var waitForTestPeriod = new WaitForSeconds(ShootingRangeTestTimeOutS);

            yield return waitForTestPeriod;


            CurrentElapsedTime += Time.time - startTime;

            float bottomAcc = 0.3f;
            float minAcc = 0.8f;
            float goodAcc = 1f;

            float lowAccScore = 0.6f;
            float highAccScore = 1f;

            float accScore = 0f;


            float rawAcc = srange.Accuracy;

            if (rawAcc < minAcc)
            {
                accScore = Mathf.Lerp(0f, lowAccScore, Mathf.InverseLerp(bottomAcc, minAcc, rawAcc));
            }
            //else if (rawAcc <= goodAcc)
            else
            {
                accScore = Mathf.Lerp(lowAccScore, highAccScore, Mathf.InverseLerp(minAcc, goodAcc, rawAcc));
            }


            float minSPM = 150f;
            float goodSPM = 215f;
            float maxSPM = 350f;

            float lowSPMScore = 0.6f;
            float highSPMScore = 0.95f;
            float maxBonusSPMScore = .1f;

            float rawSPM = srange.ShotsPerMin;

            float spmScore = 0;

            if (rawSPM < minSPM)
            {
                spmScore = Mathf.Lerp(0f, lowSPMScore, Mathf.InverseLerp(0f, minSPM, rawSPM));
            }
            else if (rawSPM < goodSPM)
            {
                spmScore = Mathf.Lerp(lowSPMScore, highAccScore, Mathf.InverseLerp(minSPM, goodSPM, rawSPM));
            }
            else
            {
                spmScore = highSPMScore + Mathf.Lerp(0, maxBonusSPMScore, Mathf.InverseLerp(goodSPM, maxSPM, rawSPM));
            }


            float accWeight = 3f;
            float spmWeight = 1f;

            float overallScore = (accScore * accWeight + spmScore * spmWeight) / (accWeight + spmWeight);

            Debug.Log($"Shooting Range: Accuracy: {srange.Accuracy} SPM: {srange.ShotsPerMin}");

            Debug.Log($"Shooting Range: accScore: {accScore} spmScore: {spmScore} overallScore: {overallScore}");


        }


    }

}

