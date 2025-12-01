"""
Module to generate language manifest file (language_manifest.json)
based on the available language folders in the root localization directory.
"""
import os
import json
from typing import List, Dict, Any
from PyQt5.QtGui import QColor


class LanguageEntry:
    """Represents a language entry with code."""
    def __init__(self, code: str):
        self.code = code
    
    def to_dict(self) -> Dict[str, str]:
        """Convert to dictionary format for JSON serialization."""
        return {"Code": self.code}


class AvailableLanguagesManifest:
    """Represents the complete language manifest structure."""
    def __init__(self, languages: List[LanguageEntry]):
        self.languages = languages
    
    def to_dict(self) -> Dict[str, List[Dict[str, str]]]:
        """Convert to dictionary format for JSON serialization."""
        return {
            "Languages": [lang.to_dict() for lang in self.languages]
        }


def generate_language_manifest(root_path: str, styles: Dict[str, Any]) -> bool:
    """
    Generate language_manifest.json based on subdirectories in the root localization path.
    
    Args:
        root_path: The root path containing language subdirectories
        styles: The application styles dictionary for notifications
    """
    if not root_path or not os.path.isdir(root_path):
        print(f"[LanguageManifestGenerator] Root path not set or invalid: {root_path}")
        return False

    try:
        # Get all directories in the root path
        language_dirs = [d for d in os.listdir(root_path) 
                         if os.path.isdir(os.path.join(root_path, d))]
        
        # Create language entries from directory names
        language_entries = [LanguageEntry(code=dir_name) for dir_name in language_dirs]
        
        # Create manifest
        manifest = AvailableLanguagesManifest(languages=language_entries)
        
        # Serialize to JSON
        manifest_data = manifest.to_dict()
        output_data = json.dumps(manifest_data, indent=4, ensure_ascii=False)
        
        # Write to file
        manifest_path = os.path.join(root_path, "language_manifest.json")
        with open(manifest_path, 'w', encoding='utf-8') as f:
            f.write(output_data)
        
        print(f"[LanguageManifestGenerator] Manifest generated successfully at: {manifest_path}")
        print(f"[LanguageManifestGenerator] Found {len(language_dirs)} language directories: {language_dirs}")
        return True
        
    except Exception as ex:
        print(f"[LanguageManifestGenerator] Failed to generate manifest: {ex}")
        return False


def regenerate_language_manifest(parent_window) -> bool:
    """
    Regenerate the language manifest file from the current root localization path.
    
    Args:
        parent_window: The main window instance with root_localization_path and styles
        
    Returns:
        bool: True if successful, False otherwise
    """
    root_path = getattr(parent_window, 'root_localization_path', None)
    
    if not root_path or not os.path.isdir(root_path):
        color = QColor(parent_window.STYLES['DarkTheme']['NotificationError'])
        parent_window.show_notification(
            "Cannot generate language manifest: No root localization path selected.", 
            color
        )
        return False

    success = generate_language_manifest(root_path, parent_window.STYLES)
    
    if success:
        color = QColor(parent_window.STYLES['DarkTheme']['NotificationSuccess'])
        parent_window.show_notification(
            f"Language manifest generated successfully in: {os.path.basename(root_path)}", 
            color
        )
        return True
    else:
        color = QColor(parent_window.STYLES['DarkTheme']['NotificationError'])
        parent_window.show_notification(
            f"Failed to generate language manifest in: {os.path.basename(root_path)}", 
            color
        )
        return False