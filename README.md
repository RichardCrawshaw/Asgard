# Asgard
Basic implementation of serial and network comms for talking to MERG CBUS using the GridConnect protocol.

Asgard contains the basic code used in CbusLogger; the intention is to turn it into a nuget package so that it become easier for a C#.NET developer to develop applications that interface with MERG CBUS modules.

## Dependencies
Currently it has a depencency on NLog for logging and on Unity container. The latter is not currently used but it was envisioned that it would be.

Consideration is being made to use DI for both a logger and a lifetime manager.

### CBUS
The latest version of cbusdefsenums from MERG Devs is embedded; when (if) this is turned into a Nuget package in its own right Asgard will have a dependency on it. Until then any update to cbusdefsenums will need to be manually updated here and a new version of Asgard released.

## Issues to Fix
* OpCode STMOD claims a property of ServiceMode but supported by a single bit, while WCVS provides a property of ServiceMode with a whole byte. The on on STMOD needs to be changed to ServiceFlag, or similar.

## Planned Changes
* Remove dependency on external modules, specifically NLog and Unity Container.
* Add tests based on cbusdefs for opcodes.
* Add mechanism for defining which OpCodes are handles and which are ignored; in addition add static objects with standard OpCodes that can be used as templates that a user of the package can add OpCodes to:
  - Absolute minimum: e.g. QNN & PNN
  - Configuration: all config OpCodes
  - DCC: all DCC OpCodes
  - Accessories: all accessory OpCodes
  - Everything
  - Probably some others...
* Split the solution into separate projects:
  - Comms functionality: Asgard.Comms
  - OpCode and CBUS message functionality: Asgard.Data
  - The reading and processing template data: Asgard.Templating
* Add interfaces to OpCodes based on
  - properties: one interface per property type
  - group: one interface per group
* Add other implementations of ICbusMessage for 
  - Span\<T\>
  - Memory\<T\>

## Possible Changes Requiring Further Consideration
* Linking OpCodes to their request or response or error OpCode.
