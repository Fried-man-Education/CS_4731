using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using System.Linq;

using GameAICourse;
using System.Text;

namespace Tests
{
    public class PathNetworkTest
    {
        // You can run the tests in this class in the Unity Editor if you open
        // the Test Runner Window and choose the EditMode tab


        // Annotate methods with [Test] or [TestCase(...)] to create tests like this one!
        [Test]
        public void TestName()
        {
            // Tests are performed through assertions. You can Google NUnit Assertion
            // documentation to learn about them. Several examples follow.
            Assert.That(CreatePathNetwork.StudentAuthorName, Is.Not.Contains("George P. Burdell"),
                "You forgot to change to your name!");
        }


        [Test]
        public void ExampleTest()
        {
            // Set up some parameters for testing

            Vector2 origin = new Vector2(-5f, -5f);
            Vector2 size = new Vector2(10f, 10f);
            List<Polygon> obstacles = new List<Polygon>();
            float agentRadius = 1f;
            List<Vector2> pathNodes = new List<Vector2>()
            {
                new Vector2(0f, 0f), //<-- In the middle of the canvas
                new Vector2(4.5f, 0f) //<-- Close to the middle of the right edge of the canvas boundary
            };

            // output param

            List<List<int>> pathEdges;


            //Execute your code!

            CreatePathNetwork.Create(origin, size.x, size.y, obstacles, agentRadius, agentRadius+0.01f, agentRadius*2.5f, ref pathNodes, out pathEdges, PathNetworkMode.Predefined);

            //Various assertions to validate your returned result

            Assert.That(pathEdges, Is.Not.Null);
            Assert.That(pathEdges, Has.Count.EqualTo(pathNodes.Count));
            Assert.That(pathEdges, Is.All.Not.Null);

            for (int i = 0; i < pathEdges.Count; ++i)
            {
                var edges = pathEdges[i];

                Debug.Log($"[{i}]:{string.Join(",", edges)}");

                //TODO check for self edges, dupe edges, out of range edge ends, etc...

            }

            // Nodes are not expected to connect because right node is too close to canvas boundary

            Assert.That(pathEdges, Is.All.Empty);


            // TODO add more asserts for things not tested in this example
        }

        // TODO write more tests!


    }
}
