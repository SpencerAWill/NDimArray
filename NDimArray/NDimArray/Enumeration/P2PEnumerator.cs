﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NDimArray
{
    internal sealed class P2PEnumerator<T> : IEnumerator<T>, IEnumerator<Tuple<INIndex, T>>
    {
        private Array _array;

        private IPath _path;
        private IEnumerationPriorities _priorities;

        private int[] _enumerationAxes;
        private bool _firstEvaluated;
        private NIndex _currentIndex;

        private Array Array { get => _array; }
        public IPath Path { get => _path; }
        public IEnumerationPriorities Priorities { get => _priorities; }
        private int[] EnumerationAxes { get => _enumerationAxes; }
        private bool FirstEvaluated { get => _firstEvaluated; set => _firstEvaluated = value; }
        public NIndex CurrentIndex { get => _currentIndex; private set => _currentIndex = value; }

        public T Current =>
            (T)Array.GetValue(CurrentIndex.Indices);

        Tuple<INIndex, T> IEnumerator<Tuple<INIndex, T>>.Current =>
            new Tuple<INIndex, T>(CurrentIndex, Current);

        object IEnumerator.Current => Current;

        public P2PEnumerator(Array array, IPath path)
        {
            if (array == null)
                throw new ArgumentNullException("array", "array is null");
            if (path == null)
                throw new ArgumentNullException("path", "path is null");

            if (array.Rank != path.DimensionCount)
                throw new ArgumentException("path", "path must have the same dimension count as the rank of the array.");

            _array = array;
            _path = path;
            _priorities = Path.DimEnumerationPriorities;
            _enumerationAxes = (NIndex.Unit(NIndex.Difference(Path.End, Path.Start)));
            Reset();
        }

        public P2PEnumerator(Array array) : 
            this(
                array, 
                new IndexPath(
                    new NIndex(array.GetLowerBoundaries()),
                    new NIndex(array.GetUpperBoundaries()),
                    EnumerationPriorities.CreateStandard(array.Rank)))
        { }

        public void Reset()
        {
            FirstEvaluated = false;
            CurrentIndex = new NIndex(Path.Start.Indices.ToArray());
        }

        public bool MoveNext()
        {
            if (FirstEvaluated)
            {
                if (!CurrentIndex.SequenceEqual(Path.End))
                {
                    Increment(0);
                    return true;
                }
                else return false;
            }
            else
            {
                FirstEvaluated = true;
                return true;
            }
        }

        private void Increment(int dimension)
        {
            int currentIncrementationDimension = Path.DimEnumerationPriorities[dimension];
            int step = EnumerationAxes[currentIncrementationDimension];

            if (ShouldMove())
            {
                CurrentIndex[currentIncrementationDimension] = Path.Start[currentIncrementationDimension]; //reset back in this dimension
                Increment(dimension + 1); //step next dimension
            }
            else
            {
                CurrentIndex[currentIncrementationDimension] += step; //step next
            }

            
            bool ShouldMove()
            {
                return CurrentIndex[currentIncrementationDimension] == Path.End[currentIncrementationDimension];
            }
        }

        public void Dispose() { }

    }
}
