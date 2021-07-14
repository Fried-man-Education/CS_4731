using System;
using System.Linq;

using UnityEngine;

[Serializable]
public class SerializableCurve
{
    SerializableKeyframe[] keys;
    Int32 postWrapMode;
    Int32 preWrapMode;

    [Serializable]
    public class SerializableKeyframe
    {
        public Single inTangent;
        public Single outTangent;
        public Single inWeight;
        public Single outWeight;
        public Single time;
        public Single value;
        public Int32 weightedMode;

        public SerializableKeyframe(Keyframe original)
        {
            inTangent = original.inTangent;
            outTangent = original.outTangent;

            //tangentMode = original.tangentMode;
            inWeight = original.inWeight;
            outWeight = original.outWeight;

            weightedMode = (int)original.weightedMode;

            time = original.time;
            value = original.value;
        }

        public Keyframe AsKeyframe()
        {
            var kf = new Keyframe();
            kf.inTangent = inTangent;
            kf.outTangent = outTangent;
            kf.inWeight = inWeight;
            kf.outWeight = outWeight;
            kf.weightedMode = (WeightedMode)weightedMode;
            kf.time = time;
            kf.value = value;
            
            return kf;
        }

        // override object.Equals
        public override bool Equals(object obj)
        {
            //       
            // See the full list of guidelines at
            //   http://go.microsoft.com/fwlink/?LinkID=85237  
            // and also the guidance for operator== at
            //   http://go.microsoft.com/fwlink/?LinkId=85238
            //

            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            // TODO: write your implementation of Equals() here

            return Equals(obj as SerializableKeyframe);
        }

        public bool Equals(SerializableKeyframe kf)
        {

            if (!kf.inTangent.Equals(inTangent)) return false;
            if (!outTangent.Equals(outTangent)) return false;
            if (!inWeight.Equals(inWeight)) return false;
            if (!outWeight.Equals(outWeight)) return false;
            if (!time.Equals(time)) return false;
            if (!value.Equals(value)) return false;
            if (!weightedMode.Equals(weightedMode)) return false;

            return true;
        }

        // TODO this needs to be properly implemented
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

    }


    public SerializableCurve(AnimationCurve original)
    {
        if (original == null)
            return;

        postWrapMode = (Int32)original.postWrapMode;  
        preWrapMode = (Int32)original.preWrapMode; 
        keys = new SerializableKeyframe[original.length];
        for (int i = 0; i < original.keys.Length; i++)
        {
            keys[i] = new SerializableKeyframe(original.keys[i]);
        }
    }

    public AnimationCurve AsAnimationCurve()
    {
        AnimationCurve res = new AnimationCurve();
        res.postWrapMode = (WrapMode)postWrapMode;
        res.preWrapMode = (WrapMode)preWrapMode;

        int len = 0;

        if (keys != null)
            len = keys.Length;


        Keyframe[] newKeys = new Keyframe[len];
        for (int i = 0; i < len; i++)
        {
            SerializableKeyframe aux = keys[i];

            newKeys[i] = aux.AsKeyframe();

        }
        res.keys = newKeys;
        return res;
    }
}