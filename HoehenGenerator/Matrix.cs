﻿using System;

namespace HoehenGenerator
{
    public class Matrix
    {
        private readonly double[,] mInnerMatrix;
        private readonly int mRowCount = 0;
        public int RowCount
        {
            get { return mRowCount; }
        }
        private readonly int mColumnCount = 0;
        public int ColumnCount
        {
            get { return mColumnCount; }
        }

        public Matrix()
        {

        }
        public Matrix(int rowCount, int columnCount)
        {
            mRowCount = rowCount;
            mColumnCount = columnCount;
            mInnerMatrix = new double[rowCount, columnCount];
        }

        public double this[int rowNumber, int columnNumber]
        {
            get
            {
                return mInnerMatrix[rowNumber, columnNumber];
            }
            set
            {
                mInnerMatrix[rowNumber, columnNumber] = value;
            }
        }

        public double[] GetRow(int rowIndex)
        {
            double[] rowValues = new double[mColumnCount];
            for (int i = 0; i < mColumnCount; i++)
            {
                rowValues[i] = mInnerMatrix[rowIndex, i];
            }
            return rowValues;
        }
        public void SetRow(int rowIndex, double[] value)
        {
            if (value.Length != mColumnCount)
            {
                throw new Exception("Größeninkongruenz");
            }
            for (int i = 0; i < value.Length; i++)
            {
                mInnerMatrix[rowIndex, i] = value[i];
            }
        }
        public double[] GetColumn(int columnIndex)
        {
            double[] columnValues = new double[mRowCount];
            for (int i = 0; i < mRowCount; i++)
            {
                columnValues[i] = mInnerMatrix[i, columnIndex];
            }
            return columnValues;
        }
        public void SetColumn(int columnIndex, double[] value)
        {
            if (value.Length != mRowCount)
            {
                throw new Exception("Größeninkongruenz");
            }
            for (int i = 0; i < value.Length; i++)
            {
                mInnerMatrix[i, columnIndex] = value[i];
            }
        }


        public static Matrix operator +(Matrix pMatrix1, Matrix pMatrix2)
        {
            if (!(pMatrix1.RowCount == pMatrix2.RowCount && pMatrix1.ColumnCount == pMatrix2.ColumnCount))
            {
                throw new Exception("Größeninkongruenz");
            }
            Matrix returnMartix = new Matrix(pMatrix1.RowCount, pMatrix2.RowCount);
            for (int i = 0; i < pMatrix1.RowCount; i++)
            {
                for (int j = 0; j < pMatrix1.ColumnCount; j++)
                {
                    returnMartix[i, j] = pMatrix1[i, j] + pMatrix2[i, j];
                }
            }
            return returnMartix;
        }
        public static Matrix operator *(double scalarValue, Matrix pMatrix)
        {
            Matrix returnMartix = new Matrix(pMatrix.RowCount, pMatrix.RowCount);
            for (int i = 0; i < pMatrix.RowCount; i++)
            {
                for (int j = 0; j < pMatrix.ColumnCount; j++)
                {
                    returnMartix[i, j] = pMatrix[i, j] * scalarValue;
                }
            }
            return returnMartix;
        }
        public static Matrix operator -(Matrix pMatrix1, Matrix pMatrix2)
        {
            if (!(pMatrix1.RowCount == pMatrix2.RowCount && pMatrix1.ColumnCount == pMatrix2.ColumnCount))
            {
                throw new Exception("Größeninkongruenz");
            }
            return pMatrix1 + (-1 * pMatrix2);
        }
        public static bool operator ==(Matrix pMatrix1, Matrix pMatrix2)
        {
            if (!(pMatrix1.RowCount == pMatrix2.RowCount && pMatrix1.ColumnCount == pMatrix2.ColumnCount))
            {
                //Größeninkongruenz
                return false;
            }
            for (int i = 0; i < pMatrix1.RowCount; i++)
            {
                for (int j = 0; j < pMatrix1.ColumnCount; j++)
                {
                    if (pMatrix1[i, j] != pMatrix2[i, j])
                    {
                        return false;
                    }
                }
            }
            return true; ;
        }
        public static bool operator !=(Matrix pMatrix1, Matrix pMatrix2)
        {
            return !(pMatrix1 == pMatrix2);
        }
        public static Matrix operator -(Matrix pMatrix)
        {
            return -1 * pMatrix;
        }
        public static Matrix operator ++(Matrix pMatrix)
        {

            for (int i = 0; i < pMatrix.RowCount; i++)
            {
                for (int j = 0; j < pMatrix.ColumnCount; j++)
                {
                    pMatrix[i, j] += 1;
                }
            }
            return pMatrix;
        }
        public static Matrix operator --(Matrix pMatrix)
        {
            for (int i = 0; i < pMatrix.RowCount; i++)
            {
                for (int j = 0; j < pMatrix.ColumnCount; j++)
                {
                    pMatrix[i, j] -= 1;
                }
            }
            return pMatrix;
        }
        public static Matrix operator *(Matrix pMatrix1, Matrix pMatrix2)
        {
            if (pMatrix1.ColumnCount != pMatrix2.RowCount)
            {
                throw new Exception("Größeninkongruenz");
            }
            Matrix returnMatrix = new Matrix(pMatrix1.RowCount, pMatrix2.ColumnCount);
            for (int i = 0; i < pMatrix1.RowCount; i++)
            {
                double[] rowValues = pMatrix1.GetRow(i);
                for (int j = 0; j < pMatrix2.ColumnCount; j++)
                {
                    double[] columnValues = pMatrix2.GetColumn(j);
                    double value = 0;
                    for (int a = 0; a < rowValues.Length; a++)
                    {
                        value += rowValues[a] * columnValues[a];
                    }
                    returnMatrix[i, j] = value;
                }
            }
            return returnMatrix;
        }
        public Matrix Transpose()
        {
            Matrix mReturnMartix = new Matrix(ColumnCount, RowCount);
            for (int i = 0; i < mRowCount; i++)
            {
                for (int j = 0; j < mColumnCount; j++)
                {
                    mReturnMartix[j, i] = mInnerMatrix[i, j];
                }
            }
            return mReturnMartix;
        }
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public bool IsZeroMatrix()
        {
            for (int i = 0; i < this.RowCount; i++)
            {
                for (int j = 0; j < this.ColumnCount; j++)
                {
                    if (mInnerMatrix[i, j] != 0)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        public bool IsSquareMatrix()
        {
            return (this.RowCount == this.ColumnCount);
        }
        public bool IsLowerTriangle()
        {
            if (!this.IsSquareMatrix())
            {
                return false;
            }
            for (int i = 0; i < this.RowCount; i++)
            {
                for (int j = i + 1; j < this.ColumnCount; j++)
                {
                    if (mInnerMatrix[i, j] != 0)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        public bool IsUpperTriangle()
        {
            if (!this.IsSquareMatrix())
            {
                return false;
            }
            for (int i = 0; i < this.RowCount; i++)
            {
                for (int j = 0; j < i; j++)
                {
                    if (mInnerMatrix[i, j] != 0)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        public bool IsDiagonalMatrix()
        {
            if (!this.IsSquareMatrix())
            {
                return false;
            }
            for (int i = 0; i < this.RowCount; i++)
            {
                for (int j = 0; j < this.ColumnCount; j++)
                {
                    if (i != j && mInnerMatrix[i, j] != 0)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        public bool IsIdentityMatrix()
        {
            if (!this.IsSquareMatrix())
            {
                return false;
            }
            for (int i = 0; i < this.RowCount; i++)
            {
                for (int j = 0; j < this.ColumnCount; j++)
                {
                    double checkValue = 0;
                    if (i == j)
                    {
                        checkValue = 1;
                    }
                    if (mInnerMatrix[i, j] != checkValue)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        public bool IsSymetricMatrix()
        {
            if (!this.IsSquareMatrix())
            {
                return false;
            }
            Matrix transposeMatrix = this.Transpose();
            if (this == transposeMatrix)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


    }
}
