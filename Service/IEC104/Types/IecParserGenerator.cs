using System.Collections.Frozen;
using System.Linq.Expressions;
using System.Reflection;

namespace PowerUnit.Service.IEC104.Types;

public class IECParserGenerator
{
    private readonly FrozenDictionary<Type, ASDUTypeInfoAttribute> _types;
    private readonly FrozenSet<int> _toClientCots;
    private readonly FrozenSet<int> _toServerCots;

    public IECParserGenerator(Assembly[] assemblies)
    {
        var types = new Dictionary<Type, ASDUTypeInfoAttribute>();
        var toClientCot = new HashSet<int>();
        var toServerCot = new HashSet<int>();
        var allAssemblies = new HashSet<Assembly>(AppDomain.CurrentDomain.GetAssemblies().Concat(assemblies));
        foreach (var type in allAssemblies.SelectMany(x => x.GetTypes())
            .Where(x => x.IsValueType))
        {
            var customAttributes = type.GetCustomAttributes(false);

            foreach (var customAttribute in customAttributes)
            {
                switch (customAttribute)
                {
                    case ASDUTypeInfoAttribute asduTypeInfo:
                        types.Add(type, asduTypeInfo);
                        foreach (var item in asduTypeInfo.ToClientCauseOfTransmits)
                        {
                            toClientCot.Add((int)asduTypeInfo.AsduType << 8 | (int)item);
                        }

                        foreach (var item in asduTypeInfo.ToServerCauseOfTransmits)
                        {
                            toServerCot.Add((int)asduTypeInfo.AsduType << 8 | (int)item);
                        }

                        break;
                }
            }
        }

        _types = types.ToFrozenDictionary();
        _toClientCots = toClientCot.ToFrozenSet();
        _toServerCots = toServerCot.ToFrozenSet();
    }

    private readonly Lock _parserLock = new Lock();

    private Action<IASDUNotification, byte[], DateTime, bool>? _parser;

    private Lazy<Action<IASDUNotification, byte[], DateTime, bool>> Parser => new(
        () =>
        {
            if (_parser == null)
            {
                lock (_parserLock)
                {
                    _parser ??= GenerateParseMethod();
                }
            }

            return _parser;
        });

    private Action<IASDUNotification, byte[], DateTime, bool> GenerateParseMethod()
    {
        Expression CreateSwitchBlock(IEnumerable<KeyValuePair<Type, ASDUTypeInfoAttribute>> types, ParameterExpression notification, ParameterExpression header, ParameterExpression asdu, ParameterExpression dateTime,
            ParameterExpression switchParam, ParameterExpression isServerSideParam)
        {
            var checkCot = typeof(IECParserGenerator).GetMethod(nameof(CheckCOT), BindingFlags.NonPublic | BindingFlags.Instance)!;
            var asduProperty = typeof(ASDUPacketHeader_2_2).GetProperty(nameof(ASDUPacketHeader_2_2.AsduType));
            var cotProperty = typeof(ASDUPacketHeader_2_2).GetProperty(nameof(ASDUPacketHeader_2_2.CauseOfTransmit));

            var notifyCommonAsduAddress = typeof(IASDUNotification).GetMethod(nameof(IASDUNotification.Notify_CommonAsduAddress))!;
            var defaultCaseUnknownCotMethod = typeof(IASDUNotification).GetMethod(nameof(IASDUNotification.Notify_Unknown_Cot_Raw))!;
            var defaultCaseUnknownAsduMethod = typeof(IASDUNotification).GetMethod(nameof(IASDUNotification.Notify_Unknown_Asdu_Raw))!;
            var defaultCaseUnknownException = typeof(IASDUNotification).GetMethod(nameof(IASDUNotification.Notify_Unknown_Exception))!;
            var defaultCase = Expression.Call(notification, defaultCaseUnknownAsduMethod, header, asdu);

            var switchCases = types.OrderBy(x => x.Value.AsduType).ThenBy(x => x.Value.SQ).Select(x =>
            {
                var method = x.Key.GetMethod("Parse",
                    BindingFlags.Static | BindingFlags.Public,
                    [typeof(Span<byte>), typeof(ASDUPacketHeader_2_2).MakeByRefType(), typeof(DateTime), typeof(IASDUNotification)]);
                var caseValue = Expression.Constant((int)x.Value.AsduType << 8 | (int)x.Value.SQ);
                var exceptionParameter = Expression.Parameter(typeof(Exception));

                return Expression.SwitchCase(
                    Expression.IfThenElse(
                        Expression.Equal(Expression.Call(Expression.Constant(this), checkCot,
                            Expression.Property(header, asduProperty!),
                            Expression.Property(header, cotProperty!),
                            isServerSideParam),
                        Expression.Constant(true)),
                            Expression.Block(
                                Expression.TryCatch(
                                    Expression.Call(null, method!, asdu, header, dateTime, notification),
                                    Expression.Catch(exceptionParameter,
                                        Expression.Call(notification, defaultCaseUnknownException, header, asdu, exceptionParameter)
                                        )
                                    )
                                ),
                            Expression.Call(notification, defaultCaseUnknownCotMethod, header, asdu)),
                        caseValue);
            }).ToArray();

            var switchExpr =
                Expression.Switch(
                    switchParam,
                    defaultCase,
                    switchCases
                    );

            var expression = Expression.Block(
                Expression.IfThen(
                    Expression.Equal(Expression.Call(notification, notifyCommonAsduAddress, header, asdu), Expression.Constant(true)),
                    switchExpr
                ));

            return expression;
        }

        var headerVar = Expression.Variable(typeof(ASDUPacketHeader_2_2), "headerVar");
        var asduVar = Expression.Variable(typeof(Span<byte>), "asduVar");
        var dateTimeVar = Expression.Variable(typeof(DateTime), "dateTimeVar");

        var asSpan3Method = typeof(MemoryExtensions).GetMethods(BindingFlags.Static | BindingFlags.Public)
            .Single(x =>
            {
                var @params = x.GetParameters();
                return x.Name.Equals("AsSpan", StringComparison.Ordinal) && @params.Length == 3 && @params[0].ParameterType.IsArray && @params[1].ParameterType == typeof(int)
                && @params[2].ParameterType == typeof(int);
            }).MakeGenericMethod(typeof(byte));

        var asSpan2Method = typeof(MemoryExtensions).GetMethods(BindingFlags.Static | BindingFlags.Public)
            .Single(x =>
            {
                var @params = x.GetParameters();
                return x.Name.Equals("AsSpan", StringComparison.Ordinal) && @params.Length == 2 && @params[0].ParameterType.IsArray && @params[1].ParameterType == typeof(int);
            }).MakeGenericMethod(typeof(byte));

        var delegateType = Expression.GetDelegateType(typeof(Span<byte>), typeof(ASDUPacketHeader_2_2).MakeByRefType(), typeof(void));
        var delegateMethod = delegateType.GetMethod("Invoke");
        var @delegate = Delegate.CreateDelegate(delegateType, null, typeof(IECParsers).GetMethod(nameof(IECParsers.ParseHeader))!);

        var switchParam = Expression.Parameter(typeof(int), "caseValue");

        // Аргументы метода
        var bufferParam = Expression.Parameter(typeof(byte[]), "bufferParam");
        var dateTimeParam = Expression.Parameter(typeof(DateTime), "dateTimeParam");
        var notificationParam = Expression.Parameter(typeof(IASDUNotification), "notificationParam");
        var isServerSideParam = Expression.Parameter(typeof(bool), "isServerSide");

        var asduCall = Expression.Call(asSpan2Method, bufferParam, Expression.Constant((int)ASDUPacketHeader_2_2.Size));
        var delegateCall = Expression.Call(Expression.Constant(@delegate), delegateMethod!,
            Expression.Call(asSpan3Method, bufferParam, Expression.Constant(0), Expression.Constant((int)ASDUPacketHeader_2_2.Size)), headerVar);

        var asduProperty = typeof(ASDUPacketHeader_2_2).GetProperty(nameof(ASDUPacketHeader_2_2.AsduType));
        var sqProperty = typeof(ASDUPacketHeader_2_2).GetProperty(nameof(ASDUPacketHeader_2_2.SQ));
        var cotProperty = typeof(ASDUPacketHeader_2_2).GetProperty(nameof(ASDUPacketHeader_2_2.CauseOfTransmit));

        var asduValue = Expression.Convert(Expression.LeftShift(
                                Expression.Convert(Expression.Property(headerVar, asduProperty!), typeof(int)),
                                Expression.Constant(8)), typeof(int));
        var sqValue = Expression.Convert(Expression.Property(headerVar, sqProperty!), typeof(int));

        var switchParamValue = Expression.Add(asduValue, sqValue);

        var returnTarget = Expression.Label("end");

        var debugConsoleWriteLine = typeof(Console).GetMethod(nameof(Console.WriteLine), [typeof(string)])!;

        var parser = Expression.Lambda<Action<IASDUNotification, byte[], DateTime, bool>>(
            Expression.Block(
                [headerVar, asduVar, dateTimeVar, switchParam],
                delegateCall,
                Expression.Assign(asduVar, asduCall),
                Expression.Assign(dateTimeVar, dateTimeParam),
                Expression.Assign(switchParam, switchParamValue),

                CreateSwitchBlock(_types, notificationParam, headerVar, asduVar, dateTimeVar, switchParam, isServerSideParam),
                Expression.Label(returnTarget)
                ),
                notificationParam,
                bufferParam,
                dateTimeParam,
                isServerSideParam
            );

        var result = parser.Compile();
        return result;
    }

    private bool CheckCOT(ASDUType asduType, COT causeOfTransmit, bool isServerSide)
    {
        var id = (int)asduType << 8 | (int)causeOfTransmit;
        return (isServerSide ? _toServerCots : _toClientCots).Contains(id);
    }

    public void Parse(IASDUNotification notification, byte[] buffer, DateTime dateTime, bool isServerSide)
    {
        Parser.Value(notification, buffer, dateTime, isServerSide);
    }

    public void Validate()
    {
        var duplicates = new HashSet<ushort>();
        foreach (var type in _types.OrderBy(x => x.Value.AsduType))
        {
            Console.WriteLine($"Type - {type.Key} {type.Value.AsduType} {type.Value.SQ}");

            var asduType = type.Value;
            var asduCode = (ushort)((ushort)asduType.AsduType << 8 | (ushort)asduType.SQ);

            if (!duplicates.Add(asduCode))
            {
                Console.WriteLine($"ASDU duplicate - {asduType.AsduType} {asduType.SQ}");
            }

            var sizeMethod = type.Key.GetProperty("Size", BindingFlags.Static | BindingFlags.Public);
            if (sizeMethod == null)
            {
                Console.WriteLine("Not Implemented - Size");
            }

            var serializeLikeMethods = type.Key.GetMethods(BindingFlags.Static | BindingFlags.Public).Where(x => x.Name.Equals("Serialize"));
            foreach (var serializeLikeMethod in serializeLikeMethods)
            {
                if (serializeLikeMethod == null)
                {
                    Console.WriteLine("Not Implemented - Serialize");
                }
                else
                {
                    var parameters = serializeLikeMethod.GetParameters();
                    if (parameters.Length < 2)
                    {
                        Console.WriteLine("Check method - Serialize");
                    }
                    else
                    {
                        var param0 = parameters[1];
                        var param1 = parameters[0];

                        if (!param0.ParameterType.IsByRef || param0.ParameterType.GetElementType() != typeof(ASDUPacketHeader_2_2))
                        {
                            Console.WriteLine("Check method param 0 - Serialize");
                        }

                        if (param1.ParameterType != typeof(byte[]))
                        {
                            Console.WriteLine("Check method param 1 - Serialize");
                        }

                        var @return = serializeLikeMethod.ReturnParameter;
                        if (@return.ParameterType != typeof(int))
                        {
                            Console.WriteLine("Check return param type - must be int");
                        }
                    }
                }
            }

            var parseMethod = type.Key.GetMethod("Parse", BindingFlags.Static | BindingFlags.Public,
                [typeof(Span<byte>), typeof(ASDUPacketHeader_2_2).MakeByRefType(), typeof(DateTime), typeof(IASDUNotification)]);
            if (parseMethod == null)
            {
                Console.WriteLine("Not Implemented - Parse");
            }
            else
            {
                var parameters = parseMethod.GetParameters();
                if (parameters.Length != 4)
                {
                    Console.WriteLine("Check method - Parse");
                }
                else
                {
                    var param0 = parameters[1];
                    var param1 = parameters[0];
                    var param2 = parameters[2];
                    var param3 = parameters[3];

                    if (!param0.ParameterType.IsByRef || param0.ParameterType.GetElementType() != typeof(ASDUPacketHeader_2_2))
                    {
                        Console.WriteLine("Check method param 0 - Parse");
                    }

                    if (param1.ParameterType != typeof(Span<byte>))
                    {
                        Console.WriteLine("Check method param 1 - Parse");
                    }

                    if (param2.ParameterType != typeof(DateTime))
                    {
                        Console.WriteLine("Check method param 2 - Parse");
                    }

                    if (param3.ParameterType != typeof(IASDUNotification))
                    {
                        Console.WriteLine("Check method param 3 - Parse");
                    }
                }
            }

            var descriptionMethod = type.Key.GetProperty("Description", BindingFlags.Static | BindingFlags.Public);
            if (descriptionMethod == null)
            {
                Console.WriteLine("Not Implemented - Description");
            }
        }
    }
}

