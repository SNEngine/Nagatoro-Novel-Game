import os
from typing import Dict, Any

class LanguageService:
    def normalize_path(self, path: str) -> str:
        if not path: return ""
        return os.path.normpath(path).lower()

    def add_folder_recursive(self, folder_path: str, structure: dict):
        normalized_path = self.normalize_path(folder_path)
        yaml_files = []
        try:
            for file_name in os.listdir(folder_path):
                full_path = os.path.join(folder_path, file_name)
                # Only look for .yaml files, ignore .yaml.meta
                if os.path.isfile(full_path) and file_name.lower().endswith('.yaml') and not file_name.lower().endswith('.yaml.meta'):
                    yaml_files.append(file_name)
        except OSError:
            pass

        structure[normalized_path] = sorted(yaml_files)

        try:
            for item_name in os.listdir(folder_path):
                full_path = os.path.join(folder_path, item_name)
                if os.path.isdir(full_path):
                    self.add_folder_recursive(full_path, structure)
        except OSError:
            pass

    def get_language_structure_from_path(self, folder_path: str) -> dict:
        normalized_root = self.normalize_path(folder_path)
        structure = {}
        if os.path.isdir(folder_path):
            self.add_folder_recursive(folder_path, structure)
        return {'root_path': normalized_root, 'structure': structure}

    def get_language_folders(self, root_path: str) -> list[str]:
        """Scans the root_path for language subfolders (e.g., 'en', 'ru')."""
        if not os.path.isdir(root_path):
            return []

        language_folders = []
        for item_name in os.listdir(root_path):
            full_path = os.path.join(root_path, item_name)
            if os.path.isdir(full_path):
                # A simple heuristic: consider any direct subdirectory a potential language folder
                # More sophisticated checks could be added here if needed (e.g., check for a specific marker file)
                language_folders.append(item_name)
        return sorted(language_folders)

    def get_language_specific_structure(self, root_localization_path: str, language_code: str) -> dict:
        """Gets the file structure for a specific language within the root localization path."""
        effective_folder_path = os.path.join(root_localization_path, language_code)
        normalized_root = self.normalize_path(effective_folder_path)
        structure = {}
        if os.path.isdir(effective_folder_path):
            self.add_folder_recursive(effective_folder_path, structure)
        return {'root_path': normalized_root, 'structure': structure}
