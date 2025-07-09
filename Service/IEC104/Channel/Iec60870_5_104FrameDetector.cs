using PowerUnit.Service.IEC104.Abstract;
using PowerUnit.Service.IEC104.Types;

namespace PowerUnit.Service.IEC104.Channel;

public class Iec60870_5_104FrameDetector : IFrameDetector
{
    private INotifyPacket? _notified;

    private readonly List<byte> _data = [];

    void IFrameDetector.AssignNotified(INotifyPacket notified)
    {
        _notified = notified;
    }

    void IFrameDetector.Reset()
    {
        _data.Clear();
    }

    void IFrameDetector.TryGetFrame(byte[] data)
    {
        var startIndex = 0;
        var firstAdd = true;
        while (true)
        {
            // пакет не фрагментирован или первый
            if (_data.Count == 0)
            {
                startIndex = Array.IndexOf(data, APCI.START_PACKET, startIndex);
                if (startIndex < 0)
                {
                    return;
                }
                else
                {
                    // в пакете есть длина
                    if (startIndex + 1 < data.Length)
                    {
                        var length = data[startIndex + 1];
                        // в пакете все данные
                        if (startIndex + 1 + length < data.Length)
                        {
                            if (length >= APCI.MIN_LENGTH)
                                _notified?.NotifyPacketDetected(data[startIndex..(startIndex + 1 + 1 + length)]);
                            startIndex += 1 + 1 + length;
                        }
                        else
                        {
                            _data.AddRange(data.AsSpan(startIndex, data.Length - startIndex));
                            return;
                        }
                    }
                    else
                    {
                        _data.Add(APCI.START_PACKET);
                        return;
                    }
                }
            }
            // фрагментированный пакет нуждающийся в сборке
            else
            {
                if (firstAdd)
                {
                    _data.AddRange(data);
                    firstAdd = false;
                }

                startIndex = _data.IndexOf(APCI.START_PACKET, startIndex);
                if (startIndex < 0)
                {
                    _data.Clear();
                    return;
                }
                else
                {
                    // в пакете есть длина
                    if (startIndex + 1 < _data.Count)
                    {
                        var length = _data[startIndex + 1];
                        // в пакете все данные
                        if (startIndex + 1 + length < _data.Count)
                        {
                            if (length >= APCI.MIN_LENGTH)
                                _notified?.NotifyPacketDetected([.. _data[startIndex..(startIndex + 1 + 1 + length)]]);
                            startIndex += 1 + 1 + length;
                        }
                        else
                        {
                            _data.RemoveRange(0, startIndex);
                            return;
                        }
                    }
                    else
                    {
                        _data.RemoveRange(0, startIndex);
                        return;
                    }
                }
            }
        }
    }
}

