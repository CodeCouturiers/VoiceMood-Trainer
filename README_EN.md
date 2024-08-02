# Emotion Recognition Training

## Installation

1. Download the .wav audio files from one of the following sources:
   - [Zenodo RAVDESS dataset](https://zenodo.org/records/1188976)
   - [Zenodo RAVDESS Speech 16K dataset](https://zenodo.org/records/11063852)

2. Run the `JsonDataGenerator` application and select the folder containing the downloaded audio files.

3. Click the "Save JSON" button and save the `ravdess_data.json` file in the project folder.

4. Open the solution in Visual Studio and run the `VoiceMood_Trainer` application.

## Usage

1. In the main window of the `VoiceMood_Trainer` application, you will see buttons with various emotion presets.

![Window](Window.png)

2. Select the preset you're interested in and click the "Start" button.

3. The application will start playing audio recordings with voice samples of emotions. Your task is to select the corresponding emotion by clicking on one of the buttons.

4. After answering each question, you will receive feedback on the correctness of your answer.

5. The bottom of the window displays your current score and statistics on the completed test.

6. The "Next" button allows you to move on to the next audio file, and the "Repeat" button allows you to repeat the current audio file.

7. You can stop the test at any time by clicking the "Stop" button.

8. The application also has a "Progression" mode, which allows you to select a set of emotions for testing. To switch to this mode, click the "Switch to Progression Mode" button.

9. In "Progression" mode, you can select the emotions you're interested in and start the test. The statistics for the completed tests are available in the "Emotion Statistics" window.

10. You can change the interface language using the dropdown list in the bottom right corner of the window.

## Supported Languages

The application supports the following languages:

- Arabic (ar.json)
- Bengali (bn.json)
- German (de.json)
- English (Australia) (en-AU.json)
- English (en.json)
- Spanish (Mexico) (es-MX.json)
- Spanish (es.json)
- French (Canada) (fr-CA.json)
- French (fr.json)
- Hindi (hi.json)
- Indonesian (id.json)
- Italian (it.json)
- Japanese (ja.json)
- Korean (ko.json)
- Portuguese (Brazil) (pt-BR.json)
- Portuguese (pt.json)
- Russian (ru.json)
- Ukrainian (uk.json)
- Chinese (China) (zh-CN.json)

## Features

- The application uses the NAudio library for audio playback.
- Vector SVG images are used to display emotions.
- The application saves the statistics of the completed tests in the `emotion_statistics.json` file.
- The interface localization is implemented using resource dictionaries.