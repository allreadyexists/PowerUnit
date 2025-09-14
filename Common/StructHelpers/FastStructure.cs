// SharedMemory (File: SharedMemory\FastStructure.cs)
// Copyright (c) 2014 Justin Stenning
// http://spazzarama.com
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
// The SharedMemory library is inspired by the following Code Project article:
//   "Fast IPC Communication Using Shared Memory and InterlockedCompareExchange"
//   http://www.codeproject.com/Articles/14740/Fast-IPC-Communication-Using-Shared-Memory-and-Int
using System.Reflection;
using System.Reflection.Emit;

namespace PowerUnit.Common.StructHelpers;

/// <summary>
/// Provides fast reading and writing of generic structures to a memory location using IL emitted functions.
/// </summary>
public static class FastStructure
{
    /// <summary>
    /// Retrieve a pointer to the passed generic structure type. This is achieved by emitting a <see cref="DynamicMethod"/> to retrieve a pointer to the structure.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="structure"></param>
    /// <returns>A pointer to the provided structure in memory.</returns>
    /// <see cref="FastStructure{T}.GetPtr"/>
    public static unsafe void* GetPtr<T>(ref T structure)
        where T : struct
    {
        return FastStructure<T>.GetPtr(ref structure);
    }

    /// <summary>
    /// Loads the generic value type <typeparamref name="T"/> from a pointer. This is achieved by emitting a <see cref="DynamicMethod"/> that returns the value in the memory location as a <typeparamref name="T"/>.
    /// <para>The equivalent non-generic C# code:</para>
    /// <code>
    /// unsafe MyStruct ReadFromPointer(byte* pointer)
    /// {
    ///     return *(MyStruct*)pointer;
    /// }
    /// </code>
    /// </summary>
    /// <typeparam name="T">Any value/structure type</typeparam>
    /// <param name="p">Unsafe pointer to memory to load the value from</param>
    /// <returns>The newly loaded value</returns>
    public static unsafe T PtrToStructure<T>(nint p)
        where T : struct
    {
        return FastStructure<T>.PtrToStructure(p);
    }

    /// <summary>
    /// Writes the generic value type <typeparamref name="T"/> to the location specified by a pointer. This is achieved by emitting a <see cref="DynamicMethod"/> that copies the value from the referenced structure into the specified memory location.
    /// <para>There is no exact equivalent possible in C#, the closest possible (generates the same IL) is the following code:</para>
    /// <code>
    /// unsafe void WriteToPointer(ref SharedHeader dest, ref SharedHeader src)
    /// {
    ///     dest = src;
    /// }
    /// </code>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="p"></param>
    /// <param name="structure"></param>
    public static unsafe void StructureToPtr<T>(ref T structure, nint p)
        where T : struct
    {
        FastStructure<T>.StructureToPtr(ref structure, p);
    }

    /// <summary>
    /// Copy bytes of structure into the existing buffer at index
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="structure"></param>
    /// <param name="buffer"></param>
    /// <param name="startIndex"></param>
    /// <returns></returns>
    public static unsafe void CopyTo<T>(ref T structure, byte[] buffer, int startIndex = 0)
        where T : struct
    {
        ArgumentNullException.ThrowIfNull(buffer);
        if (startIndex > buffer.Length || startIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(startIndex));

        fixed (byte* p = &buffer[startIndex])
        {
            StructureToPtr(ref structure, new nint(p));
        }
    }

    /// <summary>
    /// Return a byte[] for the provided structure
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="structure"></param>
    /// <returns></returns>
    public static unsafe byte[] ToBytes<T>(ref T structure)
        where T : struct
    {
        var result = new byte[FastStructure<T>.Size];
        fixed (byte* p = &result[0])
        {
            StructureToPtr(ref structure, new nint(p));
            return result;
        }
    }

    /// <summary>
    /// Read structure from the provided byte array
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="buffer"></param>
    /// <param name="startIndex"></param>
    /// <returns></returns>
    public static unsafe T FromBytes<T>(byte[] buffer, int startIndex = 0)
        where T : struct
    {
        ArgumentNullException.ThrowIfNull(buffer);
        if (startIndex > buffer.Length || startIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(startIndex));

        fixed (byte* p = &buffer[startIndex])
        {
            return PtrToStructure<T>(new nint(p));
        }
    }

    /// <summary>
    /// Retrieve the cached size of a structure
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <remarks>Caches the size by type</remarks>
    /// <see cref="FastStructure{T}.Size"/>
    public static int SizeOf<T>()
        where T : struct
    {
        return FastStructure<T>.Size;
    }

    /// <summary>
    /// Reads a number of elements from a memory location into the provided buffer starting at the specified index.
    /// </summary>
    /// <typeparam name="T">The structure type</typeparam>
    /// <param name="buffer">The destination buffer.</param>
    /// <param name="source">The source memory location.</param>
    /// <param name="index">The start index within <paramref name="buffer"/>.</param>
    /// <param name="count">The number of elements to read.</param>
    public static unsafe void ReadArray<T>(T[] buffer, nint source, int index, int count)
        where T : struct
    {
        var elementSize = (uint)SizeOf<T>();

        ArgumentNullException.ThrowIfNull(buffer);
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        if (buffer.Length - index < count)
            throw new ArgumentException("Invalid offset into array specified by index and count");

        var ptr = source.ToPointer();
        var p = (byte*)GetPtr(ref buffer[0]);

        Buffer.MemoryCopy(ptr, p + index * elementSize, elementSize * count, elementSize * count);
    }

    /// <summary>
    /// Reads a number of elements from a memory location into the provided buffer starting at the specified index.
    /// </summary>
    /// <param name="buffer">The destination buffer.</param>
    /// <param name="source">The source memory location.</param>
    /// <param name="index">The start index within <paramref name="buffer"/>.</param>
    /// <param name="count">The number of elements to read.</param>
    public static unsafe void ReadBytes(byte[] buffer, nint source, int index, int count)
    {
        uint elementSize = sizeof(byte);

        ArgumentNullException.ThrowIfNull(buffer);
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        if (buffer.Length - index < count)
            throw new ArgumentException("Invalid offset into array specified by index and count");

        var ptr = source.ToPointer();

        fixed (byte* p = &buffer[0])
        {
            Buffer.MemoryCopy(ptr, p + index * elementSize, elementSize * count, elementSize * count);
        }
    }

    /// <summary>
    /// Writes a number of elements to a memory location from the provided buffer starting at the specified index.
    /// </summary>
    /// <typeparam name="T">The structure type</typeparam>
    /// <param name="destination">The destination memory location.</param>
    /// <param name="buffer">The source buffer.</param>
    /// <param name="index">The start index within <paramref name="buffer"/>.</param>
    /// <param name="count">The number of elements to write.</param>
    public static unsafe void WriteArray<T>(nint destination, T[] buffer, int index, int count)
        where T : struct
    {
        var elementSize = (uint)SizeOf<T>();

        ArgumentNullException.ThrowIfNull(buffer);
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        if (buffer.Length - index < count)
            throw new ArgumentException("Invalid offset into array specified by index and count");

        var ptr = destination.ToPointer();
        var p = (byte*)GetPtr(ref buffer[0]);

        Buffer.MemoryCopy(p + index * elementSize, ptr, elementSize * count, elementSize * count);
    }

    /// <summary>
    /// Writes a number of elements to a memory location from the provided buffer starting at the specified index.
    /// </summary>
    /// <param name="destination">The destination memory location.</param>
    /// <param name="buffer">The source buffer.</param>
    /// <param name="index">The start index within <paramref name="buffer"/>.</param>
    /// <param name="count">The number of elements to write.</param>
    public static unsafe void WriteBytes(nint destination, byte[] buffer, int index, int count)
    {
        uint elementSize = sizeof(byte);

        ArgumentNullException.ThrowIfNull(buffer);
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        if (buffer.Length - index < count)
            throw new ArgumentException("Invalid offset into array specified by index and count");

        var ptr = destination.ToPointer();
        fixed (byte* p = &buffer[0])
        {
            Buffer.MemoryCopy(p + index * elementSize, ptr, elementSize * count, elementSize * count);
        }
    }
}

/// <summary>
/// Emits optimized IL for the reading and writing of structures to/from memory.
/// <para>For a 32-byte structure with 1 million iterations:</para>
/// <para>The <see cref="FastStructure{T}.PtrToStructure"/> method performs approx. 20x faster than
/// <see cref="System.Runtime.InteropServices.Marshal.PtrToStructure(nint, Type)"/> (8ms vs 160ms), and about 1.6x slower than the non-generic equivalent (8ms vs 5ms)</para>
/// <para>The <see cref="FastStructure{T}.StructureToPtr"/> method performs approx. 8x faster than 
/// <see cref="System.Runtime.InteropServices.Marshal.StructureToPtr(object, nint, bool)"/> (4ms vs 34ms). </para>
/// </summary>
/// <typeparam name="T"></typeparam>
public static class FastStructure<T>
    where T : struct
{
    /// <summary>
    /// Delegate that returns a pointer to the provided structure. Use with extreme caution.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
    public unsafe delegate void* GetPtrDelegate(ref T value);
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix

    /// <summary>
    /// Delegate for loading a structure from the specified memory address
    /// </summary>
    /// <param name="pointer"></param>
    /// <returns></returns>
#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
    public delegate T PtrToStructureDelegate(nint pointer);
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix

    /// <summary>
    /// Delegate for writing a structure to the specified memory address
    /// </summary>
    /// <param name="value"></param>
    /// <param name="pointer"></param>
#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
    public delegate void StructureToPtrDelegate(ref T value, nint pointer);
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix

    /// <summary>
    /// The <see cref="GetPtrDelegate"/> delegate for the generated IL to retrieve a pointer to the structure
    /// </summary>
    public static readonly unsafe GetPtrDelegate GetPtr = BuildFunction();

    /// <summary>
    /// The <see cref="PtrToStructureDelegate"/> delegate for the generated IL to retrieve a structure from a specified memory address.
    /// </summary>
    public static readonly PtrToStructureDelegate PtrToStructure = BuildLoadFromPointerFunction();

    /// <summary>
    /// The <see cref="StructureToPtrDelegate"/> delegate for the generated IL to store a structure at the specified memory address.
    /// </summary>
    public static readonly StructureToPtrDelegate StructureToPtr = BuildWriteToPointerFunction();

    /// <summary>
    /// Cached size of T as determined by <see cref="System.Runtime.InteropServices.Marshal.SizeOf(Type)"/>.
    /// </summary>
    public static readonly int Size = System.Runtime.InteropServices.Marshal.SizeOf(typeof(T));

    private static DynamicMethod _method;
    private static DynamicMethod _methodLoad;
    private static DynamicMethod _methodWrite;

    /// <summary>
    /// Performs once of type compatibility check.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if the type T is incompatible</exception>
    static FastStructure()
    {
        // Performs compatibility checks upon T
        CheckTypeCompatibility(typeof(T));
    }

    private static unsafe GetPtrDelegate BuildFunction()
    {
        _method = new DynamicMethod("GetStructurePtr<" + typeof(T).FullName + ">",
            typeof(void*), [typeof(T).MakeByRefType()], typeof(FastStructure).Module);

        var generator = _method.GetILGenerator();
        generator.Emit(OpCodes.Ldarg_0);
        generator.Emit(OpCodes.Conv_U);
        generator.Emit(OpCodes.Ret);
        return (GetPtrDelegate)_method.CreateDelegate(typeof(GetPtrDelegate));
    }

    private static unsafe PtrToStructureDelegate BuildLoadFromPointerFunction()
    {
        _methodLoad = new DynamicMethod("PtrToStructure<" + typeof(T).FullName + ">",
            typeof(T), [typeof(nint)], typeof(FastStructure).Module);

        var generator = _methodLoad.GetILGenerator();
        generator.Emit(OpCodes.Ldarg_0);
        generator.Emit(OpCodes.Ldobj, typeof(T));
        generator.Emit(OpCodes.Ret);

        return (PtrToStructureDelegate)_methodLoad.CreateDelegate(typeof(PtrToStructureDelegate));
    }

    private static unsafe StructureToPtrDelegate BuildWriteToPointerFunction()
    {
        _methodWrite = new DynamicMethod("StructureToPtr<" + typeof(T).FullName + ">",
            null, [typeof(T).MakeByRefType(), typeof(nint)], typeof(FastStructure).Module);

        var generator = _methodWrite.GetILGenerator();
        generator.Emit(OpCodes.Ldarg_1);
        generator.Emit(OpCodes.Ldarg_0);
        generator.Emit(OpCodes.Ldobj, typeof(T));
        generator.Emit(OpCodes.Stobj, typeof(T));
        generator.Emit(OpCodes.Ret);
        return (StructureToPtrDelegate)_methodWrite.CreateDelegate(typeof(StructureToPtrDelegate));
    }

    private static void CheckTypeCompatibility(Type t, HashSet<Type> checkedItems = null)
    {
        if (checkedItems == null)
        {
            checkedItems =
            [
                typeof(char),
                typeof(byte),
                typeof(sbyte),
                typeof(bool),
                typeof(double),
                typeof(float),
                typeof(decimal),
                typeof(int),
                typeof(short),
                typeof(long),
                typeof(uint),
                typeof(ushort),
                typeof(ulong),
                typeof(nint),
                typeof(void*),
            ];
        }

        if (!checkedItems.Add(t))
            return;

        var fi = t.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
        foreach (var info in fi)
        {
            if (!info.FieldType.IsPrimitive && !info.FieldType.IsValueType && !info.FieldType.IsPointer)
            {
                throw new ArgumentException(string.Format("Non-value types are not supported: field {0} is of type {1} in structure {2}", info.Name, info.FieldType.Name, info.DeclaringType?.Name));
            }

            // Example for adding future marshal attributes as incompatible
            //System.Runtime.InteropServices.MarshalAsAttribute attr;
            //if (TryGetAttribute<System.Runtime.InteropServices.MarshalAsAttribute>(info, out attr))
            //{
            //    if (attr.Value == System.Runtime.InteropServices.UnmanagedType.ByValArray)
            //    {
            //        throw new ArgumentException(String.Format("UnmanagedType.ByValArray is not supported on field {0} in type [{1}].", info.Name, typeof(T).FullName));
            //    }
            //}

            CheckTypeCompatibility(info.FieldType, checkedItems);
        }
    }

    //private static bool TryGetAttribute<T1>(MemberInfo memberInfo, out T1 customAttribute) where T1 : Attribute
    //{
    //    var attributes = memberInfo.GetCustomAttributes(typeof(T1), false).FirstOrDefault();
    //    if (attributes == null)
    //    {
    //        customAttribute = null;
    //        return false;
    //    }
    //    customAttribute = (T1)attributes;
    //    return true;
    //}
}