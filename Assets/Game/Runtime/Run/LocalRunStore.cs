using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace WaitYourTurn.Run
{
    /// <summary>Bounded JSON + checksum, temp/flush/atomic replacement and one last-good backup.</summary>
    public sealed class LocalRunStore
    {
        [Serializable] private sealed class Envelope { public int version = 1; public string payload, checksum; public bool cleared; }
        public const int MaximumBytes = 2 * 1024 * 1024;
        public string Path { get; }
        public string Diagnostic { get; private set; }
        public LocalRunStore(string path) => Path = path;
        public static string Hash(string text)
        { using (var hash = SHA256.Create()) return Convert.ToBase64String(hash.ComputeHash(Encoding.UTF8.GetBytes(text))); }
        public bool Save(RunSnapshot snapshot)
        {
            string payload = JsonUtility.ToJson(snapshot);
            return Write(new Envelope { payload = payload, checksum = Hash(payload) });
        }
        public bool Invalidate()
        {
            // Persist the retired identity first. A crash between primary/backup replacement
            // cannot revive this run through an older backup with the same identity.
            var previous = Load();
            if (previous != null && !string.IsNullOrEmpty(previous.runId))
            {
                try
                {
                    byte[] retired = Encoding.UTF8.GetBytes(previous.runId);
                    using (var stream = new FileStream(Path + ".ended.tmp", FileMode.Create, FileAccess.Write, FileShare.None))
                    { stream.Write(retired, 0, retired.Length); stream.Flush(true); }
                    if (File.Exists(Path + ".ended")) File.Replace(Path + ".ended.tmp", Path + ".ended", null);
                    else File.Move(Path + ".ended.tmp", Path + ".ended");
                }
                catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
                { Diagnostic = "Could not retire run identity: " + e.GetType().Name; return false; }
            }
            var empty = new Envelope { cleared = true, payload = "", checksum = Hash("") };
            if (!Write(empty)) return false;
            // A corrupt primary must not revive the previous live backup after a finished/new run.
            try { File.Copy(Path, Path + ".bak", true); return true; }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
            { Diagnostic = "Could not retire backup: " + e.GetType().Name; return false; }
        }
        private bool Write(Envelope value)
        {
            Diagnostic = null;
            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(value));
                if (bytes.Length > MaximumBytes) { Diagnostic = "Save exceeds size limit."; return false; }
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path));
                using (var stream = new FileStream(Path + ".tmp", FileMode.Create, FileAccess.Write, FileShare.None))
                { stream.Write(bytes, 0, bytes.Length); stream.Flush(true); }
                if (File.Exists(Path)) File.Replace(Path + ".tmp", Path, Path + ".bak");
                else File.Move(Path + ".tmp", Path);
                return true;
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException || e is NotSupportedException)
            { Diagnostic = "Save write failed: " + e.GetType().Name; return false; }
        }
        public RunSnapshot Load()
        {
            Diagnostic = null;
            if (Read(Path, out var snapshot)) return snapshot;
            if (Read(Path + ".bak", out snapshot)) { Diagnostic = "Recovered previous backup."; return snapshot; }
            Diagnostic = "No valid local run."; return null;
        }
        private bool Read(string path, out RunSnapshot snapshot)
        {
            snapshot = null;
            try
            {
                if (!File.Exists(path) || new FileInfo(path).Length > MaximumBytes) return false;
                var envelope = JsonUtility.FromJson<Envelope>(File.ReadAllText(path, Encoding.UTF8));
                if (envelope == null || envelope.version != 1 || envelope.payload == null || Hash(envelope.payload) != envelope.checksum) return false;
                if (envelope.cleared) return true;
                snapshot = JsonUtility.FromJson<RunSnapshot>(envelope.payload);
                if (snapshot != null && File.Exists(Path + ".ended") && new FileInfo(Path + ".ended").Length <= 128 &&
                    snapshot.runId == File.ReadAllText(Path + ".ended", Encoding.UTF8)) { snapshot = null; return true; }
                return snapshot != null;
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException || e is ArgumentException) { return false; }
        }
    }
}
