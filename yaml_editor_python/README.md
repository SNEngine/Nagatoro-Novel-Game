# YAML Editor - Project Structure

## Overview
The YAML editor has been refactored to separate concerns into modular, focused components. The main `view.py` now acts as a facade, delegating specific functionality to specialized modules in the `views/` package.

## Module Structure

### `src/view.py`
**Primary entry point** - Main `YAMLEditorWindow` class that orchestrates the application.
- Initializes styles, icons, and models
- Manages session restoration
- Handles main window lifecycle (init, closeEvent)
- Loads styles from `styles.yaml`
- Delegates UI creation and operations to specialized modules

### `src/views/` Package

#### `__init__.py`
Package initializer that re-exports UI helper modules for organization.

#### `toolbar.py`
Main toolbar creation and configuration.
- `create_main_toolbar(self)` - Creates the top toolbar with folder selector, reload, language/font labels

#### `file_panel.py`
File panel and directory tree rendering.
- `create_file_panel(self)` - Creates left-side file browser panel
- `draw_file_tree(self)` - Renders the full directory tree
- `draw_folder_content(self, folder_path, structure, level)` - Recursive folder rendering
- `_add_file_button(self, name, path, level)` - Adds individual file buttons
- `clear_layout(self, layout)` - Clears Qt layout widgets
- Foldout state management: `get_or_set_foldout()`, `set_foldout()`
- Search functionality: `check_folder_for_match_recursive()`

#### `editor.py`
Editor area and toolbar setup.
- `create_editor_area(self)` - Creates editor widget with tabs placeholder and text editor
- `create_editor_toolbar(self)` - Creates editor toolbar (Save, Reload, Undo, Redo)

#### `notifications.py`
Status bar notifications.
- `show_notification(self, message, color, duration_ms)` - Display temporary notification
- `hide_notification(self)` - Hide current notification

#### `tabs.py`
Tab management and rendering.
- `draw_tabs_placeholder(self)` - Renders tab bar with active/inactive styling
- `switch_tab_action(self, index)` - Switch between open tabs
- `try_close_tab(self, index)` - Close a tab with unsaved changes check
- `handle_text_change(self)` - Track text changes and mark tabs as dirty
- `update_undo_redo_ui(self)` - Enable/disable undo/redo buttons

#### `file_ops.py`
File I/O operations.
- `load_file(self, file_path)` - Load and open a file in a new tab
- `try_switch_file_action(self, new_file_path)` - Switch files with unsaved check
- `save_file_action(self, tab_to_save)` - Save tab content to disk
- `reload_file_action(self)` - Reload current file from disk

#### `validation.py`
YAML and structure validation.
- `validate_yaml(self, file_path, yaml_text)` - Check YAML syntax
- `validate_structure(self)` - Validate loaded language structure

#### `shortcuts.py`
Keyboard input handling.
- `keyPressEvent(self, event)` - Handle Ctrl+S, Ctrl+Z/Y, Ctrl+Up/Down
- `change_font_size(self, change)` - Adjust editor font size
- `handle_undo(self)` - Undo last change
- `handle_redo(self)` - Redo last undone change

## Architecture Principles

1. **Single Responsibility**: Each module has a focused, well-defined purpose
2. **Delegation Pattern**: `view.py` methods delegate to module functions, maintaining backward compatibility
3. **No Circular Dependencies**: Modules don't import each other; they only import view.py's `self` for state
4. **Cohesive Grouping**: Related functionality is grouped (e.g., all file ops in `file_ops.py`)

- **Maintainability**: Smaller, focused files are easier to understand and modify
- **Testability**: Individual modules can be tested in isolation (with mock `self` objects)
- **Scalability**: New features can be added as new modules without bloating existing ones
- **Code Reusability**: Common patterns in modules can be extracted or shared
- **Readability**: Clear module naming and organization make navigation easier

## Original `src/` Files (Dependencies)

- `main.py` - Application entry point
- `models.py` - YamlTab, LanguageService classes
- `highlighter.py` - YAML syntax highlighting
- `validator.py` - StructureValidator for language structure validation
- `icons.py` - SVG icon definitions
- `session_manager.py` - Session persistence
- `styles.yaml` - Theme and styling configuration