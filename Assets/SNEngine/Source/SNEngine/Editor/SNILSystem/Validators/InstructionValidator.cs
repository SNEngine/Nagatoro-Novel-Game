using System;
using System.Collections.Generic;
using System.Linq;

namespace SNEngine.Editor.SNILSystem.Validators
{
    public class InstructionValidator
    {
        public static List<SNILValidationError> ValidateInstructions(string[] lines)
        {
            var errors = new List<SNILValidationError>();
            
            var mainScriptLines = ScriptLineExtractor.ExtractMainScriptWithoutFunctions(lines);

            for (int i = 0; i < mainScriptLines.Length; i++)
            {
                string line = mainScriptLines[i];
                string trimmedLine = line.Trim();

                if (string.IsNullOrEmpty(trimmedLine) || IsCommentLine(trimmedLine))
                    continue;

                // Получаем оригинальный индекс строки в исходном массиве
                int originalLineIndex = GetOriginalLineIndex(lines, line, 0);

                // Проверяем специальные инструкции
                if (System.Text.RegularExpressions.Regex.IsMatch(trimmedLine, @"^name:\s*.+", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                {
                    // Уже проверили в NameDirectiveValidator
                    continue;
                }
                else if (trimmedLine.Equals("Start", System.StringComparison.OrdinalIgnoreCase))
                {
                    // Уже проверили в NameDirectiveValidator
                    continue;
                }
                else if (trimmedLine.Equals("End", System.StringComparison.OrdinalIgnoreCase))
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
                else if (trimmedLine.StartsWith("Jump To ", System.StringComparison.OrdinalIgnoreCase))
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
                else if (trimmedLine.StartsWith("call ", System.StringComparison.OrdinalIgnoreCase))
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
                else if (trimmedLine.StartsWith("function ", System.StringComparison.OrdinalIgnoreCase))
                {
                    // Уже обработали в FunctionValidator
                    continue;
                }
                else if (trimmedLine.Equals("end", System.StringComparison.Ordinal))
                {
                    // Уже обработали в FunctionValidator
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

            return errors;
        }

        private static bool IsLastSignificantLine(string[] lines, int currentIndex)
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
}