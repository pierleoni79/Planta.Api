// Ruta: /Planta.Application/GlobalUsings.cs | V1.2
#nullable enable

// BCL
global using System;
global using System.Collections.Generic;
global using System.Threading;
global using System.Threading.Tasks;
global using System.Reflection;

// DI
global using Microsoft.Extensions.DependencyInjection;

// Terceros
global using MediatR;
global using AutoMapper;
global using FluentValidation;

// Contracts
global using Planta.Contracts;
global using Planta.Contracts.Common;
global using Planta.Contracts.Enums;
global using Planta.Contracts.Recibos;
global using Planta.Contracts.Recibos.Requests;
global using Planta.Contracts.Recibos.Queries;

// Domain
global using Planta.Domain.Abstractions;
global using Planta.Domain.Repositories;
global using Planta.Domain.Recibos;
global using Planta.Domain.Enums;
global using Planta.Domain.Common;

// Application (propio) — opcional pero práctico si lo usas en varios archivos
global using Planta.Application.Common.Abstractions;
global using Planta.Application.Common.Exceptions;
global using Planta.Application.Common.Behaviors;
global using Planta.Application.Recibos.Abstractions;
