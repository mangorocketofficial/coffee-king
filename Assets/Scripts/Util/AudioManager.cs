using System;
using CoffeeKing.Scoring;
using UnityEngine;

namespace CoffeeKing.Util
{
    public sealed class AudioManager : MonoBehaviour
    {
        private const string AudioResourceRoot = "Audio";

        private AudioSource sfxSource;
        private AudioSource loopSource;
        private AudioSource bgmSource;

        private AudioClip snapClip;
        private AudioClip serveClip;
        private AudioClip uiClip;
        private AudioClip stageCompleteClip;
        private AudioClip stageFailClip;
        private AudioClip perfectClip;
        private AudioClip goodClip;
        private AudioClip badClip;
        private AudioClip grindingLoopClip;
        private AudioClip tampImpactClip;
        private AudioClip extractionLoopClip;
        private AudioClip steamLoopClip;
        private AudioClip pourClip;
        private AudioClip lidSnapClip;
        private AudioClip customerArrivalClip;
        private AudioClip customerTimeoutClip;
        private AudioClip bgmClip;

        private void Awake()
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.volume = 0.18f;

            loopSource = gameObject.AddComponent<AudioSource>();
            loopSource.playOnAwake = false;
            loopSource.loop = true;
            loopSource.volume = 0.12f;

            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.playOnAwake = false;
            bgmSource.loop = true;
            bgmSource.volume = 0.06f;

            snapClip = LoadClipOrFallback("snap_lock", () => CreateTone("Snap", 700f, 0.08f));
            serveClip = LoadClipOrFallback("serve", () => CreateTone("Serve", 880f, 0.14f));
            uiClip = LoadClipOrFallback("ui_click", () => CreateTone("Ui", 540f, 0.07f));
            stageCompleteClip = LoadClipOrFallback("stage_clear", () => CreateChord("StageComplete", new[] { 440f, 554f, 659f }, 0.45f));
            stageFailClip = LoadClipOrFallback("stage_fail", () => CreateDescendingTone("StageFail", 440f, 220f, 0.5f));
            perfectClip = LoadClipOrFallback("grade_perfect", () => CreateTone("Perfect", 980f, 0.16f));
            goodClip = LoadClipOrFallback("grade_good", () => CreateTone("Good", 760f, 0.13f));
            badClip = LoadClipOrFallback("grade_bad", () => CreateTone("Bad", 320f, 0.18f));

            grindingLoopClip = LoadClipOrFallback("grinding_loop", () => CreateGrindingLoop("GrindingLoop", 1.0f));
            tampImpactClip = LoadClipOrFallback("tamp_impact", () => CreateImpact("TampImpact", 0.1f));
            extractionLoopClip = LoadClipOrFallback("extraction_loop", () => CreateExtractionLoop("ExtractionLoop", 1.0f));
            steamLoopClip = LoadClipOrFallback("steam_loop", () => CreateNoiseLoop("SteamLoop", 1.0f));
            pourClip = LoadClipOrFallback("pour", () => CreatePourSound("Pour", 0.25f));
            lidSnapClip = LoadClipOrFallback("lid_snap", () => CreateTone("LidSnap", 1200f, 0.06f));
            customerArrivalClip = LoadClipOrFallback("customer_arrival", () => CreateChord("CustomerArrival", new[] { 880f, 1108f }, 0.18f));
            customerTimeoutClip = LoadClipOrFallback("customer_timeout", () => CreateDescendingTone("CustomerTimeout", 600f, 300f, 0.3f));
            bgmClip = LoadClipOrFallback("cafe_bgm", () => CreateBgmLoop("CafeBGM", 8.0f));
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

        public void PlayStageFail()
        {
            Play(stageFailClip);
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

        public void PlayTampImpact()
        {
            Play(tampImpactClip);
        }

        public void PlayPour()
        {
            Play(pourClip);
        }

        public void PlayLidSnap()
        {
            Play(lidSnapClip);
        }

        public void PlayCustomerArrival()
        {
            Play(customerArrivalClip);
        }

        public void PlayCustomerTimeout()
        {
            Play(customerTimeoutClip);
        }

        public void StartGrindingLoop()
        {
            StartLoop(grindingLoopClip);
        }

        public void StartExtractionLoop()
        {
            StartLoop(extractionLoopClip);
        }

        public void StartSteamLoop()
        {
            StartLoop(steamLoopClip);
        }

        public void StopLoop()
        {
            loopSource.Stop();
            loopSource.clip = null;
        }

        public void SetBgmVolume(float normalized01)
        {
            normalized01 = Mathf.Clamp01(normalized01);
            bgmSource.volume = normalized01 * 0.12f;
        }

        public void SetSfxVolume(float normalized01)
        {
            normalized01 = Mathf.Clamp01(normalized01);
            sfxSource.volume = normalized01 * 0.36f;
            loopSource.volume = normalized01 * 0.24f;
        }

        public void StartBgm()
        {
            if (bgmSource.isPlaying)
            {
                return;
            }

            bgmSource.clip = bgmClip;
            bgmSource.Play();
        }

        public void StopBgm()
        {
            bgmSource.Stop();
            bgmSource.clip = null;
        }

        private void Play(AudioClip clip)
        {
            if (clip != null)
            {
                sfxSource.PlayOneShot(clip);
            }
        }

        private void StartLoop(AudioClip clip)
        {
            if (clip == null)
            {
                return;
            }

            if (loopSource.clip == clip && loopSource.isPlaying)
            {
                return;
            }

            loopSource.clip = clip;
            loopSource.Play();
        }

        private static AudioClip LoadClipOrFallback(string resourceName, Func<AudioClip> fallbackFactory)
        {
            var loadedClip = Resources.Load<AudioClip>($"{AudioResourceRoot}/{resourceName}");
            if (loadedClip != null)
            {
                return loadedClip;
            }

            return fallbackFactory();
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

        private static AudioClip CreateDescendingTone(string clipName, float startFrequency, float endFrequency, float duration)
        {
            const int sampleRate = 44100;
            var sampleCount = Mathf.CeilToInt(sampleRate * duration);
            var samples = new float[sampleCount];

            for (var index = 0; index < sampleCount; index++)
            {
                var t = index / (float)sampleCount;
                var envelope = 1f - t;
                var frequency = Mathf.Lerp(startFrequency, endFrequency, t);
                samples[index] = Mathf.Sin(2f * Mathf.PI * frequency * index / sampleRate) * envelope * 0.25f;
            }

            var clip = AudioClip.Create(clipName, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip CreateGrindingLoop(string clipName, float duration)
        {
            const int sampleRate = 44100;
            var sampleCount = Mathf.CeilToInt(sampleRate * duration);
            var samples = new float[sampleCount];
            var state = (uint)42;

            for (var index = 0; index < sampleCount; index++)
            {
                var t = index / (float)sampleRate;
                // Low rumble with noise overlay
                var rumble = Mathf.Sin(2f * Mathf.PI * 80f * t) * 0.15f;
                var buzz = Mathf.Sin(2f * Mathf.PI * 240f * t) * 0.08f;
                state = state * 1103515245u + 12345u;
                var noise = ((state >> 16) & 0x7FFF) / (float)0x7FFF * 2f - 1f;
                samples[index] = (rumble + buzz + noise * 0.06f) * 0.4f;
            }

            var clip = AudioClip.Create(clipName, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip CreateImpact(string clipName, float duration)
        {
            const int sampleRate = 44100;
            var sampleCount = Mathf.CeilToInt(sampleRate * duration);
            var samples = new float[sampleCount];
            var state = (uint)77;

            for (var index = 0; index < sampleCount; index++)
            {
                var t = index / (float)sampleCount;
                var envelope = Mathf.Exp(-t * 20f);
                state = state * 1103515245u + 12345u;
                var noise = ((state >> 16) & 0x7FFF) / (float)0x7FFF * 2f - 1f;
                var thud = Mathf.Sin(2f * Mathf.PI * 120f * index / sampleRate);
                samples[index] = (thud * 0.5f + noise * 0.3f) * envelope * 0.35f;
            }

            var clip = AudioClip.Create(clipName, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip CreateExtractionLoop(string clipName, float duration)
        {
            const int sampleRate = 44100;
            var sampleCount = Mathf.CeilToInt(sampleRate * duration);
            var samples = new float[sampleCount];
            var state = (uint)101;

            for (var index = 0; index < sampleCount; index++)
            {
                var t = index / (float)sampleRate;
                // Dripping / bubbling pattern
                var drip = Mathf.Sin(2f * Mathf.PI * 400f * t) * Mathf.Sin(2f * Mathf.PI * 3f * t) * 0.12f;
                var flow = Mathf.Sin(2f * Mathf.PI * 150f * t) * 0.06f;
                state = state * 1103515245u + 12345u;
                var noise = ((state >> 16) & 0x7FFF) / (float)0x7FFF * 2f - 1f;
                samples[index] = (drip + flow + noise * 0.03f) * 0.35f;
            }

            var clip = AudioClip.Create(clipName, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip CreateNoiseLoop(string clipName, float duration)
        {
            const int sampleRate = 44100;
            var sampleCount = Mathf.CeilToInt(sampleRate * duration);
            var samples = new float[sampleCount];
            var state = (uint)200;
            var filteredNoise = 0f;

            for (var index = 0; index < sampleCount; index++)
            {
                var t = index / (float)sampleRate;
                state = state * 1103515245u + 12345u;
                var whiteNoise = ((state >> 16) & 0x7FFF) / (float)0x7FFF * 2f - 1f;
                // Simple low-pass filter for hiss character
                filteredNoise = filteredNoise * 0.85f + whiteNoise * 0.15f;
                var hiss = filteredNoise * 0.3f;
                var wobble = Mathf.Sin(2f * Mathf.PI * 6f * t) * 0.02f;
                samples[index] = (hiss + wobble) * 0.4f;
            }

            var clip = AudioClip.Create(clipName, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip CreatePourSound(string clipName, float duration)
        {
            const int sampleRate = 44100;
            var sampleCount = Mathf.CeilToInt(sampleRate * duration);
            var samples = new float[sampleCount];
            var state = (uint)55;
            var filteredNoise = 0f;

            for (var index = 0; index < sampleCount; index++)
            {
                var t = index / (float)sampleCount;
                var envelope = t < 0.1f ? t / 0.1f : 1f - (t - 0.1f) / 0.9f;
                state = state * 1103515245u + 12345u;
                var whiteNoise = ((state >> 16) & 0x7FFF) / (float)0x7FFF * 2f - 1f;
                filteredNoise = filteredNoise * 0.7f + whiteNoise * 0.3f;
                var trickle = Mathf.Sin(2f * Mathf.PI * 600f * index / sampleRate) * 0.05f;
                samples[index] = (filteredNoise * 0.2f + trickle) * envelope * 0.3f;
            }

            var clip = AudioClip.Create(clipName, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip CreateBgmLoop(string clipName, float duration)
        {
            const int sampleRate = 44100;
            var sampleCount = Mathf.CeilToInt(sampleRate * duration);
            var samples = new float[sampleCount];

            // Gentle cafe ambience: soft chord pad with subtle movement
            var chordFrequencies = new[] { 261.6f, 329.6f, 392.0f, 523.3f }; // C major + octave
            for (var index = 0; index < sampleCount; index++)
            {
                var t = index / (float)sampleRate;
                var value = 0f;
                for (var ci = 0; ci < chordFrequencies.Length; ci++)
                {
                    var freq = chordFrequencies[ci];
                    // Slow vibrato for warmth
                    var vibrato = 1f + Mathf.Sin(2f * Mathf.PI * (0.3f + ci * 0.1f) * t) * 0.003f;
                    value += Mathf.Sin(2f * Mathf.PI * freq * vibrato * t);
                }

                value /= chordFrequencies.Length;

                // Gentle volume swell
                var swell = 0.7f + 0.3f * Mathf.Sin(2f * Mathf.PI * 0.125f * t);
                samples[index] = value * swell * 0.08f;
            }

            var clip = AudioClip.Create(clipName, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
