# Reimu
<p align="center">
<a href="https://ravendb.net"><img src="https://img.shields.io/badge/Powered%20By-RavenDB-CA1C59.svg?longCache=true&style=flat-square"/></a>
</p>
C# Discord bot - In early stages, expect massive, possibly breaking, changes until a suitable release point is achieved (when version string in Program.cs reads 1.0.0+)

# Running Reimu
In order to build and run Reimu, you will need both the latest preview of the .Net 5.0 SDK installed, and a copy of RavenDB
 setup and running. The easiest setup for testing will be to download the latest RavenDB server version, run the included sh or bat file, 
 and follow the instructions to install a local unsecured database. This is the default setup expected by Reimu's settings.

If you want a more advanced setup of the database, refer to the documentation [here](https://ravendb.net/docs/article-page/4.2/csharp) and make sure to update the \
`settings.json` file as needed, which is created the first time the bot is run next to the exe.

To run use

`dotnet run -c Release`
