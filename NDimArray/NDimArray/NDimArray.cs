﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace NDimArray
{
    public class NDimArray<T> : IEnumerable<T>, IEnumerable<Tuple<int[], T>>
    {
        private readonly Array array;

        /// <summary>
        /// The amount of dimensions of this array.
        /// </summary>
        public int Rank => array.Rank;

        /// <summary>
        /// The total number of elements in all dimensions.
        /// </summary>
        public int Length => array.Length;

        /// <summary>
        /// Gets an object that synchronizes access to the array.
        /// </summary>
        public object SyncRoot => array.SyncRoot;

        public bool IsFixedSize => true;
        public virtual bool IsSynchronized => false;
        public bool IsReadOnly => false;

        public T this[params int[] index]
        {
            get => GetValue(index);
            set => SetValue(value, index);
        }

        #region Constructors
        public NDimArray(int[] lengths, int[] lowerBounds)
        {
            if (lengths == null)
                throw new ArgumentNullException("lengths", "dimensions array is null");
            if (lowerBounds == null)
                throw new ArgumentNullException("lowerBounds", "lowerBounds array is null");
            if (lengths.Length < 1)
                throw new ArgumentException("lengths", "lengths must have at least 1 element");
            if (lowerBounds.Length < 1)
                throw new ArgumentException("lowerBounds", "lowerBounds must have at least 1 element");

            if (lengths.Length != lowerBounds.Length)
                throw new ArgumentException("lowerBounds", "length of lowerBounds must be equal to the length of lengths");

            if (ValidDimensions(lengths))
                array = Array.CreateInstance(typeof(T), lengths, lowerBounds);
            else
                throw new ArgumentOutOfRangeException("lengths", "Every length must be > 0.");
        }

        public NDimArray(params int[] lengths) : 
            this(
                lengths, 
                lengths != null ? 
                    (lengths.Length > 0 ? 
                        (int[])Array.CreateInstance(typeof(int), lengths.Length) 
                        : throw new ArgumentException("lengths", "lengths myst have at least 1 element")) 
                    : throw new ArgumentNullException("lengths","lengths was null")
                ) { }

        public NDimArray(T[] array)
        {
            if (array == null)
                throw new ArgumentNullException("array","array is null");

            this.array = array;
        }
        #endregion

        #region Getters/Setters
        public virtual T GetValue(params int[] index)
        {
            if (ValidIndices(index))
                return (T)array.GetValue(index);
            else
                return default;
        }

        public virtual void SetValue(T value, params int[] index)
        {
            if (ValidIndices(index))
                array.SetValue(value, index);
        }

        protected virtual bool ValidIndices(int[] indices)
        {
            if (indices == null)
                throw new ArgumentNullException("indices", "indices was null");

            if (indices.Length != Rank)
                throw new ArgumentOutOfRangeException("indices", "index must match the rank of the NDimArray");

            for (int i = 0; i < indices.Length; i++)
            {
                if (indices[i] < array.GetLowerBound(i) || indices[i] > array.GetUpperBound(i))
                    return false;
                else
                    continue;
            }
            return true;
        }

        protected virtual bool ValidDimensions(int[] dims)
        {
            return Array.TrueForAll(dims, x => x > 0);
        }
        #endregion

        #region Enumeration
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<T> GetEnumerator() => new SpatialEnumerator<T>(array);

        IEnumerator<Tuple<int[], T>> IEnumerable<Tuple<int[], T>>.GetEnumerator() => (IEnumerator<Tuple<int[], T>>)(new SpatialEnumerator<T>(array));

        public void Enumerate(Action<T> action)
        {
            foreach (var item in this)
            {
                action(item);
            }
        }
        public void Enumerate(Action<int[], T> action)
        {
            foreach (var item in (IEnumerable<Tuple<int[], T>>)this)
            {
                action(item.Item1, item.Item2);
            }
        }

        public void Enumerate(int[] start, int[] end, int[] dimPriorities, Action<int[], T> action)
        {
            using (IEnumerator<Tuple<int[], T>> enumer = new SpatialEnumerator<T>(array, start, end, dimPriorities))
            {
                while (enumer.MoveNext())
                {
                    var tup = enumer.Current;
                    action(tup.Item1, tup.Item2);
                }
            }
        }
        public void Enumerate(int[] start, int[] end, int[] dimPriorities, Action<T> action) =>
            Enumerate(start, end, dimPriorities, (index, item) => action(item));

        public void Enumerate(int[] start, int[] end, Action<int[], T> action) => 
            Enumerate(start, end, SpatialEnumerator<T>.GetStandardPriorityList(Rank), action);
        public void Enumerate(int[] start, int[] end, Action<T> action) =>
            Enumerate(start, end, (index, item) => action(item));
        #endregion

        #region Methods
        public int GetLowerBound(int dimension)
        {
            return array.GetLowerBound(dimension);
        }

        public int GetUpperBound(int dimension)
        {
            return array.GetUpperBound(dimension);
        }

        public int[] GetStandardEnumerationPriorities()
        {
            return SpatialEnumerator<T>.GetStandardPriorityList(Rank);
        }
        #endregion
    }
}