#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;

using OpenCVForUnity.CoreModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.UtilsModule;


[ExecuteInEditMode]
public partial class PCGTerrain : MonoBehaviour
{

    static int cvType = CvType.CV_32F;


    [SerializeField]
    public StringyGuid guid;

    [SerializeField]
    public PCGTerrainConfigSerializableObject ConfigSerializableObject;

    [SerializeField]
    public PCGTerrainConfig Config = null;


    Terrain terrain;

    float[,] heights;
    float[] heights1d;
 

    Scalar zeroScalar = new Scalar(0);


    static float Epsilon = 0.0001f;


    public List<PCGTerrain> PCGChildren = new List<PCGTerrain>();


    public Mat Heights { get => mat; }
    Mat mat;
    Mat perlinMat;
    Mat parentMat;
    Mat temp;

    Scalar scalar = new Scalar(0f);


    static double[] ScalarZero = new double[] { 0.0 };
    static double[] ScalarOne = new double[] { 1.0 };
    static double[] ScalarNegOne = new double[] { -1.0 };
    static double[] ScalarMax = new double[] { 0.0 };

    [Header("Debug")]

    public float DB_minNoise;
    public float DB_maxNoise;


    // This method should generate perlin noise according to the arguments passed in.
    // The return should be between: [0,1]
    void ApplyPerlinNoise(
        //ref float[,] mat,
        ref Mat mat,
        // Offsets to add to the normalized and scaled i and j. The kOffset is used as a seed in the otherwise unused 3rd dimension
        // of the Perlin noise generator
        float iOffset, float jOffset, float kOffset,
        // The perlinScalar allows one to select the frequency of noise by zooming in or out
        // Multiply the scalar times the normalized i/j before passing to the Perlin noise gen
        float perlinScalar,
        // This is the maximum value that will be returned. Multiply by the remapped noise value [0,1] to get a value [0,maxVal]
        float maxVal,
        // Optionally use a mapping curve
        bool useMappingCurve,
        // If the mapping curve is to be used (see above), then curve.Evaluate() the remapped noise value [0,1]
        // This curve should ideally map [0,1] to values within [0,1], but can overshoot and be clamped later
        AnimationCurve curve,
        // invert the final calculated value (1 - v) where v is [0, 1]
        bool doComplement)
    {

        DB_minNoise = float.MaxValue;
        DB_maxNoise = float.MinValue;

        //Debug.Log("ApplyPerlinNoise()::BEGIN");

        Debug.Assert(mat != null, "mat null");

        // The width and height of the terrain heightmap array. These are used to normalize the i and j values

        int _width = mat.size(0);
        int _height = mat.size(1);


        float width = (float)_width;
        float height = (float)_height;

        //Debug.Log($"width: {width} height: {height}");

        var row = new float[_width];

        for (int _i = 0; _i < _width; ++_i)
        {
            for (int _j = 0; _j < _height; ++_j)
            {
                // The coordinates of the terrain heightmap cast to float
                float i = (float)_i;
                float j = (float)_j;


                float noise = 0f;


                noise = ImprovedPerlinNoise.Noise(
                    (i / width + iOffset) * perlinScalar 
                    , (j / height + jOffset) * perlinScalar 
                    , kOffset * perlinScalar
                    );


                //noise = Random.Range(0f, 1f);

                // remap [-1, 1] to [0, 1]
                noise = (noise + 1f) / 2f;


                float val = 0f;

                // TODO move the conditional out of loop
                if (useMappingCurve)
                    val = maxVal * curve.Evaluate(noise);
                else
                    val = maxVal * noise;


                if (val > DB_maxNoise)
                {
                    DB_maxNoise = val;
                }

                if (val < DB_minNoise)
                {
                    DB_minNoise = val;
                }

                val = Mathf.Clamp(val, 0f, 1f);

                if (doComplement)
                    val = 1.0f - val;

                row[_j] = val;

            } //for

            mat.put(_i, 0, row);

        } //for

    }



    public bool Process(in Mat mHeights)
    {
        return Process(in mHeights, 0f, 0f, 0f, false, true);
    }

    bool DetermineIfUpdateNeeded()
    {
        // Note that OnValidate() may have already set this obj to dirty

        // propagate up any dirty children so we know what branches to re-process
        bool dirtyChildFound = false;
        if (!Config.DoNotProcessDescendants && PCGChildren.Count > 0)
        {
            for (int i = 0; i < PCGChildren.Count; ++i)
            {
                var c = PCGChildren[i];

                if (c != null)
                {
                    var ret = c.DetermineIfUpdateNeeded();

                    if (ret)
                        dirtyChildFound = true;

                }
            }
        }

        if (dirtyChildFound)
            descendentDirty = true;

        return descendentDirty || localDirty;

    }


    protected float prevParentXOffset = float.MinValue;
    protected float prevParentYOffset = float.MinValue;
    protected float prevParentZOffset = float.MinValue;

    // Returns true if dirty (modified since last processed)
    public bool Process(in Mat mHeights, float parentXOffset, float parentYOffset, float parentZOffset, bool ascendentUpdated, bool doValidation)
    {

        if (doValidation)
            DetermineIfUpdateNeeded();

        //Debug.Log($"Processing: {name}");

        Debug.Assert(mHeights != null, "heights cannot be null");

        var res = mHeights.size(0);

        Debug.Assert(res == mHeights.size(1), "Expected heights to be square dims");

        if (mat != null && mat.size(0) != res)
        {
            //Debug.Log("Parent heights changed!");

            // setting localDirty after DetermineIfUpdateNeeded() should hopefully
            // work. This is because every child will also need to update to the new
            // height map resolution.
            localDirty = true;

            mat.Dispose();
            mat = null;

            if (perlinMat != null)
            {
                perlinMat.Dispose();
                perlinMat = null;
            }

            if (parentMat != null)
            {
                parentMat.Dispose();
                parentMat = null;
            }

            if (temp != null)
            {
                temp.Dispose();
                temp = null;
            }
        }

        if (mat == null)
        {
            mat = new Mat(res, res, cvType);
        }

        if (perlinMat == null)
        {
            perlinMat = mat.clone();
        }

        if (temp == null)
        {
            temp = mat.clone();
        }

        if (parentMat == null)
        {
            parentMat = mat.clone();
        }

        if (!ascendentUpdated && !localDirty && !descendentDirty)
        {
            return false;
        }

        // Small optimization. Only regen Perlin noise due to parent change
        // if the changes are offsets
        bool offsetsChanged = false;

        if (prevParentXOffset != parentXOffset ||
            prevParentYOffset != parentYOffset ||
            prevParentZOffset != parentZOffset)
        {
            offsetsChanged = true;
        }

        prevParentXOffset = parentXOffset;
        prevParentYOffset = parentYOffset;
        prevParentZOffset = parentZOffset;


        if (localDirty || offsetsChanged)
        {

            //Debug.Log($"Perlin step for: {name}");


            if (!Config.Mute)
            {
                if (Config.GenNoiseType == PCGGenNoiseType.None)
                {
                    ScalarMax[0] = Config.MaxValue;
                    scalar.set(ScalarMax);
                    perlinMat.setTo(scalar);
                }
                else
                {
                    //float parentXOffset = 0f;
                    //float parentYOffset = 0f;
                    //float parentZOffset = 0f;

                    //if(transform.parent != null)
                    //{
                    //    var parentPCG = transform.parent.GetComponent<PCGTerrain>();

                    //    if(parentPCG != null)
                    //    {
                    //        parentXOffset = parentPCG.Config.XOffset;
                    //        parentYOffset = parentPCG.Config.YOffset;
                    //        parentZOffset = parentPCG.Config.ZOffset;
                    //    }
                    //}

                    
                    ApplyPerlinNoise(ref perlinMat,
                        parentXOffset + Config.XOffset, parentYOffset + Config.YOffset, parentZOffset + Config.ZOffset,
                        Config.PerlinScalar,
                        Config.MaxValue, Config.GenNoiseType == PCGGenNoiseType.PerlinNoiseWithMappingCurve,
                        Config.GenNoiseCurve, Config.Complement);
                }
            }
            else
            {
                scalar.set(ScalarZero);
                perlinMat.setTo(scalar);
            }

        }

        perlinMat.copyTo(mat);


        if (!Config.DoNotProcessDescendants && PCGChildren.Count > 0)
        {

            //Debug.Log($"Descendent step for: {name}");


            bool foundChildHeights = false;
            int childIndex = -1;

            for (int i = 0; i < PCGChildren.Count; ++i)
            {
                var c = PCGChildren[i];

                if (c != null)
                {
                    if (!c.Config.Mute)
                    {
                        if (!foundChildHeights)
                        {
                            foundChildHeights = true;
                            childIndex = i;
                        }
                        c.Process(in mat,
                            parentXOffset + Config.XOffset, parentYOffset + Config.YOffset, parentZOffset + Config.ZOffset,
                            localDirty || ascendentUpdated, false);
                    }
                }
            }

            if (foundChildHeights)
            {
                PCGChildren[childIndex].Heights.copyTo(mat);

                for (int i = childIndex + 1; i < PCGChildren.Count; ++i)
                {
                    var c = PCGChildren[i];
                    if (c != null)
                    {
                        var ch = c.Heights;

                        Core.add(mat, ch, temp);
                        temp.copyTo(mat);
                    }

                }
            }

        }


        //Debug.Log($"Parent process step for: {name}");


        float[] hDat = new float[mHeights.total() * mHeights.channels()];

        var w = mHeights.width();
        var h = mHeights.height();

        var parentProcessType = Config.ProcessParentType;


        if (IsRoot)
            parentProcessType = PCGProcessParentType.ZeroOut;

        //if (ascendentUpdated)
        {
            switch (parentProcessType)
            {
                case PCGProcessParentType.Passthrough:
                    mHeights.copyTo(parentMat);
                    break;
                case PCGProcessParentType.ZeroOut:
                    scalar.set(ScalarZero);
                    parentMat.setTo(scalar);
                    break;
                case PCGProcessParentType.Complement:
                    scalar.set(ScalarOne);
                    Core.absdiff(mHeights, scalar, parentMat);
                    break;
                case PCGProcessParentType.ApplyTrapezoidFunction:
                    if (Config.ProcessParentVariables.Count >= 4)
                    {

                        //Debug.Log("ApplyTrapFunc");

                        // Bummer, need to pull the data out of Mat, change it, then put it back. :-(

                        //float[] hDat = new float[mHeights.total() * mHeights.channels()];

                        mHeights.get(0, 0, hDat);

                        var lowest = Config.ProcessParentVariables[0];
                        var low = Config.ProcessParentVariables[1];
                        var high = Config.ProcessParentVariables[2];
                        var highest = Config.ProcessParentVariables[3];


                        for (int i = 0; i < w; ++i)
                        {
                            for (int j = 0; j < h; ++j)
                            {
                                //hDat[j * w + i] = Random.Range(0f, 1f);//(i + j)/(w*h);

                                // stay 1D array but with 2D mapping
                                var index = j * w + i;
                                var parentVal = hDat[index];

                                if (parentVal >= lowest && parentVal <= low)
                                {
                                    var denom = low - lowest;
                                    if (denom < Epsilon)
                                        parentVal = 0f;
                                    else
                                        parentVal = (parentVal - lowest) / denom;
                                }
                                else if (parentVal >= low && parentVal <= high)
                                {
                                    parentVal = 1f;
                                }
                                else if (parentVal >= high && parentVal <= highest)
                                {
                                    var denom = highest - high;
                                    if (denom < Epsilon)
                                        parentVal = 0f;
                                    else
                                        parentVal = 1f - (parentVal - high) / denom;
                                }
                                else
                                {
                                    parentVal = 0f;
                                }

                                hDat[index] = parentVal;

                            }
                        }

                        parentMat.put(0, 0, hDat);

                    }
                    break;

                case PCGProcessParentType.MappingCurve:

                    //float[] hDat = new float[mHeights.total() * mHeights.channels()];

                    mHeights.get(0, 0, hDat);

                    //var w = mHeights.width();
                    //var h = mHeights.height();

                    for (int i = 0; i < w; ++i)
                    {
                        for (int j = 0; j < h; ++j)
                        {
                            // stay 1D array but with 2D mapping
                            var index = j * w + i;

                            hDat[index] = Config.ProcessParentCurve.Evaluate(hDat[index]);
                        }
                    }

                    parentMat.put(0, 0, hDat);


                    break;
                case PCGProcessParentType.ImageProcessing:
                    // TODO Mats here should probably be preserved
                    Mat converted = new Mat(mHeights.rows(), mHeights.cols(), CvType.CV_8U);
                    Mat processed = converted.clone();
                    mHeights.convertTo(converted, CvType.CV_8U, 255.0);
                    //OpenCVForUnity.ImgprocModule.Imgproc.Canny(converted, processed, 100.0, 200.0);
                    OpenCVForUnity.ImgprocModule.Imgproc.blur(converted, processed, new Size(10.0, 10.0));
                    processed.convertTo(parentMat, cvType, 1.0 / 255.0);
                    break;
                default:
                    // passthrough unmodified
                    mHeights.copyTo(parentMat);

                    break;
            }

        }


        //Debug.Log($"Parent augment step for: {name}");


        //switch (combineType)
        switch(Config.CombineType)
        {
            case PCGCombineType.Replace:
                // Nothing to do
                break;
            case PCGCombineType.Add:
                Core.add(mat, parentMat, mat);
                break;
            case PCGCombineType.Subtract:
                Core.subtract(parentMat, mat, mat);
                break;
            case PCGCombineType.SubtractReverse:
                Core.subtract(mat, parentMat, mat);
                break;
            case PCGCombineType.Multiply:
                Core.multiply(parentMat, mat, mat);
                break;
            case PCGCombineType.Min:
                Core.min(parentMat, mat, mat);
                break;
            case PCGCombineType.Max:
                Core.max(parentMat, mat, mat);
                break;
            case PCGCombineType.NormalizeFromParentToTop:
                scalar.set(ScalarOne);
                Core.absdiff(parentMat, scalar, temp);
                Core.multiply(mat, temp, temp);
                Core.add(parentMat, temp, mat);
                break;
            case PCGCombineType.NormalizeFromBottomToParent:
                Core.multiply(parentMat, mat, mat);
                break;
            case PCGCombineType.NormalizeFromParentToBottom:
                Core.multiply(parentMat, mat, mat);
                Core.subtract(parentMat, mat, mat);
                break;
            case PCGCombineType.NormalizeFromTopToParent:
                scalar.set(ScalarOne);
                Core.absdiff(parentMat, scalar, temp);
                Core.multiply(mat, temp, temp);
                Core.absdiff(temp, scalar, mat);

                break;
            default:
                // Do nothing (replaces parent)
                break;
        }

        descendentDirty = false;
        localDirty = false;

        return true;

    }

    void SetupTerrain()
    {

        terrain = GetComponent<Terrain>();

        // only root node should have terrain
        if (terrain != null)
        {

            var res = terrain.terrainData.heightmapResolution;

            var size = terrain.terrainData.size;

            //var width = size.x;
            //var height = size.z;

            //Debug.Log($"Res is {res}, Size is: {size}");

            if (mat != null && mat.size(0) != res)
            {
                //Debug.Log("Terrain size changed!");

                this.localDirty = true;

                mat.Dispose();
                mat = null;

                if (perlinMat != null)
                {
                    perlinMat.Dispose();
                    perlinMat = null;
                }

                if (parentMat != null)
                {
                    parentMat.Dispose();
                    parentMat = null;
                }

                if (temp != null)
                {
                    temp.Dispose();
                    temp = null;
                }
            }

            if (mat == null)
            {
                mat = new Mat(res, res, cvType);

                heights = terrain.terrainData.GetHeights(0, 0, res, res); //new float[res, res];

                heights1d = new float[res * res];
               
            }

        }

    }

    private void RootUpdate()
    {
        //Debug.Log("Root update!");

        SetupTerrain();


        if (terrain != null && heights != null)
        {

            mat.setTo(zeroScalar);


            if (Process(in mat))
            {

                //float[] heights1d = new float[mat.total() * mat.channels()];

                mat.get(0, 0, heights1d);

                Debug.Assert(heights.GetLength(0) * heights.GetLength(1) == heights1d.Length, "array size mismatch");

                // Unfortunately terrainData wants a 2d array, but OpenCV can only output 1D
                System.Buffer.BlockCopy(heights1d, 0, heights, 0, heights1d.Length * sizeof(float));

                terrain.terrainData.SetHeights(0, 0, heights);
            }

        }

    }




    [System.Serializable]
    public class PCGTerrainConfig 
    {
        const float OffsetRange = 100.0f;

        public string Name = "";

        [Range(-1000, 1000)]
        public float XOffset1000 = 0f;

        [Range(-100, 100)]
        public float XOffset100 = 0f;

        [Range(-10, 10)]
        public float XOffset10 = 0f;

        [Range(-1, 1)]
        public float XOffset1 = 0f;

        [Range(-1000, 1000)]
        public float YOffset1000 = 0f;

        [Range(-100, 100)]
        public float YOffset100 = 0f;

        [Range(-10, 10)]
        public float YOffset10 = 0f;

        [Range(-1, 1)]
        public float YOffset1 = 0f;


        [Range(-OffsetRange, OffsetRange)]
        public float ZOffset = 0f;

        [Range(0f, 1f)]
        public float MaxValue = 0.3f;


        [Range(0f, 100f)]
        public float PerlinScalar = 0.0013f;

        public PCGGenNoiseType GenNoiseType = PCGGenNoiseType.PerlinNoise;

        public PCGProcessParentType ProcessParentType = PCGProcessParentType.Passthrough;

        public PCGCombineType CombineType = PCGCombineType.Add;

        public List<float> ProcessParentVariables = new List<float>();

        public bool Complement = false;

        public bool Mute = false;

        public bool DoNotProcessDescendants = false;

        public List<StringyGuid> PCGConfigChildren = new List<StringyGuid>();

        public AnimationCurve GenNoiseCurve = new AnimationCurve();

        public AnimationCurve ProcessParentCurve = new AnimationCurve();

        public StringyGuid guid = System.Guid.NewGuid();


        public float XOffset {  get => XOffset1000 + XOffset100 + XOffset10 + XOffset1; }

        public float YOffset { get => YOffset1000 + YOffset100 + YOffset10 + YOffset1; }

        public PCGTerrainConfig DeepCopy()
        {
            PCGTerrainConfig c = new PCGTerrainConfig();

            c.Name = Name;

            c.XOffset1000 = XOffset1000;
            c.XOffset100 = XOffset100;
            c.XOffset10 = XOffset10;
            c.XOffset1 = XOffset1;

            c.YOffset1000 = YOffset1000;
            c.YOffset100 = YOffset100;
            c.YOffset10 = YOffset10;
            c.YOffset1 = YOffset1;

            c.ZOffset = ZOffset;
            c.MaxValue = MaxValue;
            c.PerlinScalar = PerlinScalar;
            c.ProcessParentVariables = new List<float>(ProcessParentVariables);
            c.GenNoiseType = GenNoiseType;
            c.ProcessParentType = ProcessParentType;
            c.CombineType = CombineType;
            c.Complement = Complement;
            c.Mute = Mute;
            c.DoNotProcessDescendants = DoNotProcessDescendants;
            c.GenNoiseCurve = new SerializableCurve(GenNoiseCurve).AsAnimationCurve();
            c.ProcessParentCurve = new SerializableCurve(ProcessParentCurve).AsAnimationCurve();
            c.PCGConfigChildren = new List<StringyGuid>(PCGConfigChildren);
            c.guid = guid;

            return c;
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

            return this.Equals(obj as PCGTerrainConfig);
        }

        public bool Equals(PCGTerrainConfig c)
        {

            if (!c.Name.Equals(Name)) return false;

            if (!c.XOffset1000.Equals(XOffset1000)) return false;
            if (!c.XOffset100.Equals(XOffset100)) return false;
            if (!c.XOffset10.Equals(XOffset10)) return false;
            if (!c.XOffset1.Equals(XOffset1)) return false;

            if (!c.YOffset1000.Equals(YOffset1000)) return false;
            if (!c.YOffset100.Equals(YOffset100)) return false;
            if (!c.YOffset10.Equals(YOffset10)) return false;
            if (!c.YOffset1.Equals(YOffset1)) return false;

            if (!c.ZOffset.Equals(ZOffset)) return false;
            if (!c.MaxValue.Equals(MaxValue)) return false;
            if (!c.PerlinScalar.Equals(PerlinScalar)) return false;
            if (!c.ProcessParentVariables.SequenceEqual(ProcessParentVariables)) return false;
            if (!c.GenNoiseType.Equals(GenNoiseType)) return false;
            if (!c.ProcessParentType.Equals(ProcessParentType)) return false;
            if (!c.CombineType.Equals(CombineType)) return false;
            if (!c.Complement.Equals(Complement)) return false;
            if (!c.Mute.Equals(Mute)) return false;
            if (!c.DoNotProcessDescendants.Equals(DoNotProcessDescendants)) return false;
            if (!c.GenNoiseCurve.Equals(GenNoiseCurve)) return false;
            if (!c.ProcessParentCurve.Equals(ProcessParentCurve)) return false;
            if (!c.PCGConfigChildren.SequenceEqual(PCGConfigChildren)) return false;
            if (!c.guid.Equals(guid)) return false;

            return true;
        }


        // TODO this needs to be properly implemented
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }




    public enum PCGGenNoiseType
    {
        PerlinNoise,
        PerlinNoiseWithMappingCurve,
        None
    }

    public enum PCGProcessParentType
    {
        Passthrough,
        ApplyTrapezoidFunction,
        MappingCurve,
        Complement,
        ZeroOut,
        ImageProcessing
    }

    public enum PCGCombineType
    {
        Add,
        Subtract,
        SubtractReverse,
        Multiply,
        Min,
        Max,
        NormalizeFromBottomToParent,
        NormalizeFromParentToTop,
        NormalizeFromParentToBottom,
        NormalizeFromTopToParent,
        Replace
    }


}




#endif