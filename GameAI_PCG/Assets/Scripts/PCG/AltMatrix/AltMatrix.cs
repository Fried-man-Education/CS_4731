using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AltMatrix
{

    public class CvType
    {
        public static int CV_32F = 0;
        public static int CV_8U = 1;
    }

    public class Scalar
    {
        float mScalar;

        public void set(float s)
        {
            mScalar = s;
        }

        public float value()
        {
            return mScalar;
        }

        public void set(double[] s)
        {
            if (s.Length <= 0)
                return;

            mScalar = (float)s[0];
        }

        public Scalar(float s)
        {
            set(s);
        }
    }


    public class Mat: System.IDisposable
    {
        float[,] mMat;
        int mType = 0;
        private bool disposedValue;

        public Mat(int rows, int cols, int type)
        {
            mMat = new float[rows, cols];
            mType = type;
        }

        public Mat clone()
        {
            var numRows = mMat.GetLength(0);
            var numCols = mMat.GetLength(1);
            Mat cMat = new Mat(numRows, numCols, mType);

            this.copyTo(cMat);

            return cMat;
        }


        public void copyTo(Mat m)
        {
            var srcNumRows = mMat.GetLength(0);
            var srcNumCols = mMat.GetLength(1);
            var destMat = m.getRaw();
            var destNumRows = destMat.GetLength(0);
            var destNumCols = destMat.GetLength(1);

            if(srcNumRows != destNumRows || srcNumCols != destNumCols)
            {
                destMat = new float[srcNumRows, srcNumCols];
            }

            for(int j = 0; j < srcNumRows; ++j)
            {
                for(int i = 0; i < srcNumCols; ++i)
                {
                    destMat[j, i] = mMat[j, i];
                }
            }

            m.setRaw(destMat);
        }



        public float[,] getRaw()
        {
            return mMat;
        }

        public void setRaw(float[,] newMat)
        {
            mMat = newMat;
        }

        public int size(int dim)
        {
            return mMat.GetLength(dim);
        }

        public int width()
        {
            return size(1);
        }

        public int height()
        {
            return size(0);
        }

        public int total()
        {
            return mMat.Length;
        }

        public int channels()
        {
            return 1;
        }

        public void get(float[,] dataOut)
        {
            var numRows = mMat.GetLength(0);
            var numCols = mMat.GetLength(1);
            var dataRows = dataOut.GetLength(0);
            var dataCols = dataOut.GetLength(1);

            if(numRows != dataRows || numCols != dataCols)
            {
                throw new System.ArgumentException("size mismatch");
            }

            for(int j = 0; j < numRows; ++j)
            {
                for(int i = 0; i < numCols; ++i)
                {
                    dataOut[j, i] = mMat[j, i];
                }
            }
        }

        public void put(int row, int col, float[] data)
        {
            var numRows = mMat.GetLength(0);
            var numCols = mMat.GetLength(1);
            var dataLen = data.Length;

            bool onEntry = true;

            int dataPos = 0;

            if (row < 0 || col < 0 || row >= numRows || col >= numCols || data == null)
                throw new System.ArgumentException("size mismatch");

            for(int j = row; j < numRows; ++j)
            {
                int startCol = 0;

                if (onEntry)
                {
                    onEntry = false;
                    startCol = col;

                }
                for(int i = startCol; i < numCols && dataPos < dataLen; ++i, ++dataPos)
                {
                    mMat[j, i] = data[dataPos];
                }

                if (dataPos >= dataLen)
                    break;
            }
        }

        public void setTo(Scalar s)
        {
            var numRows = mMat.GetLength(0);
            var numCols = mMat.GetLength(1);


            for (int j = 0; j < numRows; ++j)
            {
                for (int i = 0; i < numCols; ++i)
                {
                    mMat[j, i] = s.value();
                }
            }
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects)
                }

                // free unmanaged resources (unmanaged objects) and override finalizer
                // set large fields to null
                mMat = null;
                disposedValue = true;
            }
        }

        // // override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~Mat()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            System.GC.SuppressFinalize(this);
        }
    }


    public class Core
    {

        public static void add(Mat a, Mat b, Mat res)
        {
            apply(a, b, res, (float x, float y) => x + y);
        }

        public static void subtract(Mat a, Mat b, Mat res)
        {
            apply(a, b, res, (float x, float y) => x - y);
        }

        public static void absdiff(Mat a, Mat b, Mat res)
        {
            apply(a, b, res, (float x, float y) => Mathf.Abs(x - y));
        }

        public static void absdiff(Mat a, Scalar b, Mat res)
        {
            apply(a, b, res, (float x, float y) => Mathf.Abs(x - y));
        }

        public static void multiply(Mat a, Mat b, Mat res)
        {
            apply(a, b, res, (float x, float y) => x * y);
        }

        public static void min(Mat a, Mat b, Mat res)
        {
            apply(a, b, res, (float x, float y) => Mathf.Min(x, y));
        }

        public static void max(Mat a, Mat b, Mat res)
        {
            apply(a, b, res, (float x, float y) => Mathf.Max(x, y));
        }

        //public delegate float CellCallbackFunc(float a, float b);

        public static void apply(Mat a, Mat res, System.Func<float,float> cbk)
        {
            var rawRes = res.getRaw();
            var rawA = a.getRaw();


            var resRows = rawRes.GetLength(0);
            var resCols = rawRes.GetLength(1);
            var aRows = rawA.GetLength(0);
            var aCols = rawA.GetLength(1);


            if (resRows != aRows || resCols != aCols )
            {
                throw new System.ArgumentException("must be same size");
            }

            for (int j = 0; j < resRows; ++j)
            {
                for (int i = 0; i < resCols; ++i)
                {
                    rawRes[j, i] = cbk(rawA[j, i]);
                }
            }

            res.setRaw(rawRes);

        }



        public static void apply(Mat a, Mat b, Mat res, System.Func<float, float, float> cbk)
        {
            var rawRes = res.getRaw();
            var rawA = a.getRaw();
            var rawB = b.getRaw();

            var resRows = rawRes.GetLength(0);
            var resCols = rawRes.GetLength(1);
            var aRows = rawA.GetLength(0);
            var aCols = rawA.GetLength(1);
            var bRows = rawB.GetLength(0);
            var bCols = rawB.GetLength(1);

            if (resRows != aRows || resRows != bRows || resCols != aCols || resCols != bCols)
            {
                throw new System.ArgumentException("must be same size");
            }

            for (int j = 0; j < resRows; ++j)
            {
                for (int i = 0; i < resCols; ++i)
                {
                    rawRes[j, i] = cbk(rawA[j, i], rawB[j, i]);
                }
            }

            res.setRaw(rawRes);

        }



        public static void apply(Mat a, Scalar b, Mat res, System.Func<float, float, float> cbk)
        {
            var rawRes = res.getRaw();
            var rawA = a.getRaw();
  
            var resRows = rawRes.GetLength(0);
            var resCols = rawRes.GetLength(1);
            var aRows = rawA.GetLength(0);
            var aCols = rawA.GetLength(1);

            var bValue = b.value();

            if (resRows != aRows || resCols != aCols)
            {
                throw new System.ArgumentException("must be same size");
            }

            for (int j = 0; j < resRows; ++j)
            {
                for (int i = 0; i < resCols; ++i)
                {
                    rawRes[j, i] = cbk(rawA[j, i], bValue);
                }
            }

            res.setRaw(rawRes);

        }

    }

}