using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ImagePalette
{
    public sealed class OctreeQuantizer
    {
        private readonly int _maxDepth;
        private readonly int _maxColors;
        private readonly OctreeNode _root;
        private readonly List<OctreeNode>[] _levels;
        private int _leafCount;

        public OctreeQuantizer (int maxDepth, int maxColors)
        {
            this._maxDepth = maxDepth;
            this._maxColors = maxColors;
            this._levels = new List<OctreeNode>[maxDepth + 1];

            for (int i = 0; i < _levels.Length; i++)
            {
                this._levels[i] = new List<OctreeNode>();
            }

            this._root = new OctreeNode(level: 0, parentQuantizer: this);
        }

        public void AddColor (Rgba32 color)
        {
            _root.AddColor(color, 0, this);
            if (_leafCount > _maxColors)
            {
                ReduceTree();
            }
        }
        internal void IncrementLeafCount () => _leafCount++;

        internal void DecrementLeafCount () => _leafCount--;
        internal int MaxDepth => _maxDepth;

        internal void RegisterReducableNode (int level, OctreeNode node)
        {
            if (level >= 0 && level < _levels.Length)
            {
                if (!_levels[level].Contains(node))
                {
                    _levels[level].Add(node);
                }
            }
        }

        private void ReduceTree ()
        {
            for (int level = _maxDepth - 1; level > 0 && _leafCount > _maxColors; level--)
            {
                List<OctreeNode> nodes = _levels[level];
                if (nodes.Count == 0)
                {
                    continue;
                }

                for (int i = nodes.Count - 1; i >= 0 && _leafCount > _maxColors; i--)
                {
                    OctreeNode node = nodes[i];
                    node.MergeChildren();
                    nodes.RemoveAt(i);
                }
            }
        }
        public List<Rgba32> GetPalette (List<int>? pixelCounts = null)
        {
            var result = new List<Rgba32>(_maxColors);

            _root.CollectLeaves(result, pixelCounts);
            return result;
        }
    }
}