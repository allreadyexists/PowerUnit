namespace PowerUnit;

public enum COT
{
    DEFAULT,
    PERIODIC = 1, // периодически, циклически рег/сус
    BACKGRAUND_SCAN = 2, // фоновое сканирование back
    SPORADIC = 3, // спорадически spont
    INIT_MESSAGE = 4, // сообщение об инициализации init
    REQUEST_REQUESTED_DATA = 5, // запрос или запрашиваемые данные req
    ACTIVATE = 6, // активация act
    ACTIVATE_CONFIRMATION = 7, // подтверждение активации actcon
    DEACTIVATE = 8, // деактивация dead
    DEACTIVATE_CONFIRMATION = 9, // подтверждение деактивации deadcon
    ACTIVATE_COMPLETION = 10, // завершение активации actterm
    REMOTE_COMMANT_FEEDBACK = 11, // обратная информация, вызванная удаленной командой retrem
    LOCAL_COMMAND_FEEDBACK = 12, // обратная информация, вызванная местной командой retloc
    FILE_TRANSFER = 13, // передача файлов file

    // 14-19 резерв для дальнейших совместимых определений

    INTERROGATION_COMMON = 20, // ответ на опрос станции inrogen
    INTERROGATION_GROUP_1 = 21, // ответ на опрос группы 1 inrol
    INTERROGATION_GROUP_2 = 22, // ответ на опрос группы 2 inro2
    INTERROGATION_GROUP_3 = 23, // ответ на опрос группы 3 inro3
    INTERROGATION_GROUP_4 = 24, // ответ на опрос группы 4 inro4
    INTERROGATION_GROUP_5 = 25, // ответ на опрос группы 5 inro5
    INTERROGATION_GROUP_6 = 26, // ответ на опрос группы 6 inro6
    INTERROGATION_GROUP_7 = 27, // ответ на опрос группы 7 inro7
    INTERROGATION_GROUP_8 = 28, // ответ на опрос группы 8 inro8
    INTERROGATION_GROUP_9 = 29, // ответ на опрос группы 9 inro9
    INTERROGATION_GROUP_10 = 30, // ответ на опрос группы 10 inro10
    INTERROGATION_GROUP_11 = 31, // ответ на опрос группы 11 inro11
    INTERROGATION_GROUP_12 = 32, // ответ на опрос группы 12 inro12
    INTERROGATION_GROUP_13 = 33, // ответ на опрос группы 13 inro13
    INTERROGATION_GROUP_14 = 34, // ответ на опрос группы 14 inro14
    INTERROGATION_GROUP_15 = 35, // ответ на опрос группы 15 inro15
    INTERROGATION_GROUP_16 = 36, // ответ на опрос группы 16 inro16

    INTERROGATION_COUNTER_COMMON = 37, // ответ на общий запрос счетчиков reqcogen
    INTERROGATION_COUNTER_GROUP_1 = 38, // ответ на запрос группы счетчиков 1 reqco1
    INTERROGATION_COUNTER_GROUP_2 = 39, // ответ на запрос группы счетчиков 2 reqco2
    INTERROGATION_COUNTER_GROUP_3 = 40, // ответ на запрос группы счетчиков 3 reqco3
    INTERROGATION_COUNTER_GROUP_4 = 41, // ответ на запрос группы счетчиков 4 reqco4

    // 42-43 резерв для дальнейших совместимых определений

    UNKNOWN_TYPE_ID = 44, // неизвестный идентификатор типа
    UNKNOWN_TRANSFER_REASON = 45, // неизвестная причина передачи
    UNKNOWN_COMMON_ASDU_ADDRESS = 46, // неизвестный общий адрес ASDU
    UNKNOWN_INFORMATION_OBJECT_ADDRESS = 47, // неизвестный адрес объекта информации
}
