from PyQt5.QtWidgets import QToolBar, QLabel, QAction, QWidget, QSizePolicy
from PyQt5.QtCore import Qt, QSize


def create_main_toolbar(self):
    """Create the main toolbar (moved out from view.py)."""
    main_toolbar = QToolBar("Main Toolbar")
    main_toolbar.setIconSize(QSize(18, 18))
    self.addToolBar(Qt.TopToolBarArea, main_toolbar)
    
    main_toolbar.setToolButtonStyle(Qt.ToolButtonTextBesideIcon)

    # --- КНОПКА OPEN FOLDER ---
    open_action = QAction(self.icon_folder, "Open Folder...", self)
    open_action.triggered.connect(self.open_folder_dialog)
    main_toolbar.addAction(open_action)
    
    main_toolbar.addSeparator()
    
    # Reload Structure
    reload_action = QAction("Reload Structure", self)
    reload_action.triggered.connect(self.reload_structure_action)
    main_toolbar.addAction(reload_action)
    
    main_toolbar.addSeparator()
    
    # Флаг + метка для выбора языка
    self.flag_label = QLabel()
    self.flag_label.setFixedSize(24, 16)
    self.flag_label.setScaledContents(True)
    main_toolbar.addWidget(self.flag_label)

    self.language_label = QLabel("Language: N/A")
    main_toolbar.addWidget(self.language_label)
    
    # Заглушка для Font Size
    self.font_size_label = QLabel(f"Font Size: {self._current_font_size} (Ctrl+↑/↓)")
    main_toolbar.addWidget(self.font_size_label)
    
    # Гибкое пространство
    main_toolbar.addSeparator()
    spacer = QWidget()
    spacer.setSizePolicy(QSizePolicy.Expanding, QSizePolicy.Preferred)
    main_toolbar.addWidget(spacer)

    return main_toolbar
