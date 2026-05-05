using System.Collections.Generic;

namespace TnieYuPackage.Utils
{
    public interface IGridMapSystem<TPos, TData>
    {
        Dictionary<TPos, TData> GridMap { get; }
        
        public void WriteTile(TPos pos, TData data);
        public TData ReadTile(TPos pos);
        public bool DeleteTile(TPos pos);
    }
}