using PowerUnit.Service.IEC104.Abstract;
using PowerUnit.Service.IEC104.Types;
using PowerUnit.Service.IEC104.Types.Asdu;

using System.Buffers;

namespace PowerUnit;

public sealed partial class Iec60870_5_104ServerApplicationLayer
{
    private void Snapshot(IEnumerable<BaseValue> values)
    {
        foreach (var value in values)
        {
            if (_mapping.TryGetValue((value.EquipmentId, value.ParameterId), out var v))
                _values.AddOrUpdate((ushort)v.Address, address => (v.AsduType, value), (address, oldValue) => (v.AsduType, value));
        }
    }

    private void Stream(byte[] buffer, IEnumerable<BaseValue> values)
    {
        var M_SP_TB_1_SingleArray = ArrayPool<M_SP_TB_1_Single>.Shared.Rent(M_SP_TB_1_Single.MaxItemCount);
        var M_DP_TB_1_SingleArray = ArrayPool<M_DP_TB_1_Single>.Shared.Rent(M_DP_TB_1_Single.MaxItemCount);
        var M_ME_TF_1_SingleArray = ArrayPool<M_ME_TF_1_Single>.Shared.Rent(M_ME_TF_1_Single.MaxItemCount);

        int length = 0;
        int countTotal = 0;
        byte count = 0;

        byte currentType = 0;
        var currentTypeMaxCount = 0;

        var isInit = false;

        try
        {
            foreach (var value in values)
            {
                if (_mapping.TryGetValue((value.EquipmentId, value.ParameterId), out var v))
                {
                    if (v!.AsduType == (byte)AsduType.M_SP_TB_1 ||
                        v!.AsduType == (byte)AsduType.M_DP_TB_1 ||
                        v!.AsduType == (byte)AsduType.M_ME_TF_1)
                    {
                        if (!isInit)
                        {
                            currentType = v!.AsduType;
                            isInit = true;
                            if (v!.AsduType == (byte)AsduType.M_SP_TB_1)
                            {
                                currentTypeMaxCount = M_SP_TB_1_Single.MaxItemCount;
                                M_SP_TB_1_SingleArray[count++] = new M_SP_TB_1_Single((ushort)v.Address,
                                    ((DiscretValue)value).Value ? SIQ_Value.On : SIQ_Value.Off, 0,
                                    ((DiscretValue)value).ValueDt!.Value, 0);
                            }
                            else if (v!.AsduType == (byte)AsduType.M_DP_TB_1)
                            {
                                currentTypeMaxCount = M_DP_TB_1_Single.MaxItemCount;
                                M_DP_TB_1_SingleArray[count++] = new M_DP_TB_1_Single((ushort)v.Address,
                                    ((DiscretValue)value).Value ? DIQ_Value.On : DIQ_Value.Off, 0,
                                    ((DiscretValue)value).ValueDt!.Value, 0);
                            }
                            else if (v!.AsduType == (byte)AsduType.M_ME_TF_1)
                            {
                                currentTypeMaxCount = M_ME_TF_1_Single.MaxItemCount;
                                M_ME_TF_1_SingleArray[count++] = new M_ME_TF_1_Single((ushort)v.Address,
                                    ((AnalogValue)value).Value, 0,
                                    ((AnalogValue)value).ValueDt!.Value, 0);
                            }
                        }
                        else
                        {
                            if (currentType != v!.AsduType || count == currentTypeMaxCount)
                            {
                                AsduPacketHeader_2_2 headerReq;
                                //= new AsduPacketHeader_2_2((AsduType)currentType, SQ.Single, count, (COT)qoi, initAddr: header.InitAddr,
                                //                commonAddrAsdu: _applicationLayerOption.CommonASDUAddress);
                                if (currentType == (byte)AsduType.M_SP_TB_1)
                                {
                                    headerReq = new AsduPacketHeader_2_2(AsduType.M_SP_TB_1, SQ.Single, count, COT.SPORADIC, 0/*???*/,
                                        commonAddrAsdu: _applicationLayerOption.CommonASDUAddress);
                                    length = M_SP_TB_1_Single.Serialize(buffer, in headerReq, M_SP_TB_1_SingleArray, count);
                                }
                                else if (currentType == (byte)AsduType.M_DP_TB_1)
                                {
                                    headerReq = new AsduPacketHeader_2_2(AsduType.M_DP_TB_1, SQ.Single, count, COT.SPORADIC, 0/*???*/,
                                        commonAddrAsdu: _applicationLayerOption.CommonASDUAddress);
                                    length = M_DP_TB_1_Single.Serialize(buffer, in headerReq, M_DP_TB_1_SingleArray, count);
                                }
                                else if (currentType == (byte)AsduType.M_ME_TF_1)
                                {
                                    headerReq = new AsduPacketHeader_2_2(AsduType.M_ME_TF_1, SQ.Single, count, COT.SPORADIC, 0/*???*/,
                                        commonAddrAsdu: _applicationLayerOption.CommonASDUAddress);
                                    length = M_ME_TF_1_Single.Serialize(buffer, in headerReq, M_ME_TF_1_SingleArray, count);
                                }

                                _packetSender!.Send(buffer[..length], ChannelLayerPacketPriority.Low);

                                if (v!.AsduType == (byte)AsduType.M_SP_TB_1)
                                    currentTypeMaxCount = M_SP_TB_1_Single.MaxItemCount;
                                else if (v!.AsduType == (byte)AsduType.M_DP_TB_1)
                                    currentTypeMaxCount = M_DP_TB_1_Single.MaxItemCount;
                                else if (v!.AsduType == (byte)AsduType.M_ME_TF_1)
                                    currentTypeMaxCount = M_ME_TF_1_Single.MaxItemCount;

                                currentType = v!.AsduType;
                                count = 0;
                            }

                            if (v!.AsduType == (byte)AsduType.M_SP_TB_1)
                            {
                                M_SP_TB_1_SingleArray[count++] = new M_SP_TB_1_Single((ushort)v.Address,
                                    ((DiscretValue)value).Value ? SIQ_Value.On : SIQ_Value.Off, 0,
                                    ((DiscretValue)value).ValueDt!.Value, 0);
                            }
                            else if (v!.AsduType == (byte)AsduType.M_DP_TB_1)
                            {
                                M_DP_TB_1_SingleArray[count++] = new M_DP_TB_1_Single((ushort)v.Address,
                                    ((DiscretValue)value).Value ? DIQ_Value.On : DIQ_Value.Off, 0,
                                    ((DiscretValue)value).ValueDt!.Value, 0);
                            }
                            else if (v!.AsduType == (byte)AsduType.M_ME_TF_1)
                            {
                                M_ME_TF_1_SingleArray[count++] = new M_ME_TF_1_Single((ushort)v.Address,
                                    ((AnalogValue)value).Value, 0,
                                    ((AnalogValue)value).ValueDt!.Value, 0);
                            }
                        }
                    }
                }
            }

            if (count > 0)
            {
                AsduPacketHeader_2_2 headerReq;
                //= new AsduPacketHeader_2_2((AsduType)currentType, SQ.Single, count, (COT)qoi, initAddr: header.InitAddr,
                //                        commonAddrAsdu: _applicationLayerOption.CommonASDUAddress);
                if (currentType == (byte)AsduType.M_SP_TB_1)
                {
                    headerReq = new AsduPacketHeader_2_2(AsduType.M_SP_TB_1, SQ.Single, count, COT.SPORADIC, 0/*???*/,
                        commonAddrAsdu: _applicationLayerOption.CommonASDUAddress);
                    length = M_SP_TB_1_Single.Serialize(buffer, in headerReq, M_SP_TB_1_SingleArray, count);
                    currentTypeMaxCount = M_SP_TB_1_Single.MaxItemCount;
                }
                else if (currentType == (byte)AsduType.M_DP_TB_1)
                {
                    headerReq = new AsduPacketHeader_2_2(AsduType.M_DP_TB_1, SQ.Single, count, COT.SPORADIC, 0/*???*/,
                        commonAddrAsdu: _applicationLayerOption.CommonASDUAddress);
                    length = M_DP_TB_1_Single.Serialize(buffer, in headerReq, M_DP_TB_1_SingleArray, count);
                    currentTypeMaxCount = M_DP_TB_1_Single.MaxItemCount;
                }
                else if (currentType == (byte)AsduType.M_ME_TF_1)
                {
                    headerReq = new AsduPacketHeader_2_2(AsduType.M_ME_TF_1, SQ.Single, count, COT.SPORADIC, 0/*???*/,
                        commonAddrAsdu: _applicationLayerOption.CommonASDUAddress);
                    length = M_ME_TF_1_Single.Serialize(buffer, in headerReq, M_ME_TF_1_SingleArray, count);
                    currentTypeMaxCount = M_ME_TF_1_Single.MaxItemCount;
                }

                _packetSender!.Send(buffer[..length], ChannelLayerPacketPriority.Low);
            }
        }
        finally
        {
            ArrayPool<M_SP_TB_1_Single>.Shared.Return(M_SP_TB_1_SingleArray);
            ArrayPool<M_DP_TB_1_Single>.Shared.Return(M_DP_TB_1_SingleArray);
            ArrayPool<M_ME_TF_1_Single>.Shared.Return(M_ME_TF_1_SingleArray);
        }

        //var M_ME_TF_1_SingleArray = ArrayPool<M_ME_TF_1_Single>.Shared.Rent(M_ME_TF_1_Single.MaxItemCount);
        //try
        //{
        //    byte count = 0;
        //    foreach (var value in values)
        //    {
        //        if (_mapping.TryGetValue((value.EquipmentId, value.ParameterId), out var v))
        //        {
        //            switch (value)
        //            {
        //                case AnalogValue analogValue:
        //                    if (v.AsduType == (byte)AsduType.M_ME_TF_1)
        //                    {
        //                        M_ME_TF_1_SingleArray[count++] = new M_ME_TF_1_Single((ushort)v.Address, analogValue.Value, 0, analogValue.ValueDt ?? _timeProvider.GetUtcNow().DateTime, TimeStatus.OK);
        //                    }

        //                    break;
        //            }
        //        }
        //    }

        //    if (count > 0)
        //    {
        //        _ = SendInRentBuffer(buffer =>
        //        {
        //            var headerReq = new AsduPacketHeader_2_2(AsduType.M_ME_TF_1, SQ.Single, count, COT.SPORADIC, 0/*???*/,
        //                                    commonAddrAsdu: _applicationLayerOption.CommonASDUAddress);
        //            var length = M_ME_TF_1_Single.Serialize(buffer, in headerReq, M_ME_TF_1_SingleArray, count);
        //            _packetSender!.Send(buffer[..length], ChannelLayerPacketPriority.Low);
        //            return Task.CompletedTask;
        //        });
        //    }
        //}
        //finally
        //{
        //    ArrayPool<M_ME_TF_1_Single>.Shared.Return(M_ME_TF_1_SingleArray);
        //}
    }

    private bool ValueFilter(BaseValue value)
    {
        return _mapping.ContainsKey((value.EquipmentId, value.ParameterId));
    }
}

