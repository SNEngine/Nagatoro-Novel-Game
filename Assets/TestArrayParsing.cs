using UnityEngine;

public class TestArrayParsing : MonoBehaviour
{
    void Start()
    {
        // Test the new array parsing functionality
        Debug.Log("Testing array parsing functionality...");
        
        // Test 1: Basic array format
        string test1 = "[Option 1, Option 2, Option 3]";
        string[] result1 = ParseOptionsString(test1);
        Debug.Log($"Test 1 - Input: {test1}");
        Debug.Log($"Test 1 - Output: [{string.Join(", ", result1)}]");
        
        // Test 2: Array with quotes
        string test2 = "[\"Option 1: The 'safe' choice\", \"Option 2: The \\\"risky\\\" choice\", \"Option 3: The mysterious choice\"]";
        string[] result2 = ParseOptionsString(test2);
        Debug.Log($"Test 2 - Input: {test2}");
        Debug.Log($"Test 2 - Output: [{string.Join(", ", result2)}]");
        
        // Test 3: Comma-separated (old format)
        string test3 = "Option A, Option B, Option C";
        string[] result3 = ParseOptionsString(test3);
        Debug.Log($"Test 3 - Input: {test3}");
        Debug.Log($"Test 3 - Output: [{string.Join(", ", result3)}]");
        
        // Test 4: Dash-separated (old format)
        string test4 = " - Dash Option 1 - Dash Option 2 - Dash Option 3";
        string[] result4 = ParseOptionsString(test4);
        Debug.Log($"Test 4 - Input: {test4}");
        Debug.Log($"Test 4 - Output: [{string.Join(", ", result4)}]");
    }
    
    // Copy the parsing logic here to test it
    private string[] ParseOptionsString(string input)
    {
        // Check if the input is in array format (e.g., "[Option 1, Option 2, Option 3]")
        if (input.StartsWith("[") && input.EndsWith("]"))
        {
            // Extract content between brackets
            string arrayContent = input.Substring(1, input.Length - 2);

            // Split by comma, but handle nested commas properly by tracking brackets/quotes
            System.Collections.Generic.List<string> parts = new System.Collections.Generic.List<string>();
            int bracketLevel = 0;
            bool inDoubleQuote = false;
            bool inSingleQuote = false;
            int lastSplit = 0;

            for (int i = 0; i < arrayContent.Length; i++)
            {
                char c = arrayContent[i];
                bool isEscaped = (i > 0 && arrayContent[i-1] == '\\');

                if (c == '[' && !isEscaped) bracketLevel++;
                else if (c == ']' && !isEscaped) bracketLevel--;
                else if (c == '"' && !isEscaped)
                {
                    if (!inSingleQuote) // Only toggle double quotes if not inside single quotes
                    {
                        inDoubleQuote = !inDoubleQuote;
                    }
                }
                else if (c == '\'' && !isEscaped)
                {
                    if (!inDoubleQuote) // Only toggle single quotes if not inside double quotes
                    {
                        inSingleQuote = !inSingleQuote;
                    }
                }
                else if (c == ',' && bracketLevel == 0 && !inDoubleQuote && !inSingleQuote)
                {
                    parts.Add(arrayContent.Substring(lastSplit, i - lastSplit).Trim());
                    lastSplit = i + 1;
                }
            }

            // Add the last part
            if (lastSplit < arrayContent.Length)
            {
                parts.Add(arrayContent.Substring(lastSplit).Trim());
            }

            // Trim whitespace from each part and remove quotes if they exist
            for (int i = 0; i < parts.Count; i++)
            {
                string part = parts[i].Trim();

                // Remove surrounding quotes if they exist
                if ((part.StartsWith("\"") && part.EndsWith("\"") && part.Length >= 2) ||
                    (part.StartsWith("'") && part.EndsWith("'") && part.Length >= 2))
                {
                    part = part.Substring(1, part.Length - 2);
                }

                parts[i] = part;
            }

            return parts.ToArray();
        }
        // Check if the input uses dash separator format (e.g., " - Variant A - Variant B - Variant C")
        // If it contains the pattern " - " at the beginning, we'll treat it as dash-separated
        else if (input.Contains(" - "))
        {
            // Split by " - " (dash with spaces)
            string[] parts = input.Split(new string[] { " - " }, System.StringSplitOptions.RemoveEmptyEntries);

            // The first part might be empty if the string starts with " - ", so we remove it
            if (parts.Length > 0 && string.IsNullOrEmpty(parts[0].Trim()))
            {
                // Remove the first empty element
                System.Collections.Generic.List<string> partsList = new System.Collections.Generic.List<string>(parts);
                partsList.RemoveAt(0);
                parts = partsList.ToArray();
            }

            // Trim whitespace from each part
            for (int i = 0; i < parts.Length; i++)
            {
                parts[i] = parts[i].Trim();
            }

            return parts;
        }
        else
        {
            // Original format: split by comma
            // Example: "Option 1: The safe choice, Option 2: The risky choice, Option 3: The mysterious choice"
            string[] parts = input.Split(',');

            // Trim whitespace from each part
            for (int i = 0; i < parts.Length; i++)
            {
                parts[i] = parts[i].Trim();
            }

            return parts;
        }
    }
}