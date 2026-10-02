using System;
using System.Collections.Generic;
namespace Domain
{
    [Serializable]
    public sealed class StarRecordBook
    {
        [Serializable]
        public sealed class Entry
        {
            public string phase;
            public int mode;
            public float stars;
            public int errors;
            public int score;
        }
        public List<Entry> entries = new List<Entry>();
        public bool Update(ScoringRun run)
        {
            if (run == null || !run.Completed) return false;
            if (entries == null) entries = new List<Entry>();
            var entry = entries.Find(e => e.phase == run.PhaseId && e.mode == (int)run.Mode);
            if (entry == null)
            {
                entry = new Entry { phase = run.PhaseId, mode = (int)run.Mode, errors = run.Errors };
                entries.Add(entry);
            }
            if (run.Stars <= entry.stars && run.Score <= entry.score && run.Errors >= entry.errors) return false;
            entry.stars = Math.Max(entry.stars, run.Stars);
            entry.score = Math.Max(entry.score, run.Score);
            entry.errors = Math.Min(entry.errors, run.Errors);
            return true;
        }
        public float Best(string phase, GameMode mode) =>
            entries?.Find(e => e.phase == phase && e.mode == (int)mode)?.stars ?? 0;
        public int BestScore(string phase, GameMode mode) =>
            entries?.Find(e => e.phase == phase && e.mode == (int)mode)?.score ?? 0;
        public float Total(GameMode mode)
        {
            float total = 0;
            if (entries != null)
                foreach (var entry in entries) if (entry.mode == (int)mode) total += entry.stars;
            return total;
        }
    }
}


