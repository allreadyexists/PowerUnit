using FluentAssertions;
using FluentAssertions.Execution;

using NUnit.Framework;

using System.Text.Json;
using System.Text.Json.Serialization;

namespace K2.Common.JsonConverter.Tests;

[JsonConverter(typeof(AbstractClassConverterWithTypeDiscriminator<C>))]
public abstract class C
{
    public int IntValue { get; set; }
}

public class C1 : C
{
    public int IntValue1 { get; set; }
}

public class C2 : C
{
    public string StringValue2 { get; set; }
    public DateTime DateTime2 { get; set; }
}

[JsonConverter(typeof(AbstractClassConverterWithTypeDiscriminator<B>))]
public abstract class B
{
    public int IntValue { get; set; }
    public C C { get; set; }
}

public class B1 : B
{
    public double DoubleValue1 { get; set; }
}

public class B2 : B
{
    public double IntValue2 { get; set; }
    public double DoubleValue2 { get; set; }
}

public class A
{
    public int Id { get; set; }
    public string Name { get; set; }
    public B B { get; set; }
    public Guid GuidId { get; set; }
}

[TestFixture]
public class AbstractClassConverterWithTypeDiscriminatorTest
{
    [Test]
    public void Serializer()
    {
        var a = new A()
        {
            Id = 1,
            Name = "test",
            GuidId = Guid.Empty,
            B = new B1()
            {
                DoubleValue1 = 1.5,
                IntValue = 5,
                C = new C2()
                {
                    IntValue = 14,
                    StringValue2 = "deep",
                    DateTime2 = new DateTime(2023, 2, 12, 3, 45, 23)
                }
            }
        };

        var aAsString = JsonSerializer.Serialize(a);
        var expectedString = /*lang=json,strict*/ @"{""Id"":1,""Name"":""test"",""B"":{""$typeDiscriminator"":""B1"",""$wrappedValue"":{""DoubleValue1"":1.5,""IntValue"":5,""C"":{""$typeDiscriminator"":""C2"",""$wrappedValue"":{""StringValue2"":""deep"",""DateTime2"":""2023-02-12T03:45:23"",""IntValue"":14}}}},""GuidId"":""00000000-0000-0000-0000-000000000000""}";

        using var scope = new AssertionScope();
        aAsString.Should().Be(expectedString);
    }

    [Test]
    public void Deserializer()
    {
        var aAsString =
            /*lang=json,strict*/ @"{""Id"":1,""Name"":""test"",""B"":{""$typeDiscriminator"":""B1"",""$wrappedValue"":{""DoubleValue1"":1.5,""IntValue"":5,""C"":{""$typeDiscriminator"":""C2"",""$wrappedValue"":{""StringValue2"":""deep"",""DateTime2"":""2023-02-12T03:45:23"",""IntValue"":14}}}},""Guid"":""00000000-0000-0000-0000-000000000000""}";

        var aAsObject = JsonSerializer.Deserialize<A>(aAsString);
        var expected = new A()
        {
            Id = 1,
            Name = "test",
            GuidId = Guid.Empty,
            B = new B1()
            {
                DoubleValue1 = 1.5,
                IntValue = 5,
                C = new C2()
                {
                    IntValue = 14,
                    StringValue2 = "deep",
                    DateTime2 = new DateTime(2023, 2, 12, 3, 45, 23)
                }
            }
        };

        using var scope = new AssertionScope();
        aAsObject.Id.Should().Be(expected.Id);
        aAsObject.Name.Should().Be(expected.Name);
        aAsObject.GuidId.Should().Be(expected.GuidId);

        aAsObject.B.IntValue.Should().Be(expected.B.IntValue);
        (aAsObject.B as B1).DoubleValue1.Should().Be((expected.B as B1).DoubleValue1);
        (aAsObject.B.C as C2).IntValue.Should().Be((expected.B.C as C2).IntValue);
        (aAsObject.B.C as C2).StringValue2.Should().Be((expected.B.C as C2).StringValue2);
        (aAsObject.B.C as C2).DateTime2.Should().Be((expected.B.C as C2).DateTime2);
    }
}
