import torch
import torchaudio
import numpy as np
import matplotlib.pyplot as plt
import pandas as pd
from pyannote.audio import Model
from pyannote.audio.utils.powerset import Powerset

# 使用访问令牌加载预训练模型
auth_token = "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"  # 替换为你的Hugging Face访问令牌
try:
    model = Model.from_pretrained("pyannote/segmentation-3.0", use_auth_token=auth_token)
    print("Model loaded successfully.")
except Exception as e:
    print(f"Failed to load model: {e}")
    exit()

# 加载音频数据
audio_path = r'C:\Users\yzhao116\Desktop\voice_sentiment\test_audio.wav'  # 替换为你的音频文件路径
waveform, sample_rate = torchaudio.load(audio_path)

# 检查采样率是否匹配，如果不是16kHz则重新采样
if sample_rate != 16000:
    print("Resampling audio to 16kHz...")
    transform = torchaudio.transforms.Resample(orig_freq=sample_rate, new_freq=16000)
    waveform = transform(waveform)
    sample_rate = 16000

# 调整音频形状为 (batch_size, num_channels, num_samples)
waveform = waveform.unsqueeze(0)  # 添加批次维度

# 获取Powerset多类编码
try:
    powerset_encoding = model(waveform)
    print("Powerset encoding computed successfully.")
except Exception as e:
    print(f"Failed to compute powerset encoding: {e}")
    exit()

# 将Powerset编码转换为多标签编码
max_speakers_per_chunk, max_speakers_per_frame = 3, 2
to_multilabel = Powerset(max_speakers_per_chunk, max_speakers_per_frame).to_multilabel
try:
    multilabel_encoding = to_multilabel(powerset_encoding)
    print("Multilabel encoding computed successfully.")
except Exception as e:
    print(f"Failed to convert to multilabel encoding: {e}")
    exit()

# 将张量转换为NumPy数组进行解析
multilabel_array = multilabel_encoding.numpy()
time_step = 0.01  # 每个时间步的持续时间，具体值根据模型确定

# 遍历数组以打印每个时间步的说话人状态
data = []
for i, frame in enumerate(multilabel_array[0]):  # 假设批次维度为0
    active_speakers = np.where(frame == 1)[0]  # 获取当前时间帧的活跃说话人索引
    if len(active_speakers) > 0:
        start_time = i * time_step
        print(f"Time {start_time:.2f}s: Active speakers - {active_speakers}")
        data.append({"time": start_time, "speakers": active_speakers.tolist()})

# 保存为CSV文件
df = pd.DataFrame(data)
df.to_csv('speaker_activity.csv', index=False)
print("Speaker activity saved to speaker_activity.csv")

# 可视化结果
time_steps = np.arange(multilabel_array.shape[1]) * time_step
for speaker_idx in range(multilabel_array.shape[2]):  # 遍历每个说话人
    plt.plot(time_steps, multilabel_array[0, :, speaker_idx], label=f'Speaker {speaker_idx}')

plt.xlabel('Time (s)')
plt.ylabel('Activation')
plt.title('Speaker Activity Over Time')
plt.legend()
plt.show()

