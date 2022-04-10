using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**************************************************
* Title: Procedural Cave Generation: Marching Squares algorithm source code
* Author: Lague, s
* Date: 2011
* Code version: 9.0
* Availability: https://github.com/SebLague/Procedural-Cave-Generation/blob/master/Episode %2009/MeshGeneration.cs
***************************************************/


public class MarchingSquares : MonoBehaviour
{
    public SquareGrid squareGrid;
    public MeshFilter walls;
    public MeshFilter cave;
    private List<Vector3> _vertices;
    private List<int> _triangles;

    private readonly Dictionary<int, List<Triangle>> _triangleDictionary = new Dictionary<int, List<Triangle>>();
    readonly List<List<int>> _outlines = new List<List<int>>();
    readonly HashSet<int> _checkedVertices = new HashSet<int>();

    private void ClearMeshData()
    {
        _triangleDictionary.Clear();
        _outlines.Clear();
        _checkedVertices.Clear();
    }

    private void CreateNewMeshData(int[,] map, float squareSize)
    {
        squareGrid = new SquareGrid(map, squareSize);
        _vertices = new List<Vector3>();
        _triangles = new List<int>();
    }

    public void MeshGen(int[,] map, float squareSize)
    {
        ClearMeshData();
        CreateNewMeshData(map, squareSize);


        for (var x = 0; x < squareGrid.Squares.GetLength(0); x++)
        {
            for (var y = 0; y < squareGrid.Squares.GetLength(1); y++)
            {
                TriangulateSquare(squareGrid.Squares[x, y]);
            }
        }

        cave.mesh = new Mesh {vertices = _vertices.ToArray(), triangles = _triangles.ToArray(),};

        cave.mesh.RecalculateNormals();

        const int tileAmount = 10;
        var texCoords = new Vector2[_vertices.Count];
        for (var i = 0; i < _vertices.Count; i++)
        {
            var percentX = Mathf.InverseLerp(-map.GetLength(0) / 2 * squareSize, map.GetLength(0) / 2 * squareSize,
                _vertices[i].x) * tileAmount;
            var percentY = Mathf.InverseLerp(-map.GetLength(0) / 2 * squareSize, map.GetLength(0) / 2 * squareSize,
                _vertices[i].z) * tileAmount;
            texCoords[i] = new Vector2(percentX, percentY);
        }

        cave.mesh.uv = texCoords;
        CreateWallMesh();
    }

    private void CreateWallMesh()
    {
        if (GetComponent<MeshCollider>()) Destroy(GetComponent<MeshCollider>());
        CalculateMeshOutlines();

        var wallVerts = new List<Vector3>();
        var wallTriangles = new List<int>();
        var wallMesh = new Mesh();
        const float wallHeight = 5;

        foreach (var outline in _outlines)
        {
            for (var i = 0; i < outline.Count - 1; i++)
            {
                int startIndex = wallVerts.Count;
                wallVerts.Add(_vertices[outline[i]]); // left
                wallVerts.Add(_vertices[outline[i + 1]]); // right
                wallVerts.Add(_vertices[outline[i]] - Vector3.up * wallHeight); // bottom left
                wallVerts.Add(_vertices[outline[i + 1]] - Vector3.up * wallHeight); // bottom right

                wallTriangles.Add(startIndex + 0);
                wallTriangles.Add(startIndex + 2);
                wallTriangles.Add(startIndex + 3);

                wallTriangles.Add(startIndex + 3);
                wallTriangles.Add(startIndex + 1);
                wallTriangles.Add(startIndex + 0);
            }
        }

        wallMesh.vertices = wallVerts.ToArray();
        wallMesh.triangles = wallTriangles.ToArray();
        walls.mesh = wallMesh;

        var wallCollider = gameObject.AddComponent<MeshCollider>();
        wallCollider.sharedMesh = wallMesh;
    }

    //Different triangle configurations
    private void TriangulateSquare(Square square)
    {
        switch (square.SquareConfiguration)
        {
            case 0:
                break;

            // 1 points:
            case 1:
                MeshFromPoints(square.CentreLeft, square.CentreBottom, square.BottomLeft);
                break;
            case 2:
                MeshFromPoints(square.BottomRight, square.CentreBottom, square.CentreRight);
                break;
            case 4:
                MeshFromPoints(square.TopRight, square.CentreRight, square.CentreTop);
                break;
            case 8:
                MeshFromPoints(square.TopLeft, square.CentreTop, square.CentreLeft);
                break;

            // 2 points:
            case 3:
                MeshFromPoints(square.CentreRight, square.BottomRight, square.BottomLeft, square.CentreLeft);
                break;
            case 6:
                MeshFromPoints(square.CentreTop, square.TopRight, square.BottomRight, square.CentreBottom);
                break;
            case 9:
                MeshFromPoints(square.TopLeft, square.CentreTop, square.CentreBottom, square.BottomLeft);
                break;
            case 12:
                MeshFromPoints(square.TopLeft, square.TopRight, square.CentreRight, square.CentreLeft);
                break;
            case 5:
                MeshFromPoints(square.CentreTop, square.TopRight, square.CentreRight, square.CentreBottom,
                    square.BottomLeft, square.CentreLeft);
                break;
            case 10:
                MeshFromPoints(square.TopLeft, square.CentreTop, square.CentreRight, square.BottomRight,
                    square.CentreBottom, square.CentreLeft);
                break;

            // 3 point:
            case 7:
                MeshFromPoints(square.CentreTop, square.TopRight, square.BottomRight, square.BottomLeft,
                    square.CentreLeft);
                break;
            case 11:
                MeshFromPoints(square.TopLeft, square.CentreTop, square.CentreRight, square.BottomRight,
                    square.BottomLeft);
                break;
            case 13:
                MeshFromPoints(square.TopLeft, square.TopRight, square.CentreRight, square.CentreBottom,
                    square.BottomLeft);
                break;
            case 14:
                MeshFromPoints(square.TopLeft, square.TopRight, square.BottomRight, square.CentreBottom,
                    square.CentreLeft);
                break;

            // 4 point:
            case 15:
                MeshFromPoints(square.TopLeft, square.TopRight, square.BottomRight, square.BottomLeft);
                _checkedVertices.Add(square.TopLeft.m_VertIdx);
                _checkedVertices.Add(square.TopRight.m_VertIdx);
                _checkedVertices.Add(square.BottomRight.m_VertIdx);
                _checkedVertices.Add(square.BottomLeft.m_VertIdx);
                break;
        }
    }
//Triangle Vertex Generation
    private void MeshFromPoints(params Node[] points)
    {
        AssignVertices(points);
        if (points.Length >= 3)
            CreateTriangle(points[0], points[1], points[2]);
        if (points.Length >= 4)
            CreateTriangle(points[0], points[2], points[3]);
        if (points.Length >= 5)
            CreateTriangle(points[0], points[3], points[4]);
        if (points.Length >= 6)
            CreateTriangle(points[0], points[4], points[5]);
    }

    private void AssignVertices(IEnumerable<Node> points)
    {
        foreach (var t in points)
        {
            if (t.m_VertIdx != -1) continue;
            t.m_VertIdx = _vertices.Count;
            _vertices.Add(t.Position);
        }
    }

    private void CreateTriangle(Node a, Node b, Node c)
    {
        _triangles.Add(a.m_VertIdx);
        _triangles.Add(b.m_VertIdx);
        _triangles.Add(c.m_VertIdx);

        var triangle = new Triangle(new Vector3Int(a.m_VertIdx, b.m_VertIdx, c.m_VertIdx));
        AddTriangleToDictionary(triangle.VertexIndexA, triangle);
        AddTriangleToDictionary(triangle.VertexIndexB, triangle);
        AddTriangleToDictionary(triangle.VertexIndexC, triangle);
    }

    private void AddTriangleToDictionary(int vertexIndexKey, Triangle triangle)
    {
        if (_triangleDictionary.ContainsKey(vertexIndexKey))
            _triangleDictionary[vertexIndexKey].Add(triangle);
        else
        {
            var triangleList = new List<Triangle> {triangle};
            _triangleDictionary.Add(vertexIndexKey, triangleList);
        }
    }

    private void CalculateMeshOutlines()
    {
        for (var vertexIndex = 0; vertexIndex < _vertices.Count; vertexIndex++)
        {
            if (_checkedVertices.Contains(vertexIndex)) continue;
            var newOutlineVertex = GetConnectedOutlineVertex(vertexIndex);
            if (newOutlineVertex == -1) continue;
            _checkedVertices.Add(vertexIndex);

            var newOutline = new List<int> {vertexIndex};
            _outlines.Add(newOutline);
            FollowOutline(newOutlineVertex, _outlines.Count - 1);
            _outlines[_outlines.Count - 1].Add(vertexIndex);
        }

        SimplifyMeshOutlines();
    }

    private void SimplifyMeshOutlines()
    {
        for (var outlineIndex = 0; outlineIndex < _outlines.Count; outlineIndex++)
        {
            var simplifiedOutline = new List<int>();
            var dirOld = Vector3.zero;
            for (var i = 0; i < _outlines[outlineIndex].Count; i++)
            {
                var p1 = _vertices[_outlines[outlineIndex][i]];
                var p2 = _vertices[_outlines[outlineIndex][(i + 1) % _outlines[outlineIndex].Count]];
                var dir = p1 - p2;
                if (dir == dirOld) continue;
                dirOld = dir;
                simplifiedOutline.Add(_outlines[outlineIndex][i]);
            }

            _outlines[outlineIndex] = simplifiedOutline;
        }
    }

    private void FollowOutline(int vertexIndex, int outlineIndex)
    {
        while (true)
        {
            _outlines[outlineIndex].Add(vertexIndex);
            _checkedVertices.Add(vertexIndex);
            var nextVertexIndex = GetConnectedOutlineVertex(vertexIndex);

            if (nextVertexIndex != -1)
            {
                vertexIndex = nextVertexIndex;
                continue;
            }

            break;
        }
    }

    private int GetConnectedOutlineVertex(int vertexIndex)
    {
        var trianglesContainingVertex = _triangleDictionary[vertexIndex];

        foreach (var triangle in trianglesContainingVertex)
        {
            for (var j = 0; j < 3; j++)
            {
                var vertexB = triangle[j];
                if (vertexB == vertexIndex || _checkedVertices.Contains(vertexB)) continue;
                if (IsOutlineEdge(vertexIndex, vertexB))
                    return vertexB;
            }
        }

        return -1;
    }

    private bool IsOutlineEdge(int vertexA, int vertexB)
    {
        var trianglesContainingVertexA = _triangleDictionary[vertexA];
        var sharedTriangleCount = 0;
        for (var i = 0; i < trianglesContainingVertexA.Count; i++)
        {
            if (!trianglesContainingVertexA[i].Contains(vertexB)) continue;
            sharedTriangleCount++;
            if (sharedTriangleCount > 1)
            {
                break;
            }
        }

        return sharedTriangleCount == 1;
    }

    private struct Triangle
    {
        public readonly int VertexIndexA;
        public readonly int VertexIndexB;
        public readonly int VertexIndexC;
        private readonly int[] _vertices;

        public Triangle(Vector3Int idx)
        {
            VertexIndexA = idx.x;
            VertexIndexB = idx.y;
            VertexIndexC = idx.z;

            _vertices = new int[3];
            _vertices[0] = idx.x;
            _vertices[1] = idx.y;
            _vertices[2] = idx.z;
        }

        public int this[int i] => _vertices[i];

        public bool Contains(int vertexIndex)
        {
            return vertexIndex == VertexIndexA || vertexIndex == VertexIndexB || vertexIndex == VertexIndexC;
        }
    }

    public class SquareGrid
    {
        public readonly Square[,] Squares;

        public SquareGrid(int[,] map, float squareSize)
        {
            var nodeCountX = map.GetLength(0);
            var nodeCountY = map.GetLength(1);
            var mapWidth = nodeCountX * squareSize;
            var mapHeight = nodeCountY * squareSize;

            var controlNodes = new ControlNode[nodeCountX, nodeCountY];

            for (var x = 0; x < nodeCountX; x++)
            {
                for (var y = 0; y < nodeCountY; y++)
                {
                    var pos = new Vector3(-mapWidth / 2 + x * squareSize + squareSize / 2, 0,
                        -mapHeight / 2 + y * squareSize + squareSize / 2);
                    controlNodes[x, y] = new ControlNode(pos, map[x, y] == 1, squareSize);
                }
            }

            Squares = new Square[nodeCountX - 1, nodeCountY - 1];
            for (var x = 0; x < nodeCountX - 1; x++)
            {
                for (var y = 0; y < nodeCountY - 1; y++)
                {
                    Squares[x, y] = new Square(controlNodes[x, y + 1], controlNodes[x + 1, y + 1],
                        controlNodes[x + 1, y], controlNodes[x, y]);
                }
            }
        }
    }

    public class Square
    {
        public readonly ControlNode TopLeft, TopRight, BottomRight, BottomLeft;
        public readonly Node CentreTop, CentreRight, CentreBottom, CentreLeft;
        public readonly int SquareConfiguration;

        public Square(ControlNode topLeft, ControlNode topRight, ControlNode bottomRight,
            ControlNode bottomLeft)
        {
            TopLeft = topLeft;
            TopRight = topRight;
            BottomRight = bottomRight;
            BottomLeft = bottomLeft;

            CentreTop = TopLeft.Right;
            CentreRight = BottomRight.Above;
            CentreBottom = BottomLeft.Right;
            CentreLeft = BottomLeft.Above;

            if (TopLeft.Active)
                SquareConfiguration += 8;
            if (TopRight.Active)
                SquareConfiguration += 4;
            if (BottomRight.Active)
                SquareConfiguration += 2;
            if (BottomLeft.Active)
                SquareConfiguration += 1;
        }
    }

    public class Node
    {
        public Vector3 Position;
        public int m_VertIdx = -1;

        protected internal Node(Vector3 pos)
        {
            Position = pos;
        }
    }

    public class ControlNode : Node
    {
        public readonly bool Active;
        public readonly Node Above, Right;

        public ControlNode(Vector3 pos, bool active, float squareSize) : base(pos)
        {
            Active = active;
            Above = new Node(Position + Vector3.forward * squareSize / 2f);
            Right = new Node(Position + Vector3.right * squareSize / 2f);
        }
    }
}