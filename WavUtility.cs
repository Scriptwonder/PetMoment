using System;
using System.IO;
using UnityEngine;

public static class WavUtility
{
    // �� AudioClip ת��Ϊ WAV ��ʽ���ֽ�����
    public static byte[] FromAudioClip(AudioClip clip)
    {
        MemoryStream stream = new MemoryStream();

        int headerSize = 44; // WAV �ļ�ͷ���ֽ���
        // ��ȡ��Ƶ����
        float[] samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);

        // ������������ת��Ϊ 16 λ PCM ����
        short[] intData = new short[samples.Length];
        byte[] bytesData = new byte[samples.Length * 2]; // ÿ������ 2 ���ֽ�

        float rescaleFactor = 32767; // ���������� -1 �� 1 ӳ�䵽������

        for (int i = 0; i < samples.Length; i++)
        {
            intData[i] = (short)(samples[i] * rescaleFactor);
            byte[] byteArr = BitConverter.GetBytes(intData[i]);
            // С����
            bytesData[i * 2] = byteArr[0];
            bytesData[i * 2 + 1] = byteArr[1];
        }

        // д�� WAV �ļ�ͷ
        WriteHeader(stream, clip, bytesData.Length);

        // д����Ƶ����
        stream.Write(bytesData, 0, bytesData.Length);

        // �����ֽ�����
        return stream.ToArray();
    }

    // д�� WAV �ļ�ͷ��Ϣ
    private static void WriteHeader(Stream stream, AudioClip clip, int dataLength)
    {
        int sampleRate = clip.frequency;
        int channels = clip.channels;
        int byteRate = sampleRate * channels * 2; // ÿ���������

        // RIFF ���
        stream.Write(System.Text.Encoding.UTF8.GetBytes("RIFF"), 0, 4);
        // �ļ���С������ǰ8���ֽڣ�
        stream.Write(BitConverter.GetBytes(dataLength + 36), 0, 4);
        // WAVE ���
        stream.Write(System.Text.Encoding.UTF8.GetBytes("WAVE"), 0, 4);
        // fmt �ӿ�
        stream.Write(System.Text.Encoding.UTF8.GetBytes("fmt "), 0, 4);
        // �ӿ��С��16 for PCM��
        stream.Write(BitConverter.GetBytes(16), 0, 4);
        // ��Ƶ��ʽ��1 = PCM��
        stream.Write(BitConverter.GetBytes((ushort)1), 0, 2);
        // ͨ����
        stream.Write(BitConverter.GetBytes((ushort)channels), 0, 2);
        // ������
        stream.Write(BitConverter.GetBytes(sampleRate), 0, 4);
        // �ֽ���
        stream.Write(BitConverter.GetBytes(byteRate), 0, 4);
        // �����
        stream.Write(BitConverter.GetBytes((ushort)(channels * 2)), 0, 2);
        // ÿ��������λ��
        stream.Write(BitConverter.GetBytes((ushort)16), 0, 2);
        // data �ӿ�
        stream.Write(System.Text.Encoding.UTF8.GetBytes("data"), 0, 4);
        // ���ݴ�С
        stream.Write(BitConverter.GetBytes(dataLength), 0, 4);
    }
}
