using UnityEngine;

namespace PathCreation.Examples
{
    // Moves along a path at constant speed.
    // Depending on the end of path instruction, will either loop, reverse, or stop at the end of the path.
    public class PathFollowerAdvanced : MonoBehaviour
    {
        public PathCreator pathCreator;
        //public DynamicGeneratePathExample dynamicGenerator;

        public EndOfPathInstruction endOfPathInstruction;
        public float speed = 5;
        public float distanceTravelled;
        public int currentBezierSegment;

        public Vector3 eulerOffsetRot = Vector3.zero;

        void Start() {
            if (pathCreator != null)
            {
                // Subscribed to the pathUpdated event so that we're notified if the path changes during the game
                pathCreator.pathUpdated += OnPathChanged;
            }
        }

        void Update()
        {
            if (pathCreator != null)
            {
                distanceTravelled += speed * Time.deltaTime;
                transform.position = pathCreator.path.GetPointAtDistance(distanceTravelled, endOfPathInstruction);

                //TODO could make this 2d/3d

                transform.forward = pathCreator.path.GetDirectionAtDistance(distanceTravelled, endOfPathInstruction);
                //transform.rotation = Quaternion.Euler(eulerOffsetRot) * pathCreator.path.GetRotationAtDistance(distanceTravelled, endOfPathInstruction);

                currentBezierSegment = pathCreator.path.GetBezierSegmentIndexAtDistance(distanceTravelled, endOfPathInstruction);


                //var d = pathCreator.path.GetClosestDistanceAlongPath(transform.position);

                //var errD = Mathf.Abs(d - distanceTravelled);

                //Debug.Log($"ERR DIST PATH FOLLOWER: {errD}");

            }

        }

        // If the path changes during the game, update the distance travelled so that the follower's position on the new path
        // is as close as possible to its position on the old path
        void OnPathChanged()
        {
            distanceTravelled = pathCreator.path.GetClosestDistanceAlongPath(transform.position);
            currentBezierSegment = pathCreator.path.GetClosestBezierSegmentIndexOnPath(transform.position);
            //currentBezierSegment = pathCreator.path.GetBezierSegmentIndexAtDistance(distanceTravelled, endOfPathInstruction);
        }
    }
}