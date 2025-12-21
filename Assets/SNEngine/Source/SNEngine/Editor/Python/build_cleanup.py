import os
import shutil
import sys

# Определяем корень проекта относительно расположения скрипта
# Скрипт в Assets/SNEngine/Source/SNEngine/Editor/Python (глубина 6)
script_dir = os.path.dirname(os.path.abspath(__file__))
project_root = os.path.abspath(os.path.join(script_dir, "../../../../../.."))

PATHS_TO_DELETE = [
    "Assets/SNEngine/Source/SNEngine/Resources/Custom",
    "Assets/StreamingAssets",
    "Assets/SNEngine/Demo"
]

PATHS_TO_CLEAR = [
    "Assets/SNEngine/Source/SNEngine/Resources/Characters",
    "Assets/SNEngine/Source/SNEngine/Resources/Dialogues"
]

def cleanup():
    print(f"Starting cleanup from root: {project_root}")
    
    for path in PATHS_TO_DELETE:
        full_path = os.path.join(project_root, path)
        if os.path.exists(full_path):
            shutil.rmtree(full_path)
            print(f"Deleted folder: {path}")

    for path in PATHS_TO_CLEAR:
        full_path = os.path.join(project_root, path)
        if os.path.exists(full_path):
            for item in os.listdir(full_path):
                item_path = os.path.join(full_path, item)
                try:
                    if os.path.isfile(item_path):
                        os.remove(item_path)
                    elif os.path.isdir(item_path):
                        shutil.rmtree(item_path)
                except Exception as e:
                    print(f"Failed to delete {item_path}: {e}")
            print(f"Cleared folder: {path}")

if __name__ == "__main__":
    cleanup()