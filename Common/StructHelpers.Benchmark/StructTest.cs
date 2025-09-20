using BenchmarkDotNet.Attributes;

using PowerUnit.Common.StructHelpers;

using System.Runtime.InteropServices;

using static PowerUnit.Common.StructHelpers.StructHelper;

namespace Test;

public class TestClass
{
    public int A;
    public double B;
}

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct TestStruct
{
    [FieldOffset(0)]
    public int A;
    [FieldOffset(4)]
    public double B;
}

[MemoryDiagnoser]
public class StructTest
{
    private TestClass[] _testClass;
    private int _size;
    private byte[] _buffer;
    private ObjectToStructConverter<TestClass, TestStruct> _converter;

    [Params(16, 32, 256, 1024, 4096)]
    public int Count;

    [GlobalSetup]
    public unsafe void Setup()
    {
        _size = FastStructure.SizeOf<TestStruct>();
        _buffer = new byte[Count * FastStructure.SizeOf<TestStruct>()];

        _testClass = new TestClass[Count];
        for (var i = 0; i < Count; i++)
            _testClass[i] = new TestClass()
            {
                A = Random.Shared.Next(int.MinValue, int.MaxValue),
                B = Random.Shared.NextDouble(),
            }
        ;
        _converter = static (testClass, testStruct) =>
        {
            testStruct->A = testClass.A;
            testStruct->B = testClass.B;
        };
    }

    [Benchmark]
    public unsafe int MemoryBlockWrapper_Serialize()
    {
        using var memoryWrapper = new MemoryBlockWrapper<byte>(_buffer);
        for (var i = 0; i < Count; i++)
            ZeroCopySerialize<TestClass, TestStruct>(_testClass[i], memoryWrapper, i * _size, (testClass, testStruct) =>
            {
                testStruct->A = testClass.A;
                testStruct->B = testClass.B;
            });
        return 1;
    }

    [Benchmark]
    public unsafe int MemoryBlockWrapper_SerializeFixed()
    {
        fixed (byte* memory = _buffer)
        {
            for (var i = 0; i < Count; i++)
                ZeroCopySerialize<TestClass, TestStruct>(_testClass[i], memory, i * _size, (testClass, testStruct) =>
                {
                    testStruct->A = testClass.A;
                    testStruct->B = testClass.B;
                });
        }

        return 1;
    }

    [Benchmark]
    public unsafe int MemoryBlockWrapper_SerializeStaticConvertor()
    {
        using var memoryWrapper = new MemoryBlockWrapper<byte>(_buffer);
        for (var i = 0; i < Count; i++)
            ZeroCopySerialize(_testClass[i], memoryWrapper, i * _size, _converter);
        return 1;
    }

    [Benchmark(Baseline = true)]
    public unsafe int MemoryBlockWrapper_SerializeStaticConvertorFixed()
    {
        fixed (byte* memory = _buffer)
        {
            for (var i = 0; i < Count; i++)
                ZeroCopySerialize(_testClass[i], memory, i * _size, _converter);
        }

        return 1;
    }

    [Benchmark]
    public unsafe int SerializeUnsafe()
    {
        for (var i = 0; i < Count; i++)
        {
            var testStruct = new TestStruct() { A = _testClass[i].A, B = _testClass[i].B };
            testStruct.SerializeUnsafe(_buffer, i * _size);
        }

        return 1;
    }
}