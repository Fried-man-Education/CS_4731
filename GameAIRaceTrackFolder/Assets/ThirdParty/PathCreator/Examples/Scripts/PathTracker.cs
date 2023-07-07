using UnityEngine;

namespace PathCreation.Examples
{
    // Moves along a path at constant speed.
    // Depending on the end of path instruction, will either loop, reverse, or stop at the end of the path.
    public class PathTracker : MonoBehaviour
    {
        public PathCreator pathCreator;
        RoadMeshCreator roadMeshCreator;
        float lastDistanceTravelled = 0f;
        public float distanceTravelled = 0f;
        public float totalDistanceTravelled = 0f;
        public Vector3 closestPointOnPath;
        public Vector3 closestPointDirectionOnPath;
        public int currentBezierSegmentIndex;
        public int currentClosestPathPointIndex;
        //public int currentSegment;

        public Vector3 previousPosition;

        //float oldestBezierSegmentLen =  -1f;

        public Vector3 eulerOffsetRot = Vector3.zero;

        public float HalfRoadWidth
        {
            get => roadMeshCreator.roadWidth;
        }


        private void Awake()
        {
            if (pathCreator == null)
            {
                Debug.LogError("No pathCreator assigned!");
            }

            if (pathCreator != null)
            {
                roadMeshCreator = pathCreator.GetComponent<RoadMeshCreator>();

                if (roadMeshCreator == null)
                {
                    Debug.LogError("No roadMeshCreator found!");
                }
            }

        }

        void Start()
        {
            if (pathCreator != null)
            {
                // Subscribed to the pathUpdated event so that we're notified if the path changes during the game
                pathCreator.pathUpdated += OnPathChanged;

            }
        }


        public float MaxPathDistance
        {
            get => pathCreator.path.cumulativeLengthAtEachVertex[pathCreator.path.cumulativeLengthAtEachVertex.Length - 1];
        }



        public void ResetToDistance(float dist)
        {
            if (dist < 0f)
                Debug.LogError("Don't pass negative distances!");

            if (dist > MaxPathDistance)
                Debug.LogError("Don't pass distance further than the end!");


            SetDistance(dist);
        }

        public void ResetTotalDistance()
        {
            this.totalDistanceTravelled = 0f;
            //this.lastDistanceTravelled = 0f;
            //this.distanceTravelled = 0f;
        }

        void SetDistance(float dist)
        {
            //Debug.Log($"SetDistance({dist})");
            distanceTravelled = dist;

            // this could be subtracting and that is ok
            totalDistanceTravelled += (distanceTravelled - lastDistanceTravelled);

            lastDistanceTravelled = distanceTravelled;

            var tup = pathCreator.path.GetPointAndDirAtDistance(distanceTravelled);

            //closestPointOnPath = pathCreator.path.GetPointAtDistance(distanceTravelled);
            //closestPointDirectionOnPath = pathCreator.path.GetDirectionAtDistance(distanceTravelled);

            closestPointOnPath = tup.Item1;
            closestPointDirectionOnPath = tup.Item2;


            currentBezierSegmentIndex = pathCreator.path.GetBezierSegmentIndexAtDistance(distanceTravelled);

            currentClosestPathPointIndex = pathCreator.path.GetPreviousSegmentIndexAtDistance(distanceTravelled);
        }

        // called if the path is adjusted by deleting the oldest bezier segment
        void AdjustDistance(float dist)
        {
            distanceTravelled = dist;

            // this could be subtracting and that is ok
            //totalDistanceTravelled += (distanceTravelled - lastDistanceTravelled);

            lastDistanceTravelled = distanceTravelled;

            //closestPointOnPath = pathCreator.path.GetPointAtDistance(distanceTravelled);

            //closestPointDirectionOnPath = pathCreator.path.GetDirectionAtDistance(distanceTravelled);

            var tup = pathCreator.path.GetPointAndDirAtDistance(distanceTravelled);

            closestPointOnPath = tup.Item1;
            closestPointDirectionOnPath = tup.Item2;


            currentBezierSegmentIndex = pathCreator.path.GetBezierSegmentIndexAtDistance(distanceTravelled);

            currentClosestPathPointIndex = pathCreator.path.GetPreviousSegmentIndexAtDistance(distanceTravelled);

            //if (currentBezierSegmentIndex > 3)
            //    Debug.Log($"WHY IS THIS GREATER THAN 3 RIGHT NOW?: {currentBezierSegmentIndex}");
        }


        void Update()
        {
            //if (oldestBezierSegmentLen < 0f)
            //{

            //    oldestBezierSegmentLen = pathCreator.path.cumulativeLengthAtEachVertex[];
            //}

            if (pathCreator != null)
            {

                SetDistance(pathCreator.path.GetClosestDistanceAlongPath(transform.position));

                previousPosition = transform.position;

            }

        }

        // If the path changes during the game, update the distance travelled so that the follower's position on the new path
        // is as close as possible to its position on the old path
        void OnPathChanged()
        {
            // this is a bit fragile. assumes that only the last bezier segment was deleted on the change

            //            distanceTravelled -= oldestBezierSegmentLen;

            //            SetDistance(distanceTravelled);

            //var bindex = pathCreator.path.localAnchorVertexIndex[1];//pathCreator.path.GetBezierSegmentIndex(1);
            //var b2index = pathCreator.path.localAnchorVertexIndex[0];//pathCreator.path.GetBezierSegmentIndex(0);

            //Debug.Log($"bindex is: {bindex} b2ind: {b2index} cummLenVertSiz: {pathCreator.path.cumulativeLengthAtEachVertex.Length}");

            //            oldestBezierSegmentLen =
            //pathCreator.path.cumulativeLengthAtEachVertex[bindex - 1];

            AdjustDistance(pathCreator.path.GetClosestDistanceAlongPath(previousPosition));

            //currentBezierSegment = pathCreator.path.GetClosestBezierSegmentIndexOnPath(transform.position);

        }
    }
}