//#define PATH_DEBUG

using System.Collections.Generic;

using UnityEngine;

using System.Linq;

namespace PathCreation.Examples {
    // Example of creating a path at runtime from a set of points.

    [RequireComponent(typeof(PathCreator))]
    public class GenerateDynamicPathExample : MonoBehaviour {

        bool closedLoop = false;
        public PathTracker pathFollower;
        PathCreator pathCreator;

        public bool Mode2D = true;

        public float RandAbsAngle = 30f;

        public string seed = "424242";

        public float AngleMaxRandomAccel = 10f;

        public float AngleAbsMaxVel = 15f;

        public float AngleVel = 0f;

        public float TargetAngle = 0f;

        public float AngleAbsMax = 20f;

        public float AngleVelDecay = 0.8f;

        public float AngleDecay = 0.6f;

        public float SegMinLen = 2f;
        public float SegMaxLen = 15f;

        private void Awake()
        {

            pathCreator = GetComponent<PathCreator>();
            if(pathCreator == null)
            {
                Debug.LogError("pathCreator is null");
            }

            if(pathFollower == null)
            {
                Debug.LogError("path follower is null");
            }
        }


        Random.State currState;

#if PATH_DEBUG
        public Vector3[] DB_initPts;
        public float DB_initLen;
        public int DB_initSegs;
#endif

        void Start() {

            int iseed;

            if(!int.TryParse(seed, out iseed))
            {
                iseed = seed.GetHashCode();
            }
            
            Random.InitState(iseed);

            //Vector3 v2 = Mode2D ?
            //    Quaternion.AngleAxis(Random.Range(-180f, 180f), Vector3.up) * Vector3.right :
            //    Random.onUnitSphere * Random.Range(5f, 15f);

            Vector3 v2 = new Vector3(0f, 0f, -10f);


            Vector3[] pts = new Vector3[9];
            //= { v2*-1f, Vector3.zero, v2 };

            var fval = 60f; //was 10 (car was -48.24)
            var size = 10f;
            for (int i = 0; i < 9; ++i)
            {
                pts[i] = new Vector3(0f, 0f, fval - i * size);
            }

            

            BezierPath bezierPath = new BezierPath(pts, closedLoop, Mode2D ? PathSpace.xz : PathSpace.xyz);
            pathCreator.bezierPath = bezierPath;

#if PATH_DEBUG
            DB_initPts = pts;
            DB_initLen = pathCreator.path.length;
            DB_initSegs = pathCreator.bezierPath.NumSegments;
#endif

            bezierPath.ControlPointMode = BezierPath.ControlMode.Aligned;

            //// Need enough points that follower doesn't run off the path
            //for (int i = 0; i < 6; ++i)
            //{
            //    bezierPath.AddSegmentToEnd((i + 2) * v2);

            //    //AddRandomSegmentToPath();
            //}

            currState = Random.state;
        }

        // Need for random rotations. Uses plane eqn to find perpendicular vector
        // if v is (approximately) zero, returns zero
        Vector3 AnyPerpUnitV(Vector3 v)
        {
            Vector3 ret = Vector3.zero;

            if (!Mathf.Approximately(0f, v.x))
            {
                ret = new Vector3((-v.y - v.z) / v.x, 1f, 1f);
            }
            else if (!Mathf.Approximately(0f, v.y))
            {
                ret = new Vector3(1f, (-v.x - v.z) / v.y, 1f);
            }
            else if (!Mathf.Approximately(0f, v.z))
            {
                ret = new Vector3(1f, 1f, (-v.x - v.y) / v.z);
            }

            return ret.normalized;
        }




        float RBinom(float rangeMagnitude)
        {
            // Not really a binomial distribution but something similar with central tendency
            // around 0
            return Random.Range(0f, rangeMagnitude) - Random.Range(0f, rangeMagnitude);
        }




        float VelSmoothedRandAngle()
        {
            var aaccel = RBinom(1f) * AngleMaxRandomAccel;
            AngleVel = Mathf.Clamp( AngleVelDecay * AngleVel + aaccel, -AngleAbsMaxVel, AngleAbsMaxVel);
            TargetAngle = Mathf.Clamp(AngleDecay * TargetAngle + AngleVel, -AngleAbsMax, AngleAbsMax);

            return TargetAngle;
        }

        Vector3 GenerateRandomAnchor()
        {
            var bp = pathCreator.bezierPath;

            var lasti = bp.NumSegments - 1;
            var lastSeg = bp.GetPointsInSegment(lasti);

            var lastAnchor = lastSeg[3];
            var nextToLastAnchor = lastSeg[0];
            var lastControl = lastSeg[2];

            // we assume that path is currently headed in direction of last tangent
            // But we will rotate the new control point from that direction by some max rand angle

            var contDir = (lastAnchor - lastControl).normalized;

            // any perpendicular v will do
            Vector3 perpDir = Mode2D ? Vector3.up : AnyPerpUnitV(contDir);

            var angleRange = RandAbsAngle;

            //var angRot = Quaternion.AngleAxis(Random.Range(-angleRange, angleRange), perpDir);
            //var angRot = Quaternion.AngleAxis(RBinom(angleRange), perpDir);

            var ang = VelSmoothedRandAngle();

            var angRot = Quaternion.AngleAxis(ang, perpDir);

            var segLenHalfRange = (SegMaxLen - SegMinLen)*0.5f;

            var segLen = SegMinLen + segLenHalfRange + RBinom(segLenHalfRange);

            //Debug.Log($"segLen is: {segLen}");

            var newAnchorDir = angRot * (contDir * segLen);

            Vector3 newAnchor = Vector3.zero;

            if (Mode2D)
            {
                newAnchor = lastAnchor + newAnchorDir;

                newAnchor.y = 0f;
            }
            else
            {
                //angRot = Quaternion.AngleAxis(Random.Range(-180f, 180f), contDir);
                angRot = Quaternion.AngleAxis(RBinom( 180f), contDir);

                newAnchor = lastAnchor + angRot * newAnchorDir;
            }

            return newAnchor;

        }


        void RandomizeLastControl()
        {
            var bp = pathCreator.bezierPath;

            var lasti = bp.NumSegments - 1;
            var lastSeg = bp.GetPointsInSegment(lasti);

            var lastAnchor = lastSeg[3];
            var firstAnchor = lastSeg[0];

            var anchorDist = Vector3.Distance(firstAnchor, lastAnchor);
            var halfAnchorDist = 0.5f * anchorDist;

            var nextToLastAnchor = lastSeg[0];
            var lastControl = lastSeg[2];
            var firstControl = lastSeg[1];

            // We don't want first tangent to be too long and cause extreme path
            var firstTangentVec = firstControl - firstAnchor;
            var firstTangLen = firstTangentVec.magnitude;

            if(firstTangLen > halfAnchorDist)
            {
                firstControl = firstAnchor + (firstTangentVec / firstTangLen) * halfAnchorDist;
            }

            var contRel =  firstControl - lastAnchor;
            var contDist = contRel.magnitude;
            var contDir = contRel / contDist;

            // any perpendicular v will do
            Vector3 perpDir = Mode2D ? Vector3.up : AnyPerpUnitV(contDir);

            //var angleRange = RandAbsAngle;

            //var angRot = Quaternion.AngleAxis(Random.Range(-angleRange, angleRange), perpDir);
            var ang = VelSmoothedRandAngle();


            var angRot = Quaternion.AngleAxis(ang, perpDir);


            var newControlDir = angRot * (contDir * Random.Range(1f, contDist));

            Vector3 newControl = Vector3.zero;

            if (Mode2D)
            {
                newControl = lastAnchor + newControlDir;

                newControl.y = 0f;
            }
            else
            {
                //angRot = Quaternion.AngleAxis(Random.Range(-180f, 180f), contDir);
                angRot = Quaternion.AngleAxis(RBinom( 180f), contDir);

                newControl = lastAnchor + angRot * newControlDir;
            }

            // this is the possibly revised control for first tangent (maybe shortened)
            bp.SetPoint(1, firstControl, true);
            bp.SetPoint(2, newControl);

        }


        void AddRandomSegmentToPath()
        {

            var bp = pathCreator.bezierPath;

            var newAnchor = GenerateRandomAnchor();

            //bp.AddSegmentToEnd(newAnchor);

            bp.AddSegmentToEnd(newAnchor);


            //RandomizeLastControl();

        }


        void RemoveFirstAndAddRandomSegmentToPath()
        {

            var bp = pathCreator.bezierPath;

            var newAnchor = GenerateRandomAnchor();

            //bp.AddSegmentToEnd(newAnchor);

            bp.DeleteSegmentFromBeginningAndAddToEnd(newAnchor);

        }


        public int DB_updateCount = 0;
        public float DB_followerPos = 0f;      
        public float DB_length = 0f;
        public int DB_currSeg = 0;
        public int DB_numSegs = 0;
        public float[] DB_items;
        public float[] DB_segLens;

        private void Update()
        {
            Random.state = currState;

            ++DB_updateCount;

            //var pathLength = pathCreator.path.length;
            //var dist = pathFollower.distanceTravelled;
            var currSeg = pathFollower.currentBezierSegmentIndex;

            DB_followerPos = pathFollower.distanceTravelled;
            

            var bp = pathCreator.bezierPath;

            var path = pathCreator.path;

            DB_length = pathCreator.path.length;

            DB_currSeg = currSeg;

            DB_numSegs = bp.NumSegments;

            var items = path.cumulativeLengthAtEachVertex.Where((item, index) => path.localAnchorVertexIndex.Contains(index));

            DB_items = items.ToArray();

            items = items.Zip(items.Skip(1), (x, y) => y - x);

            var arrItems = items.ToArray();

            DB_segLens = arrItems;

            if (currSeg > 3)
            {
                //Debug.Log($"Del-Add seg b/c currSeg is: {currSeg}");
                // del first seg
                //bp.DeleteSegment(0);

                // replace del seg with new random one at the end
                RemoveFirstAndAddRandomSegmentToPath();

            }

            currState = Random.state;

        }

    }
}