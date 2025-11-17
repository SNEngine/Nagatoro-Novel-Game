
using UnityEngine;
using SNEngine.Services;
namespace CoreGame.Services
{
    [CreateAssetMenu(menuName = "SNEngine/Service/New FightService")]
    public class FightService : ServiceBase
    {

        public override void Initialize()
        {
        }

        public override void ResetState()
        {
            // Called after a clear screen, or when starting a new dialogue/game state
        }
    }
}