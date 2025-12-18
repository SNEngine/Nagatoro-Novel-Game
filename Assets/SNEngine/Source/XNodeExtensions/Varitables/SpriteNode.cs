using SNEngine.Serialisation;
using UnityEngine;

namespace SiphoinUnityHelpers.XNodeExtensions.Varitables
{
    [NodeTint("#464778")]
    public class SpriteNode : UnityVaritableNode<Sprite, SpriteLibrary>
    {
        protected override void OnValueChanged(Sprite oldValue, Sprite newValue)
        {
            SNEngineSerialization.AddAssetToLibrary<SpriteLibrary>(newValue);
        }
    }
}
