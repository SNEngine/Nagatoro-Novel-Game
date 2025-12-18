using Newtonsoft.Json;
using SNEngine.Debugging;
using SNEngine.Serialisation;
using SNEngine.Utils;
using UnityEngine;

namespace SNEngine.Services
{
    [CreateAssetMenu(menuName = "SNEngine/Services/Asset Library Service")]
    public class AssetLibraryService : ServiceBase
    {
        private UnityContractResolver _resolver;

        public override void Initialize()
        {
            _resolver = new();

            var libraries = ResourceLoader.LoadAllCustomizable<SpriteLibrary>("AssetLibraries");
            foreach (var library in libraries)
            {
                _resolver.RegisterLibrary(library);
            }

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                ContractResolver = _resolver,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.Indented
            };

            NovelGameDebug.Log($"JSON settings up");
        }
    }
}
