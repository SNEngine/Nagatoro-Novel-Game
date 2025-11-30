import os
from PyQt5.QtWidgets import QWidget, QHBoxLayout, QComboBox, QLabel, QSizePolicy
from PyQt5.QtGui import QIcon, QPixmap
from PyQt5.QtCore import Qt, QSize

# Вспомогательная функция для создания иконок флагов
def create_flag_icon(flag_path: str, size: QSize = QSize(20, 20)) -> QIcon:
    if os.path.exists(flag_path):
        pixmap = QPixmap(flag_path)
        if not pixmap.isNull():
            return QIcon(pixmap.scaled(size, Qt.KeepAspectRatio, Qt.SmoothTransformation))
    return QIcon()

def create_language_selector_widget(self) -> QWidget:
    widget = QWidget(self)
    layout = QHBoxLayout(widget)
    layout.setContentsMargins(0, 0, 0, 0)
    layout.setSpacing(5)
    
    self.language_selector_combo = QComboBox(self)
    self.language_selector_combo.setStyleSheet("QComboBox { padding: 2px; border: 1px solid transparent; border-radius: 3px; } QComboBox::drop-down { border: none; } QComboBox::down-arrow { image: none; } QComboBox::item { padding: 2px 5px; } QComboBox::item:selected { background-color: #0078D7; color: white; }")
    self.language_selector_combo.setFixedSize(QSize(80, 24)) # Увеличиваем ширину для иконки и текста
    self.language_selector_combo.setIconSize(QSize(20, 20)) # Устанавливаем размер иконки
    
    # self.active_language_flag_label больше не нужен, так как иконка будет в QComboBox
    # self.active_language_flag_label = QLabel(self)
    # self.active_language_flag_label.setFixedSize(QSize(20, 20))
    # self.active_language_flag_label.setAlignment(Qt.AlignCenter)

    # layout.addWidget(self.active_language_flag_label) # Удаляем флаг из layout
    layout.addWidget(self.language_selector_combo)

    # Изначально скрываем, будем показывать в режиме 'languages'
    widget.setVisible(False)
    self.language_selector_widget = widget # Сохраняем ссылку на виджет
    
    # Подключаем сигнал, но метод switch_active_language будет реализован позже
    self.language_selector_combo.currentIndexChanged.connect(self._on_language_selected)
    
    return widget

def populate_language_selector(self):
    self.language_selector_combo.clear()
    if self.editor_mode == 'languages' and self.all_language_structures:
        sorted_lang_codes = sorted(self.all_language_structures.keys())
        print(f"[DEBUG LANG_SELECTOR] Populating for editor_mode: {self.editor_mode}, available languages: {sorted_lang_codes}")
        for lang_code in sorted_lang_codes:
            lang_path = self.all_language_structures[lang_code]['root_path']
            flag_path = os.path.join(os.path.dirname(lang_path), lang_code, 'flag.png') # Путь к flag.png
            icon = create_flag_icon(flag_path) # Создаем иконку
            self.language_selector_combo.addItem(icon, lang_code.upper()) # Добавляем иконку и текст

        # Устанавливаем текущий язык
        if self.active_language_code and self.active_language_code in sorted_lang_codes:
            index = sorted_lang_codes.index(self.active_language_code)
            self.language_selector_combo.setCurrentIndex(index)
            print(f"[DEBUG LANG_SELECTOR] Active language: {self.active_language_code}, setting index: {index}, item text: {self.language_selector_combo.itemText(index)}")
        else:
            print(f"[DEBUG LANG_SELECTOR] active_language_code {self.active_language_code} not found in sorted_lang_codes: {sorted_lang_codes}")
        
        self.language_selector_widget.setVisible(True)
    else:
        print(f"[DEBUG LANG_SELECTOR] Not populating language selector. editor_mode: {self.editor_mode}, all_language_structures: {bool(self.all_language_structures)}")
        self.language_selector_widget.setVisible(False)
