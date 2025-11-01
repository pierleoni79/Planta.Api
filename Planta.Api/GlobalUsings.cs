// Ruta: /Planta.Api/GlobalUsings.cs | V1.0
#nullable enable

// BCL / ASP.NET Core
global using System;
global using System.Collections.Generic;
global using System.ComponentModel.DataAnnotations;
global using System.Linq;
global using System.Security.Cryptography;
global using System.Text;

global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.Http;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;

// Terceros
global using MediatR;

// Capas internas
global using Planta.Application;
global using Planta.Data;
global using Planta.Infrastructure;

// Contratos (DTOs)
global using Planta.Contracts.Common;
global using Planta.Contracts.Recibos;
global using Planta.Contracts.Recibos.Queries;
global using Planta.Contracts.Recibos.Requests;
