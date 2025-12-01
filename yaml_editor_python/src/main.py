# main.py
import sys
import os

# Import PyQt5 first
from PyQt5.QtWidgets import QApplication

# Add the src directory to the path so imports work correctly
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

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