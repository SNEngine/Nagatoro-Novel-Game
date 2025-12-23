using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SNEngine.Editor.SNILSystem.Importers;
using SNEngine.Editor.SNILSystem.Parsers;
using SNEngine.Editor.SNILSystem.Validators;
using UnityEngine;

namespace SNEngine.Editor.SNILSystem
{
    public class SNILCompiler
    {
        public static void ImportScript(string filePath)
        {
            SNILScriptImporter.ImportScript(filePath);
        }

        public static bool ValidateScript(string filePath, out List<SNILValidationError> errors)
        {
            return SNILScriptValidator.ValidateScript(filePath, out errors);
        }

        public static void ImportScriptWithoutPostProcessing(string filePath)
        {
            SNILScriptImporter.ImportScriptWithoutPostProcessing(filePath);
        }

        public static List<string> GetAllGraphNamesInFile(string filePath)
        {
            return SNILScriptValidator.GetAllGraphNamesInFile(filePath);
        }

        public static void CreateAllGraphsInFile(string filePath)
        {
            SNILScriptValidator.CreateAllGraphsInFile(filePath);
        }

        public static void ProcessAllGraphsInFile(string filePath)
        {
            var scriptParts = SNILMultiScriptParser.ParseMultiScript(filePath);

            foreach (string[] part in scriptParts)
            {
                SNILScriptProcessor.ProcessSingleGraph(part);
            }
        }
    }

    public class SNILInstruction
    {
        public SNILInstructionType Type { get; set; }
        public Dictionary<string, string> Parameters { get; set; }
        public string NodeTypeName { get; set; }
        public Type NodeType { get; set; }
    }

    public enum SNILInstructionType { Generic }
}