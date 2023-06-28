using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class ThrowTestEditorMode
    {
 
        [Test]
        public void ThrowTest()
        {

            // This test is a template to show you how to create a test.
            // You should make several more!

            Vector3 projectilePos = Vector3.zero;
            float maxProjectileSpeed = 5f;
            Vector3 projectileGravity = new Vector3(0f, -9.8f, 0f);
            Vector3 targetInitPos = new Vector3(0f, -5f, 0f);
            Vector3 targetConstVel = new Vector3(0f, 0f, 0f);
            Vector3 targetForwardDir = Vector3.left;
            float maxAllowedErrorDist = 1f;
            Vector3 projectileDir = Vector3.zero;
            float projectileSpeed = 0f;
            float interceptT = -1f;
            float altT = -1f;

            var ret = GameAIStudent.ThrowMethods.PredictThrow(
                projectilePos, maxProjectileSpeed, projectileGravity,
                targetInitPos, targetConstVel, targetForwardDir, maxAllowedErrorDist, 
                out projectileDir, out projectileSpeed, out interceptT, out altT);

            Assert.That(ret, Is.True);

            // TODO actually check if collision occurs at interceptT
        }


    }
}
