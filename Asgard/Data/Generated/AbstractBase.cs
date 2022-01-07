﻿using System;

/*	This file is automatically generated by a T4 template from a data file.
	cbus-4.0-Rev-8d-Guide-6b-opcodes
	It was last generated at 01/07/2022 17:43:03.
	Any changes made manually will be lost when the file is regenerated.
*/

namespace Asgard.Data
{
	#region Licence

/*
 *	This work is licensed under the:
 *	    Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License.
 *	To view a copy of this license, visit:
 *	    http://creativecommons.org/licenses/by-nc-sa/4.0/
 *	or send a letter to Creative Commons, PO Box 1866, Mountain View, CA 94042, USA.
 *	
 *	License summary:
 *	  You are free to:
 *	    Share, copy and redistribute the material in any medium or format
 *	    Adapt, remix, transform, and build upon the material
 *	
 *	  The licensor cannot revoke these freedoms as long as you follow the license terms.
 *	
 *	  Attribution : You must give appropriate credit, provide a link to the license,
 *	                 and indicate if changes were made. You may do so in any reasonable manner,
 *	                 but not in any way that suggests the licensor endorses you or your use.
 *	
 *	  NonCommercial : You may not use the material for commercial purposes. **(see note below)
 *	
 *	  ShareAlike : If you remix, transform, or build upon the material, you must distribute
 *	                your contributions under the same license as the original.
 *	
 *	  No additional restrictions : You may not apply legal terms or technological measures that
 *	                                legally restrict others from doing anything the license permits.
 *	
 *	 ** For commercial use, please contact the original copyright holder(s) to agree licensing terms
 *	
 *	  This software is distributed in the hope that it will be useful, but WITHOUT ANY
 *	  WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE

 *	*/
	#endregion

	#region History

/*	Date		Author
 *	2021-11-06	Richard Crawshaw	Original from Developers' Guide for CBUS version 6b

 *	*/

	#endregion

	#region Common abstract base class

	/// <summary>
	/// Abstract base class for all OpCodes.
	/// </summary>
	public abstract partial class OpCodeData :
		ICbusOpCode
	{
		public static OpCodeData Create(ICbusMessage message)
		{
			return message[0] switch
			{
                0x00 => new GeneralAcknowledgement(message),
                0x01 => new GeneralNoAcknowledgement(message),
                0x02 => new BusHalt(message),
                0x03 => new BusOn(message),
                0x04 => new TrackOff(message),
                0x05 => new TrackOn(message),
                0x06 => new EmergencyStop(message),
                0x07 => new SystemReset(message),
                0x08 => new RequestTrackOff(message),
                0x09 => new RequestTrackOn(message),
                0x0A => new RequestEmergencyStopAll(message),
                0x0C => new RequestCommandStationStatus(message),
                0x0D => new QueryNodeNumber(message),
                0x10 => new RequestNodeParameters(message),
                0x11 => new RequestModuleName(message),
                0x21 => new ReleaseEngine(message),
                0x22 => new QueryEngine(message),
                0x23 => new SessionKeepAlive(message),
                0x30 => new DebugWithOneDataByte(message),
                0x3F => new ExtendedOpcodeWithNoAdditionalBytes(message),
                0x40 => new RequestEngineSession(message),
                0x41 => new QueryConsist(message),
                0x42 => new SetNodeNumber(message),
                0x43 => new AllocateLocoToActivity(message),
                0x44 => new SetCabSessionMode(message),
                0x45 => new ConsistEngine(message),
                0x46 => new RemoveEngineFromConsist(message),
                0x47 => new SetEngineSpeedAndDirection(message),
                0x48 => new SetEngineFlags(message),
                0x49 => new SetEngineFunctionOn(message),
                0x4A => new SetEngineFunctionOff(message),
                0x4C => new ServiceModeStatus(message),
                0x50 => new RequestNodeNumber(message),
                0x51 => new NodeNumberRelease(message),
                0x52 => new NodeNumberAcknowledge(message),
                0x53 => new SetNodeIntoLearnMode(message),
                0x54 => new ReleaseNodeFromLearnMode(message),
                0x55 => new ClearAllEventsFromANode(message),
                0x56 => new ReadNumberOfEventsAvailableInANode(message),
                0x57 => new ReadBackAllStoredEventsInANode(message),
                0x58 => new RequestToReadNumberOfStoredEvents(message),
                0x59 => new WriteAcknowledge(message),
                0x5A => new RequestNodeDataEvent(message),
                0x5B => new RequestDeviceDataShortMode(message),
                0x5C => new PutNodeIntoBootloadMode(message),
                0x5D => new ForceASelfEnumerationCycleForUseWithCan(message),
                0x5F => new ExtendedOpcodeWith1AdditionalByte(message),
                0x60 => new SetEngineFunctions(message),
                0x61 => new GetEngineSession(message),
                0x63 => new CommandStationErrorReport(message),
                0x6F => new ErrorMessagesFromNodesDuringConfiguration(message),
                0x70 => new EventSpaceLeftReplyFromNode(message),
                0x71 => new RequestReadOfANodeVariable(message),
                0x72 => new RequestReadOfStoredEventsByEventIndex(message),
                0x73 => new RequestReadOfANodeParameterByIndex(message),
                0x74 => new NumberOfEventsStoredInNode(message),
                0x75 => new SetACan_idInExistingFlimNode(message),
                0x7F => new ExtendedOpcodeWith2AdditionalBytes(message),
                0x80 => new Request3ByteDccPacket(message),
                0x82 => new WriteCvByteInOpsMode(message),
                0x83 => new WriteCbBitInOpsMode(message),
                0x84 => new ReadCv(message),
                0x85 => new ReportCv(message),
                0x90 => new AccessoryOn(message),
                0x91 => new AccessoryOff(message),
                0x92 => new AccessoryRequestEvent(message),
                0x93 => new AccessoryOnResponseEvent(message),
                0x94 => new AccessoryOffResponseEvent(message),
                0x95 => new UnlearnAnEventInLearnMode(message),
                0x96 => new SetANodeVariable(message),
                0x97 => new ResponseToARequestForANodeVariableValue(message),
                0x98 => new AccessoryShortOn(message),
                0x99 => new AccessoryShortOff(message),
                0x9A => new AccessoryShortRequestEvent(message),
                0x9B => new ResponseToRequestForIndividualNodeParameter(message),
                0x9C => new RequestForReadOfAnEventVariable(message),
                0x9D => new AccessoryShortResponseOn(message),
                0x9E => new AccessoryShortResponseOff(message),
                0x9F => new ExtendedOpcodeWith3AdditionalBytes(message),
                0xA0 => new Request4ByteDccPacket(message),
                0xA2 => new WriteCvInServiceMode(message),
                0xB0 => new AccessoryOn1(message),
                0xB1 => new AccessoryOff1(message),
                0xB2 => new ReadEventVariableInLearnMode(message),
                0xB3 => new AccessoryOnResponseEvent1(message),
                0xB4 => new AccessoryOffResponseEvent1(message),
                0xB5 => new ResponseToRequestForReadOfEvValue(message),
                0xB6 => new ResponseToQueryNode(message),
                0xB8 => new AccessoryShortOn1(message),
                0xB9 => new AccessoryShortOff1(message),
                0xBD => new AccessoryShortResponseOn1(message),
                0xBE => new AccessoryShortResponseOff1(message),
                0xBF => new ExtendedOpcodeWith4DataBytes(message),
                0xC0 => new Request5ByteDccPacket(message),
                0xC1 => new WriteCvByteInOpsModeByAddress(message),
                0xCF => new FastClock(message),
                0xD0 => new AccessoryOn2(message),
                0xD1 => new AccessoryOff2(message),
                0xD2 => new TeachAnEventInLearnMode(message),
                0xD3 => new ResponseToARequestForAnEvValueInANodeInLearnMode(message),
                0xD4 => new AccessoryOnResponseEvent2(message),
                0xD5 => new AccessoryOffResponseEvent2(message),
                0xD8 => new AccessoryShortOn2(message),
                0xD9 => new AccessoryShortOff2(message),
                0xDD => new AccessoryShortResponseOn2(message),
                0xDE => new AccessoryShortResponseOff2(message),
                0xDF => new ExtendedOpcodeWith5DataBytes(message),
                0xE0 => new Request6ByteDccPacket(message),
                0xE1 => new EngineReport(message),
                0xE2 => new ResponseToRequestForNodeNameString(message),
                0xE3 => new CommandStationStatusReport(message),
                0xEF => new ResponseToRequestForNodeParameters(message),
                0xF0 => new AccessoryOn3(message),
                0xF1 => new AccessoryOff3(message),
                0xF2 => new ResponseToRequestToReadNodeEvents(message),
                0xF3 => new AccessoryOnResponseEvent3(message),
                0xF4 => new AccessoryOffResponseEvent3(message),
                0xF5 => new TeachAnEventInLearnModeUsingEventIndexing(message),
                0xF6 => new AccessoryNodeDataEvent(message),
                0xF7 => new AccessoryNodeDataResponse(message),
                0xF8 => new AccessoryShortOn3(message),
                0xF9 => new AccessoryShortOff3(message),
                0xFA => new DeviceDataEventShortMode(message),
                0xFB => new DeviceDataResponseShortMode(message),
                0xFD => new AccessoryShortResponseOn3(message),
                0xFE => new AccessoryShortResponseOff3(message),
                0xFF => new ExtendedOpcodeWith6DaBytes(message),
				_ => null,
			};
		}

		public static OpCodeData Create(string opcodeName)
		{
			return opcodeName.ToUpper() switch
			{
				"ACK" => new GeneralAcknowledgement(),
				"NAK" => new GeneralNoAcknowledgement(),
				"HLT" => new BusHalt(),
				"BON" => new BusOn(),
				"TOF" => new TrackOff(),
				"TON" => new TrackOn(),
				"ESTOP" => new EmergencyStop(),
				"ARST" => new SystemReset(),
				"RTOF" => new RequestTrackOff(),
				"RTON" => new RequestTrackOn(),
				"RESTP" => new RequestEmergencyStopAll(),
				"RSTAT" => new RequestCommandStationStatus(),
				"QNN" => new QueryNodeNumber(),
				"RQNP" => new RequestNodeParameters(),
				"RQMN" => new RequestModuleName(),
				"KLOC" => new ReleaseEngine(),
				"QLOC" => new QueryEngine(),
				"DKEEP" => new SessionKeepAlive(),
				"DBG1" => new DebugWithOneDataByte(),
				"EXTC" => new ExtendedOpcodeWithNoAdditionalBytes(),
				"RLOC" => new RequestEngineSession(),
				"QCON" => new QueryConsist(),
				"SNN" => new SetNodeNumber(),
				"ALOC" => new AllocateLocoToActivity(),
				"STMOD" => new SetCabSessionMode(),
				"PCON" => new ConsistEngine(),
				"KCON" => new RemoveEngineFromConsist(),
				"DSPD" => new SetEngineSpeedAndDirection(),
				"DFLG" => new SetEngineFlags(),
				"DFNON" => new SetEngineFunctionOn(),
				"DFNOF" => new SetEngineFunctionOff(),
				"SSTAT" => new ServiceModeStatus(),
				"RQNN" => new RequestNodeNumber(),
				"NNREL" => new NodeNumberRelease(),
				"NNACK" => new NodeNumberAcknowledge(),
				"NNLRN" => new SetNodeIntoLearnMode(),
				"NNULN" => new ReleaseNodeFromLearnMode(),
				"NNCLR" => new ClearAllEventsFromANode(),
				"NNEVN" => new ReadNumberOfEventsAvailableInANode(),
				"NERD" => new ReadBackAllStoredEventsInANode(),
				"RQEVN" => new RequestToReadNumberOfStoredEvents(),
				"WRACK" => new WriteAcknowledge(),
				"RQDAT" => new RequestNodeDataEvent(),
				"RQDDS" => new RequestDeviceDataShortMode(),
				"BOOTM" => new PutNodeIntoBootloadMode(),
				"ENUM" => new ForceASelfEnumerationCycleForUseWithCan(),
				"EXTC1" => new ExtendedOpcodeWith1AdditionalByte(),
				"DFUN" => new SetEngineFunctions(),
				"GLOC" => new GetEngineSession(),
				"ERR" => new CommandStationErrorReport(),
				"CMDERR" => new ErrorMessagesFromNodesDuringConfiguration(),
				"EVNLF" => new EventSpaceLeftReplyFromNode(),
				"NVRD" => new RequestReadOfANodeVariable(),
				"NENRD" => new RequestReadOfStoredEventsByEventIndex(),
				"RQNPN" => new RequestReadOfANodeParameterByIndex(),
				"NUMEV" => new NumberOfEventsStoredInNode(),
				"CANID" => new SetACan_idInExistingFlimNode(),
				"EXTC2" => new ExtendedOpcodeWith2AdditionalBytes(),
				"RDCC3" => new Request3ByteDccPacket(),
				"WCVO" => new WriteCvByteInOpsMode(),
				"WCVB" => new WriteCbBitInOpsMode(),
				"QCVS" => new ReadCv(),
				"PCVS" => new ReportCv(),
				"ACON" => new AccessoryOn(),
				"ACOF" => new AccessoryOff(),
				"AREQ" => new AccessoryRequestEvent(),
				"ARON" => new AccessoryOnResponseEvent(),
				"AROF" => new AccessoryOffResponseEvent(),
				"EVULN" => new UnlearnAnEventInLearnMode(),
				"NVSET" => new SetANodeVariable(),
				"NVANS" => new ResponseToARequestForANodeVariableValue(),
				"ASON" => new AccessoryShortOn(),
				"ASOF" => new AccessoryShortOff(),
				"ASRQ" => new AccessoryShortRequestEvent(),
				"PARAN" => new ResponseToRequestForIndividualNodeParameter(),
				"REVAL" => new RequestForReadOfAnEventVariable(),
				"ARSON" => new AccessoryShortResponseOn(),
				"ARSOF" => new AccessoryShortResponseOff(),
				"EXTC3" => new ExtendedOpcodeWith3AdditionalBytes(),
				"RDCC4" => new Request4ByteDccPacket(),
				"WCVS" => new WriteCvInServiceMode(),
				"ACON1" => new AccessoryOn1(),
				"ACOF1" => new AccessoryOff1(),
				"REQEV" => new ReadEventVariableInLearnMode(),
				"ARON1" => new AccessoryOnResponseEvent1(),
				"AROF1" => new AccessoryOffResponseEvent1(),
				"NEVAL" => new ResponseToRequestForReadOfEvValue(),
				"PNN" => new ResponseToQueryNode(),
				"ASON1" => new AccessoryShortOn1(),
				"ASOF1" => new AccessoryShortOff1(),
				"ARSON1" => new AccessoryShortResponseOn1(),
				"ARSOF1" => new AccessoryShortResponseOff1(),
				"EXTC4" => new ExtendedOpcodeWith4DataBytes(),
				"RDCC5" => new Request5ByteDccPacket(),
				"WCVOA" => new WriteCvByteInOpsModeByAddress(),
				"FCLK" => new FastClock(),
				"ACON2" => new AccessoryOn2(),
				"ACOF2" => new AccessoryOff2(),
				"EVLRN" => new TeachAnEventInLearnMode(),
				"EVANS" => new ResponseToARequestForAnEvValueInANodeInLearnMode(),
				"ARON2" => new AccessoryOnResponseEvent2(),
				"AROF2" => new AccessoryOffResponseEvent2(),
				"ASON2" => new AccessoryShortOn2(),
				"ASOF2" => new AccessoryShortOff2(),
				"ARSON2" => new AccessoryShortResponseOn2(),
				"ARSOF2" => new AccessoryShortResponseOff2(),
				"EXTC5" => new ExtendedOpcodeWith5DataBytes(),
				"RDCC6" => new Request6ByteDccPacket(),
				"PLOC" => new EngineReport(),
				"NAME" => new ResponseToRequestForNodeNameString(),
				"STAT" => new CommandStationStatusReport(),
				"PARAMS" => new ResponseToRequestForNodeParameters(),
				"ACON3" => new AccessoryOn3(),
				"ACOF3" => new AccessoryOff3(),
				"ENRSP" => new ResponseToRequestToReadNodeEvents(),
				"ARON3" => new AccessoryOnResponseEvent3(),
				"AROF3" => new AccessoryOffResponseEvent3(),
				"EVLRNI" => new TeachAnEventInLearnModeUsingEventIndexing(),
				"ACDAT" => new AccessoryNodeDataEvent(),
				"ARDAT" => new AccessoryNodeDataResponse(),
				"ASON3" => new AccessoryShortOn3(),
				"ASOF3" => new AccessoryShortOff3(),
				"DDES" => new DeviceDataEventShortMode(),
				"DDRS" => new DeviceDataResponseShortMode(),
				"ARSON3" => new AccessoryShortResponseOn3(),
				"ARSOF3" => new AccessoryShortResponseOff3(),
				"EXTC6" => new ExtendedOpcodeWith6DaBytes(),
				_ => null,
			};
		}
	}

	#endregion

	#region Abstract base class for OpCodes with 0 data bytes

	/// <summary>
	/// Abstract base class for OpCodes with 0 data bytes.
	/// </summary>
	public abstract partial class OpCodeData0 : OpCodeData
	{
		public const int DATA_LENGTH = 0;

		public override sealed int DataLength => DATA_LENGTH;

		protected OpCodeData0(ICbusMessage cbusMessage) : base(cbusMessage) { }
	}
	
	#endregion

	#region Abstract base class for OpCodes with 1 data bytes

	/// <summary>
	/// Abstract base class for OpCodes with 1 data bytes.
	/// </summary>
	public abstract partial class OpCodeData1 : OpCodeData
	{
		public const int DATA_LENGTH = 1;

		public override sealed int DataLength => DATA_LENGTH;

		protected OpCodeData1(ICbusMessage cbusMessage) : base(cbusMessage) { }
	}
	
	#endregion

	#region Abstract base class for OpCodes with 2 data bytes

	/// <summary>
	/// Abstract base class for OpCodes with 2 data bytes.
	/// </summary>
	public abstract partial class OpCodeData2 : OpCodeData
	{
		public const int DATA_LENGTH = 2;

		public override sealed int DataLength => DATA_LENGTH;

		protected OpCodeData2(ICbusMessage cbusMessage) : base(cbusMessage) { }
	}
	
	#endregion

	#region Abstract base class for OpCodes with 3 data bytes

	/// <summary>
	/// Abstract base class for OpCodes with 3 data bytes.
	/// </summary>
	public abstract partial class OpCodeData3 : OpCodeData
	{
		public const int DATA_LENGTH = 3;

		public override sealed int DataLength => DATA_LENGTH;

		protected OpCodeData3(ICbusMessage cbusMessage) : base(cbusMessage) { }
	}
	
	#endregion

	#region Abstract base class for OpCodes with 4 data bytes

	/// <summary>
	/// Abstract base class for OpCodes with 4 data bytes.
	/// </summary>
	public abstract partial class OpCodeData4 : OpCodeData
	{
		public const int DATA_LENGTH = 4;

		public override sealed int DataLength => DATA_LENGTH;

		protected OpCodeData4(ICbusMessage cbusMessage) : base(cbusMessage) { }
	}
	
	#endregion

	#region Abstract base class for OpCodes with 5 data bytes

	/// <summary>
	/// Abstract base class for OpCodes with 5 data bytes.
	/// </summary>
	public abstract partial class OpCodeData5 : OpCodeData
	{
		public const int DATA_LENGTH = 5;

		public override sealed int DataLength => DATA_LENGTH;

		protected OpCodeData5(ICbusMessage cbusMessage) : base(cbusMessage) { }
	}
	
	#endregion

	#region Abstract base class for OpCodes with 6 data bytes

	/// <summary>
	/// Abstract base class for OpCodes with 6 data bytes.
	/// </summary>
	public abstract partial class OpCodeData6 : OpCodeData
	{
		public const int DATA_LENGTH = 6;

		public override sealed int DataLength => DATA_LENGTH;

		protected OpCodeData6(ICbusMessage cbusMessage) : base(cbusMessage) { }
	}
	
	#endregion

	#region Abstract base class for OpCodes with 7 data bytes

	/// <summary>
	/// Abstract base class for OpCodes with 7 data bytes.
	/// </summary>
	public abstract partial class OpCodeData7 : OpCodeData
	{
		public const int DATA_LENGTH = 7;

		public override sealed int DataLength => DATA_LENGTH;

		protected OpCodeData7(ICbusMessage cbusMessage) : base(cbusMessage) { }
	}
	
	#endregion

}
