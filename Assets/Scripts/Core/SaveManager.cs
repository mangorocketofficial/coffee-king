using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CoffeeKing.Core
{
    [Serializable]
    public sealed class SaveData
    {
        public int version = 2;
        public int highestCompletedDay = 0;
        public long totalEarnings = 0;
        public List<string> tutorialFlags = new List<string>();
        public int bgmVolume = 50;
        public int sfxVolume = 50;
        public bool vibrationEnabled = true;
    }

    public sealed class SaveManager
    {
        private const string FileName = "save.json";

        public static SaveManager Instance { get; private set; }

        private SaveData data;
        private readonly string filePath;

        public SaveManager()
        {
            filePath = Path.Combine(Application.persistentDataPath, FileName);
            Instance = this;
            Load();
        }

        public int HighestCompletedDay => data.highestCompletedDay;
        public int NextDay => data.highestCompletedDay + 1;
        public long TotalEarnings => data.totalEarnings;

        public int BgmVolume => data.bgmVolume;
        public int SfxVolume => data.sfxVolume;
        public bool VibrationEnabled => data.vibrationEnabled;

        public void SetBgmVolume(int volume)
        {
            volume = Math.Max(0, Math.Min(100, volume));
            if (data.bgmVolume != volume)
            {
                data.bgmVolume = volume;
                Save();
            }
        }

        public void SetSfxVolume(int volume)
        {
            volume = Math.Max(0, Math.Min(100, volume));
            if (data.sfxVolume != volume)
            {
                data.sfxVolume = volume;
                Save();
            }
        }

        public void SetVibrationEnabled(bool enabled)
        {
            if (data.vibrationEnabled != enabled)
            {
                data.vibrationEnabled = enabled;
                Save();
            }
        }

        public bool HasSeenTutorial(string key)
        {
            return data.tutorialFlags.Contains(key);
        }

        public void MarkTutorialSeen(string key)
        {
            if (!data.tutorialFlags.Contains(key))
            {
                data.tutorialFlags.Add(key);
                Save();
            }
        }

        public void RecordDayResult(int dayNumber, int dailyEarnings)
        {
            var changed = false;

            if (dayNumber > data.highestCompletedDay)
            {
                data.highestCompletedDay = dayNumber;
                changed = true;
            }

            if (dailyEarnings > 0)
            {
                data.totalEarnings += dailyEarnings;
                changed = true;
            }

            if (changed)
            {
                Save();
            }
        }

        public void ClearSave()
        {
            data = new SaveData();
            Save();
            Debug.Log("[SaveManager] Save data cleared.");
        }

        public void Save()
        {
            try
            {
                var json = JsonUtility.ToJson(data, true);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveManager] Failed to save: {ex.Message}");
            }
        }

        private void Load()
        {
            if (File.Exists(filePath))
            {
                try
                {
                    var json = File.ReadAllText(filePath);
                    data = JsonUtility.FromJson<SaveData>(json);

                    if (data == null)
                    {
                        data = new SaveData();
                    }

                    EnsureDefaults();
                    Debug.Log($"[SaveManager] Loaded save from {filePath}");
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[SaveManager] Failed to load save, creating new: {ex.Message}");
                    data = new SaveData();
                }
            }
            else
            {
                data = new SaveData();
                Debug.Log("[SaveManager] No save file found, starting fresh.");
            }
        }

        private void EnsureDefaults()
        {
            if (data.tutorialFlags == null)
            {
                data.tutorialFlags = new List<string>();
            }
        }
    }
}
