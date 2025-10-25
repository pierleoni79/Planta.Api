// Ruta: /Planta.Api/Common/ProblemDetailsExtensions.cs | V1.0
using Microsoft.AspNetCore.Mvc;

namespace Planta.Api.Common;

public static class ProblemDetailsExtensions
{
    public static IActionResult ConflictProblem(this ControllerBase c, string code, string title, string detail) =>
        c.Problem(statusCode: 409, title: title, detail: detail, type: $"https://errors.planta/{code}");

    public static IActionResult PreconditionFailedProblem(this ControllerBase c, string code, string title, string detail) =>
        c.Problem(statusCode: 412, title: title, detail: detail, type: $"https://errors.planta/{code}");

    public static IActionResult BadRequestProblem(this ControllerBase c, string code, string title, string detail) =>
        c.Problem(statusCode: 400, title: title, detail: detail, type: $"https://errors.planta/{code}");
}
