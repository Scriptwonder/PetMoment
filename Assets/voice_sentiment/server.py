from flask import Flask, request, jsonify
import torch
import torchaudio
import numpy as np
from pyannote.audio import Model
from pyannote.audio.utils.powerset import Powerset
import warnings
from transformers import pipeline

# Suppress warnings
warnings.filterwarnings("ignore")

app = Flask(__name__)

# 使用访问令牌加载预训练模型
auth_token = "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"  # 替换为你的Hugging Face访问令牌
try:
    model = Model.from_pretrained("pyannote/segmentation-3.0", use_auth_token=auth_token)
    emotion_pipe = pipeline(
        "audio-classification",
        model="ehcalabres/wav2vec2-lg-xlsr-en-speech-emotion-recognition",
        use_auth_token=auth_token
    )
    print("Models loaded successfully.")
except Exception as e:
    print(f"Failed to load models: {e}")
    exit()

# 初始化Powerset转换器
max_speakers_per_chunk, max_speakers_per_frame = 3, 2
to_multilabel = Powerset(max_speakers_per_chunk, max_speakers_per_frame).to_multilabel

@app.route('/analyze', methods=['POST'])
def analyze_audio():
    try:
        audio_file = request.files['file']
        if not audio_file:
            return jsonify({"error": "No audio file received"}), 400
        
        # 加载音频文件
        waveform, sample_rate = torchaudio.load(audio_file)
        
        # 重新采样到16kHz（如果需要）
        if sample_rate != 16000:
            transform = torchaudio.transforms.Resample(orig_freq=sample_rate, new_freq=16000)
            waveform = transform(waveform)
        
        # 添加批次维度
        waveform = waveform.unsqueeze(0)
        
        # 获取Powerset多类编码
        powerset_encoding = model(waveform)
        
        # 转换为多标签编码
        multilabel_encoding = to_multilabel(powerset_encoding)
        multilabel_array = multilabel_encoding.numpy()

        # 解析输出并格式化结果
        time_step = 0.01  # 每个时间步的持续时间
        result = []
        for i, frame in enumerate(multilabel_array[0]):
            active_speakers = np.where(frame == 1)[0]
            if len(active_speakers) > 0:
                start_time = i * time_step
                result.append({"time": start_time, "speakers": active_speakers.tolist()})

        # 使用pipeline分析情绪
        emotion_result = emotion_pipe(audio_file)
        
        return jsonify({"speaker_analysis": result, "emotion_analysis": emotion_result})
    
    except Exception as e:
        print(f"Error occurred: {e}")
        return jsonify({"error": str(e)}), 500

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000)
