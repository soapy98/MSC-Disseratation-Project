using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using Random = System.Random;
//    https://github.com/luisparravicini/abrigo-pinos
//  https://github.com/celusniak/cavegenerator/blob/master/CellularAutomaton.java
public class CellularAutomataWithFloodFill : MonoBehaviour
{     
    public int width;
    public int height;

    private const int Wall = 1, Empty = 0;
    public string seed;//Defines how much should stay filled
    public bool NewMap;
    const int borderSize = 1;
    [Range(0, 100)] public int randomFillPercent; // Guideline of how much should be filled
    private int[,] _map; // Defines a grid of integers

    private void Start()
    {
        FillMap();
    }

    private void Update()
    {
        if (NewMap)
        {
            NewMap = false;
            FillMap();
        }
    }

    private bool BoundsCheck(int x, int y, int borderSize)
    {
        return x >= borderSize && x < width + borderSize && y >= borderSize && y < height + borderSize;
    }

    private int[,] BorederedMapcreation(int x, int y, int borderSize)
    {
        return new int[x + borderSize * 2, y + borderSize * 2];
    }
    
    private void FillMap()
    {
        _map = new int[width, height];
        RandomFillMap();
        for (var i = 0; i < 5; i++)
        {
            SmoothMap();
        }
    
        var mapWithBorder = BorederedMapcreation(width,height,borderSize);
        FloodFill();

        for (var x = 0; x < mapWithBorder.GetLength(0); x++)
        {
            for (var y = 0; y < mapWithBorder.GetLength(1); y++)
            {
                //Bound check to determine if the cell on x,y axis is within the map border
                mapWithBorder[x, y] = BoundsCheck(x, y, borderSize) ? _map[x - borderSize, y - borderSize] : 1;
            }
        }
        var meshGen = GetComponent<MarchingSquares>();
        meshGen.MeshGen(mapWithBorder, 1);
    }

    private void RandomFillMap()
    {
        var rand = new Random(seed.GetHashCode());
        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < height; y++)
            {
                _map[x, y] = RandomBoundCheck(x, y) ? Wall : rand.Next(0, 100) < randomFillPercent ? Wall : Empty;
            }
        }
    }

    private void SmoothMap()
    {
        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < height; y++)
            {
                var neighbourWallCount = NeighbourWallCount(x, y, new Vector2Int(1, 1));
                var current = _map[x, y];
                _map[x, y] = neighbourWallCount > 4 ? Wall : neighbourWallCount < 4 ? Empty : current;
            }
        }
    }

    private int NeighbourWallCount(int gridX, int gridY, Vector2Int mapScope)
    {
        var wallCount = 0;
        int xNeighStart = gridX - mapScope.x, neighYStart = gridY - mapScope.y;
        int xNeighEnd = gridX + mapScope.x, yNeighEnd = gridY + mapScope.y;
        for (var x = xNeighStart; x <= xNeighEnd; x++)
        {
            for (var y = neighYStart; y <= yNeighEnd; y++)
            {
                if (InMapBounds(x, y))
                {
                    if (x != gridX || y != gridY)
                        wallCount += _map[x, y];
                }
                else
                    wallCount++;
            }
        }
        return wallCount;
    }

    bool InMapBoundsAndNotNeighbour(int x, int y, int gridX, int gridY)
    {
        return InMapBounds(x, y) && (x != gridX || y != gridY);
    }

/**************************************************
* Title: Procedural Cave Generation: Flood fill algorithm source code
* Author: Lague, s
* Date: 2011
* Code version: 9.0
* Availability: https://github.com/SebLague/Procedural-Cave-Generation/blob/master/Episode %2009/MapGenerator.cs
***************************************************/
    private void FloodFill()
    {
        var wallRegions = FindCavernOfType(Wall);
        const int wallThreshold = 50;

        foreach (var tile in from wallRegion in wallRegions
            where wallRegion.Count < wallThreshold
            from tile in wallRegion
            select tile)
        {
            _map[tile.TileX, tile.TileY] = Empty;
        }

        var roomRegions = FindCavernOfType(Empty);
        const int roomThreshold = 50;
        var survivingRooms = new List<Cavern>();

        foreach (var roomRegion in roomRegions)
        {
            if (roomRegion.Count < roomThreshold)
            {
                foreach (var tile in roomRegion) 
                    _map[tile.TileX, tile.TileY] = Wall;
            }
            else survivingRooms.Add(new Cavern(roomRegion, _map));
        }

        SetMainRoom(survivingRooms);
    }

    private void SetMainRoom(List<Cavern> roomsLeft)
    {
        roomsLeft.Sort();
        roomsLeft[0].IsMainCavern = true;
        roomsLeft[0].IsAccessibleFromMainCavern = true;
        ConnectClosestCavern(roomsLeft);
    }
/**************************************************
* Title: Procedural Cave Generation: Passageway connection source code
* Author: Lague, s
* Date: 2011
* Code version: 9.0
* Availability: https://github.com/SebLague/Procedural-Cave-Generation/blob/master/Episode %2009/MapGenerator.cs
***************************************************/
    private void ConnectClosestCavern(List<Cavern> allRooms, bool forceAccessibilityFromMainRoom = false)
    {
        while (true)
        {
            var roomListA = new List<Cavern>();
            var roomListB = new List<Cavern>();

            if (forceAccessibilityFromMainRoom)
            {
                foreach (var room in allRooms)
                {
                    if (room.IsAccessibleFromMainCavern) 
                        roomListB.Add(room);
                    else 
                        roomListA.Add(room);
                }
            }
            else
            {
                roomListA = allRooms;
                roomListB = allRooms;
            }

            var bestDistance = 0;
            var bestTileA = new Tile();
            var bestTileB = new Tile();
            var bestRoomA = new Cavern();
            var bestRoomB = new Cavern();
            var possibleConnectionFound = false;

            foreach (var roomA in roomListA)
            {
                if (!forceAccessibilityFromMainRoom)
                {
                    possibleConnectionFound = false;
                    if (roomA.ConnectedCaverns.Count > 0) continue;
                }

                foreach (var roomB in roomListB)
                {
                    if (roomA == roomB || roomA.ConnectionCheck(roomB)) continue;
                    foreach (var t1 in roomA.EdgeTiles)
                    {
                        foreach (var t in roomB.EdgeTiles)
                        {
                            var tileA = t1;
                            var tileB = t;
                            var distanceBetweenRooms =
                                (int) (Mathf.Pow(tileA.TileX - tileB.TileX, 2) +
                                       Mathf.Pow(tileA.TileY - tileB.TileY, 2));
                            if (distanceBetweenRooms >= bestDistance && possibleConnectionFound) continue;
                            bestDistance = distanceBetweenRooms;
                            possibleConnectionFound = true;
                            bestTileA = tileA;
                            bestTileB = tileB;
                            bestRoomA = roomA;
                            bestRoomB = roomB;
                        }
                    }
                }

                if (possibleConnectionFound && !forceAccessibilityFromMainRoom)
                    CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            }

            if (possibleConnectionFound && forceAccessibilityFromMainRoom)
            {
                CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
                ConnectClosestCavern(allRooms, true);
            }

            if (!forceAccessibilityFromMainRoom)
            {
                forceAccessibilityFromMainRoom = true;
                continue;
            }

            break;
        }
    }

    private void CreatePassage(Cavern cavernA, Cavern cavernB, Tile tileA, Tile tileB)
    {
        Cavern.ConnectCaverns(cavernA, cavernB);
        var line = GetLine(tileA, tileB);
        foreach (var c in line) 
            SetPassageCircleSize(c, 10);
    }

    private void SetPassageCircleSize(Tile c, int r)
    {
        for (var x = -r; x <= r; x++)
        {
            for (var y = -r; y <= r; y++)
            {
                if (x * x + y * y > r * r) continue;
                var realX = c.TileX + x;
                var realY = c.TileY + y;
                if (InMapBounds(realX, realY)) 
                    _map[realX, realY] = Empty;
            }
        }
    }

    private static IEnumerable<Tile> GetLine(Tile from, Tile to)
    {
        var line = new List<Tile>();

        var x = from.TileX;
        var y = from.TileY;

        var dx = to.TileX - from.TileX;
        var dy = to.TileY - from.TileY;

        var inverted = false;
        var step = Math.Sign(dx);
        var gradientStep = Math.Sign(dy);

        var longest = Mathf.Abs(dx);
        var shortest = Mathf.Abs(dy);

        if (longest < shortest)
        {
            inverted = true;
            longest = Mathf.Abs(dy);
            shortest = Mathf.Abs(dx);

            step = Math.Sign(dy);
            gradientStep = Math.Sign(dx);
        }

        var gradientAccumulation = longest / 2;
        for (var i = 0; i < longest; i++)
        {
            line.Add(new Tile(x, y));
            if (inverted) 
                y += step;
            else
                x += step;
            gradientAccumulation += shortest;
            if (gradientAccumulation < longest) continue;
            if (inverted) 
                x += gradientStep;
            else 
                y += gradientStep;
            gradientAccumulation -= longest;
        }
        return line;
    }

  

    private IEnumerable<List<Tile>> FindCavernOfType(int tileType)
    {
        var regions = new List<List<Tile>>();
        var mapFlags = new int[width, height];

        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < height; y++)
            {
                if (mapFlags[x, y] != 0 || _map[x, y] != tileType) continue;
                var newCavern = FindCavernSize(x, y);
                regions.Add(newCavern);
                foreach (var tile in newCavern) mapFlags[tile.TileX, tile.TileY] = Wall;
            }
        }

        return regions;
    }

    // Flood-Fill Method
    private List<Tile> FindCavernSize(int startX, int startY)
    {
        var tiles = new List<Tile>();
        var mapFlags = new int[width, height];
        var tileType = _map[startX, startY];

        var queue = new Queue<Tile>();
        queue.Enqueue(new Tile(startX, startY)); // Add each tile into Queue
        mapFlags[startX, startY] = 1; // Set flags for tiles that has been looked at

        while (queue.Count > 0)
        {
            var tile = queue.Dequeue(); // Returns and removes the first item in the Queue
            tiles.Add(tile);
            for (var x = tile.TileX - 1; x <= tile.TileX + 1; x++)
            {
                for (var y = tile.TileY - 1; y <= tile.TileY + 1; y++)
                {
                    if (!InMapBounds(x, y) || (y != tile.TileY && x != tile.TileX)) continue;
                    if (mapFlags[x, y] != 0 || _map[x, y] != tileType)
                        continue; //Check tile hasn't changed or is the same tile type
                    mapFlags[x, y] = 1;
                    //Add to queue to carry on flood fill
                    queue.Enqueue(new Tile(x, y));
                }
            }
        }

        return tiles;
    }

    private bool InMapBounds(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    private bool RandomBoundCheck(int x, int y)
    {
        return x == 0 || x == width - 1 || y == 0 || y == height - 1;
    }


    private struct Tile
    {
        public readonly int TileX;
        public readonly int TileY;
        public readonly Vector2Int TileXY;

        public Tile(int x, int y)
        {
            TileX = x;
            TileY = y;
            TileXY = new Vector2Int(x, y);
        }

        public Tile(Vector2Int tile)
        {
            TileX = tile.x;
            TileY = tile.y;
            TileXY = tile;
        }
    }

    private class Cavern : IComparable<Cavern>
    {
        public List<Tile> EdgeTiles { get; }
        public List<Cavern> ConnectedCaverns { get; }
        private readonly int _roomSize;
        public bool IsAccessibleFromMainCavern { get; set; }
        public bool IsMainCavern { [UsedImplicitly] internal get; set; }

        public Cavern()
        {
        }

        public Cavern(List<Tile> roomTiles, int[,] map)
        {
            var tiles = roomTiles;
            _roomSize = tiles.Count;
            ConnectedCaverns = new List<Cavern>();
            EdgeTiles = new List<Tile>();
            foreach (var tile in tiles)
            {
                for (var x = tile.TileX - 1; x <= tile.TileX + 1; x++)
                {
                    for (var y = tile.TileY - 1; y <= tile.TileY + 1; y++)
                    {
                        if (x != tile.TileX && y != tile.TileY) continue;
                        if (map[x, y] == 1) 
                            EdgeTiles.Add(tile);
                    }
                }
            }
        }

        private void AccessibleFromMainCavern()
        {
            if (IsAccessibleFromMainCavern) return;
            IsAccessibleFromMainCavern = true;
            foreach (var connectedRoom in ConnectedCaverns) connectedRoom.AccessibleFromMainCavern();
        }

        public static void ConnectCaverns(Cavern cavernA, Cavern cavernB)
        {
            if (cavernA.IsAccessibleFromMainCavern) cavernB.AccessibleFromMainCavern();
            else if (cavernB.IsAccessibleFromMainCavern) cavernA.AccessibleFromMainCavern();
            cavernA.ConnectedCaverns.Add(cavernB);
            cavernB.ConnectedCaverns.Add(cavernA);
        }

        public bool ConnectionCheck(Cavern otherCavern)
        {
            return ConnectedCaverns.Contains(otherCavern);
        }

        public int CompareTo(Cavern otherCavern)
        {
            return otherCavern._roomSize.CompareTo(_roomSize);
        }
    }
}