"""Validation functions for YAML and file structure."""
import yaml
from PyQt5.QtGui import QColor


def validate_yaml(self, file_path: str, yaml_text: str) -> bool:
    """Validate YAML syntax."""
    try:
        yaml.safe_load(yaml_text)
        return True
    except yaml.YAMLError as e:
        color = QColor(self.STYLES['DarkTheme']['NotificationWarning'])
        self.show_notification(f"YAML Syntax Error in {file_path}!", color)
        print(f"YAML Error: {e}")
        return False


def validate_structure(self):
    """Validate current language structure."""
    is_valid = self.validator.validate_structure(self.temp_structure)
    if is_valid:
        self.status_bar.showMessage("Structure validation passed.", 3000)
    else:
        color = QColor(self.STYLES['DarkTheme']['NotificationError'])
        self.show_notification(f"Structure validation failed: {self.validator.get_last_error()}", color, duration_ms=5000)
