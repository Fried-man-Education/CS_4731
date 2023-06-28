using UnityEngine;
using UnityEngine.SceneManagement;

public class WaitForCondition : CustomYieldInstruction
{
    readonly float timeout;
    readonly float startTime;
    bool timedOut;
    public delegate bool EvalCondition();

    public EvalCondition IsCompletedCondition;

    public bool TimedOut => timedOut;

    public override bool keepWaiting
    {
        get
        {


            if (Time.realtimeSinceStartup - startTime >= timeout)
            {
                timedOut = true;
            }

            return !IsCompletedCondition() && !timedOut;
        }
    }

    public WaitForCondition(EvalCondition isCompletedCondition, float newTimeout = 10)
    {
        IsCompletedCondition = isCompletedCondition;
        timeout = newTimeout;
        startTime = Time.realtimeSinceStartup;
    }
}