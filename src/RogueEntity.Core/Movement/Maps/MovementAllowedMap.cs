using System;
using RogueEntity.Core.Movement.ItemCosts;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.MapChunks;
using RogueEntity.Core.Utils.Maps;
using Serilog;

namespace RogueEntity.Core.Movement.Maps
{
    public class MovementAllowedMap : IReadOnlyMapData<MovementAllowedProperties>
    {
        static readonly ILogger Logger = SLog.ForContext<MovementAllowedMap>();

        class DataProcessor : ChunkProcessor<IReadOnlyMapData<MovementCostProperties>>
        {
            public readonly bool[,] DirtyFlags;
            public readonly MovementAllowedProperties[] Data;

            public DataProcessor(int width, int height, int spanSizeX, int spanSizeY) : base(width, height, spanSizeX, spanSizeY)
            {
                Data = new MovementAllowedProperties[width * height];
                var flagsWidth = (int)Math.Ceiling(width / (float)SpanSizeX);
                var flagsHeight = (int)Math.Ceiling(height / (float)SpanSizeY);
                DirtyFlags = new bool[flagsWidth, flagsHeight];
                
                for (int y = 0; y < flagsHeight; y += 1)
                {
                    for (int x = 0; x < flagsWidth; x += 1)
                    {
                        DirtyFlags[x, y] = true;
                    }
                }
            }

            protected override void Process(IReadOnlyMapData<MovementCostProperties> groundData, int yStart, int yEnd, int xStart, int xEnd)
            {
                for (var y = yStart; y < yEnd; y += 1)
                {
                    for (var x = xStart; x < xEnd; x += 1)
                    {
                        var offset = (x + y * Width);
                        FetchNeighbors(groundData, x, y,
                                       out var north,
                                       out var northEast,
                                       out var east,
                                       out var southEast,
                                       out var south,
                                       out var southWest,
                                       out var west,
                                       out var northWest);

                        var walking = ComputeCardinals(north.Walking, east.Walking, south.Walking, west.Walking);
                        var flying = ComputeCardinals(north.Flying, east.Flying, south.Flying, west.Flying);
                        var ether = ComputeCardinals(north.Ethereal, east.Ethereal, south.Ethereal, west.Ethereal);
                        var swim = ComputeCardinals(north.Swimming, east.Swimming, south.Swimming, west.Swimming);

                        walking = ComputeDiagonals(walking, northEast.Walking, southEast.Walking, southWest.Walking, northWest.Walking);
                        flying = ComputeDiagonals(flying, northEast.Flying, southEast.Flying, southWest.Flying, northWest.Flying);
                        ether = ComputeDiagonals(ether, northEast.Ethereal, southEast.Ethereal, southWest.Ethereal, northWest.Ethereal);
                        swim = ComputeDiagonals(swim, northEast.Swimming, southEast.Swimming, southWest.Swimming, northWest.Swimming);
                        
                        Data[offset] = new MovementAllowedProperties(walking, flying, ether, swim);
                    }
                }
            }

            public override bool CanProcess(int x, int y)
            {
                return DirtyFlags[x / SpanSizeX, y / SpanSizeY];
            }

            public void MarkDirty(int x, int y, int radius)
            {
                var minX = Math.Max(0, (x - radius) / SpanSizeX);
                var minY = Math.Max(0, (y - radius) / SpanSizeY);
                var maxX = Math.Min(Width, x + radius) / SpanSizeX;
                var maxY = Math.Min(Width, y + radius) / SpanSizeY;

                for (var sy = minY; sy <= maxY; sy += 1)
                {
                    for (var sx = minX; sx <= maxX; sx += 1)
                    {
                        DirtyFlags[sx, sy] = true;
                    }
                }
            }

            public void ResetDirtyFlags()
            {
                Array.Clear(DirtyFlags, 0, DirtyFlags.Length);
            }

        }

        static MovementAllowedData ComputeCardinals(MovementCost north, 
                                                    MovementCost east,
                                                    MovementCost south,
                                                    MovementCost west)
        {
            var d = MovementAllowedData.Blocked;
            d = north.CanMove(out _) ? d.With(MovementDirection.North) : d;
            d = east.CanMove(out _) ? d.With(MovementDirection.East) : d;
            d = south.CanMove(out _) ? d.With(MovementDirection.South) : d;
            d = west.CanMove(out _) ? d.With(MovementDirection.West) : d;
            return d;
        }

        static MovementAllowedData ComputeDiagonals(MovementAllowedData d, 
                                                    MovementCost northEast, 
                                                    MovementCost southEast,
                                                    MovementCost southWest,
                                                    MovementCost northWest)
        {
            d = northEast.CanMove(out _) && d[MovementDirection.North | MovementDirection.East] ? d.With(MovementDirection.NorthEast) : d;
            d = southEast.CanMove(out _) && d[MovementDirection.South | MovementDirection.East] ? d.With(MovementDirection.SouthEast) : d;
            d = southWest.CanMove(out _) && d[MovementDirection.South | MovementDirection.West] ? d.With(MovementDirection.SouthWest) : d;
            d = northWest.CanMove(out _) && d[MovementDirection.North | MovementDirection.West] ? d.With(MovementDirection.NorthWest) : d;
            return d;
        }

        static void FetchNeighbors(IReadOnlyMapData<MovementCostProperties> data,
                                            int x, int y,
                                            out MovementCostProperties north,
                                            out MovementCostProperties northEast,
                                            out MovementCostProperties east,
                                            out MovementCostProperties southEast,
                                            out MovementCostProperties south,
                                            out MovementCostProperties southWest,
                                            out MovementCostProperties west,
                                            out MovementCostProperties northWest)
        {
            north = Fetch(data, x, y - 1);
            south = Fetch(data, x, y + 1);
            east = Fetch(data, x + 1, y);
            west = Fetch(data, x - 1, y);

            northEast = Fetch(data, x + 1, y - 1);
            southEast = Fetch(data, x + 1, y + 1);
            southWest = Fetch(data, x - 1, y + 1);
            northWest = Fetch(data, x - 1, y - 1);
        }

        static MovementCostProperties Fetch(IReadOnlyMapData<MovementCostProperties> data, int x, int y)
        {
            if (x < 0 || x >= data.Width)
            {
                return MovementCostProperties.Blocked;
            }

            if (y < 0 || y >= data.Height)
            {
                return MovementCostProperties.Blocked;
            }

            return data[x, y];
        }

        readonly DataProcessor dataProcessor;

        public int Height { get; }
        public int Width { get; }

        public MovementAllowedMap(IAddByteBlitter blitter, int width, int height)
        {
            var (spanSizeX, spanSizeY) = ChunkProcessor.ComputeSpanSize(blitter, width, height);

            dataProcessor = new DataProcessor(width, height, spanSizeX, spanSizeY);

            Height = height;
            Width = width;
        }

        public MovementAllowedProperties this[int x, int y]
        {
            get
            {
                var offset = (x + y * Width);
                return dataProcessor.Data[offset];
            }
        }

        public void MarkDirty(int x, int y)
        {
            dataProcessor.MarkDirty(x, y, 1);
        }

        public void ResetDirtyFlags()
        {
            dataProcessor.ResetDirtyFlags();
        }

        public void Process(IReadOnlyMapData<MovementCostProperties> c)
        {
            var chunksProcessed = dataProcessor.Process(c);
            Logger.Verbose("Movement allowed data update: {ChunksProcessed}", chunksProcessed);
        }
    }
}