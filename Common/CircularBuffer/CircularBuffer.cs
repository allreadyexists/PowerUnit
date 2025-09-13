// Copyright The OpenTelemetry Authors
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace PowerUnit.Common.CircularBuffer;

/// <summary>
/// Lock-free implementation of single-reader multi-writer circular buffer.
/// </summary>
/// <typeparam name="T">The type of the underlying value.</typeparam>
public sealed class CircularBuffer<T>
    where T : class
{
    private readonly T?[] _trait;
    private long _head;
    private long _tail;

    /// <summary>
    /// Initializes a new instance of the <see cref="CircularBuffer{T}"/> class.
    /// </summary>
    /// <param name="capacity">The capacity of the circular buffer, must be a positive integer.</param>
    public CircularBuffer(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(capacity, 1);

        Capacity = capacity;
        _trait = new T[capacity];
    }

    /// <summary>
    /// Gets the capacity of the <see cref="CircularBuffer{T}"/>.
    /// </summary>
    public int Capacity { get; }

    /// <summary>
    /// Gets the number of items contained in the <see cref="CircularBuffer{T}"/>.
    /// </summary>
    public int Count
    {
        get
        {
            var tailSnapshot = Volatile.Read(ref _tail);
            return (int)(Volatile.Read(ref _head) - tailSnapshot);
        }
    }

    /// <summary>
    /// Gets the number of items added to the <see cref="CircularBuffer{T}"/>.
    /// </summary>
    public long AddedCount => Volatile.Read(ref _head);

    /// <summary>
    /// Gets the number of items removed from the <see cref="CircularBuffer{T}"/>.
    /// </summary>
    public long RemovedCount => Volatile.Read(ref _tail);

    /// <summary>
    /// Adds the specified item to the buffer.
    /// </summary>
    /// <param name="value">The value to add.</param>
    /// <returns>
    /// Returns <c>true</c> if the item was added to the buffer successfully;
    /// <c>false</c> if the buffer is full.
    /// </returns>
    public bool Add(T value)
    {
        Debug.Assert(value != null, "value was null");

        while (true)
        {
            var tailSnapshot = Volatile.Read(ref _tail);
            var headSnapshot = Volatile.Read(ref _head);

            if (headSnapshot - tailSnapshot >= Capacity)
            {
                return false; // buffer is full
            }

            if (Interlocked.CompareExchange(ref _head, headSnapshot + 1, headSnapshot) != headSnapshot)
            {
                continue;
            }

            Volatile.Write(ref _trait[headSnapshot % Capacity], value);

            return true;
        }
    }

    /// <summary>
    /// Attempts to add the specified item to the buffer.
    /// </summary>
    /// <param name="value">The value to add.</param>
    /// <param name="maxSpinCount">The maximum allowed spin count, when set to a negative number or zero, will spin indefinitely.</param>
    /// <returns>
    /// Returns <c>true</c> if the item was added to the buffer successfully;
    /// <c>false</c> if the buffer is full or the spin count exceeded <paramref name="maxSpinCount"/>.
    /// </returns>
    public bool TryAdd(T value, int maxSpinCount)
    {
        if (maxSpinCount <= 0)
        {
            return Add(value);
        }

        Debug.Assert(value != null, "value was null");

        var spinCountDown = maxSpinCount;

        while (true)
        {
            var tailSnapshot = Volatile.Read(ref _tail);
            var headSnapshot = Volatile.Read(ref _head);

            if (headSnapshot - tailSnapshot >= Capacity)
            {
                return false; // buffer is full
            }

            if (Interlocked.CompareExchange(ref _head, headSnapshot + 1, headSnapshot) != headSnapshot)
            {
                if (spinCountDown-- == 0)
                {
                    return false; // exceeded maximum spin count
                }

                continue;
            }

            Volatile.Write(ref _trait[headSnapshot % Capacity], value);

            return true;
        }
    }

    /// <summary>
    /// Reads an item from the <see cref="CircularBuffer{T}"/>.
    /// </summary>
    /// <remarks>
    /// This function is not reentrant-safe, only one reader is allowed at any given time.
    /// Warning: There is no bounds check in this method. Do not call unless you have verified Count > 0.
    /// </remarks>
    /// <returns>Item read.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Read()
    {
        var tail = Volatile.Read(ref _tail);
        var index = (int)(tail % Capacity);
        while (true)
        {
            var previous = Interlocked.Exchange(ref _trait[index], null);
            if (previous == null)
            {
                // If we got here it means a writer isn't done.
                continue;
            }

            Volatile.Write(ref _tail, tail + 1);
            return previous;
        }
    }
}
