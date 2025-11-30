"""Keyboard shortcuts and input handling."""
from PyQt5.QtCore import Qt
from views.search import show_search, hide_search


def keyPressEvent(self, event):
    """Handle keyboard events: Ctrl+S, Ctrl+Z, Ctrl+Y, Ctrl+Up/Down."""
    if event.modifiers() == Qt.ControlModifier:
        if event.key() == Qt.Key_Up:
            self.change_font_size(1)
            event.accept()
            return
        if event.key() == Qt.Key_Down:
            self.change_font_size(-1)
            event.accept()
            return
        if event.key() == Qt.Key_S and self.current_tab:
            self.save_file_action(self.current_tab)
            event.accept()
            return
        if event.key() == Qt.Key_F:
            # open in-file search
            try:
                show_search(self)
            except Exception:
                # fallback: show widget if present
                if hasattr(self, 'editor_search_widget'):
                    self.editor_search_widget.setVisible(True)
            event.accept()
            return

    # If Esc pressed and search visible, hide it
    if event.key() == Qt.Key_Escape:
        if hasattr(self, 'editor_search_widget') and self.editor_search_widget.isVisible():
            try:
                hide_search(self)
            except Exception:
                try:
                    self.editor_search_widget.setVisible(False)
                    self.text_edit.setExtraSelections([])
                except Exception:
                    pass
            event.accept()
            return
        if event.key() == Qt.Key_Z and self.undo_action.isEnabled():
            self.handle_undo()
            event.accept()
            return
        if event.key() == Qt.Key_Y and self.redo_action.isEnabled():
            self.handle_redo()
            event.accept()
            return

    super(self.__class__, self).keyPressEvent(event)


def change_font_size(self, change):
    """Change font size in editor (Ctrl+Up/Down)."""
    new_size = self._current_font_size + change

    if 10 <= new_size <= 24:
        self._current_font_size = new_size
        font = self.text_edit.font()
        font.setPointSize(new_size)
        self.text_edit.setFont(font)
        self.font_size_label.setText(f"Font Size: {new_size} (Ctrl+↑/↓)")


def handle_undo(self):
    """Undo last change."""
    if not self.current_tab:
        return

    if len(self.current_tab.undo_stack) > 0:
        self.current_tab.redo_stack.append(self.current_tab.yaml_text)
        text_to_restore = self.current_tab.undo_stack.pop()

        self.current_tab._yaml_text = text_to_restore
        self.current_tab.is_dirty = True

        self.text_edit.setText(text_to_restore)
        self.draw_tabs_placeholder()
        self.draw_file_tree()
        self.update_status_bar()
        self.update_undo_redo_ui()


def handle_redo(self):
    """Redo last undone change."""
    if not self.current_tab:
        return

    if len(self.current_tab.redo_stack) > 0:
        self.current_tab.undo_stack.append(self.current_tab.yaml_text)
        text_to_restore = self.current_tab.redo_stack.pop()

        self.current_tab._yaml_text = text_to_restore
        self.current_tab.is_dirty = True

        self.text_edit.setText(text_to_restore)
        self.draw_tabs_placeholder()
        self.draw_file_tree()
        self.update_status_bar()
        self.update_undo_redo_ui()
