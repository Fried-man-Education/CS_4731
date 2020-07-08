using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public partial class PCGTerrain : MonoBehaviour
{

    [SerializeField]
    public PCGTerrainConfig Config = null;

    [SerializeField]
    public StringyGuid guid;

    Terrain terrain;

    public PCGTerrainConfigSerializableObject ConfigSerializableObject;

    float Epsilon = 0.001f;


    public Status status = Status.OK;

    public enum Status
    {
        OK,
        ERROR_PROCESS_PARENT_BANDPASS_MISSING_VARS,
        NOT_IMPLEMENTED,
    }

  
    ImprovedPerlinNoise PerlinNoise = new ImprovedPerlinNoise();

    public List<PCGTerrain> PCGChildren = new List<PCGTerrain>();


    // This method should generate perlin noise according to the arguments passed in.
    // The return should be between: [0,1]
    public static float FindPerlinNoiseAtPos(
        // A Perlin noise generator. You can call pnoise.Noise(x,y,z) to generate a value between [-1,1]
        ImprovedPerlinNoise pnoise, 
        // The coordinates of the terrain heightmap value, but cast to float
        float i, float j, 
        // Offsets to add to the normalized and scaled i and j. The kOffset is used as a seed in the otherwise unused 3rd dimension
        // of the Perlin noise generator
        float iOffset, float jOffset, float kOffset,
        // The width and height of the terrain heightmap array (cast to float). These are used to normalize the i and j values
        float width, float height,
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
        bool doInvert)
    {


        // TODO You need to get noise from the Perlin Noise generator via ImprovedPerlinNoise.Noise() of the pnoise object.
        // Use i and j for the x and y parameters, but first normalize by width or height (div by), then multiply each by the
        // perlinScalar. This controls "zoom" on the noise pattern and therefore affects the frequency with which values change.
        // Lastly, add iOffset/jOffset to allow for panning around in the noise.
        // For the Z value, just pass in kOffset. In the Inspector, the z value can be modified to find different noise patterns

        // Placeholder - to be replaced!
        var noise = Random.Range(0f, 1f);

        // TODO The Perlin Noise generator returns values between [-1, 1]. Remap to [0, 1]
  
        // TODO if useMappingCurve is true, process your noise with AnimationCurve.Evaluate() of the curve object

        // Scale the output noise to a max value
        float val = maxVal * noise;

        // TODO clamp your output value to be between [0, 1] just in case it's a little off 

        // TODO if doInvert is set true, then invert it

        return val;
    }

    // The following function allows for a variety of PCG terrain effects to be realized.
    // This is a recursive function that is executed according to a rules graph affecting generation and combination at each level.
    // It generates a value for the current level in the graph, then evalutates child nodes
    // to see if the children modify the current node's value. If there is more than one child the sum
    // of childrens' values replace the current node's value. A single child replaces the current node's
    // value. If there are no children, then the current node keeps its generated value.
    // 
    // Next the value of the current node's parent (or a default value if the current node is root) is processed.
    // This could involve keeping it as is or transforming the parent's value in some way.
    //
    // The processed parent value is then combined with the current node's generated value (which itself may have been 
    // modified by its children).
    float ApplyPerlinNoiseContribution(float i, float j, float width, float height, float parentVal)
    {
        status = Status.OK;

        float genVal = 0f;

        if (!Config.Mute)
        {

            if (Config.GenNoiseType == PCGGenNoiseType.None)
            {
                genVal = Config.MaxValue;
            }
            else
            {
                genVal = FindPerlinNoiseAtPos(PerlinNoise, i, j, Config.XOffset, Config.YOffset, Config.ZOffset, width, height,
                            Config.PerlinScalar, Config.MaxValue,
                            Config.GenNoiseType == PCGGenNoiseType.PerlinNoiseWithMappingCurve,
                            Config.GenNoiseCurve, Config.Invert);
            }

        }

        if (PCGChildren.Count > 0 && !Config.DoNotProcessDescendants)
        {
            float descendantCombGenVal = 0f;

            foreach(var pcg in PCGChildren)
            {
                if (pcg != null)
                {

                    descendantCombGenVal += pcg.ApplyPerlinNoiseContribution(i + Config.XOffset, j + Config.YOffset, width, height, genVal);
                }
            }

            genVal = descendantCombGenVal;

        }

        switch (Config.ProcessParentType)
        {
            case PCGProcessParentType.Passthrough:
                //Do nothing
                // same as parentVal = parentVal;
                break;
            case PCGProcessParentType.Invert:

                // TODO Invert the parentVal

                status = Status.NOT_IMPLEMENTED;

                break;
            case PCGProcessParentType.MappingCurve:

                // TODO Using Config.ProessParentCurve, evaluate parentVal with the curve and replace parentVal with the result

                status = Status.NOT_IMPLEMENTED;

                break;
            case PCGProcessParentType.BandpassLinearCrossFade:

                // TODO Check if the parent value is in an identified region (bandpass), or in the fringe (crossfade area).
                // At at high level, this is determining if the parent value (as dist along an x axis) is in a trapezoidal 
                // region and then returning the height of the trapezoid (1.0 high in the thickest part, but sloping down on the sides)
                //
                // Use 4 variables from Config.ProcessParentVariables list to identify whether the parent value
                // is fully contained within, or in a crossfade region.
                // The 4 variables go in order (index 0 to 3) as 
                // 0: beginning of low cross fade area
                // 1: end of low cross fade area and also the beginning of the bandpass area
                // 2: end of the bandpass area and also the END of the high crossfade area
                // 3: beginning of the high cross fade area
                // Crossfade areas are linearly interpolated from beginning (output==0.0) to end (output==1.0)
                // The bandpass (inner) area always maps to 1.0
                // Assume that all 4 values are in order lowest to highest (no error checking needed)
                // 

                if (Config.ProcessParentVariables.Count >= 4)
                {

                    var lowest = Config.ProcessParentVariables[0];
                    var low = Config.ProcessParentVariables[1];
                    var high = Config.ProcessParentVariables[2];
                    var highest = Config.ProcessParentVariables[3];

                    // TODO See comments above for implementation

                    status = Status.NOT_IMPLEMENTED;

                }
                else
                {
                    status = Status.ERROR_PROCESS_PARENT_BANDPASS_MISSING_VARS;
                }

                break;
            default:
                // Do nothing
                status = Status.NOT_IMPLEMENTED;
                break;
        }


        PCGCombineType combType = Config.CombineType;

        switch (combType)
        {
            case PCGCombineType.Add:
                // parent val + output val
                parentVal += genVal;
                break;
            case PCGCombineType.Subtract:
                // TODO - parent val - output val
                status = Status.NOT_IMPLEMENTED;
                break;
            case PCGCombineType.SubtractReverse:
                // TODO - output val - parent val
                status = Status.NOT_IMPLEMENTED;
                break;
            case PCGCombineType.Multiply:
                // TODO - multiply parent val and output val
                status = Status.NOT_IMPLEMENTED;
                break;
            case PCGCombineType.Min:
                // TODO - minimum of parent val and output val
                status = Status.NOT_IMPLEMENTED;
                break;
            case PCGCombineType.Max:
                // TODO - maximum of parent value and output val
                status = Status.NOT_IMPLEMENTED;
                break;
            case PCGCombineType.NormalizeFromParentToTop:
                // TODO - parent value is 0.0, top (1.0) is 1.0
                status = Status.NOT_IMPLEMENTED;
                break;
            case PCGCombineType.NormalizeFromBottomToParent:
                // TODO - bottom (0.0) is 0.0, parent value is 1.0
                status = Status.NOT_IMPLEMENTED;
                break;
            case PCGCombineType.NormalizeFromParentToBottom:
                // TODO - parent value is 0.0, bottom (0.0) is 1.0
                status = Status.NOT_IMPLEMENTED;
                break;
            case PCGCombineType.NormalizeFromTopToParent:
                // TODO - top (1.0) is 0.0, parent value is 1.0
                status = Status.NOT_IMPLEMENTED;
                break;
            default:
                status = Status.NOT_IMPLEMENTED;
                parentVal += genVal;
                break;
        }
    
        parentVal = Mathf.Clamp(parentVal, 0f, 1f);


        return parentVal;

    }

 
    // This is the starting point of the PCG terrain rules processing. It iterates through
    // each cell of the terrain heightmap array starting from the root rule node
    void UpdateTerrain(Terrain _terrain)
    {

        if(_terrain == null)
        {
            Debug.LogError("No terrain assigned!");
            return;
        }

        // prepare children for generation of noise
        ProcessChildren();

        var res = _terrain.terrainData.heightmapResolution;

        var size = _terrain.terrainData.size;

        //var width = size.x;
        //var height = size.z;

        Debug.Log($"Res is {res}, Size is: {size}");

        float[,] heights = _terrain.terrainData.GetHeights(0, 0, res, res); //new float[res, res];

        for (int i = 0; i < res; ++i)
        {
            for (int j = 0; j < res; ++j)
            {

                heights[i, j] = 0f;
  
                heights[i,j] = ApplyPerlinNoiseContribution((float)i, (float)j, res, res, 0f);

            }
        }

        _terrain.terrainData.SetHeights(0, 0, heights);

    }


    [System.Serializable]
    public class PCGTerrainConfig
    {
        const float OffsetRange = 50.0f;

        public string Name = "";

        [Range(-OffsetRange, OffsetRange)]
        public float XOffset = 0f;

        [Range(-OffsetRange, OffsetRange)]
        public float YOffset = 0f;

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

        public bool Invert = false;

        public bool Mute = false;

        public bool DoNotProcessDescendants = false;

        public List<StringyGuid> PCGConfigChildren = new List<StringyGuid>();

        public AnimationCurve GenNoiseCurve = new AnimationCurve();

        public AnimationCurve ProcessParentCurve = new AnimationCurve();

        public StringyGuid guid = System.Guid.NewGuid();

        public PCGTerrainConfig DeepCopy()
        {
            PCGTerrainConfig c = new PCGTerrainConfig();

            c.Name = Name;
            c.XOffset = XOffset;
            c.YOffset = YOffset;
            c.ZOffset = ZOffset;
            c.MaxValue = MaxValue;
            c.PerlinScalar = PerlinScalar;
            c.ProcessParentVariables = new List<float>(ProcessParentVariables);
            c.GenNoiseType = GenNoiseType;
            c.ProcessParentType = ProcessParentType;
            c.CombineType = CombineType;
            c.Invert = Invert;
            c.Mute = Mute;
            c.DoNotProcessDescendants = DoNotProcessDescendants;
            c.GenNoiseCurve = new SerializableCurve(GenNoiseCurve).AsAnimationCurve();
            c.ProcessParentCurve = new SerializableCurve(ProcessParentCurve).AsAnimationCurve();
            c.PCGConfigChildren = new List<StringyGuid>(PCGConfigChildren);
            c.guid = guid;

            return c;
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
        BandpassLinearCrossFade,
        MappingCurve,
        Invert
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
        NormalizeFromTopToParent
    }

}



