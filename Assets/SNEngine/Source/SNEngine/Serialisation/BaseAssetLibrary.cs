using System;
using System.Collections.Generic;
using UnityEngine;
using SNEngine.Debugging;
using System.Linq;
using UnityEditor.VersionControl;





#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SNEngine.Serialization
{
    public abstract class BaseAssetLibrary : ScriptableObjectIdentity
    {

    }
    public abstract partial class BaseAssetLibrary<T> : BaseAssetLibrary where T : UnityEngine.Object
    {

        [SerializeField] private List<Entry> _entries = new List<Entry>();

        public IReadOnlyList<Entry> Entries => _entries;

        public void Add(object asset)
        {
            var targetType = GetTypeAsset();

            if (asset.GetType() != targetType)
            {
                NovelGameDebug.LogError($"invalid type asset for library {GetType().Name} Type: {asset.GetType().Name}");
                return;
            }
            if (asset is null)
            {
                return;
            }

            string guid = string.Empty;
            T convertedAsset = asset as T;

#if UNITY_EDITOR
            string path = AssetDatabase.GetAssetPath(convertedAsset);
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
                _entries.Add(new Entry { Guid = guid, Asset = convertedAsset });
#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
#endif
            }
        }

        public T GetAsset(string guid)
        {
            var entity = Entries.FirstOrDefault(x => x.Guid == guid);
            if (entity is null)
            {
                return null;
            }

            else
            {
                return entity.Asset;
            }
        }

        public string GetGuid(T asset)
        {
            var entity = Entries.FirstOrDefault(x => x.Asset == asset);
            if (entity is null)
            {
                return null;
            }

            else
            {
                return entity.Guid;
            }
        }
             

        public Type GetTypeAsset ()
        {
            return typeof(T); 
        }
    }

}