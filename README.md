# Asgard
Basic implementation of serial and network comms for talking to MERG CBUS using the GridConnect protocol.

Asgard contains the basic code used in CbusLogger; the intention is to turn it into a nuget package so that it become easier for a C#.NET developer to develop applications that interface with MERG CBUS modules.

## Dependencies
There are no current dependencies other than standard Microsoft libraries. There may be a future depencency on CBUSdefs if that ever becomes a NuGet package; currently it is just generated code which has to be manually retrieved and included.

### CBUS
The latest version of cbusdefsenums from MERG Devs is embedded; when (if) this is turned into a Nuget package in its own right Asgard will have a dependency on it. Until then any update to cbusdefsenums will need to be manually updated here and a new version of Asgard released.

## Issues to Fix
* OpCode STMOD claims a property of ServiceMode but supported by a single bit, while WCVS provides a property of ServiceMode with a whole byte. The on on STMOD needs to be changed to ServiceFlag, or similar.

## Planned Developments
* ~Remove dependency on external modules, specifically NLog and Unity Container.~
* ~Add tests based on cbusdefs for opcodes.~
* Add mechanism for defining which OpCodes are handled and which are ignored; in addition add static objects with standard OpCodes that can be used as templates that a user of the package can add OpCodes to:
  - Absolute minimum: e.g. QNN & PNN
  - Configuration: all config OpCodes
  - DCC: all DCC OpCodes
  - Accessories: all accessory OpCodes
  - Everything
  - Probably some others...
* Split the solution into separate projects / namespaces:
  - Comms functionality: Asgard.Comms in main project
  - OpCode and CBUS message functionality: Asgard.Data in main project
  - The reading and processing template data: Asgard.Templating in templating project
* Add interfaces to OpCodes based on
  - ~properties: one interface per property type~
  - group: one interface per group
* Add other implementations of ICbusMessage for 
  - Span\<T\>
  - Memory\<T\>
* ~Linking OpCodes to their request or response or error OpCode.~

## Possible Changes Requiring Further Consideration
* Add a management class for handling incoming CBUS events. The idea is to allow a developer to register CBUS events by simply providing the event number (either long or short) and the callback that is to be run.
