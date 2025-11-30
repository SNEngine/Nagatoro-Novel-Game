import os
from typing import Dict, Any, List
from validator import StructureValidator # Импортируем StructureValidator

class LanguageService:
    def __init__(self, validator: StructureValidator):
        self.validator = validator

    def normalize_path(self, path: str) -> str:
        if not path: return ""
        # Используем abspath для получения последовательного абсолютного пути, затем normpath и lower
        return os.path.normpath(os.path.abspath(path)).lower()

    def add_folder_recursive(self, folder_path: str, structure: dict):
        normalized_path = self.normalize_path(folder_path)
        yaml_files = []
        try:
            for file_name in os.listdir(folder_path):
                full_path = os.path.join(folder_path, file_name)
                # Ищем только .yaml файлы, игнорируем .yaml.meta
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

    def get_multiple_language_structures(self, root_dir: str) -> Dict[str, Dict[str, Any]]:
        # Сканирует root_dir на наличие языковых подкаталогов (например, 'en', 'ru')
        # Возвращает словарь, где ключ - код языка, а значение - его структура
        languages = {}
        if not os.path.isdir(root_dir):
            return languages

        print(f"[DEBUG] Scanning root directory: {root_dir}")

        for item_name in os.listdir(root_dir):
            full_path = os.path.join(root_dir, item_name)
            print(f"[DEBUG] Found item: {full_path}")
            if os.path.isdir(full_path): 
                # Предполагаем, что имя папки является кодом языка (например, 'en', 'ru')
                # Добавляем проверку на формат имени папки (две буквы)
                if len(item_name) == 2 and item_name.isalpha():
                    language_code = item_name.lower()
                    print(f"[DEBUG] Potential language folder found: {language_code} at {full_path}")
                    lang_structure = self.get_language_structure_from_path(full_path)
                    
                    if self.validator.validate_structure(lang_structure): # Проверяем валидность языковой папки
                        print(f"[DEBUG] Language structure for '{language_code}' is VALID.")
                        languages[language_code] = lang_structure
                    else:
                        print(f"[DEBUG] Language structure for '{language_code}' is INVALID. Error: {self.validator.get_last_error()}")
                        print(f"Warning: Language folder '{item_name}' is invalid. Skipping. Error: {self.validator.get_last_error()}")
                else:
                    print(f"[DEBUG] Skipping non-language folder: {item_name}")
        return languages
