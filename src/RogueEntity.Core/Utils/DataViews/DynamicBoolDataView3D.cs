﻿using JetBrains.Annotations;
using MessagePack;
using RogueEntity.Api.Utils;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace RogueEntity.Core.Utils.DataViews
{
    [MessagePackObject]
    [DataContract]
    [SuppressMessage("ReSharper", "ConvertToAutoProperty")]
    public class DynamicBoolDataView3D : IDynamicDataView3D<bool>
    {
        public event EventHandler<DynamicDataView3DEventArgs<bool>> ViewCreated;
        public event EventHandler<DynamicDataView3DEventArgs<bool>> ViewExpired;

        [DataMember(Order = 4)]
        [Key(4)]
        readonly Dictionary<int, DynamicBoolDataView> index;

        [DataMember(Order = 0)]
        [Key(0)]
        readonly int tileSizeX;

        [DataMember(Order = 1)]
        [Key(1)]
        readonly int tileSizeY;

        [DataMember(Order = 2)]
        [Key(2)]
        readonly int offsetX;

        [DataMember(Order = 3)]
        [Key(3)]
        readonly int offsetY;

        public DynamicBoolDataView3D() : this(0, 0, 64, 64)
        { }

        public DynamicBoolDataView3D(DynamicDataViewConfiguration config) : this(config.OffsetX, config.OffsetY, config.TileSizeX, config.TileSizeY)
        { }

        public DynamicBoolDataView3D(int tileSizeX, int tileSizeY) : this(0, 0, tileSizeX, tileSizeY)
        { }

        public DynamicBoolDataView3D(int offsetX, int offsetY, int tileSizeX, int tileSizeY)
        {
            this.offsetX = offsetX;
            this.offsetY = offsetY;
            this.tileSizeX = tileSizeX;
            this.tileSizeY = tileSizeY;

            index = new Dictionary<int, DynamicBoolDataView>();
        }

        [SerializationConstructor]
        public DynamicBoolDataView3D(int tileSizeX, int tileSizeY, int offsetX, int offsetY, [NotNull] Dictionary<int, DynamicBoolDataView> index)
        {
            this.index = index ?? throw new ArgumentNullException(nameof(index));
            this.tileSizeX = tileSizeX;
            this.tileSizeY = tileSizeY;
            this.offsetX = offsetX;
            this.offsetY = offsetY;
        }

        [IgnoreMember]
        [IgnoreDataMember]
        public int TileSizeX => tileSizeX;

        [IgnoreMember]
        [IgnoreDataMember]
        public int TileSizeY => tileSizeY;

        [IgnoreMember]
        [IgnoreDataMember]
        public int OffsetX => offsetX;

        [IgnoreMember]
        [IgnoreDataMember]
        public int OffsetY => offsetY;

        public bool RemoveView(int z)
        {
            if (index.TryGetValue(z, out var rdata))
            {
                ViewExpired?.Invoke(this, new DynamicDataView3DEventArgs<bool>(z, rdata));
                index.Remove(z);
                return true;
            }

            return false;
        }

        public void RemoveAllViews()
        {
            var length = index.Count;
            var k = ArrayPool<int>.Shared.Rent(length);
            index.Keys.CopyTo(k, 0);
            for (var i = 0; i < length; i++)
            {
                RemoveView(k[i]);
            }

            ArrayPool<int>.Shared.Return(k);
        }

        public void Clear()
        {
            foreach (var v in index.Values)
            {
                v.Clear();
            }
        }

        public bool TryGetView(int z, out IReadOnlyDynamicDataView2D<bool> view)
        {
            if (TryGetWritableView(z, out var raw))
            {
                view = raw;
                return true;
            }

            view = default;
            return false;
        }

        public bool TryGetWritableView(int z, out IDynamicDataView2D<bool> data, DataViewCreateMode mode = DataViewCreateMode.Nothing)
        {
            if (index.TryGetValue(z, out var rdata))
            {
                data = rdata;
                return true;
            }

            if (mode == DataViewCreateMode.Nothing)
            {
                data = default;
                return false;
            }

            rdata = new DynamicBoolDataView(OffsetX, OffsetY, TileSizeX, TileSizeY);
            index[z] = rdata;
            ViewCreated?.Invoke(this, new DynamicDataView3DEventArgs<bool>(z, rdata));
            data = rdata;
            return true;
        }

        public BufferList<int> GetActiveLayers(BufferList<int> buffer = null)
        {
            buffer = BufferList.PrepareBuffer(buffer);

            foreach (var b in index.Keys)
            {
                buffer.Add(b);
            }

            return buffer;
        }
    }
}
