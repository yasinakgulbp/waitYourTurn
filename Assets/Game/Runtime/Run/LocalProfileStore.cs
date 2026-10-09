using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace WaitYourTurn.Run
{
    /// <summary>Profile is independent from an active run; credit and receipt are written together.</summary>
    public sealed class LocalProfileStore
    {
        [Serializable] private sealed class Envelope { public int version = 1; public string payload, checksum; }
        public string Path { get; }
        public string Diagnostic { get; private set; }
        private const int MaximumBytes = 64 * 1024;
        public LocalProfileStore(string path) => Path = path;
        public PlayerProfile Load()
        {
            Diagnostic = null;
            if (Read(Path, out var profile)) return profile;
            if (Read(Path + ".bak", out profile)) { Diagnostic = "Profile backup recovered"; return profile; }
            if (!File.Exists(Path) && !File.Exists(Path + ".bak")) return new PlayerProfile();
            Diagnostic = "Profile unreadable; existing files preserved"; return null;
        }
        private bool Read(string path, out PlayerProfile profile)
        {
            profile = null;
            try
            {
                if (!File.Exists(path) || new FileInfo(path).Length > MaximumBytes) return false;
                var envelope = JsonUtility.FromJson<Envelope>(File.ReadAllText(path, Encoding.UTF8));
                if (envelope == null || envelope.version != 1 || envelope.payload == null || LocalRunStore.Hash(envelope.payload) != envelope.checksum) return false;
                profile = JsonUtility.FromJson<PlayerProfile>(envelope.payload); return profile != null && profile.Valid();
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException || e is ArgumentException) { return false; }
        }
        public bool Save(PlayerProfile profile)
        {
            if (profile == null || !profile.Valid()) { Diagnostic = "Invalid profile"; return false; }
            try
            {
                string payload = JsonUtility.ToJson(profile);
                byte[] bytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(new Envelope { payload = payload, checksum = LocalRunStore.Hash(payload) }));
                if (bytes.Length > MaximumBytes) { Diagnostic = "Profile size limit"; return false; }
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path));
                using (var file = new FileStream(Path + ".tmp", FileMode.Create, FileAccess.Write, FileShare.None))
                { file.Write(bytes, 0, bytes.Length); file.Flush(true); }
                if (File.Exists(Path)) File.Replace(Path + ".tmp", Path, Path + ".bak"); else File.Move(Path + ".tmp", Path);
                Diagnostic = null; return true;
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException || e is NotSupportedException)
            { Diagnostic = "Profile write failed: " + e.GetType().Name; return false; }
        }
    }
}
