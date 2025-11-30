# src/validator.py
import os
from typing import Dict, Any, List

class StructureValidator:
    """
    Class for validating the minimum required structure of a language folder.
    It is considered invalid if key files are missing in the root folder.
    """

    # Defines mandatory files that must be in the root folder of the language
    REQUIRED_ROOT_FILES: List[str] = [
        "metadata.yaml",
        "characters.yaml",
        "ui.yaml"
    ]

    def validate_structure(self, language_structure: Dict[str, Any]) -> bool:
        """
        Checks if the language structure contains the required files in the root.

        Args:
            language_structure: Dictionary obtained from LanguageService,
                                containing 'root_path' and 'structure'.

        Returns:
            True if the structure is valid, False otherwise.
        """
        root_path_normalized = language_structure.get('root_path')
        structure_map = language_structure.get('structure', {})
        
        if not root_path_normalized:
            return False

        # 1. Get the list of files found in the root folder
        root_files_found = structure_map.get(root_path_normalized, [])
        root_files_set = set(f.lower() for f in root_files_found)
        
        is_valid = True
        missing_files = []

        # 2. Check for the presence of each required file
        for required_file in self.REQUIRED_ROOT_FILES:
            if required_file.lower() not in root_files_set:
                is_valid = False
                missing_files.append(required_file)

        if not is_valid:
            # Error information can be saved for display in the UI
            self.last_error = f"Missing required files in root: {', '.join(missing_files)}"
            return False
        
        self.last_error = ""
        return True
    
    def get_last_error(self) -> str:
        """Returns the last validation error message."""
        return getattr(self, 'last_error', "")