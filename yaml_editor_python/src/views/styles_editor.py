from PyQt5.QtWidgets import QDialog, QVBoxLayout, QHBoxLayout, QGridLayout, QLabel, QLineEdit, QPushButton, QColorDialog, QScrollArea, QWidget, QFrame
from PyQt5.QtCore import pyqtSignal
from PyQt5.QtGui import QColor
import yaml
import os

class StylesEditorDialog(QDialog):
    styles_changed = pyqtSignal(dict)

    def __init__(self, parent=None, styles_file_path=None):
        super().__init__(parent)
        self.setWindowTitle("Styles Editor")
        self.setGeometry(200, 200, 700, 600)
        
        self.styles_file_path = styles_file_path or self._get_default_styles_path()
        self.original_styles = self._load_current_styles()
        self.current_styles = self._load_current_styles()
        self.non_color_inputs = {}

        self.init_ui()
        
    def _get_default_styles_path(self):
        """Получение пути к файлу styles.yaml"""
        import sys
        import os
        if getattr(sys, 'frozen', False):
            # In PyInstaller (EXE) mode
            # PyInstaller creates a temp folder and stores path in _MEIPASS
            base_path = os.path.dirname(sys.executable)

            # Check in executable directory (where --add-data puts files)
            full_path = os.path.join(base_path, 'styles.yaml')
            if os.path.exists(full_path):
                return full_path

            # Check in PyInstaller temp directory
            try:
                temp_path = sys._MEIPASS
                temp_full_path = os.path.join(temp_path, 'styles.yaml')
                if os.path.exists(temp_full_path):
                    return temp_full_path
            except AttributeError:
                # _MEIPASS not available, skip this check
                pass

            # If not found, default to executable directory
            return full_path
        else:
            # In development mode
            return os.path.join(os.path.dirname(os.path.dirname(os.path.abspath(__file__))), 'styles.yaml')
    
    def _load_current_styles(self):
        """Загрузка текущих стилей из файла"""
        try:
            with open(self.styles_file_path, 'r', encoding='utf-8') as f:
                styles = yaml.safe_load(f)
                if styles and 'DarkTheme' in styles:
                    return styles['DarkTheme']
                else:
                    # Возвращаем стили по умолчанию из отдельного файла
                    return self._load_default_styles()
        except Exception as e:
            print(f"Error loading styles: {e}")
            # Возвращаем стили по умолчанию из отдельного файла
            return self._load_default_styles()

    def _load_default_styles(self):
        """Загрузка стилей по умолчанию из отдельного файла"""
        import sys
        import os
        if getattr(sys, 'frozen', False):
            # В PyInstaller исполняемом файле
            base_path = os.path.dirname(sys.executable)
            default_file_path = os.path.join(base_path, 'default_styles.yaml')
        else:
            # В режиме разработки
            base_path = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
            default_file_path = os.path.join(base_path, 'default_styles.yaml')

        try:
            with open(default_file_path, 'r', encoding='utf-8') as f:
                styles = yaml.safe_load(f)
                if styles and 'DarkTheme' in styles:
                    return styles['DarkTheme']
                else:
                    # Если файл default_styles.yaml поврежден, возвращаем стили по умолчанию
                    return {
                        'Background': "#1A1A1A",
                        'Foreground': "#E8E8E8",
                        'SecondaryBackground': "#2A2A2A",
                        'EditorBackground': "#1F1F1F",
                        'BorderColor': "#3A3A3A",
                        'HighlightColor': "#C84B31",
                        'HoverColor': "#3A3A3A",
                        'FilePanelBackground': "#181818",
                        'FilePanelHover': "#2D2D2D",
                        'FolderColor': "#E06C75",
                        'StatusDefault': "#999999",
                        'NotificationSuccess': "#6BA878",
                        'NotificationError': "#D9685A",
                        'NotificationWarning': "#E8C56B",
                        'NotificationTextColor': "#1A1A1A",
                        'EditorCloseButtonColor': "#E8E8E8",
                        'EditorFontName': "Consolas",
                        'EditorDefaultFontSize': 14,
                        'ActiveHighlightColor': "#C84B31",
                        'SyntaxCommentColor': "#608B4E",
                        'SyntaxKeyColor': "#E06C75",
                        'SyntaxKeywordColor': "#AF55C4",
                        'SyntaxStringColor': "#ABB2BF",
                        'SyntaxDefaultColor': "#CCCCCC",
                        'ScrollbarWidth': 10,
                        'ScrollbarBackground': "transparent",
                        'ScrollbarHandle': "#3A3A3A",
                        'ScrollbarHandleHover': "#5A5A5A",
                        'ScrollbarRadius': 6,
                        'BrandTextColor': "#E06C75"
                    }
        except Exception as e:
            print(f"Error loading default styles: {e}")
            # Если файл не найден или поврежден, возвращаем стили по умолчанию
            return {
                'Background': "#1A1A1A",
                'Foreground': "#E8E8E8",
                'SecondaryBackground': "#2A2A2A",
                'EditorBackground': "#1F1F1F",
                'BorderColor': "#3A3A3A",
                'HighlightColor': "#C84B31",
                'HoverColor': "#3A3A3A",
                'FilePanelBackground': "#181818",
                'FilePanelHover': "#2D2D2D",
                'FolderColor': "#E06C75",
                'StatusDefault': "#999999",
                'NotificationSuccess': "#6BA878",
                'NotificationError': "#D9685A",
                'NotificationWarning': "#E8C56B",
                'NotificationTextColor': "#1A1A1A",
                'EditorCloseButtonColor': "#E8E8E8",
                'EditorFontName': "Consolas",
                'EditorDefaultFontSize': 14,
                'ActiveHighlightColor': "#C84B31",
                'SyntaxCommentColor': "#608B4E",
                'SyntaxKeyColor': "#E06C75",
                'SyntaxKeywordColor': "#AF55C4",
                'SyntaxStringColor': "#ABB2BF",
                'SyntaxDefaultColor': "#CCCCCC",
                'ScrollbarWidth': 10,
                'ScrollbarBackground': "transparent",
                'ScrollbarHandle': "#3A3A3A",
                'ScrollbarHandleHover': "#5A5A5A",
                'ScrollbarRadius': 6,
                'BrandTextColor': "#E06C75"
            }
    
    def init_ui(self):
        """Инициализация пользовательского интерфейса"""
        layout = QVBoxLayout()
        
        # Создаем прокручиваемую область для размещения большого количества полей
        scroll = QScrollArea()
        scroll_widget = QWidget()
        scroll_layout = QVBoxLayout(scroll_widget)
        
        # Создаем сетку для элементов управления
        grid_layout = QGridLayout()
        
        self.color_inputs = {}
        row = 0
        
        # Добавляем все цветовые параметры
        for key, value in self.current_styles.items():
            if isinstance(value, str) and value.startswith('#'):
                # Метка
                label = QLabel(key)
                grid_layout.addWidget(label, row, 0)

                # Поле ввода цвета
                color_input = QLineEdit(value)
                color_input.setMaxLength(7)  # Максимальная длина для HEX цвета
                color_input.setFixedWidth(80)
                grid_layout.addWidget(color_input, row, 1)

                # Кнопка выбора цвета
                color_button = QPushButton("...")
                color_button.setFixedWidth(30)
                color_button.clicked.connect(lambda _, input=color_input: self.choose_color(input))
                grid_layout.addWidget(color_button, row, 2)

                # Предварительный просмотр цвета
                color_preview = QLabel()
                color_preview.setFixedSize(30, 20)
                color_preview.setStyleSheet(f"background-color: {value}; border: 1px solid #555;")
                grid_layout.addWidget(color_preview, row, 3)

                self.color_inputs[key] = {
                    'input': color_input,
                    'preview': color_preview,
                    'original_value': value
                }

                # Обновляем предварительный просмотр при изменении текста
                color_input.textChanged.connect(lambda text, preview=color_preview:
                                               preview.setStyleSheet(f"background-color: {text}; border: 1px solid #555;"))

                row += 1

        # Добавляем нецветовые параметры (ширина, радиус и т.д.) - для ползунков прокрутки
        for key, value in self.current_styles.items():
            if not (isinstance(value, str) and value.startswith('#')):
                # Проверяем, является ли это параметром, связанным с прокруткой
                if 'Scrollbar' in key:
                    label = QLabel(key)
                    grid_layout.addWidget(label, row, 0)

                    # Текстовое поле для числовых значений
                    text_input = QLineEdit(str(value))
                    text_input.setFixedWidth(80)
                    grid_layout.addWidget(text_input, row, 1)

                    # Пустая метка вместо кнопки выбора цвета
                    empty_label = QLabel("")
                    empty_label.setFixedWidth(30)
                    grid_layout.addWidget(empty_label, row, 2)

                    # Пустая метка вместо предварительного просмотра
                    empty_preview = QLabel("")
                    empty_preview.setFixedSize(30, 20)
                    grid_layout.addWidget(empty_preview, row, 3)

                    self.non_color_inputs[key] = {
                        'input': text_input,
                        'original_value': value
                    }

                    row += 1
        
        scroll_layout.addLayout(grid_layout)
        scroll.setWidget(scroll_widget)
        scroll.setWidgetResizable(True)
        
        layout.addWidget(scroll)
        
        # Кнопки
        button_layout = QHBoxLayout()
        
        save_button = QPushButton("Save")
        save_button.clicked.connect(self.save_styles)
        button_layout.addWidget(save_button)
        
        apply_button = QPushButton("Apply")
        apply_button.clicked.connect(self.apply_styles)
        button_layout.addWidget(apply_button)
        
        reset_button = QPushButton("Reset")
        reset_button.clicked.connect(self.reset_styles)
        button_layout.addWidget(reset_button)
        
        cancel_button = QPushButton("Cancel")
        cancel_button.clicked.connect(self.reject)
        button_layout.addWidget(cancel_button)
        
        layout.addLayout(button_layout)
        self.setLayout(layout)
    
    def choose_color(self, color_input):
        """Открытие диалога выбора цвета"""
        current_color = color_input.text()
        if not current_color.startswith('#'):
            current_color = "#FFFFFF"
        
        color = QColorDialog.getColor(QColor(current_color), self)
        if color.isValid():
            hex_color = color.name()
            color_input.setText(hex_color)
    
    def save_styles(self):
        """Сохранение стилей в файл"""
        updated_styles = self.get_current_styles()
        try:
            # Читаем текущий файл, чтобы сохранить структуру
            with open(self.styles_file_path, 'r', encoding='utf-8') as f:
                content = yaml.safe_load(f)
            
            if content is None:
                content = {}
            
            # Обновляем тему
            if 'DarkTheme' not in content:
                content['DarkTheme'] = {}
            content['DarkTheme'].update(updated_styles)
            
            # Записываем обновленный файл
            with open(self.styles_file_path, 'w', encoding='utf-8') as f:
                yaml.dump(content, f, default_flow_style=False, allow_unicode=True)
                
            self.styles_changed.emit(updated_styles)
            self.accept()
        except Exception as e:
            print(f"Error saving styles: {e}")
    
    def apply_styles(self):
        """Применение стилей без сохранения"""
        updated_styles = self.get_current_styles()
        self.styles_changed.emit(updated_styles)
    
    def reset_styles(self):
        """Сброс к стилям по умолчанию из файла default_styles.yaml"""
        # Загружаем стили по умолчанию
        default_styles = self._load_default_styles()

        # Сбрасываем цветовые параметры
        for key, data in self.color_inputs.items():
            default_value = default_styles.get(key, data['original_value'])  # fallback to original if not in defaults
            data['input'].setText(str(default_value))
            # Обновляем предварительный просмотр
            data['preview'].setStyleSheet(f"background-color: {default_value}; border: 1px solid #555;")

        # Сбрасываем нецветовые параметры
        for key, data in self.non_color_inputs.items():
            default_value = default_styles.get(key, data['original_value'])  # fallback to original if not in defaults
            data['input'].setText(str(default_value))
    
    def get_current_styles(self):
        """Получение текущих значений стилей из полей ввода"""
        styles = {}
        for key, data in self.color_inputs.items():
            value = data['input'].text()
            if value.startswith('#') and len(value) == 7:
                styles[key] = value
            else:
                # Если значение некорректно, используем оригинальное
                styles[key] = data['original_value']

        # Добавляем нецветовые значения
        for key, data in self.non_color_inputs.items():
            input_value = data['input'].text()
            original_value = data['original_value']

            # Если оригинальное значение было числом, конвертируем в число
            if isinstance(original_value, int):
                try:
                    styles[key] = int(input_value)
                except ValueError:
                    styles[key] = original_value
            elif isinstance(original_value, float):
                try:
                    styles[key] = float(input_value)
                except ValueError:
                    styles[key] = original_value
            else:
                # Для строковых значений просто сохраняем как есть
                styles[key] = input_value

        return styles