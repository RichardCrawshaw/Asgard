# Asgard
Basic implementation of serial and network comms for talking to MERG CBUS using the GridConnect protocol.

Asgard contains the basic code used in CbusLogger; the intention is to turn it into a nuget package so that it become easier for a C#.NET developer to develop applications that interface with MERG CBUS modules.

## Dependencies
Currently it has a depencency on NLog for logging and on Unity container. The latter is not currently used but it was envisioned that it would be.

Consideration is being made to use DI for both a logger and a lifetime manager.

### CBUS
The latest version of cbusdefsenums from MERG Devs is embedded; when (if) this is turned into a Nuget package in its own right Asgard will have a dependency on it. Until then any update to cbusdefsenums will need to be manually updated here and a new version of Asgard released.
