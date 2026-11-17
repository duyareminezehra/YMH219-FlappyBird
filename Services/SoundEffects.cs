using System;
using System.IO;
using System.Media;
using System.Threading.Tasks;

namespace YMH219_FlappyBird.Services
{
    /// <summary>
    /// Harici dosya (.wav/.mp3) gerektirmeden, 100% saf C# ile bellekte anlık ses dalgaları (PCM WAV)
    /// üreterek çalan ve uygulamanın her bilgisayarda sorunsuz çalışmasını sağlayan ses motoru.
    /// </summary>
    public static class SoundEffects
    {
        public static bool Enabled { get; set; } = true;

        public static void PlayFlap()
        {
            if (!Enabled) return;
            Task.Run(() => PlayToneSweep(400, 680, 55, 0.25));
        }

        public static void PlayCoin()
        {
            if (!Enabled) return;
            Task.Run(() =>
            {
                PlayPcmWav(GenerateArpeggio(new[] { 988, 1318 }, 45, 0.35));
            });
        }

        public static void PlayPowerUp()
        {
            if (!Enabled) return;
            Task.Run(() =>
            {
                PlayPcmWav(GenerateArpeggio(new[] { 523, 659, 784, 1046 }, 50, 0.38));
            });
        }

        public static void PlayShieldBreak()
        {
            if (!Enabled) return;
            Task.Run(() => PlayToneSweep(720, 220, 110, 0.4));
        }

        public static void PlayCrash()
        {
            if (!Enabled) return;
            Task.Run(() => PlayToneSweep(340, 110, 180, 0.45));
        }

        public static void PlayNewRecord()
        {
            if (!Enabled) return;
            Task.Run(() =>
            {
                PlayPcmWav(GenerateArpeggio(new[] { 523, 659, 784, 1046, 1318 }, 70, 0.4));
            });
        }

        private static void PlayToneSweep(int startFreq, int endFreq, int durationMs, double volume)
        {
            try
            {
                int sampleRate = 22050;
                int totalSamples = sampleRate * durationMs / 1000;
                byte[] wav = GenerateWavBytes(sampleRate, totalSamples, (i, t) =>
                {
                    double progress = (double)i / totalSamples;
                    double freq = startFreq + (endFreq - startFreq) * progress;
                    double env = 1.0 - progress; // Linear decay
                    return Math.Sin(2 * Math.PI * freq * t) * env * volume;
                });
                PlayPcmWav(wav);
            }
            catch
            {
                // Ses kartı veya oturum hatasında oyunu durdurma
            }
        }

        private static byte[] GenerateArpeggio(int[] frequencies, int noteDurationMs, double volume)
        {
            int sampleRate = 22050;
            int samplesPerNote = sampleRate * noteDurationMs / 1000;
            int totalSamples = samplesPerNote * frequencies.Length;

            return GenerateWavBytes(sampleRate, totalSamples, (i, t) =>
            {
                int noteIndex = Math.Min(frequencies.Length - 1, i / samplesPerNote);
                int sampleInNote = i % samplesPerNote;
                double noteProgress = (double)sampleInNote / samplesPerNote;
                double env = Math.Max(0.0, 1.0 - (noteProgress * 0.8));
                return Math.Sin(2 * Math.PI * frequencies[noteIndex] * t) * env * volume;
            });
        }

        private static byte[] GenerateWavBytes(int sampleRate, int totalSamples, Func<int, double, double> sampleFunc)
        {
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);

            // WAV RIFF Başlığı
            bw.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
            bw.Write(36 + totalSamples * 2);
            bw.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));
            bw.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
            bw.Write(16); // Alt blok boyutu
            bw.Write((short)1); // PCM
            bw.Write((short)1); // Mono
            bw.Write(sampleRate);
            bw.Write(sampleRate * 2); // Byte hızı
            bw.Write((short)2); // Blok hizalama
            bw.Write((short)16); // Bit derinliği
            bw.Write(System.Text.Encoding.ASCII.GetBytes("data"));
            bw.Write(totalSamples * 2);

            for (int i = 0; i < totalSamples; i++)
            {
                double t = (double)i / sampleRate;
                double sample = Math.Clamp(sampleFunc(i, t), -1.0, 1.0);
                short val = (short)(sample * short.MaxValue);
                bw.Write(val);
            }

            return ms.ToArray();
        }

        private static void PlayPcmWav(byte[] wavBytes)
        {
            try
            {
                using var ms = new MemoryStream(wavBytes);
                using var player = new SoundPlayer(ms);
                player.Play();
            }
            catch
            {
                // Yoksay
            }
        }
    }
}
