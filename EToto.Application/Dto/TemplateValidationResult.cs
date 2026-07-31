using System;
using System.Collections.Generic;
using System.Text;

namespace EToto.Application.Dto
{
    /// <summary>
    /// Resultado da validação do template Excel.
    /// </summary>
    public class TemplateValidationResult
    {
        public bool IsValid { get; init; }
        public List<string> Errors { get; init; } = new();

        public static TemplateValidationResult Success() =>
            new() { IsValid = true };

        public static TemplateValidationResult Failure(List<string> errors) =>
            new() { IsValid = false, Errors = errors };
    }
}
