using SixLabors.ImageSharp.PixelFormats;
#nullable enable

namespace ImagePalette
{
    public sealed class OctreeNode
    {
        private readonly OctreeNode?[] _children = new OctreeNode?[8];

        private long _redSum;
        private long _greenSum;
        private long _blueSum;
        private int _pixelCount;

        private readonly int _level;
        private bool _isLeaf;
        private readonly OctreeQuantizer _parentQuantizer;

        public OctreeNode (int level, OctreeQuantizer parentQuantizer)
        {
            this._level = level;
            this._parentQuantizer = parentQuantizer;
            this._isLeaf = level == _parentQuantizer.MaxDepth;

            if (level > 0 && level < _parentQuantizer.MaxDepth)
            {
                this._parentQuantizer.RegisterReducableNode(level, this);
            }

        }

        public void AddColor (Rgba32 color, int level, OctreeQuantizer quantizer)
        {
            if (_isLeaf)
            {
                _redSum += color.R;
                _greenSum += color.G;
                _blueSum += color.B;

                if (_pixelCount == 0)
                {
                    _parentQuantizer.IncrementLeafCount();
                }

                _pixelCount++;
                return;
            }

            int childIndex = GetChildIndex(color, _level);
            OctreeNode? child = _children[childIndex];

            if (child == null)
            {
                child = new OctreeNode(_level + 1, quantizer);
                _children[childIndex] = child;
            }

            child.AddColor(color, level + 1, quantizer);

        }

        public void MergeChildren ()
        {
            if (_isLeaf)
            {
                return;
            }

            bool hadAnyChild = false;

            for (int i = 0; i < _children.Length; i++)
            {
                OctreeNode? child = _children[i];
                if (child == null)
                {
                    continue;
                }

                hadAnyChild = true;

                child.MergeChildren();

                if (child._pixelCount > 0)
                {
                    _redSum += child._redSum;
                    _greenSum += child._greenSum;
                    _blueSum += child._blueSum;
                    _pixelCount += child._pixelCount;

                    if (child._isLeaf)
                    {
                        _parentQuantizer.DecrementLeafCount();
                    }
                }

                _children[i] = null;
            }

            if (!hadAnyChild)
            {
                return;
            }

            if (!_isLeaf && _pixelCount > 0)
            {
                _isLeaf = true;
                _parentQuantizer.IncrementLeafCount();
            }
        }

        public void CollectLeaves (List<Rgba32> palette, List<int>? pixelCounts = null)
        {
            if (_isLeaf)
            {
                if (_pixelCount > 0)
                {
                    byte r = (byte)(_redSum / _pixelCount);
                    byte g = (byte)(_greenSum / _pixelCount);
                    byte b = (byte)(_blueSum / _pixelCount);
                    palette.Add(new Rgba32(r, g, b));

                    if (pixelCounts != null)
                    {
                        pixelCounts.Add(_pixelCount);
                    }
                }
                return;
            }

            for (int i = 0; i < _children.Length; i++)
            {
                _children[i]?.CollectLeaves(palette, pixelCounts);
            }
        }

        private static int GetChildIndex (Rgba32 color, int level)
        {
            int shift = 7 - level;
            if (shift < 0)
            {
                shift = 0;
            }

            int rBit = (color.R >> shift) & 1;
            int gBit = (color.G >> shift) & 1;
            int bBit = (color.B >> shift) & 1;

            return (rBit << 2) | (gBit << 1) | bBit;
        }
    }
}