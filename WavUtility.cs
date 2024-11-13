using System;
using System.IO;
using UnityEngine;

public static class WavUtility
{
    // 将 AudioClip 转换为 WAV 格式的字节数组
    public static byte[] FromAudioClip(AudioClip clip)
    {
        MemoryStream stream = new MemoryStream();

        int headerSize = 44; // WAV 文件头的字节数
        // 提取音频数据
        float[] samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);

        // 将浮点数样本转换为 16 位 PCM 数据
        short[] intData = new short[samples.Length];
        byte[] bytesData = new byte[samples.Length * 2]; // 每个样本 2 个字节

        float rescaleFactor = 32767; // 将浮点数从 -1 到 1 映射到短整数

        for (int i = 0; i < samples.Length; i++)
        {
            intData[i] = (short)(samples[i] * rescaleFactor);
            byte[] byteArr = BitConverter.GetBytes(intData[i]);
            // 小端序
            bytesData[i * 2] = byteArr[0];
            bytesData[i * 2 + 1] = byteArr[1];
        }

        // 写入 WAV 文件头
        WriteHeader(stream, clip, bytesData.Length);

        // 写入音频数据
        stream.Write(bytesData, 0, bytesData.Length);

        // 返回字节数组
        return stream.ToArray();
    }

    // 写入 WAV 文件头信息
    private static void WriteHeader(Stream stream, AudioClip clip, int dataLength)
    {
        int sampleRate = clip.frequency;
        int channels = clip.channels;
        int byteRate = sampleRate * channels * 2; // 每秒的数据量

        // RIFF 标记
        stream.Write(System.Text.Encoding.UTF8.GetBytes("RIFF"), 0, 4);
        // 文件大小（不含前8个字节）
        stream.Write(BitConverter.GetBytes(dataLength + 36), 0, 4);
        // WAVE 标记
        stream.Write(System.Text.Encoding.UTF8.GetBytes("WAVE"), 0, 4);
        // fmt 子块
        stream.Write(System.Text.Encoding.UTF8.GetBytes("fmt "), 0, 4);
        // 子块大小（16 for PCM）
        stream.Write(BitConverter.GetBytes(16), 0, 4);
        // 音频格式（1 = PCM）
        stream.Write(BitConverter.GetBytes((ushort)1), 0, 2);
        // 通道数
        stream.Write(BitConverter.GetBytes((ushort)channels), 0, 2);
        // 采样率
        stream.Write(BitConverter.GetBytes(sampleRate), 0, 4);
        // 字节率
        stream.Write(BitConverter.GetBytes(byteRate), 0, 4);
        // 块对齐
        stream.Write(BitConverter.GetBytes((ushort)(channels * 2)), 0, 2);
        // 每个样本的位数
        stream.Write(BitConverter.GetBytes((ushort)16), 0, 2);
        // data 子块
        stream.Write(System.Text.Encoding.UTF8.GetBytes("data"), 0, 4);
        // 数据大小
        stream.Write(BitConverter.GetBytes(dataLength), 0, 4);
    }
}
