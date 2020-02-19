# Reimu
C# Discord bot - In early stages, expect massive, possibly breaking, changes until a suitable release point is achieved (when version string in Program.cs reads 1.0.0+)

# Running Reimu
Reimu contains two noteworthy configurations for running, Release and Public. Typical users will want to use 'Release'
as it is designed for running as a single instance. Note that you will need both .Net Core 3.1 and 2.2.8 installed to 
run this configuration. This is a limitation of the embedded database software used and should be fixed in the future.

Those planning to run a distributed setup (multiple shards and/or database nodes) will want to use the 'Public' configuration.
This only requires .Net Core 3.1 to run but requires you to setup database nodes yourself and be familiar with adding the
required information to the `settings.json` file.

Reimu can be run either through your ide with the appropriate configuration or from the command line.

`dotnet run -c Release`

`dotnet run -c Public`
