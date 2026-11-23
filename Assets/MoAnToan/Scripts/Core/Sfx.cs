using System.Collections.Generic;
using UnityEngine;

namespace MoAnToan
{
    /// <summary>Âm thanh tổng hợp bằng code (không cần file âm thanh).</summary>
    public static class Sfx
    {
        static AudioSource src;
        static readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();
        public static bool Muted;

        public static void Init(GameObject host)
        {
            src = host.AddComponent<AudioSource>();
            src.playOnAwake = false;
            Make("ok", new[] { 660f, .08f, 880f, .13f }, 0);
            Make("bad", new[] { 230f, .14f, 170f, .22f }, 0);
            Make("tick", new[] { 990f, .05f }, 0);
            Make("tap", new[] { 520f, .03f }, 0);
            Make("win", new[] { 523f, .1f, 659f, .1f, 784f, .1f, 1046f, .28f }, 0);
            Make("step", new[] { 110f, .06f, 0f, .12f, 100f, .06f }, 1);
        }

        static void Make(string name, float[] seq, int wave)
        {
            const int rate = 44100;
            var data = new List<float>();
            for (int i = 0; i < seq.Length; i += 2)
            {
                float f = seq[i], d = seq[i + 1];
                int n = (int)(d * rate);
                for (int k = 0; k < n; k++)
                {
                    float t = k / (float)rate;
                    float env = Mathf.Exp(-5f * t / d) * 0.22f;
                    float ph = f * t;
                    float s = f <= 0 ? 0 : (wave == 0 ? (2f * Mathf.Abs(2f * (ph - Mathf.Floor(ph + 0.5f))) - 1f) : Mathf.Sin(2 * Mathf.PI * ph));
                    data.Add(s * env);
                }
            }
            var c = AudioClip.Create(name, data.Count, 1, rate, false);
            c.SetData(data.ToArray(), 0);
            clips[name] = c;
        }

        public static void Play(string name)
        {
            AudioClip c;
            if (Muted || src == null || !clips.TryGetValue(name, out c)) return;
            src.PlayOneShot(c);
        }
    }
}
