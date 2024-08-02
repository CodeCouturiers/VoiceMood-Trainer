import os

def list_files_and_folders(start_path):
    for root, dirs, files in os.walk(start_path):
        level = root.replace(start_path, '').count(os.sep)
        indent = ' ' * 4 * (level)
        print(f'{indent}{os.path.basename(root)}/')
        subindent = ' ' * 4 * (level + 1)
        for f in files:
            print(f'{subindent}{f}')

# Укажите начальную директорию, которую хотите просмотреть
start_path = r'C:\Users\user\Desktop\VoiceMood Trainer\Help Tools\lang'
list_files_and_folders(start_path)
