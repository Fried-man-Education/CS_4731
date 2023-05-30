using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HardCodedPathNetworkCases
{

    static public PathNetworkCase FindCase(PathNetworkCase c)
    {

        if (c.Equals(pn1))
            return pn1;
        else if (c.Equals(pn2))
            return pn2;
        else if (c.Equals(pn3))
            return pn3;
        else if (c.Equals(pn4))
            return pn4;
        else if (c.Equals(pn5))
            return pn5;
        else if (c.Equals(pn6))
            return pn6;
        else if (c.Equals(pn7))
            return pn7;

        return null;
    }



    static PathNetworkCase pn1 = new PathNetworkCase(
    1,
    new Vector2(-5f, -5f),
    10f, 10f,
    new Vector2[][] {
new Vector2[] {new Vector2(0.6f, 0.6f), new Vector2(-0.6f, 0.6f), new Vector2(-0.6f, -0.6f), new Vector2(0.6f, -0.6f), },
    },
    0.5f,
    new Vector2[] { new Vector2(-2.5f, 0f), new Vector2(2.5f, 0f), new Vector2(0f, -2.5f), new Vector2(0f, 2.5f), },
    new int[][] {new int[] {2, 3, },
new int[] {2, 3, },
new int[] {0, 1, },
new int[] {0, 1, },
    });

    static PathNetworkCase pn2 = new PathNetworkCase(
    2,
    new Vector2(-15f, -15f),
    10f, 10f,
    new Vector2[][] {
new Vector2[] {new Vector2(-8.066987f, -11.48205f), new Vector2(-10.06699f, -8.017949f), new Vector2(-10.93301f, -8.517949f), new Vector2(-8.933013f, -11.98205f), },
new Vector2[] {new Vector2(-6.75f, -8.25f), new Vector2(-6.75f, -6.75f), new Vector2(-8.25f, -6.75f), new Vector2(-8.25f, -8.25f), },
new Vector2[] {new Vector2(-11.79915f, -12.958f), new Vector2(-11.6248f, -12.17615f), new Vector2(-12.36449f, -12.48362f), new Vector2(-12.98681f, -12.16116f), new Vector2(-13.00988f, -12.84672f), new Vector2(-13.52206f, -13.3451f), new Vector2(-12.82231f, -13.60329f), new Vector2(-12.44653f, -14.24757f), new Vector2(-12.04466f, -13.65663f), new Vector2(-11.37352f, -13.51487f), },
    },
    0.25f,
    new Vector2[] { new Vector2(-14.5f, -14.5f), new Vector2(-5.5f, -5.5f), new Vector2(-5.5f, -14.5f), new Vector2(-14.5f, -5.5f), new Vector2(-10.39666f, -5.603279f), new Vector2(-8.477206f, -10.11165f), new Vector2(-13.53288f, -5.543574f), new Vector2(-8.586579f, -9.748789f), new Vector2(-12.17072f, -9.671462f), new Vector2(-14.27947f, -9.55695f), new Vector2(-9.704721f, -7.892857f), new Vector2(-11.38547f, -8.577192f), new Vector2(-6.439354f, -6.816024f), new Vector2(-8.653172f, -13.82786f), },
    new int[][] {new int[] {2, 3, 4, 6, 8, 9, 11, },
new int[] {2, 3, 4, 6, 12, },
new int[] {0, 1, 4, 5, 7, 9, 10, 12, 13, },
new int[] {0, 1, 4, 6, 8, 9, 10, 11, 13, },
new int[] {0, 1, 2, 3, 5, 6, 7, 8, 9, 10, 11, },
new int[] {2, 4, 7, 10, },
new int[] {0, 1, 3, 4, 8, 9, 10, 11, 13, },
new int[] {2, 4, 5, 10, },
new int[] {0, 3, 4, 6, 9, 11, 13, },
new int[] {0, 2, 3, 4, 6, 8, 11, 13, },
new int[] {2, 3, 4, 5, 6, 7, },
new int[] {0, 3, 4, 6, 8, 9, 13, },
new int[] {1, 2, },
new int[] {2, 3, 6, 8, 9, 11, },
    });

    static PathNetworkCase pn3 = new PathNetworkCase(
    3,
    new Vector2(90.5f, -105f),
    20f, 10f,
    new Vector2[][] {
new Vector2[] {new Vector2(102.1107f, -99.87218f), new Vector2(100.2313f, -99.18813f), new Vector2(99.8893f, -100.1278f), new Vector2(101.7687f, -100.8119f), },
new Vector2[] {new Vector2(103.6f, -101.92f), new Vector2(103.6f, -100.42f), new Vector2(102.1f, -100.42f), new Vector2(102.1f, -101.92f), },
new Vector2[] {new Vector2(104.15f, -97.93f), new Vector2(104.15f, -95.73f), new Vector2(101.55f, -95.73f), new Vector2(101.55f, -97.93f), },
new Vector2[] {new Vector2(98.51f, -101.45f), new Vector2(98.1f, -100.66f), new Vector2(97.68999f, -101.45f), new Vector2(96.93f, -101.62f), new Vector2(97.4f, -102.22f), new Vector2(97.32f, -103.01f), new Vector2(98.1f, -102.73f), new Vector2(98.88f, -103.01f), new Vector2(98.8f, -102.22f), new Vector2(99.27f, -101.62f), },
    },
    0.25f,
    new Vector2[] { new Vector2(91f, -104.5f), new Vector2(110f, -95.5f), new Vector2(110f, -104.5f), new Vector2(91f, -95.5f), new Vector2(106.3937f, -101.3809f), new Vector2(105.0008f, -97.00217f), new Vector2(97.74057f, -99.32016f), new Vector2(108.4122f, -98.3476f), new Vector2(92.60334f, -98.43045f), new Vector2(107.069f, -102.546f), new Vector2(98.82802f, -104.2664f), new Vector2(100.5249f, -97.65695f), new Vector2(90.88091f, -101.6223f), new Vector2(97.51548f, -100.538f), new Vector2(107.6568f, -104.5817f), new Vector2(91.50835f, -99.00614f), new Vector2(96.88289f, -95.31416f), new Vector2(104.3286f, -103.2173f), new Vector2(108.4968f, -99.45306f), },
    new int[][] {new int[] {2, 3, 6, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, },
new int[] {2, 4, 5, 7, 9, 14, 16, 17, 18, },
new int[] {0, 1, 4, 5, 7, 9, 10, 11, 14, 16, 17, 18, },
new int[] {0, 6, 8, 11, 12, 13, 15, 16, },
new int[] {1, 2, 5, 7, 9, 10, 11, 14, 16, 17, 18, },
new int[] {1, 2, 4, 7, 9, 14, 17, 18, },
new int[] {0, 3, 8, 11, 12, 13, 15, 16, },
new int[] {1, 2, 4, 5, 8, 9, 14, 15, 17, 18, },
new int[] {0, 3, 6, 7, 11, 12, 13, 15, 16, 18, },
new int[] {0, 1, 2, 4, 5, 7, 10, 11, 14, 16, 17, 18, },
new int[] {0, 2, 4, 9, 12, 14, 17, },
new int[] {0, 2, 3, 4, 6, 8, 9, 12, 13, 15, 16, },
new int[] {0, 3, 6, 8, 10, 11, 13, 15, 16, },
new int[] {0, 3, 6, 8, 11, 12, 15, 16, },
new int[] {0, 1, 2, 4, 5, 7, 9, 10, 17, 18, },
new int[] {0, 3, 6, 7, 8, 11, 12, 13, 16, },
new int[] {0, 1, 2, 3, 4, 6, 8, 9, 11, 12, 13, 15, },
new int[] {0, 1, 2, 4, 5, 7, 9, 10, 14, 18, },
new int[] {1, 2, 4, 5, 7, 8, 9, 14, 17, },
    });

    static PathNetworkCase pn4 = new PathNetworkCase(
    4,
    new Vector2(-11.6f, -3.8f),
    20f, 10f,
    new Vector2[][] {
new Vector2[] {new Vector2(5.5f, 5.8f), new Vector2(-8.5f, 5.8f), new Vector2(-8.5f, 4.8f), new Vector2(5.5f, 4.8f), },
new Vector2[] {new Vector2(5.5f, 3.35f), new Vector2(-8.5f, 3.35f), new Vector2(-8.5f, 3.25f), new Vector2(5.5f, 3.25f), },
new Vector2[] {new Vector2(3.65f, -4.05f), new Vector2(3.65f, -2.55f), new Vector2(2.15f, -2.55f), new Vector2(2.15f, -4.05f), },
new Vector2[] {new Vector2(-5.35f, -1.55f), new Vector2(-5.349999f, 0.9499999f), new Vector2(-6.85f, 0.9500003f), new Vector2(-6.85f, -1.55f), },
new Vector2[] {new Vector2(-5.369929f, -2.128686f), new Vector2(-5.33f, -1.239526f), new Vector2(-6.08007f, -1.718686f), new Vector2(-6.82325f, -1.48591f), new Vector2(-6.716218f, -2.240525f), new Vector2(-7.1805f, -2.884686f), new Vector2(-6.365f, -3.032198f), new Vector2(-5.8295f, -3.664686f), new Vector2(-5.503782f, -2.940526f), new Vector2(-4.79675f, -2.65591f), },
    },
    0.5f,
    new Vector2[] { new Vector2(6.4f, 4.3f), new Vector2(-8.6f, 4.2f), new Vector2(-10.6f, -2.8f), new Vector2(7.4f, 5.2f), new Vector2(7.4f, -2.8f), new Vector2(-10.6f, 5.2f), new Vector2(-10.246f, 3.671271f), new Vector2(-9.739081f, 0.123946f), new Vector2(7.845613f, 2.040468f), new Vector2(7.343145f, -1.698737f), new Vector2(1.578512f, -3.279974f), new Vector2(1.892153f, -1.875505f), new Vector2(-4.620164f, 1.32214f), new Vector2(-8.786647f, -0.6135375f), new Vector2(-9.399368f, -2.236613f), new Vector2(-4.46811f, 0.1471846f), new Vector2(4.264736f, 0.6988175f), new Vector2(-4.769503f, 2.700627f), new Vector2(6.10927f, 2.300941f), new Vector2(-7.94202f, -0.6166666f), new Vector2(-2.153099f, 0.5570333f), },
    new int[][] {new int[] {1, 3, 4, 8, 9, 18, },
new int[] {0, 5, 6, },
new int[] {5, 6, 7, 13, 14, 19, },
new int[] {0, 4, 8, 9, 18, },
new int[] {0, 3, 8, 9, 12, 15, 16, 17, 18, 20, },
new int[] {1, 2, 6, 7, 13, 14, 19, },
new int[] {1, 2, 5, 7, 13, 14, 19, },
new int[] {2, 5, 6, 13, 14, 17, 19, },
new int[] {0, 3, 4, 9, 11, 12, 15, 16, 17, 18, 20, },
new int[] {0, 3, 4, 8, 11, 12, 15, 16, 17, 18, 20, },
new int[] {12, 15, 17, 20, },
new int[] {8, 9, 12, 15, 16, 17, 18, 20, },
new int[] {4, 8, 9, 10, 11, 15, 16, 17, 18, 20, },
new int[] {2, 5, 6, 7, 14, 19, },
new int[] {2, 5, 6, 7, 13, 19, },
new int[] {4, 8, 9, 10, 11, 12, 16, 17, 18, 20, },
new int[] {4, 8, 9, 11, 12, 15, 17, 18, 20, },
new int[] {4, 7, 8, 9, 10, 11, 12, 15, 16, 18, 20, },
new int[] {0, 3, 4, 8, 9, 11, 12, 15, 16, 17, 20, },
new int[] {2, 5, 6, 7, 13, 14, },
new int[] {4, 8, 9, 10, 11, 12, 15, 16, 17, 18, },
    });

    static PathNetworkCase pn5 = new PathNetworkCase(
    5,
    new Vector2(-13f, -1f),
    20f, 10f,
    new Vector2[][] {
new Vector2[] {new Vector2(-5.525126f, -0.7677671f), new Vector2(-9.767767f, 3.474874f), new Vector2(-10.47487f, 2.767767f), new Vector2(-6.232233f, -1.474874f), },
new Vector2[] {new Vector2(-2.482233f, 8.174873f), new Vector2(-6.724874f, 3.932233f), new Vector2(-6.017767f, 3.225126f), new Vector2(-1.775126f, 7.467767f), },
new Vector2[] {new Vector2(0.25f, 3.3f), new Vector2(-3.75f, 3.3f), new Vector2(-3.75f, -0.7f), new Vector2(0.25f, -0.7f), },
new Vector2[] {new Vector2(6.497767f, 2.954874f), new Vector2(2.255126f, -1.287767f), new Vector2(2.962233f, -1.994874f), new Vector2(7.204874f, 2.247767f), },
new Vector2[] {new Vector2(5.03007f, 8.171314f), new Vector2(5.07f, 9.060474f), new Vector2(4.31993f, 8.581314f), new Vector2(3.576751f, 8.81409f), new Vector2(3.683782f, 8.059475f), new Vector2(3.2195f, 7.415315f), new Vector2(4.035f, 7.267801f), new Vector2(4.5705f, 6.635314f), new Vector2(4.896218f, 7.359475f), new Vector2(5.60325f, 7.64409f), },
new Vector2[] {new Vector2(-10.99f, 6.45f), new Vector2(-11.4f, 7.24f), new Vector2(-11.81f, 6.45f), new Vector2(-12.57f, 6.28f), new Vector2(-12.1f, 5.68f), new Vector2(-12.18f, 4.89f), new Vector2(-11.4f, 5.17f), new Vector2(-10.62f, 4.89f), new Vector2(-10.7f, 5.68f), new Vector2(-10.23f, 6.28f), },
    },
    0.25f,
    new Vector2[] { new Vector2(-3f, 4f), new Vector2(-12.5f, 4.5f), new Vector2(-3f, -1.5f), new Vector2(-2f, 1f), new Vector2(-5f, 0f), new Vector2(0.4000001f, -1.5f), new Vector2(-3f, 8.7f), new Vector2(-12.5f, -0.5f), new Vector2(6.5f, 8.5f), new Vector2(6.5f, -0.5f), new Vector2(-12.5f, 8.5f), new Vector2(4.928774f, 4.819587f), new Vector2(-9.467964f, 6.319695f), new Vector2(4.462442f, 2.556602f), new Vector2(1.735389f, -0.001898766f), new Vector2(-11.72438f, 8.410263f), new Vector2(-7.999627f, -0.5931578f), new Vector2(-0.3204041f, 8.12856f), new Vector2(-6.280698f, 2.536372f), new Vector2(-10.92319f, 8.079276f), new Vector2(-7.973148f, 7.204439f), new Vector2(-9.916506f, 1.267679f), new Vector2(2.311093f, 0.8979251f), new Vector2(1.198704f, 4.987261f), new Vector2(-10.12159f, 0.2425268f), new Vector2(-7.837159f, 3.511952f), },
    new int[][] {new int[] {11, 17, 18, 23, },
new int[] {7, 16, 21, 24, 25, },
new int[] {},
new int[] {},
new int[] {12, 18, 19, 25, },
new int[] {},
new int[] {10, 12, 15, 17, 19, 20, 25, },
new int[] {1, 12, 16, 21, 24, },
new int[] {11, 13, 14, 22, },
new int[] {},
new int[] {6, 12, 15, 19, 20, },
new int[] {0, 8, 13, 14, 17, 22, 23, },
new int[] {4, 6, 7, 10, 15, 18, 19, 20, 25, },
new int[] {8, 11, 14, 17, 22, 23, },
new int[] {8, 11, 13, 17, 22, 23, },
new int[] {6, 10, 12, 18, 19, 20, },
new int[] {1, 7, 21, 24, },
new int[] {0, 6, 11, 13, 14, 18, 22, 23, },
new int[] {0, 4, 12, 15, 17, 19, 25, },
new int[] {4, 6, 10, 12, 15, 18, 20, 25, },
new int[] {6, 10, 12, 15, 19, 25, },
new int[] {1, 7, 16, 24, },
new int[] {8, 11, 13, 14, 17, 23, },
new int[] {0, 11, 13, 14, 17, 22, },
new int[] {1, 7, 16, 21, },
new int[] {1, 4, 6, 12, 18, 19, 20, },
    });

    static PathNetworkCase pn6 = new PathNetworkCase(
    6,
    new Vector2(1f, -5f),
    10f, 10f,
    new Vector2[][] {
new Vector2[] {new Vector2(7f, 1f), new Vector2(6f, 1f), new Vector2(6f, 0f), new Vector2(7f, 0f), },
new Vector2[] {new Vector2(9f, 3f), new Vector2(8f, 3f), new Vector2(8f, 2f), new Vector2(9f, 2f), },
    },
    0.25f,
    new Vector2[] { new Vector2(1.5f, -4.5f), new Vector2(10.5f, 4.5f), new Vector2(10.5f, -4.5f), new Vector2(1.5f, 4.5f), new Vector2(5.603343f, 4.396721f), new Vector2(7.522794f, -0.1116495f), new Vector2(2.467118f, 4.456426f), new Vector2(7.31626f, -1.775975f), new Vector2(7.413421f, 0.2512116f), new Vector2(3.829277f, 0.3285379f), new Vector2(1.720533f, 0.4430504f), new Vector2(6.295279f, 2.107143f), new Vector2(4.614532f, 1.422808f), new Vector2(9.560646f, 3.183976f), },
    new int[][] {new int[] {2, 3, 4, 5, 6, 7, 9, 10, 11, 12, },
new int[] {2, 3, 4, 6, 10, 13, },
new int[] {0, 1, 4, 5, 7, 8, 9, 10, 13, },
new int[] {0, 1, 4, 6, 9, 10, 11, 12, 13, },
new int[] {0, 1, 2, 3, 6, 9, 10, 11, 12, 13, },
new int[] {0, 2, 7, 8, },
new int[] {0, 1, 3, 4, 9, 10, 11, 12, 13, },
new int[] {0, 2, 5, 8, 9, 10, },
new int[] {2, 5, 7, },
new int[] {0, 2, 3, 4, 6, 7, 10, 11, 12, },
new int[] {0, 1, 2, 3, 4, 6, 7, 9, 11, 12, },
new int[] {0, 3, 4, 6, 9, 10, 12, },
new int[] {0, 3, 4, 6, 9, 10, 11, },
new int[] {1, 2, 3, 4, 6, },
    });

    static PathNetworkCase pn7 = new PathNetworkCase(
    7,
    new Vector2(5f, 5f),
    10f, 10f,
    new Vector2[][] {
new Vector2[] {new Vector2(11.41421f, 10f), new Vector2(10f, 11.41421f), new Vector2(8.585787f, 10f), new Vector2(10f, 8.585787f), },
new Vector2[] {new Vector2(15f, 14.5f), new Vector2(5f, 14.5f), new Vector2(5f, 13.5f), new Vector2(15f, 13.5f), },
new Vector2[] {new Vector2(12.1f, 7.05f), new Vector2(10.5f, 7.05f), new Vector2(10.5f, 6.75f), new Vector2(12.1f, 6.75f), },
    },
    0.5f,
    new Vector2[] { new Vector2(18f, 10f), new Vector2(14.9f, 10f), new Vector2(8.35f, 6f), new Vector2(8.35f, 12.7f), new Vector2(13f, 6f), new Vector2(13f, 13.2f), new Vector2(10f, 7.8f), new Vector2(6f, 10f), },
    new int[][] {new int[] {},
new int[] {},
new int[] {4, 6, 7, },
new int[] {7, },
new int[] {2, },
new int[] {},
new int[] {2, 7, },
new int[] {2, 3, 6, },
    });



}