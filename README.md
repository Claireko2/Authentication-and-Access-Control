# Authentication and Access Control

An ASP.NET Core MVC application developed for the BCIT Database Security course. The project demonstrates authentication, authorization, database security, secure communication, software licensing, and protection against reverse engineering.

## Overview

This application is a corporate client management system built with ASP.NET Core MVC, Entity Framework Core, SQL Server, and ASP.NET Core Identity.

The application supports different types of users and provides access to functionality based on their assigned roles.

### User Roles

The application defines four roles:

* **Administrator**
* **Manager**
* **Employee**
* **Client**

Each role has different permissions:

| Role          | Main Access                                        |
| ------------- | -------------------------------------------------- |
| Administrator | Manage employees and clients, assign Manager roles |
| Manager       | Manage employee and client information             |
| Employee      | View client information and their own profile      |
| Client        | View their own profile                             |

Authorization is enforced on the server side using ASP.NET Core authorization policies and role-based `[Authorize]` attributes.

## Technologies

* ASP.NET Core MVC
* C#
* Entity Framework Core
* Microsoft SQL Server
* ASP.NET Core Identity
* Kestrel
* HTTPS / TLS
* Mutual TLS (mTLS)
* Windows Authentication infrastructure
* Obfuscar
* Cloudflare Tunnel
* Wireshark

## Database

The application uses Microsoft SQL Server with Entity Framework Core.

The database contains information related to:

* Clients
* Employees
* Services
* Client-Service relationships
* User accounts
* Roles
* Authentication and authorization data

ASP.NET Core Identity manages authentication-related tables, including:

* `AspNetUsers`
* `AspNetRoles`
* `AspNetUserRoles`
* `AspNetUserClaims`
* `AspNetRoleClaims`
* `AspNetUserLogins`
* `AspNetUserTokens`

The application uses Entity Framework Core migrations to create and maintain the database schema.

## Authentication and Authorization

ASP.NET Core Identity is used to manage user accounts and authentication.

After a user signs in, their assigned roles determine which operations they are authorized to perform.

For example:

```csharp
[Authorize(Roles = "Administrator,Manager")]
```

restricts an action to Administrators and Managers.

Authorization is enforced on the server side rather than relying only on hiding navigation links or pages from users.

## HTTPS and Mutual TLS

The application supports HTTPS through Kestrel.

Two HTTPS configurations are used:

* **Port 7148:** Standard HTTPS
* **Port 7443:** Mutual TLS demonstration

For the mTLS configuration, the server requires a client certificate:

```csharp
httpsOptions.ClientCertificateMode =
    ClientCertificateMode.RequireCertificate;
```

The client certificate is retrieved by the application using:

```csharp
await context.Connection.GetClientCertificateAsync();
```

The mTLS connection was tested from a mobile device connected to the same local network as the server.

Wireshark was used to inspect the TLS handshake and verify the presence of the client certificate exchange.

## IP Filtering

The application includes IP-based access filtering.

Requests are checked against an allowlist of permitted IP addresses. Requests from unauthorized IP addresses are rejected by the middleware.

This provides an additional network-level access control mechanism alongside application authentication and authorization.

## Software Licensing

The application includes a software licensing mechanism designed to support:

* A two-month free trial
* Annual licensing
* License validation

License information is stored separately from the main application code, with configuration files used for license-related data and public-key verification.

## Code Obfuscation

Obfuscar is used to make the compiled .NET application more difficult to reverse engineer.

The obfuscation configuration includes protections such as:

* Renaming private members
* Hiding private APIs
* String protection
* Suppression of IL disassembly metadata

Obfuscation does not make reverse engineering impossible. Its purpose is to increase the difficulty of understanding and modifying the compiled application.

## SQL Server File Protection

The SQL Server database files that require protection include:

* `.mdf` database files
* `.ldf` transaction log files
* `.bak` database backup files

These files should be protected using least-privilege Windows file system permissions.

Normal application users should not have direct access to the underlying database files. Database access should occur through SQL Server and the application's authorized data-access layer.

## Reverse Tunnel

Cloudflare Tunnel was used to provide external access to the locally hosted application without configuring inbound router port forwarding.

The architecture is approximately:

```text
External Client
       |
       v
Cloudflare HTTPS Endpoint
       |
       v
Encrypted Tunnel
       |
       v
cloudflared
       |
       v
ASP.NET Core Application
```

This allows the application to be accessed externally while the local server remains behind NAT.

## Security Testing

Several security mechanisms were tested during development, including:

* Role-based authorization
* Authentication through ASP.NET Core Identity
* IP filtering
* HTTPS
* Mutual TLS
* Client certificate validation
* SQL Server file access protection
* License validation
* Code obfuscation
* Reverse tunnel connectivity

Wireshark was used to inspect network traffic during the TLS/mTLS testing process.

## Project Structure

The project follows the standard ASP.NET Core MVC structure:

```text
MVP_1B2/
├── Controllers/
├── Models/
├── Views/
├── Data/
├── Services/
├── Middleware/
├── Migrations/
├── App_Data/
├── Keys/
├── wwwroot/
├── Program.cs
├── appsettings.json
└── MVP_1B2.csproj
```

## Running the Application

### Prerequisites

* .NET SDK
* Microsoft SQL Server
* Visual Studio or another compatible .NET development environment

### Database

Configure the SQL Server connection string in `appsettings.json` or the appropriate local configuration.

Apply the Entity Framework Core migrations before running the application.

### Run

Start the ASP.NET Core application from Visual Studio or using:

```bash
dotnet run
```

The application can then be accessed through the configured HTTP/HTTPS endpoints.

## Security Considerations

This project is an educational demonstration of database security, authentication, authorization, and application security concepts.

Security mechanisms such as HTTPS, mTLS, IP filtering, licensing, and obfuscation provide different layers of protection and should be considered complementary controls rather than complete security solutions.

Sensitive credentials, private keys, certificates containing private keys, database connection strings, and license signing keys should not be committed to a public Git repository.

## Course

**BCIT Bachelor of Computer Science**
**COMP 7071: Database Security / Authentication and Access Control**
