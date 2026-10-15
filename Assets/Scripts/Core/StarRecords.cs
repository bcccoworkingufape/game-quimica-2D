using System;
using Domain;
using UnityEngine;

namespace Core
{
    /// <summary>Personal records on this device, separated by phase and difficulty.</summary>
    public static class StarRecords
    {
        private const string Key = "molz.starRecords.v4";
        private static StarRecordBook Read()
        {
            try { return JsonUtility.FromJson<StarRecordBook>(PlayerPrefs.GetString(Key, "{}")) ?? new StarRecordBook(); }
            catch (ArgumentException) { return new StarRecordBook(); }
        }
        public static void SaveBest(ScoringRun run)
        {
            var records = Read();
            if (!records.Update(run)) return;
            PlayerPrefs.SetString(Key, JsonUtility.ToJson(records));
            PlayerPrefs.Save();
        }
        public static float Best(string phase, GameMode mode) => Read().Best(phase, mode);
        public static int BestScore(string phase, GameMode mode) => Read().BestScore(phase, mode);
        public static float Total(GameMode mode) => Read().Total(mode);
    }
}

