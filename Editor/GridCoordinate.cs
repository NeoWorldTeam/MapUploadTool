using UnityEngine;


public class GridCoordinate
    {
        public Matrix4x4 CoordinateTransformedMatrix { get; private set; }
        public Matrix4x4 CoordinateTransformationInvert { get; private set; }



        public GridCoordinate()
        {
            LoadCoordinate();
            InitializeMapMatrix();
        }



        void InitializeMapMatrix()
        {
            float determinant = CoordinateTransformedMatrix.m00 * CoordinateTransformedMatrix.m11 - CoordinateTransformedMatrix.m01 * CoordinateTransformedMatrix.m10;
            if (Mathf.Abs(determinant) < 1e-6)
            {
                // The matrix is not invertible.
                Debug.LogError("The transformation matrix is not invertible.");
                return;
            }
            float invDet = 1.0f / determinant;
            Matrix4x4 CoordinateTransformationInvert = Matrix4x4.identity;
            CoordinateTransformationInvert.m00 = CoordinateTransformedMatrix.m11 * invDet;
            CoordinateTransformationInvert.m01 = -CoordinateTransformedMatrix.m01 * invDet;
            CoordinateTransformationInvert.m10 = -CoordinateTransformedMatrix.m10 * invDet;
            CoordinateTransformationInvert.m11 = CoordinateTransformedMatrix.m00 * invDet;

            Debug.Log($"CoordinateTransformationInvert: {CoordinateTransformationInvert}");
            this.CoordinateTransformationInvert = CoordinateTransformationInvert;
        }

        void LoadCoordinate()
        {
            Vector2[] beforeTransformVertices = new Vector2[]
            {
                new Vector2(0.5f, 0.5f),
                new Vector2(-0.5f, 0.5f),
                new Vector2(-0.5f, -0.5f),
                new Vector2(0.5f, -0.5f)
            };
            Vector2[] afterTransformVertices = GetPlaceHolderGridVertices();
            Debug.Log($"afterTransformVertices: {afterTransformVertices[0]}, {afterTransformVertices[1]}, {afterTransformVertices[2]}, {afterTransformVertices[3]}");
            CoordinateTransformedMatrix = CalculateTransformationMatrix(beforeTransformVertices, afterTransformVertices);

            Debug.Log($"CoordinateTransformedMatrix: {CoordinateTransformedMatrix}");

            //test
            // Vector2 testV = new Vector2(0.5f, 0.5f);
            // Vector2 testV2 = CoordinateTransformedMatrix.MultiplyPoint3x4(testV);
            // Debug.Log($"testV2: {testV2}");
        }

        Vector2[] GetPlaceHolderGridVertices()
        {
            float gridAnchorPosMinX = 102;
            float gridAnchorPosMaxX = 922;
            float gridAnchorPosMinY = 1024 - 795;
            float gridAnchorPosMaxY = 1024 - 384;
            Vector2 up = new Vector2(512, gridAnchorPosMaxY);
            Vector2 left = new Vector2(gridAnchorPosMinX, (gridAnchorPosMaxY + gridAnchorPosMinY) / 2); 
            Vector2 down = new Vector2(512, gridAnchorPosMinY);
            Vector2 right = new Vector2(gridAnchorPosMaxX, (gridAnchorPosMaxY + gridAnchorPosMinY) / 2);
            Vector2 center = new Vector2(512, (gridAnchorPosMaxY + gridAnchorPosMinY) / 2);

            Vector2 tileLeft = left - center;
            Vector2 tileRight = right - center;
            Vector2 tileTop = up - center;
            Vector2 tileBottom = down - center;
            Vector2 tileCenter = center;
            float edgeLength = (tileTop - tileLeft).magnitude;
            //以中心点为原点，以边长为单位1，获取4个顶点的坐标
            Vector2[] vertices = new Vector2[4] {
                tileTop / edgeLength,
                tileLeft / edgeLength,
                tileBottom / edgeLength,
                tileRight / edgeLength
            };

            return vertices;
        }

        Matrix4x4 CalculateTransformationMatrix(Vector2[] squareVertices, Vector2[] diamondVertices)
        {
            Matrix4x4 transformationMatrix = new Matrix4x4();
            for (int i = 0; i < 4; i++)
            {
                // 计算系数矩阵的逆矩阵
                Matrix4x4 inverseMatrix = Matrix4x4.identity;
                inverseMatrix.m00 = squareVertices[i].x;
                inverseMatrix.m01 = squareVertices[i].y;
                inverseMatrix.m10 = squareVertices[(i + 1) % 4].x;
                inverseMatrix.m11 = squareVertices[(i + 1) % 4].y;
                inverseMatrix = inverseMatrix.inverse;

                // 计算变换矩阵的元素
                Vector2 transformedVertex = inverseMatrix.MultiplyPoint3x4(diamondVertices[i]);
                transformationMatrix.m00 = transformedVertex.x;
                transformationMatrix.m01 = transformedVertex.y;

                transformedVertex = inverseMatrix.MultiplyPoint3x4(diamondVertices[(i + 1) % 4]);
                transformationMatrix.m10 = transformedVertex.x;
                transformationMatrix.m11 = transformedVertex.y;
            }

            // 返回变换矩阵
            return transformationMatrix;
        }



        public Vector3 GetWorldPosition(int x, int y, float scale = 1.0f)
        {
            Vector3 gridPosition = new Vector3(x, y, 0);
            Vector3 centerGridPosition = gridPosition + new Vector3(scale / 2, scale / 2, 0);
            Vector3 centerWorldPosition = CoordinateTransformedMatrix.MultiplyPoint3x4(centerGridPosition);
            return centerWorldPosition;
        }

        public Vector2Int GetGridPosition(Vector3 worldPosition)
        {
            Vector3 gridPosition = CoordinateTransformationInvert.MultiplyPoint3x4(worldPosition);
            return new Vector2Int(Mathf.FloorToInt(gridPosition.x), Mathf.FloorToInt(gridPosition.y));
        }

    }