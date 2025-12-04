"""
Custom QPlainTextEdit with line numbers and particle effects
Based on PyQt5 example for creating a text editor with line numbers
"""
from PyQt5.QtWidgets import QWidget, QPlainTextEdit, QFrame, QLabel, QScrollBar
from PyQt5.QtCore import Qt, QRect
from PyQt5.QtGui import QPainter, QColor, QTextFormat, QFontMetrics, QTextCursor
from .particle_system import ParticleEffect


class LineNumberArea(QFrame):
    """Widget to display line numbers"""

    def __init__(self, editor):
        super().__init__(editor)
        self.code_editor = editor
        self.setFrameShape(QFrame.NoFrame)  # Remove any frame
        # Устанавливаем стиль фона из темы
        secondary_bg = editor.styles.get('DarkTheme', {}).get('SecondaryBackground', '#2A2A2A')
        self.setStyleSheet(f"background-color: {secondary_bg};")

    def sizeHint(self):
        from PyQt5.QtCore import QSize
        return QSize(self.code_editor.line_number_area_width(), 0)

    def paintEvent(self, event):
        self.code_editor.line_number_paint_event(event)


class CodeEditor(QPlainTextEdit):
    """Custom text editor with line numbers"""

    def __init__(self, styles=None, settings_manager=None):
        super().__init__()

        self.styles = styles or {}
        self.settings_manager = settings_manager

        # Create line number area
        self.line_numbers = LineNumberArea(self)

        # Create particle effect system
        try:
            from .particle_system import ParticleEffect
            # Get particle colors from styles, with defaults
            particle_colors = {
                'ParticlePrimaryColor': self.styles.get('DarkTheme', {}).get('ParticlePrimaryColor', '#FF6B6B'),
                'ParticleSecondaryColor': self.styles.get('DarkTheme', {}).get('ParticleSecondaryColor', '#4ECDC4'),
                'ParticleAccentColor': self.styles.get('DarkTheme', {}).get('ParticleAccentColor', '#FFE66D'),
                'ParticleGlowColor': self.styles.get('DarkTheme', {}).get('ParticleGlowColor', '#C84B31'),
                'ParticleTrailColor': self.styles.get('DarkTheme', {}).get('ParticleTrailColor', '#A0A0A0'),
                'ParticleLinePrimaryColor': self.styles.get('DarkTheme', {}).get('ParticleLinePrimaryColor', '#FF6B6B80'),
                'ParticleLineSecondaryColor': self.styles.get('DarkTheme', {}).get('ParticleLineSecondaryColor', '#4ECDC480'),
                'ParticleConnectionColor': self.styles.get('DarkTheme', {}).get('ParticleConnectionColor', '#C84B3140')
            }
            self.particle_effect = ParticleEffect(self, style_colors=particle_colors)
            self.particle_effect.hide()  # Изначально скрыт

            # Initialize the particle effect geometry to match the editor
            self.particle_effect.update_parent_geometry()
        except ImportError:
            self.particle_effect = None  # Disable particles if import fails

        # Initialize line number area width
        self.update_line_number_area_width(0)

        # Set font for line numbers area to match editor
        self.line_numbers.setFont(self.font())

        # Get highlight color from styles for current line number
        self.current_line_color = self.styles.get('DarkTheme', {}).get('ActiveLineNumberColor', '#C84B31')  # Default to highlight color

        # Connect signals - correct PyQt5 signals (after initialization)
        self.blockCountChanged.connect(self.update_line_number_area_width)
        self.updateRequest.connect(self.update_line_number_area)
        self.cursorPositionChanged.connect(self.highlight_current_line)  # Highlight current line when cursor moves
        self.verticalScrollBar().valueChanged.connect(self.update_line_numbers_scroll)

        # Connect text input event to particle effect
        self.textChanged.connect(self.on_text_changed)

    def resizeEvent(self, event):
        """Handle resize events to ensure particle effect is properly sized"""
        # Call the original resize event
        super().resizeEvent(event)

        # Update particle effect geometry to match new editor size
        if hasattr(self, 'particle_effect'):
            self.particle_effect.update_parent_geometry()

    def line_number_area_width(self):
        """Calculate the width needed for line numbers"""
        digits = len(str(max(1, self.blockCount())))
        # Увеличиваем базовый отступ для большего расстояния между номерами строк и текстом
        space = 25 + self.fontMetrics().width('9') * digits
        return space

    def update_line_number_area_width(self, new_block_count):
        """Update the width of the line number area"""
        self.setViewportMargins(self.line_number_area_width(), 0, 0, 0)

    def update_line_number_area(self, rect, dy):
        """Update the line number area"""
        if dy:
            self.line_numbers.scroll(0, dy)
        else:
            # Update only the changed region
            self.line_numbers.update(0, rect.y(), self.line_numbers.width(), rect.height())

        if rect.contains(self.viewport().rect()):
            self.update_line_number_area_width(0)

    def highlight_current_line(self):
        """Highlight the current line in the line number area"""
        # Trigger a repaint of the line number area to update the highlight
        self.line_numbers.update()

    def resizeEvent(self, event):
        """Handle resize events to update line number area"""
        super().resizeEvent(event)

        cr = self.contentsRect()
        self.line_numbers.setGeometry(QRect(cr.left(), cr.top(),
                                          self.line_number_area_width(), cr.height()))

    def line_number_paint_event(self, event):
        """Paint the line numbers"""
        painter = QPainter(self.line_numbers)

        # Fill the background
        background_color = self.styles.get('DarkTheme', {}).get('SecondaryBackground', '#2A2A2A')
        painter.fillRect(event.rect(), QColor(background_color))

        # Get the current line number (cursor position)
        current_line = self.textCursor().blockNumber() + 1

        block = self.firstVisibleBlock()
        block_number = block.blockNumber()
        top = self.blockBoundingGeometry(block).translated(self.contentOffset()).top()
        bottom = top + self.blockBoundingRect(block).height()

        while block.isValid() and top <= event.rect().bottom():
            if block.isVisible() and bottom >= event.rect().top():
                number = str(block_number + 1)

                # Check if this is the current line (where cursor is located)
                is_current_line = (block_number + 1 == current_line)

                # Draw background for the current line if it's the active line
                if is_current_line:
                    # Highlight the current line with the highlight color
                    highlight_color = QColor(self.current_line_color)
                    highlight_color.setAlpha(100)  # Make it semi-transparent
                    painter.fillRect(0, int(top), int(self.line_numbers.width()),
                                   int(self.fontMetrics().height()), highlight_color)
                else:
                    # Calculate alternating background color based on line number for non-current lines
                    if (block_number + 1) % 2 == 0:
                        # Even line numbers get a slightly different background
                        alt_bg_color = QColor(self.styles.get('DarkTheme', {}).get('Background', '#1A1A1A'))
                        alt_bg_color.setAlpha(100)  # Make it semi-transparent
                        painter.fillRect(0, int(top), int(self.line_numbers.width()),
                                       int(self.fontMetrics().height()), alt_bg_color)

                # Set text color based on whether this is the current line
                if is_current_line:
                    # Use the current line highlight color for the text to make it more visible
                    text_color = QColor(self.current_line_color)
                else:
                    # Use foreground color from styles for other line numbers
                    text_color = QColor(self.styles.get('DarkTheme', {}).get('StatusDefault', '#999999'))

                painter.setPen(text_color)  # Color for line numbers

                font = self.font()
                font.setPointSize(font.pointSize() - 1)  # Slightly smaller font for line numbers
                painter.setFont(font)

                # Draw the line number with padding
                painter.drawText(3, int(top), int(self.line_numbers.width()) - 6,
                               int(self.fontMetrics().height()),
                               Qt.AlignRight | Qt.AlignVCenter, number)
            block = block.next()
            top = bottom
            bottom = top + self.blockBoundingRect(block).height()
            block_number += 1

    def update_line_numbers_scroll(self, value):
        """Sync line number area with editor scrolling"""
        self.line_numbers.update()

    def setPlainText(self, text):
        """Override setPlainText to ensure line numbers are updated"""
        super().setPlainText(text)
        self.update_line_number_area_width(0)

    def insertFromMimeData(self, source):
        """Override to make sure line numbers are updated after paste operations"""
        super().insertFromMimeData(source)
        self.update_line_number_area_width(0)

    def setFont(self, font):
        """Override setFont to update line numbers font as well"""
        super().setFont(font)
        self.line_numbers.setFont(font)
        self.update_line_number_area_width(0)

    def on_text_changed(self):
        """Событие изменения текста - запускает эффект частиц если включено в настройках"""
        # Проверяем, включены ли частицы в настройках
        if (self.settings_manager and
            hasattr(self.settings_manager, 'typing_particles_enabled') and
            not self.settings_manager.typing_particles_enabled):
            return  # Не создаем частицы, если опция отключена

        # Проверяем, что particle_effect доступен
        if not self.particle_effect:
            return

        # Получаем текущую позицию курсора
        cursor = self.textCursor()
        current_pos = cursor.position()

        # Проверяем, есть ли предыдущий символ (чтобы не создавать частицы при удалении)
        if current_pos > 0 and len(self.toPlainText()) > 0:
            # Создаем новый курсор и устанавливаем его на позицию предыдущего символа
            prev_cursor = QTextCursor(cursor)
            prev_cursor.setPosition(current_pos - 1, QTextCursor.MoveAnchor)

            # Получаем прямоугольник для позиции предыдущего символа
            cursor_rect = self.cursorRect(prev_cursor)

            # Преобразуем координаты в координаты виджета
            pos = self.mapToGlobal(cursor_rect.topLeft())
            local_pos = self.mapFromGlobal(pos)

            # Calculate the absolute position within the document, considering horizontal scroll
            # The particle effect is a child widget that covers the entire editor area,
            # so we need to position particles relative to the editor's visible area
            content_offset = self.contentOffset()
            x_pos = local_pos.x() + cursor_rect.width() - content_offset.x()
            y_pos = local_pos.y() - content_offset.y()

            # Ensure the particle position is within the editor bounds (but allow positions beyond width
            # for long lines that extend beyond visible area)
            x_pos = max(0, x_pos)
            y_pos = max(0, y_pos)

            # Добавляем частицы в позицию последнего символа
            self.particle_effect.add_particles_at(x_pos, y_pos)

            # Обновляем геометрию эффекта частиц, чтобы она соответствовала родительскому элементу
            self.particle_effect.update_parent_geometry()

    def update_font_from_settings(self):
        """Update the editor font based on settings manager"""
        if self.settings_manager:
            font_family = self.settings_manager.font_family
            font_size = self.settings_manager.font_size

            # Create a new font and apply it to the editor
            current_font = self.font()
            current_font.setFamily(font_family)
            current_font.setPointSize(font_size)
            self.setFont(current_font)

            # Also update the line numbers font if line numbers are enabled
            show_line_numbers = getattr(self.settings_manager, 'show_line_numbers', True)
            if show_line_numbers:
                self.line_numbers.setFont(current_font)
                # Update the viewport margins to accommodate the new font size for line numbers
                self._update_line_numbers_layout()
            else:
                # If line numbers are disabled, make sure the editor has no left margin
                self.setViewportMargins(0, 0, 0, 0)
                # Still update the font for line numbers area so it's ready if enabled later
                self.line_numbers.setFont(current_font)

    def _update_line_numbers_layout(self):
        """Helper method to update line numbers layout after font changes"""
        # Recalculate and update the line number area width
        self.update_line_number_area_width(0)

        # Update the line numbers area geometry to match new font metrics
        if self.isVisible():
            cr = self.contentsRect()
            self.line_numbers.setGeometry(QRect(cr.left(), cr.top(),
                                              self.line_number_area_width(), cr.height()))

    def update_line_numbers_visibility(self):
        """Update the visibility of line numbers based on settings"""
        if self.settings_manager:
            show_line_numbers = self.settings_manager.show_line_numbers
            if show_line_numbers:
                self.line_numbers.show()
                # Update the viewport margins to make space for line numbers
                self._update_line_numbers_layout()
            else:
                self.line_numbers.hide()
                # Remove the margin for line numbers
                self.setViewportMargins(0, 0, 0, 0)

    def update_line_number_styles(self):
        """Update styles for the line number area"""
        secondary_bg = self.styles.get('DarkTheme', {}).get('SecondaryBackground', '#2A2A2A')
        self.current_line_color = self.styles.get('DarkTheme', {}).get('ActiveLineNumberColor', '#C84B31')  # Update the highlight color
        self.line_numbers.setStyleSheet(f"background-color: {secondary_bg};")

    def update_particle_colors(self, styles):
        """Update particle colors from new styles"""
        self.styles = styles
        # Update the current line highlight color from the new styles
        self.current_line_color = styles.get('DarkTheme', {}).get('ActiveLineNumberColor', '#C84B31')
        if self.particle_effect:
            particle_colors = {
                'ParticlePrimaryColor': styles.get('DarkTheme', {}).get('ParticlePrimaryColor', '#FF6B6B'),
                'ParticleSecondaryColor': styles.get('DarkTheme', {}).get('ParticleSecondaryColor', '#4ECDC4'),
                'ParticleAccentColor': styles.get('DarkTheme', {}).get('ParticleAccentColor', '#FFE66D'),
                'ParticleGlowColor': styles.get('DarkTheme', {}).get('ParticleGlowColor', '#C84B31'),
                'ParticleTrailColor': styles.get('DarkTheme', {}).get('ParticleTrailColor', '#A0A0A0'),
                'ParticleLinePrimaryColor': styles.get('DarkTheme', {}).get('ParticleLinePrimaryColor', '#FF6B6B80'),
                'ParticleLineSecondaryColor': styles.get('DarkTheme', {}).get('ParticleLineSecondaryColor', '#4ECDC480'),
                'ParticleConnectionColor': styles.get('DarkTheme', {}).get('ParticleConnectionColor', '#C84B3140')
            }
            self.particle_effect.update_style_colors(particle_colors)