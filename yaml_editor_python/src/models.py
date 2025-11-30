# src/models.py
import os
import collections
from typing import Any, Dict # <-- FIXED: Import necessary types!

# -------------------------------------------------------------------
# 1. YamlTab (Tab Model)
# -------------------------------------------------------------------

class YamlTab:
    """
    Model for an editor tab.
    Corresponds to the YamlTab class from C# (using Python lists as stacks).
    """
    def __init__(self, file_path: str, yaml_text: str):
        self.file_path = file_path
        self._yaml_text = yaml_text
        self.is_dirty = False
        # Use a list as a stack. Unlike C# Stack<T>,
        # here we store the initial text in the stack
        self.undo_stack = [yaml_text] 
        self.redo_stack = []

    @property
    def yaml_text(self):
        return self._yaml_text

    @yaml_text.setter
    def yaml_text(self, new_text):
        """Simple logic for marking 'dirty'."""
        if self._yaml_text != new_text:
            self.is_dirty = True
        self._yaml_text = new_text


# -------------------------------------------------------------------
# 2. LanguageService (Scanning Business Logic)
# -------------------------------------------------------------------

class LanguageService:
    """
    Mimics LanguageService.Editor, which scans the file structure
    and provides data for the editor.
    """
    
    def __init__(self):
        pass # Initialization is not needed

    def get_language_structure_from_path(self, folder_path: str) -> Dict[str, Any]:
        """
        Implementation mimicking folder scanning (as in C# AddFolderRecursive).
        This is a method that was required in src/view.py.
        
        Returns:
        {
            'root_path': str,
            'structure': {
                'folder/path': ['file1.yaml', 'file2.yaml'],
                'folder/path/subfolder': ['file3.yaml']
            }
        }
        """
        # Normalize path
        normalized_path = os.path.normpath(folder_path)

        # Mimic typical structure that the scanning function should find
        structure = {
            # Root folder
            normalized_path: [
                "metadata.yaml", 
                "characters.yaml", 
                "terms.yaml", 
                "image.png",          
                "temp_file.yaml.meta" 
            ],
            # Subfolders
            os.path.normpath(os.path.join(normalized_path, "dialogues")): [
                "01_scene_a.yaml", 
                "02_scene_b.yaml"
            ],
            os.path.normpath(os.path.join(normalized_path, "tutorial")): [
                "tut_01_intro.yaml"
            ]
        }
        
        return {
            'root_path': normalized_path,
            'structure': structure
        }

    def get_language_structure(self, language_name: str) -> Dict[str, Any]:
        """Stub for compatibility."""
        return {'root_path': None, 'structure': {}}
    
    def get_language_path(self, language_name: str) -> str:
         """Stub for compatibility."""
         return os.path.join("temp_lang_data", language_name)


# -------------------------------------------------------------------
# 3. LanguageMetaData (Port from LanguageMetaData.cs)
# -------------------------------------------------------------------
class LanguageMetaData:
    """
    Localization language metadata.
    """
    def __init__(self, name: str = "", author: str = "", version: int = 0):
        # NameLanguage (string)
        self.name_language: str = name
        
        # Author (string)
        self.author: str = author
        
        # Version (uint) -> int
        self.version: int = version


# -------------------------------------------------------------------
# 4. NodeLocalizationData (Port from NodeLocalizationData.cs)
# -------------------------------------------------------------------
class NodeLocalizationData:
    """
    Localization data for an individual node (dialogue branch, etc.).
    """
    def __init__(self, guid: str = "", value: Any = None):
        # GUID (string)
        self.guid: str = guid
        
        # Value (object) -> Any
        self.value: Any = value


# -------------------------------------------------------------------
# 5. CharacterLocalizationData (Port from CharacterLocalizationData.cs)
# -------------------------------------------------------------------
class CharacterLocalizationData:
    """
    Localization data for a character.
    """
    def __init__(self, guid: str = "", name: str = "", description: str = ""):
        # GUID (string)
        self.guid: str = guid
        
        # Name (string)
        self.name: str = name
        
        # Description (string)
        self.description: str = description