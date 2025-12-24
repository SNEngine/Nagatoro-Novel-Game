using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SNEngine.Editor.SNILSystem.Validators
{
    public class SNILSyntaxValidator : SNILValidator
    {
        public SNILSyntaxValidator()
        {
        }

        public override bool Validate(string[] lines, out string errorMessage)
        {
            return Validate(lines, out errorMessage, out _);
        }

        public bool Validate(string[] lines, out string errorMessage, out List<SNILValidationError> errors)
        {
            errors = new List<SNILValidationError>();
            errorMessage = "";

            if (lines == null || lines.Length == 0)
            {
                errors.Add(new SNILValidationError
                {
                    LineNumber = 0,
                    LineContent = "",
                    ErrorType = SNILValidationErrorType.EmptyFile,
                    Message = "SNIL script is empty."
                });
                return false;
            }

            // Проверяем синтаксис функций
            ValidateFunctions(lines, errors);

            // Разделяем строки на основной скрипт (вне функций) и тела функций
            var mainScriptLines = ExtractMainScriptWithoutFunctions(lines);

            // Проверяем наличие директивы name в основном скрипте:
            bool hasNameDirective = false;
            int nameLineIndex = -1;
            for (int i = 0; i < mainScriptLines.Length; i++)
            {
                if (Regex.IsMatch(mainScriptLines[i].Trim(), @"^name:\s*.+", RegexOptions.IgnoreCase))
                {
                    hasNameDirective = true;
                    nameLineIndex = i;
                    break;
                }
            }

            if (!hasNameDirective)
            {
                errors.Add(new SNILValidationError
                {
                    LineNumber = 0,
                    LineContent = "No name directive found",
                    ErrorType = SNILValidationErrorType.MissingNameDirective,
                    Message = "Missing 'name:' directive in script."
                });
            }

            // Проверяем, что первая значимая строка (после name:) - это Start
            if (hasNameDirective)
            {
                // Ищем первую строку после name:
                for (int i = nameLineIndex + 1; i < mainScriptLines.Length; i++)
                {
                    string trimmed = mainScriptLines[i].Trim();
                    if (!string.IsNullOrEmpty(trimmed) && !IsCommentLine(trimmed))
                    {
                        if (!trimmed.Equals("Start", StringComparison.OrdinalIgnoreCase))
                        {
                            errors.Add(new SNILValidationError
                            {
                                LineNumber = GetOriginalLineIndex(lines, mainScriptLines[i], 0) + 1,
                                LineContent = trimmed,
                                ErrorType = SNILValidationErrorType.InvalidStart,
                                Message = "Script must start with 'Start' line after 'name:' directive."
                            });
                        }
                        break; // Проверяем только первую значимую строку
                    }
                }
            }

            // Валидируем все инструкции
            for (int i = 0; i < mainScriptLines.Length; i++)
            {
                string line = mainScriptLines[i];
                string trimmedLine = line.Trim();

                if (string.IsNullOrEmpty(trimmedLine) || IsCommentLine(trimmedLine))
                    continue;

                // Получаем оригинальный индекс строки в исходном массиве
                int originalLineIndex = GetOriginalLineIndex(lines, line, 0);

                // Проверяем специальные инструкции
                if (Regex.IsMatch(trimmedLine, @"^name:\s*.+", RegexOptions.IgnoreCase))
                {
                    // Уже проверили выше
                    continue;
                }
                else if (trimmedLine.Equals("Start", StringComparison.OrdinalIgnoreCase))
                {
                    // Уже проверили выше
                    continue;
                }
                else if (trimmedLine.Equals("End", StringComparison.OrdinalIgnoreCase))
                {
                    // Проверяем, что это последняя значимая строка
                    if (!IsLastSignificantLine(mainScriptLines, i))
                    {
                        errors.Add(new SNILValidationError
                        {
                            LineNumber = originalLineIndex + 1,
                            LineContent = trimmedLine,
                            ErrorType = SNILValidationErrorType.InvalidEnd,
                            Message = "Script must end with 'End' line."
                        });
                    }
                }
                else if (trimmedLine.StartsWith("Jump To ", StringComparison.OrdinalIgnoreCase))
                {
                    // Проверяем, что это последняя значимая строка
                    if (!IsLastSignificantLine(mainScriptLines, i))
                    {
                        errors.Add(new SNILValidationError
                        {
                            LineNumber = originalLineIndex + 1,
                            LineContent = trimmedLine,
                            ErrorType = SNILValidationErrorType.InvalidEnd,
                            Message = "Script with 'Jump To' must end with this line."
                        });
                    }
                }
                else if (trimmedLine.StartsWith("call ", StringComparison.OrdinalIgnoreCase))
                {
                    // Проверяем формат вызова функции
                    string functionName = trimmedLine.Substring(5).Trim();
                    if (string.IsNullOrEmpty(functionName))
                    {
                        errors.Add(new SNILValidationError
                        {
                            LineNumber = originalLineIndex + 1,
                            LineContent = trimmedLine,
                            ErrorType = SNILValidationErrorType.InvalidFunctionDefinition,
                            Message = "Function name is required after 'call' keyword."
                        });
                    }
                }
                else if (trimmedLine.StartsWith("function ", StringComparison.OrdinalIgnoreCase))
                {
                    // Уже обработали в ValidateFunctions
                    continue;
                }
                else if (trimmedLine.Equals("end", StringComparison.Ordinal))
                {
                    // Уже обработали в ValidateFunctions
                    continue;
                }
                else
                {
                    // Для всех остальных инструкций используем систему валидаторов
                    var validationResult = InstructionValidatorManager.Instance.ValidateInstruction(trimmedLine);
                    if (!validationResult.IsValid)
                    {
                        errors.Add(new SNILValidationError
                        {
                            LineNumber = originalLineIndex + 1,
                            LineContent = trimmedLine,
                            ErrorType = SNILValidationErrorType.UnknownNode,
                            Message = validationResult.ErrorMessage
                        });
                    }
                }
            }

            if (errors.Count > 0)
            {
                errorMessage = string.Join("\n", errors.Select(e => e.ToString()));
                return false;
            }

            return true;
        }

        private bool IsLastSignificantLine(string[] lines, int currentIndex)
        {
            for (int i = currentIndex + 1; i < lines.Length; i++)
            {
                string trimmed = lines[i].Trim();
                if (!string.IsNullOrEmpty(trimmed) && !IsCommentLine(trimmed))
                {
                    return false; // Найдена еще одна значимая строка
                }
            }
            return true; // Это последняя значимая строка
        }

        private string[] ExtractMainScriptWithoutFunctions(string[] lines)
        {
            List<string> mainScript = new List<string>();
            bool insideFunction = false;

            foreach (string line in lines)
            {
                string trimmedLine = line.Trim();

                if (trimmedLine.StartsWith("function ", StringComparison.OrdinalIgnoreCase))
                {
                    insideFunction = true;
                    // Do NOT add function definition to main script
                }
                // Only consider lowercase "end" as function end, not uppercase "End"
                else if (trimmedLine.Equals("end", StringComparison.Ordinal) && insideFunction)
                {
                    insideFunction = false;
                    // Do NOT add function end to main script
                }
                else if (!insideFunction)
                {
                    // Add line to main script only if we are not inside a function
                    mainScript.Add(line);
                }
                // Lines inside functions are ignored when extracting main script
            }

            return mainScript.ToArray();
        }

        private void ValidateFunctions(string[] lines, List<SNILValidationError> errors)
        {
            int functionDepth = 0;
            int functionStartLine = -1;

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();

                if (line.StartsWith("function ", StringComparison.OrdinalIgnoreCase))
                {
                    if (functionDepth > 0)
                    {
                        errors.Add(new SNILValidationError
                        {
                            LineNumber = i + 1,
                            LineContent = line,
                            ErrorType = SNILValidationErrorType.InvalidFunctionDefinition,
                            Message = "Nested functions are not allowed."
                        });
                    }
                    else
                    {
                        functionDepth++;
                        functionStartLine = i;

                        // Проверяем, что после "function" идет имя функции
                        string functionName = line.Substring(9).Trim();
                        if (string.IsNullOrEmpty(functionName))
                        {
                            errors.Add(new SNILValidationError
                            {
                                LineNumber = i + 1,
                                LineContent = line,
                                ErrorType = SNILValidationErrorType.InvalidFunctionDefinition,
                                Message = "Function name is required after 'function' keyword."
                            });
                        }
                    }
                }
                // Only consider lowercase "end" as function end, not uppercase "End"
                else if (line.Equals("end", StringComparison.Ordinal))
                {
                    if (functionDepth == 0)
                    {
                        errors.Add(new SNILValidationError
                        {
                            LineNumber = i + 1,
                            LineContent = line,
                            ErrorType = SNILValidationErrorType.InvalidFunctionDefinition,
                            Message = "'end' statement without matching 'function'."
                        });
                    }
                    else
                    {
                        functionDepth--;
                    }
                }
            }

            if (functionDepth > 0)
            {
                errors.Add(new SNILValidationError
                {
                    LineNumber = functionStartLine + 1,
                    LineContent = lines[functionStartLine].Trim(),
                    ErrorType = SNILValidationErrorType.FunctionNotClosed,
                    Message = "Function definition is not closed with 'end' statement."
                });
            }
        }

        private static bool IsCommentLine(string line)
        {
            string trimmed = line.Trim();
            return trimmed.StartsWith("//") || trimmed.StartsWith("#");
        }

        private static int GetOriginalLineIndex(string[] originalLines, string content, int startIndex)
        {
            for (int i = startIndex; i < originalLines.Length; i++)
            {
                if (originalLines[i].Trim() == content.Trim())
                {
                    return i;
                }
            }
            return startIndex; // fallback
        }
    }

    public class SNILValidationError
    {
        public int LineNumber { get; set; }
        public string LineContent { get; set; }
        public SNILValidationErrorType ErrorType { get; set; }
        public string Message { get; set; }

        public override string ToString()
        {
            return $"Line {LineNumber}: {ErrorType} - {Message} (Content: '{LineContent}')";
        }
    }

    public enum SNILValidationErrorType
    {
        EmptyFile,
        MissingNameDirective,
        NoContent,
        InvalidStart,
        InvalidEnd,
        UnknownNode,
        InvalidJumpToFormat,
        InvalidFunctionDefinition,
        FunctionNotClosed
    }
}