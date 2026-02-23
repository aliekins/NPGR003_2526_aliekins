using System;
using _08_Fireworks.Core;

namespace _08_Fireworks.Particles
{
    public sealed class ParticlePool
    {
        private readonly Particle[] _particles;

        private readonly int[] _alive;
        private readonly int[] _aliveSlot;

        private readonly int[] _free;
        private int _freeTop;

        public int Capacity { get; }
        public int AliveCount { get; private set; }

        public ParticlePool (int capacity)
        {
            if (capacity <= 0)
            {
                throw new ArgumentException("Capacity must be over 0");
            }

            Capacity = capacity;

            _particles = new Particle[capacity];
            _alive = new int[capacity];
            _aliveSlot = new int[capacity];

            _free = new int[capacity];
            _freeTop = capacity;

            for (int i = 0; i < capacity; ++i)
            {
                _free[i] = capacity - 1 - i;
                _aliveSlot[i] = -1;
            }

            AliveCount = 0;
        }

        public ref Particle Get (int id)
        {
            return ref _particles[id];
        }

        public ReadOnlySpan<int> AliveIds => new ReadOnlySpan<int>(_alive, 0, AliveCount);

        public bool IsFull => _freeTop == 0;

        public void Clear ()
        {
            AliveCount = 0;
            _freeTop = Capacity;

            for (int i = 0; i < Capacity; i++)
            {
                _free[i] = Capacity - 1 - i;
                _aliveSlot[i] = -1;
            }
        }

        // Allocate a new particle id, mark it alive, return its id.
        public bool TryAllocate (out int id)
        {
            if (_freeTop == 0)
            {
                id = -1;
                return false;
            }

            _freeTop--;
            id = _free[_freeTop];

            int slot = AliveCount;
            _alive[slot] = id;
            _aliveSlot[id] = slot;
            AliveCount++;

            return true;
        }

        // Retire a particle id
        public void Free (int id)
        {
            int slot = _aliveSlot[id];
            if (slot < 0)
            {
                return;
            }

            int lastSlot = AliveCount - 1;
            int lastId = _alive[lastSlot];

            // move last alive into removed slot
            _alive[slot] = lastId;
            _aliveSlot[lastId] = slot;

            _aliveSlot[id] = -1;
            AliveCount--;

            _free[_freeTop] = id;
            _freeTop++;
        }
    }
}