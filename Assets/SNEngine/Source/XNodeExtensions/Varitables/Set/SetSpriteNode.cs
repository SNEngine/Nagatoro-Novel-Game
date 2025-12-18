using SNEngine.Serialisation;
using UnityEngine;

namespace SiphoinUnityHelpers.XNodeExtensions.Varitables.Set
{
    public class SetSpriteNode : SetVaritableNode<Sprite>
    {
        protected override void OnSetTargetValueChanged(VaritableNode<Sprite> targetNode, Sprite newValue)
        {
            SNEngineSerialization.AddAssetToLibrary<SpriteLibrary>(newValue);
        }
    }
}
