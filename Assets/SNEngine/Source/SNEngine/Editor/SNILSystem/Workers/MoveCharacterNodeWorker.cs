using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SiphoinUnityHelpers.XNodeExtensions;
using SNEngine.CharacterSystem.Animations;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SNEngine.Editor.SNILSystem.Workers
{
    public class MoveCharacterNodeWorker : SNILWorker
    {
        public override void ApplyParameters(BaseNode node, Dictionary<string, string> parameters)
        {
            // Проверяем, что это действительно MoveCharacterNode или его наследник
            if (!(node is MoveCharacterNode moveNode))
            {
                return;
            }

            // Получаем все поля, включая из базовых классов
            var fields = GetAllFields(node.GetType());

            foreach (var kvp in parameters)
            {
                var field = fields.FirstOrDefault(f =>
                    f.Name.Equals(kvp.Key, System.StringComparison.OrdinalIgnoreCase) ||
                    f.Name.Equals("_" + kvp.Key, System.StringComparison.OrdinalIgnoreCase));

                if (field != null)
                {
                    object val = ConvertValue(kvp.Value, field.FieldType);
                    if (val != null || !field.FieldType.IsValueType)
                    {
                        field.SetValue(node, val);
                    }
                }
            }
        }

        private static FieldInfo[] GetAllFields(System.Type type)
        {
            var fields = new List<FieldInfo>();
            while (type != null && type != typeof(object))
            {
                fields.AddRange(type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
                type = type.BaseType;
            }
            return fields.ToArray();
        }

        private static object ConvertValue(string value, System.Type targetType)
        {
            if (targetType == typeof(string)) return value;
            if (targetType == typeof(int)) return int.TryParse(value, out int i) ? i : 0;
            if (targetType == typeof(float)) return float.TryParse(value, out float f) ? f : 0f;
            if (targetType == typeof(bool)) return bool.TryParse(value, out bool b) ? b : false;
            if (targetType.IsEnum) return System.Enum.Parse(targetType, value, true);

            // Handle Color type
            if (targetType == typeof(Color))
            {
                if (ColorUtility.TryParseHtmlString(value, out Color color))
                {
                    return color;
                }
                // Try parsing as a standard color name
                switch (value.ToLower())
                {
                    case "red": return Color.red;
                    case "green": return Color.green;
                    case "blue": return Color.blue;
                    case "white": return Color.white;
                    case "black": return Color.black;
                    case "yellow": return Color.yellow;
                    case "cyan": return Color.cyan;
                    case "magenta": return Color.magenta;
                    case "gray": return Color.gray;
                    case "grey": return Color.grey;
                    case "clear": return Color.clear;
                }
            }

            // Handle Vector3 type
            if (targetType == typeof(Vector3))
            {
                // Try to parse as "x,y,z" or "x y z" format
                string[] parts = value.Split(new char[] { ',', ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 3 &&
                    float.TryParse(parts[0], out float x) &&
                    float.TryParse(parts[1], out float y) &&
                    float.TryParse(parts[2], out float z))
                {
                    return new Vector3(x, y, z);
                }
            }

            if (typeof(Object).IsAssignableFrom(targetType))
            {
                string filter = $"t:{targetType.Name} {value}";
                string[] guids = AssetDatabase.FindAssets(filter);
                if (guids.Length > 0)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    return AssetDatabase.LoadAssetAtPath(path, targetType);
                }
            }

            return null;
        }
    }
}