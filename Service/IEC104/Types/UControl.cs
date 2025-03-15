namespace PowerUnit.Service.IEC104.Types;

public enum UControl
{
    /// <summary>
    /// Старт передачи данных
    /// </summary>
    StartDtAct = 0b00000111, // активация
    StartDtCon = 0b00001011, // подтверждение

    /// <summary>
    /// Прекращение передачи данных
    /// </summary>
    StopDtAct = 0b00010011, // активация
    StopDtCon = 0b00100011, // подтверждение

    /// <summary>
    /// Тестовый блок
    /// </summary>
    TestFrAct = 0b01000011, // активация
    TestFrCon = 0b10000011, // подтверждение
}

