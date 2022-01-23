namespace Asgard.Data
{
    public interface ICbusOpCode
    {
		/// <summary>
		/// Gets the code.
		/// </summary>
		string Code { get; }

		/// <summary>
		/// Gets the number of data-bytes.
		/// </summary>
		int DataLength { get; }

		/// <summary>
		/// Gets the description.
		/// </summary>
		string Description { get; }

		/// <summary>
		/// Gets the group.
		/// </summary>
		OpCodeGroup Group { get; }

		/// <summary>
		/// Gets the name.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Gets the op-code number.
		/// </summary>
		byte Number { get; }

		/// <summary>
		/// Gets the op-code priority.
		/// </summary>
		int Priority { get; }

		/// <summary>
		/// Gets the underlying CBUS message.
		/// </summary>
		ICbusMessage Message { get; }
	}

    #region Parameter interfaces

    public interface IHasNodeNumber
    {
        ushort NodeNumber { get; set; }
    }

    public interface IHasSession
    {
        byte Session { get; set; }
    }

    public interface IHasDebugStatus
    {
        byte DebugStatus { get; set; }
    }

    public interface IHasExtendedOpCode
    {
        byte ExtendedOpCode { get; set; }
    }

    public interface IHasAddress
    {
        ushort Address { get; set; }
    }

    public interface IHasConsist
    {
        byte Consist { get; set; }
    }

    public interface IHasIndex
    {
        byte Index { get; set; }
    }

    public interface IHasAllocationCode
    {
        byte AllocationCode { get; set; }
    }

    public interface IHasSpeedMode
    {
        SpeedModeEnum SpeedMode { get; set; }
    }

    public interface IHasServiceMode
    {
        ServiceModeEnum ServiceMode { get; set; }
    }

    public interface IHasSoundMode
    {
        bool SoundMode { get; set; }
    }

    public interface IHasSpeedDir
    {
        byte SpeedDir { get; set; }
    }

    public interface IHasLights
    {
        bool Lights { get; set; }
    }

    public interface IHasDirection
    {
        bool Direction { get; set; }
    }

    public interface IHasEngineState
    {
        EngineStateEnum EngineState { get; set; }
    }

    public interface IHasFunctionNumber
    {
        byte FunctionNumber { get; set; }
    }

    public interface IHasSessionStatus
    {
        SessionStatusEnum SessionStatus { get; set; }
    }

    public interface IHasDeviceNumber
    {
        ushort DeviceNumber { get; set; }
    }

    public interface IHasData1
    {
        byte Data1 { get; set; }
    }

    public interface IHasFunctionRange
    {
        FunctionRangeEnum FunctionRange { get; set; }
    }

    public interface IHasValue
    {
        byte Value { get; set; }
    }

    public interface IHasSessionFlags
    {
        SessionFlagsEnum SessionFlags { get; set; }
    }

    public interface IHasData2
    {
        byte Data2 { get; set; }
    }

    public interface IHasDccErrorCode
    {
        DccErrorCodeEnum DccErrorCode { get; set; }
    }

    public interface IHasAccErrorCode
    {
        AccErrorCodeEnum AccErrorCode { get; set; }
    }

    public interface IHasNVIndex
    {
        byte NVIndex { get; set; }
    }

    public interface IHasENIndex
    {
        byte ENIndex { get; set; }
    }

    public interface IHasParamIndex
    {
        byte ParamIndex { get; set; }
    }

    public interface IHasCAN_ID
    {
        byte CAN_ID { get; set; }
    }

    public interface IHasRepetitions
    {
        byte Repetitions { get; set; }
    }

    public interface IHasData3
    {
        byte Data2 { get; set; }
    }

    public interface IHasCV
    {
        ushort CV { get; set; }
    }

    public interface IHasMode
    {
        byte Mode { get; set; }
    }

    public interface IHasEventNumber
    {
        ushort EventNumber { get; set; }
    }

    public interface IHasEVIndex
    {
        byte EVIndex { get; set; }
    }

    public interface IHasData4
    {
        byte Data4 { get; set; }
    }

    public interface IHasManufId
    {
        byte ManufId { get; set; }
    }

    public interface IHasModuleId
    {
        byte ModuleId { get; set; }
    }

    public interface IHasNodeFlags
    {
        NodeFlagsEnum NodeFlags { get; set; }
    }

    public interface IHasData5
    {
        byte Data5 { get; set; }
    }

    public interface IHasMinutes
    {
        byte Minutes { get; set; }
    }

    public interface IHasHours
    {
        byte Hours { get; set; }
    }

    public interface IHasWeekday
    {
        WeekdayEnum Weekday { get; set; }
    }

    public interface IHasMonth
    {
        MonthEnum Month { get; set; }
    }

    public interface IHasDiv
    {
        byte Div { get; set; }
    }

    public interface IHasMonthDay
    {
        byte MonthDay { get; set; }
    }

    public interface IHasTemperature
    {
        byte Temperature { get; set; }
    }

    public interface IHasData6
    {
        byte Data6 { get; set; }
    }

    public interface IHasFn1
    {
        byte Fn1 { get; set; }
    }

    public interface IHasFn2
    {
        byte Fn2 { get; set; }
    }

    public interface IHasFn3
    {
        byte Fn3 { get; set; }
    }

    public interface IHasChar1
    {
        char Char1 { get; set; }
    }

    public interface IHasChar2
    {
        char Char2 { get; set; }
    }

    public interface IHasChar3
    {
        char Char3 { get; set; }
    }

    public interface IHasChar4
    {
        char Char4 { get; set; }
    }

    public interface IHasChar5
    {
        char Char5 { get; set; }
    }

    public interface IHasChar6
    {
        char Char6 { get; set; }
    }

    public interface IHasChar7
    {
        char Char7 { get; set; }
    }

    public interface IHasCSNumber
    {
        byte CSNumber { get; set; }
    }

    public interface IHasCSFlags
    {
        CSFlagsEnum CSFlags { get; set; }
    }

    public interface IHasMajor
    {
        byte Major { get; set; }
    }

    public interface IHasMinor
    {
        byte Minor { get; set; }
    }

    public interface IHasBuild
    {
        byte Build { get; set; }
    }

    public interface IHasParam1
    {
        byte Param1 { get; set; }
    }

    public interface IHasParam2
    {
        byte Param2 { get; set; }
    }

    public interface IHasParam3
    {
        byte Param3 { get; set; }
    }

    public interface IHasParam4
    {
        byte Param4 { get; set; }
    }

    public interface IHasParam5
    {
        byte Param5 { get; set; }
    }

    public interface IHasParam6
    {
        byte Param6 { get; set; }
    }

    public interface IHasParam7
    {
        byte Param7 { get; set; }
    }

    public interface IHasEventData
    {
        uint EventData { get; set; }
    }

    #endregion
}
