# TechMove GLMS — Core Prototype

[![.NET Build and Test](https://github.com/ST10351970/TechMove.GLMS/actions/workflows/dotnet.yml/badge.svg)](https://github.com/ST10351970/TechMove.GLMS/actions/workflows/dotnet.yml)

ASP.NET Core MVC monolith implementing the Global Logistics Management System for TechMove Logistics.



**Module:** Enterprise Application Development — Part 2

**Stack:** ASP.NET Core MVC, EF Core, SQL Server, xUnit



## Projects



- **TechMove.GLMS.Web** — MVC presentation layer

- **TechMove.GLMS.Core** — Domain models, services, design patterns, EF Core DbContext

- **TechMove.GLMS.Tests** — xUnit unit tests



## Running locally



1. Update the connection string in `appsettings.json`

2. `dotnet ef database update --project TechMove.GLMS.Web`

3. `dotnet run --project TechMove.GLMS.Web`



## Running tests



`dotnet test`

