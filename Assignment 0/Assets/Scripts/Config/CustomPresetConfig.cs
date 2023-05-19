using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomPresetConfig : PresetConfig
{

    void Awake()
    {
        SceneConfigs.Add(SC0());
        SceneConfigs.Add(SC1());
        SceneConfigs.Add(SC2());
        SceneConfigs.Add(SC3());
        SceneConfigs.Add(SC4());
        SceneConfigs.Add(SC5());
        SceneConfigs.Add(SC6());
        SceneConfigs.Add(SC7());
    }

    SceneConfig SC0()
    {

        Vector2 origin = new Vector2(0f, 0f);


        ObstacleConfig[] obstacleConfigs =
        {
            new ObstacleConfig(ObstacleType.Cube, new Vector2(0f, 0f), new Vector2(1.2f, 1.2f), 0f)
        };


        for (int i = 0; i < obstacleConfigs.Length; ++i)
        {
            obstacleConfigs[i].pos = obstacleConfigs[i].pos + origin;
        }

        Vector2[] pathNodePositions = {
            new Vector2(-2.5f, 0f),
            new Vector2(2.5f, 0f),
            new Vector2(0f, -2.5f),
            new Vector2(0f, 2.5f),
        };

        for (int i = 0; i < pathNodePositions.Length; ++i)
            pathNodePositions[i] = pathNodePositions[i] + origin;

        return new SceneConfig(new Vector2(10f, 10f), origin, origin + new Vector2(-4f, -4f), 1f, obstacleConfigs, pathNodePositions, 1f, 0, 670238407);

    }

    SceneConfig SC1()
    {

        Vector2 origin = new Vector2(-10f, -10f);

        ObstacleConfig[] obstacleConfigs =
        {
            new ObstacleConfig(ObstacleType.Cube, new Vector2(0.5f, 0f), new Vector2(4f, 1f), 60f),
            new ObstacleConfig(ObstacleType.Cube,  new Vector2(2.5f, 2.5f), new Vector2(1.5f, 1.5f), 90f),
            new ObstacleConfig(ObstacleType.SoftStar, new Vector2(-2.4f, -3.1f), new Vector2(0.9f, 0.9f), 40f),
        };

        for(int i = 0; i < obstacleConfigs.Length; ++i)
        {
            obstacleConfigs[i].pos = obstacleConfigs[i].pos + origin;
        }

        Vector2[] pathNodePositions = { };

        for (int i = 0; i < pathNodePositions.Length; ++i)
            pathNodePositions[i] = pathNodePositions[i] + origin;

        return new SceneConfig(new Vector2(10f, 10f), origin, origin + new Vector2(-2f, 2f), 0.5f, obstacleConfigs, pathNodePositions, 0.5f, 10, 670238407);

    }


    SceneConfig SC2()
    {

        Vector2 origin = new Vector2(100.5f, -100f);

        ObstacleConfig[] obstacleConfigs =
        {
            new ObstacleConfig(ObstacleType.Cube, new Vector2(0.5f, 0f), new Vector2(2f, 1f), 20f),
            new ObstacleConfig(ObstacleType.Cube, new Vector2(2.35f, -1.17f), new Vector2(1.5f, 1.5f), 90f),
            new ObstacleConfig(ObstacleType.Cube, new Vector2(2.35f, 3.17f), new Vector2(2.2f, 2.6f), 90f),
            new ObstacleConfig(ObstacleType.SoftStar, new Vector2(-2.4f, -2f), new Vector2(1f, 1f), 0f),
        };

        for (int i = 0; i < obstacleConfigs.Length; ++i)
        {
            obstacleConfigs[i].pos = obstacleConfigs[i].pos + origin;
        }

        Vector2[] pathNodePositions = { };

        for (int i = 0; i < pathNodePositions.Length; ++i)
            pathNodePositions[i] = pathNodePositions[i] + origin;


        return new SceneConfig(new Vector2(20f, 10f), origin, origin + new Vector2(-2f, 2f), 0.5f, obstacleConfigs, pathNodePositions, 1f, 15, 1561408197);
    }


    SceneConfig SC3()
    {

        Vector2 origin = new Vector2(-1.6f, 1.2f);

        ObstacleConfig[] obstacleConfigs =
        {
            new ObstacleConfig(ObstacleType.Cube, new Vector2(0.1f, 4.1f), new Vector2(14f, 1f), 0f),
            new ObstacleConfig(ObstacleType.Cube, new Vector2(0.1f, 2.1f), new Vector2(14f, 0.1f), 0f),
            new ObstacleConfig(ObstacleType.Cube, new Vector2(4.5f, -4.5f), new Vector2(1.5f, 1.5f), 90f),
            new ObstacleConfig(ObstacleType.Cube, new Vector2(-4.5f, -1.5f), new Vector2(2.5f, 1.5f), 90f),
            new ObstacleConfig(ObstacleType.SoftStar, new Vector2(-4.4f, -3.6f), new Vector2(1f, 1f), 30f),
        };

        for (int i = 0; i < obstacleConfigs.Length; ++i)
        {
            obstacleConfigs[i].pos = obstacleConfigs[i].pos + origin;
        }

        Vector2[] pathNodePositions =
            {
                //new Vector2(0f, 3.1f),
                new Vector2(8f, 3.1f),
                new Vector2(-7f, 3f)
            };


        for (int i = 0; i < pathNodePositions.Length; ++i)
            pathNodePositions[i] = pathNodePositions[i] + origin;



        float ballSize = 1f;
        var grid = DiscretizedSpace as GameGrid;

        if (grid != null)
        {
            ballSize = 0.25f;
        }

        return new SceneConfig(new Vector2(20f, 10f), origin, origin + new Vector2(-6f, 3f), ballSize, obstacleConfigs, pathNodePositions, 0.25f, 15, 1827320275);
    }



    SceneConfig SC4()
    {

        Vector2 origin = new Vector2(-3f, 4f);

        ObstacleConfig[] obstacleConfigs =
        {
            new ObstacleConfig(ObstacleType.Cube, new Vector2(-5f, -3f), new Vector2(6f, 1f), 45f),
            new ObstacleConfig(ObstacleType.Cube, new Vector2(-1.25f, 1.7f), new Vector2(6f, 1f), -45f),
            new ObstacleConfig(ObstacleType.Cube, new Vector2(1.25f, -2.7f), new Vector2(4f, 4f), 0f),
            new ObstacleConfig(ObstacleType.Cube, new Vector2(7.73f, -3.52f), new Vector2(6f, 1f), -45f),
            new ObstacleConfig(ObstacleType.SoftStar, new Vector2(7.4f, 3.9f), new Vector2(1f, 1f), 30f),
            new ObstacleConfig(ObstacleType.SoftStar, new Vector2(-8.4f, 1.9f), new Vector2(1f, 1f), 0f),
        };

        for (int i = 0; i < obstacleConfigs.Length; ++i)
        {
            obstacleConfigs[i].pos = obstacleConfigs[i].pos + origin;
        }

        Vector2[] pathNodePositions =
            {
                new Vector2(0f, 0f),
                new Vector2(-9.5f, 0.5f),
                new Vector2(0f, -5.5f),
                new Vector2(1f, -3f),
                new Vector2(-2f, -4f),
                new Vector2(3.4f, -5.5f),
                new Vector2(0f, 4.7f),
            };

        for (int i = 0; i < pathNodePositions.Length; ++i)
            pathNodePositions[i] = pathNodePositions[i] + origin;



        return new SceneConfig(new Vector2(20f, 10f), origin, origin + new Vector2(-8.125f, -3.125f), 0.5f, obstacleConfigs, pathNodePositions, 0.75f, 15, 309667876);
    }



    SceneConfig SC5()
    {
        Vector2 origin = new Vector2(6f, 0f);

        ObstacleConfig[] obstacleConfigs =
        {
            new ObstacleConfig(ObstacleType.Cube, new Vector2(0.5f, 0.5f), new Vector2(1f, 1f), 0f),
            new ObstacleConfig(ObstacleType.Cube, new Vector2(2.5f, 2.5f), new Vector2(1f, 1f), 0f),

        };

        for (int i = 0; i < obstacleConfigs.Length; ++i)
        {
            obstacleConfigs[i].pos = obstacleConfigs[i].pos + origin;
        }

        Vector2[] pathNodePositions =
            {
                
            };

        for (int i = 0; i < pathNodePositions.Length; ++i)
            pathNodePositions[i] = pathNodePositions[i] + origin;



        return new SceneConfig(new Vector2(10f, 10f), origin, origin + new Vector2(-2f, 2f), 0.5f, obstacleConfigs, pathNodePositions, 1f, 10, 670238407);

    }


    SceneConfig SC6()
    {

        Vector2 origin = new Vector2(10f, 10f);

 
        ObstacleConfig[] obstacleConfigs =
        {
            new ObstacleConfig(ObstacleType.Cube, Vector2.zero, 2f * Vector2.one, 45f),
            new ObstacleConfig(ObstacleType.Cube, new Vector2(0f, 4f), new Vector2(10f, 1f), 0f),
            new ObstacleConfig(ObstacleType.Cube, new Vector2(1.3f, -3.1f), new Vector2(1.6f, 0.3f), 0f)
        };

        for (int i = 0; i < obstacleConfigs.Length; ++i)
        {
            obstacleConfigs[i].pos = obstacleConfigs[i].pos + origin;
        }


        Vector2[] pathNodePositions = {
            new Vector2(8f, 0f),
            new Vector2(4.9f, 0f),
            new Vector2(-1.65f, -4f),
            new Vector2(-1.65f, 2.7f),
            new Vector2(3f, -4f),
            new Vector2(3f, 3.2f),
            new Vector2(0f, -2.2f),
            new Vector2(-4f, 0f),
        };

        for (int i = 0; i < pathNodePositions.Length; ++i)
            pathNodePositions[i] = pathNodePositions[i] + origin;



        return new SceneConfig(new Vector2(10f, 10f), origin, origin + new Vector2(-4f, -4f), 1f, obstacleConfigs, pathNodePositions, 10f, 0, 670238407);

    }

    SceneConfig SC7()
    {

        // Some setup showing how to place custom objects relative to the final canvasOrigin
        float agentScale = 1f;
        Vector2 worldSize = new Vector2(40f, 20f);
        Vector2 halfWS = 0.5f * worldSize;
        Vector2 origin = halfWS;
        Vector2 canvasOrigin = Vector2.zero;


        // Assume the canvasOrigin is 0,0 for now
        ObstacleConfig[] obstacleConfigs =
        {
            new ObstacleConfig(ObstacleType.Custom, Vector2.zero, Vector2.one, 0f,
                    new Vector2[] {
                        new Vector2(0f, 0f),
                        new Vector2(1f, 0f),
                        new Vector2(1f, 20f),
                        new Vector2(0f, 20f),                    
                    }),

            new ObstacleConfig(ObstacleType.Custom, Vector2.zero, Vector2.one, 0f,
                    new Vector2[] {
                        new Vector2(1f, 20f),
                        new Vector2(1f, 19f),
                        new Vector2(10f, 1f),
                        new Vector2(11f, 1f),
                        new Vector2(2f, 20f),
                    }),

            new ObstacleConfig(ObstacleType.Custom, Vector2.zero, Vector2.one, 0f,
                    new Vector2[] {
                        new Vector2(20f, 4f),
                        new Vector2(22f, 4f),
                        new Vector2(21f, 10f),
                    }),

                        new ObstacleConfig(ObstacleType.Custom, Vector2.zero, Vector2.one, 0f,
                    new Vector2[] {
                        new Vector2(12f, 15f),
                        new Vector2(12f, 13f),
                        new Vector2(30f, 14f),
                    }),

        };

        // Adjust if the canvasOrigin isn't actually 0,0 
        for (int i = 0; i < obstacleConfigs.Length; ++i)
        {
            obstacleConfigs[i].pos = obstacleConfigs[i].pos + canvasOrigin;
        }


        Vector2[] pathNodePositions = {
        };

        for (int i = 0; i < pathNodePositions.Length; ++i)
            pathNodePositions[i] = pathNodePositions[i] + canvasOrigin;



        return new SceneConfig(worldSize, origin, origin -halfWS + Vector2.one * 1.5f * agentScale, agentScale, obstacleConfigs, pathNodePositions, agentScale, 0, 670238407);

    }


    //SceneConfig SC7()
    //{

    //    Vector2 origin = new Vector2(10f, 10f);

    //    ObstacleConfig[] obstacleConfigs =
    //    {
    //    };

    //    for (int i = 0; i < obstacleConfigs.Length; ++i)
    //    {
    //        obstacleConfigs[i].pos = obstacleConfigs[i].pos + origin;
    //    }


    //    Vector2[] pathNodePositions = {
    //        new Vector2(8f, 0f),
    //        new Vector2(4.9f, 0f),
    //        new Vector2(-1.65f, -4f),
    //        new Vector2(-1.65f, 2.7f),
    //        new Vector2(3f, -4f),
    //        new Vector2(3f, 3.2f),
    //        new Vector2(0f, -2.2f),
    //        new Vector2(-4f, 0f),
    //    };

    //    for (int i = 0; i < pathNodePositions.Length; ++i)
    //        pathNodePositions[i] = pathNodePositions[i] + origin;



    //    return new SceneConfig(new Vector2(10f, 10f), origin, origin + new Vector2(-4f, -4f), 1f, obstacleConfigs, pathNodePositions, 10f, 0, 670238407);

    //}


    //SceneConfig SC8()
    //{

    //    Vector2 origin = new Vector2(10f, 10f);

    //    ObstacleConfig[] obstacleConfigs =
    //    {
    //    };

    //    for (int i = 0; i < obstacleConfigs.Length; ++i)
    //    {
    //        obstacleConfigs[i].pos = obstacleConfigs[i].pos + origin;
    //    }

    //    Vector2[] pathNodePositions = {
    //        new Vector2(8f, 0f),
    //        new Vector2(4.9f, 0f),
    //        new Vector2(-1.65f, -4f),
    //        new Vector2(-1.65f, 2.7f),
    //        new Vector2(3f, -4f),
    //        new Vector2(3f, 3.2f),
    //        new Vector2(0f, -2.2f),
    //        new Vector2(-4f, 0f),
    //    };

    //    for (int i = 0; i < pathNodePositions.Length; ++i)
    //        pathNodePositions[i] = pathNodePositions[i] + origin;



    //    return new SceneConfig(new Vector2(10f, 20f), origin, origin + new Vector2(-4f, -4f), 1f, obstacleConfigs, pathNodePositions, 10f, 0, 670238407);

    //}

    //SceneConfig SC9()
    //{

    //    Vector2 origin = new Vector2(10f, 10f);

    //    ObstacleConfig[] obstacleConfigs =
    //    {
    //    };

    //    for (int i = 0; i < obstacleConfigs.Length; ++i)
    //    {
    //        obstacleConfigs[i].pos = obstacleConfigs[i].pos + origin;
    //    }

    //    Vector2[] pathNodePositions = {
    //        new Vector2(8f, 0f),
    //        new Vector2(4.9f, 0f),
    //        new Vector2(-1.65f, -4f),
    //        new Vector2(-1.65f, 2.7f),
    //        new Vector2(3f, -4f),
    //        new Vector2(3f, 3.2f),
    //        new Vector2(0f, -2.2f),
    //        new Vector2(-4f, 0f),
    //    };


    //    for (int i = 0; i < pathNodePositions.Length; ++i)
    //        pathNodePositions[i] = pathNodePositions[i] + origin;



    //    return new SceneConfig(new Vector2(30f, 10f), origin, origin + new Vector2(-4f, -4f), 1f, obstacleConfigs, pathNodePositions, 10f, 0, 670238407);

    //}



}
