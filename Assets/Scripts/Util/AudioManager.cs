using CoffeeKing.Scoring;
using UnityEngine;

namespace CoffeeKing.Util
{
    public sealed class AudioManager : MonoBehaviour
    {
        private AudioSource sfxSource;
        private AudioClip snapClip;
        private AudioClip serveClip;
        private AudioClip uiClip;
        private AudioClip stageCompleteClip;
        private AudioClip perfectClip;
        private AudioClip goodClip;
        private AudioClip badClip;

        private void Awake()
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.volume = 0.18f;

            snapClip = CreateTone("Snap", 700f, 0.08f);
            serveClip = CreateTone("Serve", 880f, 0.14f);
            uiClip = CreateTone("Ui", 540f, 0.07f);
            stageCompleteClip = CreateChord("StageComplete", new[] { 440f, 554f, 659f }, 0.45f);
            perfectClip = CreateTone("Perfect", 980f, 0.16f);
            goodClip = CreateTone("Good", 760f, 0.13f);
            badClip = CreateTone("Bad", 320f, 0.18f);
        }

        public void PlaySnap()
        {
            Play(snapClip);
        }

        public void PlayServe()
        {
            Play(serveClip);
        }

        public void PlayUiClick()
        {
            Play(uiClip);
        }

        public void PlayStageComplete()
        {
            Play(stageCompleteClip);
        }

        public void PlayGauge(QualityGrade qualityGrade)
        {
            switch (qualityGrade)
            {
                case QualityGrade.Perfect:
                    Play(perfectClip);
                    break;
                case QualityGrade.Good:
                    Play(goodClip);
                    break;
                default:
                    Play(badClip);
                    break;
            }
        }

        private void Play(AudioClip clip)
        {
            if (clip != null)
            {
                sfxSource.PlayOneShot(clip);
            }
        }

        private static AudioClip CreateTone(string clipName, float frequency, float duration)
        {
            const int sampleRate = 44100;
            var sampleCount = Mathf.CeilToInt(sampleRate * duration);
            var samples = new float[sampleCount];

            for (var index = 0; index < sampleCount; index++)
            {
                var envelope = 1f - (index / (float)sampleCount);
                samples[index] = Mathf.Sin(2f * Mathf.PI * frequency * index / sampleRate) * envelope * 0.3f;
            }

            var clip = AudioClip.Create(clipName, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip CreateChord(string clipName, float[] frequencies, float duration)
        {
            const int sampleRate = 44100;
            var sampleCount = Mathf.CeilToInt(sampleRate * duration);
            var samples = new float[sampleCount];

            for (var index = 0; index < sampleCount; index++)
            {
                var envelope = 1f - (index / (float)sampleCount);
                var value = 0f;
                for (var frequencyIndex = 0; frequencyIndex < frequencies.Length; frequencyIndex++)
                {
                    value += Mathf.Sin(2f * Mathf.PI * frequencies[frequencyIndex] * index / sampleRate);
                }

                samples[index] = value / frequencies.Length * envelope * 0.25f;
            }

            var clip = AudioClip.Create(clipName, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
