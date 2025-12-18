using System;
using System.Collections.Generic;
using UnityEngine;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SNEngine.Serialization
{
    public abstract partial class BaseAssetLibrary<T> : ScriptableObject where T : UnityEngine.Object
    {

        [SerializeField] private List<Entry> _entries = new List<Entry>();

        public IReadOnlyList<Entry> Entries => _entries;

        protected Dictionary<string, T> GuidToAsset { get; set; } = new Dictionary<string, T>();
        protected Dictionary<T, string> AssetToGuid { get; set; } = new Dictionary<T, string>();

        public virtual void Initialize()
        {
            GuidToAsset.Clear();
            AssetToGuid.Clear();

            foreach (var entry in _entries)
            {
                if (entry.Asset != null && !string.IsNullOrEmpty(entry.Guid))
                {
                    GuidToAsset[entry.Guid] = entry.Asset;
                    AssetToGuid[entry.Asset] = entry.Guid;
                }
            }
        }

        public void Add(T asset)
        {
            if (asset == null) return;

            string guid = string.Empty;

#if UNITY_EDITOR
            string path = AssetDatabase.GetAssetPath(asset);
            if (!string.IsNullOrEmpty(path))
            {
                guid = AssetDatabase.AssetPathToGUID(path);
            }
#endif

            if (string.IsNullOrEmpty(guid))
            {
                guid = Guid.NewGuid().ToString();
            }

            if (!_entries.Exists(e => e.Guid == guid))
            {
                _entries.Add(new Entry { Guid = guid, Asset = asset });
#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
#endif
            }

            GuidToAsset[guid] = asset;
            AssetToGuid[asset] = guid;
        }

        public T GetAsset(string guid) =>
            GuidToAsset.TryGetValue(guid, out var asset) ? asset : null;

        public string GetGuid(T asset) =>
            AssetToGuid.TryGetValue(asset, out var guid) ? guid : null;
    }

}