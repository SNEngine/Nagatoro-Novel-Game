import zipfile
import os
import tempfile

def create_test_apk():
    """Creates a test APK with language files for testing purposes."""
    
    # Create temporary directory structure
    with tempfile.NamedTemporaryFile(suffix='.apk', delete=False) as temp_apk:
        apk_path = temp_apk.name
    
    # Create the ZIP file (APK is a ZIP file)
    with zipfile.ZipFile(apk_path, 'w') as apk:
        # Create some test language files
        test_content = """# Sample language file
character_names:
  protagonist: "Player"
  npc1: "Guide"
  
dialogue:
  intro: "Welcome to the game!"
  tutorial: "This is a tutorial message."
"""
        
        # Add to assets/StreamingAssets/Language (common Unity path)
        apk.writestr('assets/StreamingAssets/Language/en/texts.yaml', test_content)
        apk.writestr('assets/StreamingAssets/Language/ru/texts.yaml', test_content.replace('Player', 'Игрок').replace('Welcome', 'Добро пожаловать'))
        apk.writestr('assets/StreamingAssets/Language/es/texts.yaml', test_content.replace('Player', 'Jugador').replace('Welcome', 'Bienvenido'))
        
        # Add some additional files to simulate a real APK
        apk.writestr('AndroidManifest.xml', '<manifest package="com.example.game"/>')
        apk.writestr('classes.dex', b'dummy_dex_content')
    
    print(f"Test APK created at: {apk_path}")
    return apk_path

if __name__ == "__main__":
    create_test_apk()