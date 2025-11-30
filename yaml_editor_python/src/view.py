# src/view.py
import os
import sys
import yaml
import collections
from typing import Dict, Any, Optional

from PyQt5.QtWidgets import QMainWindow
from PyQt5.QtWidgets import QWidget
from PyQt5.QtWidgets import QVBoxLayout
from PyQt5.QtWidgets import QHBoxLayout
from PyQt5.QtWidgets import QSplitter
from PyQt5.QtWidgets import QTextEdit
from PyQt5.QtWidgets import QToolBar
from PyQt5.QtWidgets import QLabel
from PyQt5.QtWidgets import QAction
from PyQt5.QtWidgets import QStatusBar
from PyQt5.QtWidgets import QPushButton
from PyQt5.QtWidgets import QLineEdit
from PyQt5.QtWidgets import QSizePolicy
from PyQt5.QtWidgets import QFileDialog
from PyQt5.QtWidgets import QScrollArea
from PyQt5.QtWidgets import QMessageBox
from PyQt5.QtWidgets import QMenu

from PyQt5.QtGui import QIcon
from PyQt5.QtGui import QFont
from PyQt5.QtGui import QColor
from PyQt5.QtGui import QPixmap

from PyQt5.QtCore import Qt
from PyQt5.QtCore import QSize
from PyQt5.QtCore import QTimer
from PyQt5.QtCore import QCoreApplication
from PyQt5.QtCore import QByteArray
from PyQt5.QtCore import QUrl
from PyQt5.QtCore import QPropertyAnimation, QEasingCurve

from PyQt5.QtWidgets import QGraphicsOpacityEffect

# Импорты .models и .icons должны быть доступны, если они в той же папке src
from models import YamlTab, LanguageService
from highlighter import YamlHighlighter
from validator import StructureValidator
from icons import SVG_FOLDER_ICON, SVG_YAML_FILE_ICON
from session_manager import SessionManager # Добавьте это, если session_manager.py находится в src/

# --- УТИЛИТА ДЛЯ СОЗДАНИЯ QIcon ИЗ SVG-строки ---
def create_icon_from_svg(svg_content: str, size: QSize = QSize(16, 16)) -> QIcon:
    """Создает QIcon из SVG-кода с использованием data URI."""
    svg_bytes = QByteArray(svg_content.encode('utf-8'))
    base64_data = svg_bytes.toBase64().data().decode()
    data_uri = f'data:image:svg+xml;base64,{base64_data}'
    icon = QIcon(data_uri)
    return icon
# -------------------------------------------------------------------

# --- Класс LanguageService (Оставлен для полноты и корректного сканирования) ---
# ... (Код LanguageService) ...
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
# -------------------------------------------------------------------


class YAMLEditorWindow(QMainWindow):

    def __init__(self):
        super().__init__()

        # --- Инициализация Стилей ---
        self.STYLES = self._load_styles()
        self.CSS_STYLES = self._generate_css(self.STYLES)

        self.setWindowTitle("YAML Editor")
        self.setGeometry(100, 100, 1200, 800)
        self.setStyleSheet(self.CSS_STYLES) # Применяем сгенерированные стили

        # --- Инициализация Иконок ---
        self.icon_folder = create_icon_from_svg(SVG_FOLDER_ICON)
        self.icon_yaml = create_icon_from_svg(SVG_YAML_FILE_ICON)
        self._last_open_dir: str = os.path.expanduser("~")
        # self.icon_close = create_icon_from_svg(SVG_CLOSE_ICON) # Игнорируем SVG крестик

        # --- Модель/Сервисы ---
        self.lang_service = LanguageService()
        self.validator = StructureValidator() # <-- ИНИЦИАЛИЗАЦИЯ ВАЛИДАТОРА
        self.temp_structure = {'root_path': None, 'structure': {}}
        self.open_tabs: list[YamlTab] = []
        self.current_tab_index = -1
        self.current_tab: YamlTab | None = None

        # UI-переменные (как в C#)
        self.root_lang_path_normalized: str | None = None
        self._notification_timer = QTimer(self)
        self._notification_timer.timeout.connect(self.hide_notification)
        self._notification_label: QLabel | None = None
        self._current_font_size = 14
        self._foldouts: Dict[str, bool] = {} # Для хранения состояния папок

        self.init_ui()
        self.update_status_bar()

        self.session_manager = SessionManager(self)
        self.session_manager.restore_session() # Восстанавливаем сессию при старте

    def closeEvent(self, event):
        """
        Перехватывает событие закрытия окна.
        Сначала проверяет несохраненные изменения, затем сохраняет сессию.
        """

        # Проверяем, есть ли вкладки с несохраненными изменениями
        dirty_tab = next((t for t in self.open_tabs if t.is_dirty), None)

        if dirty_tab:
            reply = QMessageBox.question(self, 'Unsaved Changes',
                "You have unsaved changes. Do you want to save all files before quitting?",
                QMessageBox.Save | QMessageBox.Discard | QMessageBox.Cancel, QMessageBox.Cancel)

            if reply == QMessageBox.Save:
                # Пытаемся сохранить все несохраненные
                for tab in [t for t in self.open_tabs if t.is_dirty]:
                    self.save_file_action(tab)

                # Если после попытки сохранения все еще есть несохраненные (например, из-за ошибки синтаксиса YAML), отменяем закрытие.
                if any(t.is_dirty for t in self.open_tabs):
                    event.ignore()
                    return

            elif reply == QMessageBox.Cancel:
                event.ignore() # Отмена закрытия
                return

        # Если закрытие разрешено (нет несохраненных или пользователь нажал Discard/Save)
        self.session_manager.save_session() # Сохраняем состояние сессии
        event.accept()

    def _get_resource_path(self, relative_path: str) -> str:
        """
        Получает путь к ресурсу.
        В режиме EXE файл находится рядом с исполняемым файлом.
        """
        if getattr(sys, 'frozen', False):
            # В режиме PyInstaller (EXE): файл styles.yaml находится в папке dist
            # рядом с YAML_Editor.exe. sys.executable - это путь к EXE.
            base_path = os.path.dirname(sys.executable)
        else:
            # В режиме разработки: файл styles.yaml находится рядом с view.py (в папке src)
            base_path = os.path.dirname(os.path.abspath(__file__))

        # Соединяем базовый путь и имя файла.
        return os.path.join(base_path, relative_path)

    def _load_styles(self) -> Dict[str, Any]:
        """Загружает стили из styles.yaml или использует резервные."""

        # КЛЮЧЕВОЕ ИЗМЕНЕНИЕ: Используем _get_resource_path для доступа к файлу
        # Поскольку в PyInstaller мы указали --add-data "src/styles.yaml;src",
        # файл будет доступен в папке 'src' относительно базового пути PyInstaller.
        # Но поскольку view.py сам находится в 'src' в режиме разработки,
        # нам нужно просто обращаться к файлу 'styles.yaml' в нашей папке.

        # Если view.py находится в src/, и styles.yaml рядом:

        # СТАРЫЙ КОД:
        # style_file = os.path.join(os.path.dirname(os.path.abspath(__file__)), 'styles.yaml')

        # НОВЫЙ КОД (Использует путь, который работает в PyInstaller и в разработке)
        style_file = self._get_resource_path('styles.yaml')

        # --- РЕЗЕРВНЫЕ СТИЛИ (Оставлены без изменений) ---
        default_styles = {
            # ... (Ваши стили) ...
            'DarkTheme': {
                'Background': "#3C3C3C", 'Foreground': "#CCCCCC", 'SecondaryBackground': "#4C4C4C",
                'EditorBackground': "#2D2D2D", 'BorderColor': "#1D1D1D", 'HighlightColor': "#0078D7",
                'HoverColor': "#5C5C5C", 'FilePanelBackground': "#333333", 'FilePanelHover': "#4C4C4C",
                'FolderColor': "#FFC107", 'StatusDefault': "#AAAAAA", 'NotificationSuccess': "#28A745",
                'NotificationError': "#DC3545",
                'NotificationWarning': "#FFC107"
            }
        }

        # ... (Остальная логика загрузки файла) ...
        try:
            if os.path.exists(style_file):
                 # ... (Остальная логика загрузки) ...
                with open(style_file, 'r', encoding='utf-8') as f:
                    styles = yaml.safe_load(f)
                    # ... (Проверка ключей и возврат) ...
                    if styles and 'DarkTheme' in styles:
                        return styles
                    else:
                        print("Warning: styles.yaml found, but missing 'DarkTheme' key. Using default styles.")
                        return default_styles
            else:
                 print(f"Warning: styles.yaml not found at {style_file}. Using default styles.")
                 return default_styles
        except Exception as e:
            print(f"Error loading styles.yaml ({e}). Using default styles.")
            return default_styles

    # ... (Остальная часть _generate_css) ...
    def _generate_css(self, styles: Dict[str, Any]) -> str:
        """Генерирует CSS-строку на основе загруженных стилей."""
        theme = styles.get('DarkTheme', {})

        css = f"""
        QMainWindow, QWidget {{ background-color: {theme.get('Background')}; color: {theme.get('Foreground')}; }}
        QSplitter::handle {{ background-color: {theme.get('Background')}; }}

        /* TULBAR & ACTIONS (Исправлены визуальные ошибки кнопок QAction) */
        QToolBar {{
            background-color: {theme.get('SecondaryBackground')};
            spacing: 5px;
            border-bottom: 1px solid {theme.get('Background')};
        }}
        QToolButton {{
            color: {theme.get('Foreground')};
            background-color: {theme.get('SecondaryBackground')};
            border: 1px solid {theme.get('SecondaryBackground')};
            padding: 3px 6px;
        }}
        QToolButton:hover {{ background-color: {theme.get('HoverColor')}; border: 1px solid {theme.get('HoverColor')}; }}
        QToolButton:pressed {{ background-color: {theme.get('Background')}; }}

        /* File Panel */
        QWidget#FilePanelWidget {{ background-color: {theme.get('FilePanelBackground')}; }}
        QScrollArea {{ border: none; }}

        QPushButton {{ color: {theme.get('Foreground')}; background-color: {theme.get('FilePanelBackground')}; border: none; }}
        QPushButton:hover {{ background-color: {theme.get('FilePanelHover')}; }}

        /* Editor */
        QLineEdit {{
            background-color: {theme.get('EditorBackground')};
            color: {theme.get('Foreground')};
            border: 1px solid {theme.get('BorderColor')};
            padding: 2px;
        }}
        QTextEdit {{
            background-color: {theme.get('EditorBackground')}; 
            color: {theme.get('Foreground')}; 
            border: 1px solid {theme.get('NotificationError')}; 
            padding: 2px; 
        }}
        
        /* Tab Bar */
        QToolBar#TabPlaceholder {{ background-color: {theme.get('Background')}; border: none; }}

        /* Status Bar */
        QStatusBar {{
            background-color: {theme.get('SecondaryBackground')};
            border-top: 1px solid {theme.get('Background')};
            color: {theme.get('StatusDefault')};
        }}
        
        /* Context Menu */
        QMenu {{
            background-color: {theme.get('SecondaryBackground')};
            color: {theme.get('Foreground')};
            border: 1px solid {theme.get('NotificationError')};
            border-radius: 4px;
        }}
        QMenu::item {{
            padding: 5px 15px 5px 25px; /* Add padding for better spacing */
            background-color: transparent;
        }}
        QMenu::item:selected {{
            background-color: {theme.get('HoverColor')};
            color: {theme.get('HighlightColor')};
        }}
        QMenu::separator {{
            height: 1px;
            background-color: {theme.get('BorderColor')};
            margin: 4px 5px;
        }}
        """
        return css

    def init_ui(self):
        # ... (Код init_ui) ...
        self.create_main_toolbar()

        central_widget = QWidget()
        self.setCentralWidget(central_widget)
        main_layout = QVBoxLayout(central_widget)
        main_layout.setContentsMargins(0, 0, 0, 0)

        self.splitter = QSplitter(Qt.Horizontal)

        self.file_panel = self.create_file_panel()
        self.splitter.addWidget(self.file_panel)

        self.editor_area = self.create_editor_area()
        self.splitter.addWidget(self.editor_area)

        self.splitter.setSizes([250, 950])

        main_layout.addWidget(self.splitter)

        self.create_status_bar()

    def create_main_toolbar(self):
        from views.toolbar import create_main_toolbar as _create_main_toolbar
        return _create_main_toolbar(self)

    def open_folder_dialog(self):
        """Открывает диалог для выбора корневой папки локализации."""
        initial_dir = self._last_open_dir if self._last_open_dir and os.path.isdir(self._last_open_dir) else os.path.expanduser("~")
        folder_path = QFileDialog.getExistingDirectory(self, "Select Language Root Folder", initial_dir)

        if folder_path:
            self._last_open_dir = folder_path
            self.reload_language_structure(folder_path)


    def reload_language_structure(self, folder_path: str):
        """Перезагружает структуру файлов из выбранной папки."""

        dirty_tab = next((t for t in self.open_tabs if t.is_dirty), None)

        if dirty_tab:
            reply = QMessageBox.question(self, 'Unsaved Changes',
                f"Do you want to save changes to file '{os.path.basename(dirty_tab.file_path)}' before reloading the structure? Changes will be lost for other unsaved files.",
                QMessageBox.Save | QMessageBox.Discard | QMessageBox.Cancel, QMessageBox.Cancel)

            if reply == QMessageBox.Save:
                for tab in [t for t in self.open_tabs if t.is_dirty]:
                    self.save_file_action(tab)
            elif reply == QMessageBox.Cancel:
                return

        # Сбрасываем открытые вкладки
        self.open_tabs.clear()
        self.current_tab_index = -1
        self.current_tab = None

        # Загружаем новую структуру
        new_structure = self.lang_service.get_language_structure_from_path(folder_path)

        # --- ПРОВЕРКА ВАЛИДАЦИИ ---
        if not self.validator.validate_structure(new_structure):
            # Если невалидно, сбрасываем и показываем ошибку
            self.temp_structure = {'root_path': None, 'structure': {}}
            self.root_lang_path_normalized = None
            self.language_label.setText(f"Language: N/A (Invalid)")
            self.draw_file_tree()
            self.draw_tabs_placeholder()
            self.update_status_bar()
            color = QColor(self.STYLES['DarkTheme']['NotificationError'])
            error_msg = self.validator.get_last_error()
            self.show_notification(f"Validation failed for {os.path.basename(folder_path)}: {error_msg}", color, duration_ms=5000)
            return

        # Если валидно
        self.temp_structure = new_structure
        self.root_lang_path_normalized = self.temp_structure.get('root_path')

        if self.root_lang_path_normalized and self.temp_structure.get('structure'):
            self.language_label.setText(f"Language: {os.path.basename(folder_path)}")
            self.draw_file_tree()
            self.draw_tabs_placeholder()
            self.update_status_bar()
            # try to load language flag image from the folder (flag.png)
            try:
                flag_path = os.path.join(folder_path, 'flag.png')
                if os.path.isfile(flag_path):
                    pix = QPixmap(flag_path)
                    if not pix.isNull() and hasattr(self, 'flag_label'):
                        # scale to fit label size
                        self.flag_label.setPixmap(pix.scaled(self.flag_label.size(), Qt.KeepAspectRatio, Qt.SmoothTransformation))
                else:
                    if hasattr(self, 'flag_label'):
                        self.flag_label.clear()
            except Exception:
                # ignore failures to load flag
                pass
            color = QColor(self.STYLES['DarkTheme']['NotificationSuccess'])
            self.show_notification(f"Structure loaded and validated from: {os.path.basename(folder_path)}", color)
        else:
            self.language_label.setText(f"Language: N/A")
            self.draw_file_tree()
            self.draw_tabs_placeholder()
            color = QColor(self.STYLES['DarkTheme']['NotificationError'])
            self.show_notification("Failed to load structure. Folder not found or empty.", color)

    # --- Валидация ---
    def validate_structure(self):
        from views.validation import validate_structure as _validate_struct
        return _validate_struct(self)
    def validate_yaml(self, file_path: str, yaml_text: str) -> bool:
        from views.validation import validate_yaml as _validate
        return _validate(self, file_path, yaml_text)

    # ... (Остальная часть File Panel: create_file_panel, clear_layout, draw_file_tree, foldout methods) ...
    def create_file_panel(self) -> QWidget:
        from views.file_panel import create_file_panel as _create_file_panel
        return _create_file_panel(self)

    def clear_layout(self, layout):
        from views.file_panel import clear_layout as _clear_layout
        return _clear_layout(self, layout)

    def draw_file_tree(self):
        from views.file_panel import draw_file_tree as _draw_file_tree
        return _draw_file_tree(self)

    def get_or_set_foldout(self, path: str, default: bool = False) -> bool:
        from views.file_panel import get_or_set_foldout as _get_or_set_foldout
        return _get_or_set_foldout(self, path, default)

    def set_foldout(self, path: str, state: bool):
        from views.file_panel import set_foldout as _set_foldout
        return _set_foldout(self, path, state)

    def check_folder_for_match_recursive(self, folder_path_normalized: str) -> bool:
        from views.file_panel import check_folder_for_match_recursive as _check
        return _check(self, folder_path_normalized)

    def draw_folder_content(self, folder_path: str, structure: dict, level: int):
        from views.file_panel import draw_folder_content as _draw_folder_content
        return _draw_folder_content(self, folder_path, structure, level)


    def _add_file_button(self, name: str, path: str, level: int):
        from views.file_panel import _add_file_button as _add_btn
        return _add_btn(self, name, path, level)

    def load_file(self, file_path: str):
        from views.file_ops import load_file as _load
        return _load(self, file_path)

    def try_switch_file_action(self, new_file_path: str):
        from views.file_ops import try_switch_file_action as _try_switch
        return _try_switch(self, new_file_path)


    def create_editor_area(self) -> QWidget:
        from views.editor import create_editor_area as _create_editor_area
        return _create_editor_area(self)

    def create_editor_toolbar(self):
        from views.editor import create_editor_toolbar as _create_editor_toolbar
        return _create_editor_toolbar(self)


    def create_status_bar(self):
        # ... (Код create_status_bar) ...
        self.status_bar = QStatusBar()
        self.setStatusBar(self.status_bar)
        self.status_label = QLabel("Ready.")
        self.status_bar.addWidget(self.status_label)

        self._notification_label = QLabel()
        self._notification_label.setVisible(False)
        self.status_bar.addPermanentWidget(self._notification_label)

    def show_notification(self, message: str, color: QColor, duration_ms: int = 3000):
        from views.notifications import show_notification as _show
        return _show(self, message, color, duration_ms)

    def hide_notification(self):
        from views.notifications import hide_notification as _hide
        return _hide(self)


    def draw_tabs_placeholder(self):
        from views.tabs import draw_tabs_placeholder as _draw_tabs
        return _draw_tabs(self)


    def switch_tab_action(self, index):
        from views.tabs import switch_tab_action as _switch
        return _switch(self, index)

    def handle_text_change(self):
        from views.tabs import handle_text_change as _handle
        return _handle(self)


    def try_close_tab(self, index: int):
        """
        Обрабатывает запрос на закрытие вкладки по индексу,
        проверяя несохраненные изменения и обновляя содержимое QTextEdit.

        ИСПРАВЛЕНИЕ: Гарантирует, что после удаления вкладки поле ввода
        обновится содержимым новой активной вкладки или очистится.
        """

        from views.tabs import try_close_tab as _try_close
        return _try_close(self, index)

    def update_status_bar(self):
        # ... (Код update_status_bar) ...
        status = "Ready."
        if self.current_tab and self.current_tab.is_dirty:
            status = f"Unsaved changes in {os.path.basename(self.current_tab.file_path)}*"
        self.status_label.setText(status)

    def update_undo_redo_ui(self):
        from views.tabs import update_undo_redo_ui as _update
        return _update(self)


    def save_file_action(self, tab_to_save):
        from views.file_ops import save_file_action as _save
        return _save(self, tab_to_save)

    def reload_file_action(self):
        from views.file_ops import reload_file_action as _reload
        return _reload(self)


    def reload_structure_action(self):
        # ... (Код reload_structure_action) ...
        root_path = self.temp_structure.get('root_path')
        if root_path:
            original_path = os.path.normpath(root_path)
            self.reload_language_structure(original_path)
        else:
            color = QColor(self.STYLES['DarkTheme']['NotificationWarning'])
            self.show_notification("No folder open to reload.", color)

    def handle_undo(self):
        from views.shortcuts import handle_undo as _undo
        return _undo(self)

    def handle_redo(self):
        from views.shortcuts import handle_redo as _redo
        return _redo(self)


    def keyPressEvent(self, event):
        from views.shortcuts import keyPressEvent as _key
        return _key(self, event)

    def change_font_size(self, change):
        from views.shortcuts import change_font_size as _resize
        return _resize(self, change)

    def show_text_edit_context_menu(self, pos):
        menu = QMenu(self)

        # Add animation
        effect = QGraphicsOpacityEffect(menu)
        effect.setOpacity(0)
        menu.setGraphicsEffect(effect)

        animation = QPropertyAnimation(effect, b"opacity")
        animation.setDuration(150)  # milliseconds
        animation.setStartValue(0.0)
        animation.setEndValue(1.0)
        animation.setEasingCurve(QEasingCurve.InQuad)
        animation.start(QPropertyAnimation.DeleteWhenStopped)

        cut_action = QAction("Cut", self)
        cut_action.triggered.connect(self.text_edit.cut)
        menu.addAction(cut_action)

        copy_action = QAction("Copy", self)
        copy_action.triggered.connect(self.text_edit.copy)
        menu.addAction(copy_action)

        paste_action = QAction("Paste", self)
        paste_action.triggered.connect(self.text_edit.paste)
        menu.addAction(paste_action)

        menu.addSeparator()

        select_all_action = QAction("Select All", self)
        select_all_action.triggered.connect(self.text_edit.selectAll)
        menu.addAction(select_all_action)

        menu.exec_(self.text_edit.mapToGlobal(pos))