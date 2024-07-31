import os
import json
import random

# Путь к главной папке с папками актеров
main_folder = r'\ravdess_samples\Audio_Speech_Actors_01-24_16k_low'

# Словари для маппинга кодов в имени файла на понятные значения
emotions = {
    '01': 'neutral',
    '02': 'calm',
    '03': 'happy',
    '04': 'sad',
    '05': 'angry',
    '06': 'fearful',
    '07': 'disgust',
    '08': 'surprised'
}

intensities = {
    '01': 'normal',
    '02': 'strong'
}

statements = {
    '01': 'Kids are talking by the door',
    '02': 'Dogs are sitting by the door'
}

# Словарь для хранения данных
data = {
    'actors': {},
    'emotions': {},
    'intensities': {},
    'statements': {}
}

# Проходим по всем папкам актеров
for actor_folder in os.listdir(main_folder):
    actor_path = os.path.join(main_folder, actor_folder)
    if os.path.isdir(actor_path):
        actor_number = actor_folder.split('_')[1]
        data['actors'][actor_number] = []

        # Проходим по всем файлам в папке актера
        for filename in os.listdir(actor_path):
            if filename.endswith('.wav'):
                # Разбиваем имя файла
                parts = filename.split('-')

                # Извлекаем информацию
                emotion_code = parts[2]
                intensity_code = parts[3]
                statement_code = parts[4]
                repetition = parts[5]

                file_info = {
                    'filename': filename,
                    'path': os.path.join(actor_path, filename),
                    'emotion': emotions[emotion_code],
                    'intensity': intensities[intensity_code],
                    'statement': statements[statement_code],
                    'repetition': repetition
                }

                # Добавляем информацию в соответствующие категории
                data['actors'][actor_number].append(file_info)

                if emotion_code not in data['emotions']:
                    data['emotions'][emotion_code] = []
                data['emotions'][emotion_code].append(file_info)

                if intensity_code not in data['intensities']:
                    data['intensities'][intensity_code] = []
                data['intensities'][intensity_code].append(file_info)

                if statement_code not in data['statements']:
                    data['statements'][statement_code] = []
                data['statements'][statement_code].append(file_info)

# Создаем пресеты
presets = {
    'all_emotions': list(emotions.values()),
    'basic_emotions': ['happy', 'sad', 'angry', 'fearful'],
    'positive_emotions': ['happy', 'calm'],
    'negative_emotions': ['sad', 'angry', 'fearful', 'disgust'],
    'neutral_and_extreme': ['neutral', 'happy', 'angry', 'fearful']
}

# Добавляем пресеты в данные
data['presets'] = presets

# Сохраняем данные в JSON файл
with open('ravdess_data.json', 'w', encoding='utf-8') as f:
    json.dump(data, f, ensure_ascii=False, indent=4)

print("JSON файл успешно создан: ravdess_data.json")