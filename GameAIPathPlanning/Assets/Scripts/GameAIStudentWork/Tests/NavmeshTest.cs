using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using GameAICourse;

namespace Tests
{
    public class NavmeshTest
    {
        // You can run the tests in this class in the Unity Editor if you open
        // the Test Runner Window and choose the EditMode tab


        // Annotate methods with [Test] or [TestCase(...)] to create tests like this one!
        [Test]
        public void TestName()
        {
            // Tests are performed through assertions. You can Google NUnit Assertion
            // documentation to learn about them. Several examples follow.
            Assert.That(CreateNavMesh.StudentAuthorName, Is.Not.Contains("George P. Burdell"),
                "You forgot to change to your name!");
        }



        // TODO Write some tests! See GridTest and PathNetworkTest for more examples
        // PathNetworkTest(s) you have previously worked on will be useful
        // to validate you are creating a good path graph from your navmesh!


    }
}
