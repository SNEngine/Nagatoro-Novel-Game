using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SNEngine.SelectVariantsSystem;
using SiphoinUnityHelpers.XNodeExtensions;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using System;

namespace SNEngine.Editor.SNILSystem.Workers
{
    public class ShowVariantsNodeWorker : SNILWorker
    {
        public override void ApplyParameters(BaseNode node, Dictionary<string, string> parameters)
        {
            Debug.Log("Using ShowVariantsNodeWorker");

            // Проверяем, что это действительно ShowVariantsNode или его наследник
            if (!(node is ShowVariantsNode showVariantsNode))
            {
                Debug.LogError($"Node {node.GetType().Name} is not a ShowVariantsNode");
                return;
            }

            Debug.Log($"ShowVariantsNodeWorker.ApplyParameters called with {parameters.Count} parameters");

            // Устанавливаем параметры для ShowVariantsNode
            foreach (var param in parameters)
            {
                Debug.Log($"Processing parameter: '{param.Key}' = '{param.Value}'");

                switch (param.Key.ToLower())
                {
                    case "_variants":
                        // Разбираем строку с вариантами
                        string variantsStr = param.Value;
                        Debug.Log($"Processing _variants parameter: '{variantsStr}'");

                        // Handle the format: "Option 1, Option 2, Option 3"
                        // Split by comma, but be careful about commas that might be inside text
                        string[] variantsArray = ParseOptionsString(variantsStr);
                        Debug.Log($"Parsed variants: [{string.Join(", ", variantsArray)}]");

                        // Set the _variants field using reflection
                        FieldInfo variantsField = node.GetType().GetField("_variants", BindingFlags.NonPublic | BindingFlags.Instance);
                        if (variantsField != null)
                        {
                            Debug.Log($"Setting _variants field with {variantsArray.Length} variants");
                            variantsField.SetValue(node, variantsArray);
                        }
                        else
                        {
                            Debug.LogError("_variants field not found!");
                        }

                        // Also try to set _currentVariants if it exists
                        FieldInfo currentVariantsField = node.GetType().GetField("_currentVariants", BindingFlags.NonPublic | BindingFlags.Instance);
                        if (currentVariantsField != null)
                        {
                            currentVariantsField.SetValue(node, variantsArray);
                        }
                        break;
                    case "variants": // Also support the non-underscore version for flexibility
                        // Разбираем строку с вариантами
                        string variantsStr2 = param.Value;
                        Debug.Log($"Processing variants parameter: '{variantsStr2}'");

                        // Handle the format: "Option 1, Option 2, Option 3"
                        // Split by comma, but be careful about commas that might be inside text
                        string[] variantsArray2 = ParseOptionsString(variantsStr2);
                        Debug.Log($"Parsed variants: [{string.Join(", ", variantsArray2)}]");

                        // Set the _variants field using reflection
                        FieldInfo variantsField2 = node.GetType().GetField("_variants", BindingFlags.NonPublic | BindingFlags.Instance);
                        if (variantsField2 != null)
                        {
                            Debug.Log($"Setting _variants field with {variantsArray2.Length} variants");
                            variantsField2.SetValue(node, variantsArray2);
                        }
                        else
                        {
                            Debug.LogError("_variants field not found!");
                        }

                        // Also try to set _currentVariants if it exists
                        FieldInfo currentVariantsField2 = node.GetType().GetField("_currentVariants", BindingFlags.NonPublic | BindingFlags.Instance);
                        if (currentVariantsField2 != null)
                        {
                            currentVariantsField2.SetValue(node, variantsArray2);
                        }
                        break;
                    default:
                        Debug.LogWarning($"Unknown parameter for ShowVariantsNode: {param.Key}");
                        break;
                }
            }
        }


        private string[] ParseOptionsString(string input)
        {
            // Используем общую логику из SNILParameterApplier
            return SNILParameterApplier.ParseOptionsString(input);
        }
    }
}