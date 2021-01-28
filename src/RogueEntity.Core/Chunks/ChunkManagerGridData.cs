using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Utils.DataViews;
using System;

namespace RogueEntity.Core.Chunks
{
    public class ChunkManagerGridData<T> : IChunkManagerData
    {
        readonly IGridMapDataContext<T> dataView;
        public event EventHandler<int> ViewCreated;
        public event EventHandler<int> ViewExpired;
        public event EventHandler<int> ViewMarkedDirty;

        public ChunkManagerGridData(IGridMapDataContext<T> dataView)
        {
            this.dataView = dataView;
            this.dataView.ViewCreated += OnViewCreated;
            this.dataView.ViewExpired += OnViewExpired;
            this.dataView.PositionDirty += OnPositionDirty;
        }

        void OnPositionDirty(object sender, PositionDirtyEventArgs e)
        {
            if (e.Position.IsInvalid) return;
            
            ViewMarkedDirty?.Invoke(this, e.Position.GridZ);
        }

        public void Dispose()
        {
            this.dataView.ViewCreated -= OnViewCreated;
            this.dataView.ViewExpired -= OnViewExpired;
            this.dataView.PositionDirty -= OnPositionDirty;
        }

        void OnViewCreated(object sender, DynamicDataView3DEventArgs<T> e)
        {
            ViewCreated?.Invoke(this, e.ZLevel);
            e.Data.ViewCreated += OnBoundedViewCreated;
            e.Data.ViewExpired += OnBoundedViewExpired;
        }

        void OnViewExpired(object sender, DynamicDataView3DEventArgs<T> e)
        {
            e.Data.ViewCreated -= OnBoundedViewCreated;
            e.Data.ViewExpired -= OnBoundedViewExpired;
            ViewExpired?.Invoke(this, e.ZLevel);
        }

        void OnBoundedViewExpired(object sender, DynamicDataView2DEventArgs<T> e)
        {
            // a tile has been removed.
        }

        void OnBoundedViewCreated(object sender, DynamicDataView2DEventArgs<T> e)
        {
            // a tile has been added.
        }

        public void RemoveView(int z)
        {
            dataView.RemoveView(z);
        }
    }
}
