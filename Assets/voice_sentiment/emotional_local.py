import warnings
from transformers import pipeline

# Suppress warnings
warnings.filterwarnings("ignore")


def use_pipeline(audio_path, access_token):
    print("Using the pipeline for audio emotion classification...")
    pipe = pipeline(
        "audio-classification",
        model="ehcalabres/wav2vec2-lg-xlsr-en-speech-emotion-recognition",
        use_auth_token=access_token
    )
    result = pipe(audio_path)
    print("Pipeline result:", result)

if __name__ == "__main__":
    # Replace with the path to your audio file
    audio_file_path = "test_audio.wav"

    # Input Hugging Face access token
    access_token = input("Please enter your Hugging Face access token: ")

    # Run pipeline analysis
    use_pipeline(audio_file_path, access_token)
