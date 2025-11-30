# main.py
import sys
from PyQt5.QtWidgets import QApplication

# After creating __init__.py in the src folder, this import works reliably!
from view import YAMLEditorWindow

def main():
    # 1. QApplication initialization
    app = QApplication(sys.argv)
    
    # 2. Create and display the main window
    editor_window = YAMLEditorWindow()
    editor_window.show()
    
    # 3. Start the main event loop
    sys.exit(app.exec_())

if __name__ == '__main__':
    main()