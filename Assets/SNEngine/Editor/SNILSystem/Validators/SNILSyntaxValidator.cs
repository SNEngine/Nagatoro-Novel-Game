using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SNEngine.Editor.SNILSystem.Validators
{
    public class SNILSyntaxValidator : SNILValidator
    {
        private readonly Dictionary<string, string> _nodeTemplates;

        public SNILSyntaxValidator()
        {
            // Загружаем шаблоны для проверки существования нод
            _nodeTemplates = new Dictionary<string, string>();
            
            string snilDirectory = "Assets/SNEngine/Source/SNEngine/Editor/SNIL";
            if (Directory.Exists(snilDirectory))
            {
                string[] templateFiles = Directory.GetFiles(snilDirectory, "*.snil");

                foreach (string templateFile in templateFiles)
                {
                    string fileName = Path.GetFileNameWithoutExtension(templateFile);
                    if (fileName.EndsWith(".cs")) fileName = Path.GetFileNameWithoutExtension(fileName);

                    string[] lines = File.ReadAllLines(templateFile);
                    string templateContent = "";

                    foreach (string line in lines)
                    {
                        if (!line.StartsWith("worker:", StringComparison.OrdinalIgnoreCase) && 
                            !string.IsNullOrEmpty(line.Trim()) && 
                            !IsCommentLine(line))
                        {
                            templateContent = line; // Берём первую непустую строку как шаблон
                            break;
                        }
                    }

                    if (!string.IsNullOrEmpty(templateContent))
                    {
                        _nodeTemplates[fileName] = templateContent;
                    }
                }
            }
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

            // Проверяем наличие директивы name:
            bool hasNameDirective = false;
            int nameDirectiveLine = -1;
            for (int i = 0; i < lines.Length; i++)
            {
                if (Regex.IsMatch(lines[i].Trim(), @"^name:\s*.+", RegexOptions.IgnoreCase))
                {
                    hasNameDirective = true;
                    nameDirectiveLine = i;
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

            // Проверяем существование нод в строках (кроме служебных и комментариев)
            List<ContentLineInfo> contentLines = new List<ContentLineInfo>();
            for (int i = 0; i < lines.Length; i++)
            {
                string trimmed = lines[i].Trim();
                if (!string.IsNullOrEmpty(trimmed) && 
                    !IsCommentLine(trimmed) && 
                    !trimmed.StartsWith("name:", StringComparison.OrdinalIgnoreCase) &&
                    !trimmed.StartsWith("worker:", StringComparison.OrdinalIgnoreCase))
                {
                    contentLines.Add(new ContentLineInfo { LineIndex = i, LineContent = trimmed });
                }
            }

            if (contentLines.Count == 0)
            {
                errors.Add(new SNILValidationError
                {
                    LineNumber = 0,
                    LineContent = "No content lines found",
                    ErrorType = SNILValidationErrorType.NoContent,
                    Message = "No content lines found in script."
                });
                errorMessage = string.Join("\n", errors.Select(e => e.ToString()));
                return false;
            }

            // Проверяем, что первая строка - это Start
            if (contentLines[0].LineContent != "Start")
            {
                errors.Add(new SNILValidationError
                {
                    LineNumber = contentLines[0].LineIndex + 1,
                    LineContent = contentLines[0].LineContent,
                    ErrorType = SNILValidationErrorType.InvalidStart,
                    Message = "Script must start with 'Start' line."
                });
            }

            // Проверяем, что последняя строка - это End или JumpTo
            ContentLineInfo lastLine = contentLines[contentLines.Count - 1];
            bool endsWithValidExit = lastLine.LineContent.Equals("End", StringComparison.OrdinalIgnoreCase) ||
                                   lastLine.LineContent.StartsWith("Jump To ", StringComparison.OrdinalIgnoreCase);

            if (!endsWithValidExit)
            {
                errors.Add(new SNILValidationError
                {
                    LineNumber = lastLine.LineIndex + 1,
                    LineContent = lastLine.LineContent,
                    ErrorType = SNILValidationErrorType.InvalidEnd,
                    Message = "Script must end with 'End' or 'Jump To [dialogue_name]' line."
                });
            }

            // Проверяем существование нод в строках
            for (int i = 1; i < contentLines.Count - 1; i++) // Пропускаем первую (Start) и последнюю (End/JumpTo)
            {
                ContentLineInfo lineInfo = contentLines[i];
                bool isValidNode = false;

                foreach (var template in _nodeTemplates)
                {
                    if (SNILTemplateMatcher.MatchLineWithTemplate(lineInfo.LineContent, template.Value) != null)
                    {
                        isValidNode = true;
                        break;
                    }
                }

                if (!isValidNode)
                {
                    errors.Add(new SNILValidationError
                    {
                        LineNumber = lineInfo.LineIndex + 1,
                        LineContent = lineInfo.LineContent,
                        ErrorType = SNILValidationErrorType.UnknownNode,
                        Message = $"Unknown node format: '{lineInfo.LineContent}'"
                    });
                }
            }

            // Если последняя строка - JumpTo, проверяем её формат
            if (lastLine.LineContent.StartsWith("Jump To ", StringComparison.OrdinalIgnoreCase))
            {
                // Принимаем оба формата: 'Jump To [dialogue_name]' и 'Jump To dialogue_name'
                if (!Regex.IsMatch(lastLine.LineContent, @"^Jump To \[.*\]$") && 
                    !Regex.IsMatch(lastLine.LineContent, @"^Jump To [^$]+$"))
                {
                    errors.Add(new SNILValidationError
                    {
                        LineNumber = lastLine.LineIndex + 1,
                        LineContent = lastLine.LineContent,
                        ErrorType = SNILValidationErrorType.InvalidJumpToFormat,
                        Message = $"Invalid Jump To format. Expected format: 'Jump To [dialogue_name]' or 'Jump To dialogue_name'"
                    });
                }
            }

            if (errors.Count > 0)
            {
                errorMessage = string.Join("\n", errors.Select(e => e.ToString()));
                return false;
            }

            return true;
        }
        
        private static bool IsCommentLine(string line)
        {
            string trimmed = line.Trim();
            return trimmed.StartsWith("//") || trimmed.StartsWith("#");
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
        InvalidJumpToFormat
    }

    internal class ContentLineInfo
    {
        public int LineIndex { get; set; }
        public string LineContent { get; set; }
    }
}