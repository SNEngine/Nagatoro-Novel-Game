using CoreGame.FightSystem;
using CoreGame.FightSystem.Models;
using CoreGame.FightSystem.UI;
using SNEngine;
using SNEngine.CharacterSystem;
using SNEngine.MainMenuSystem;
using SNEngine.Services;
using SNEngine.Utils;
using System.Collections.Generic;
using UnityEngine;
using System;
using CoreGame.FightSystem.AI;
using Object = UnityEngine.Object;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace CoreGame.Services
{
    [CreateAssetMenu(menuName = "SNEngine/Service/New FightService")]
    public class FightService : ServiceBase
    {
        private CharacterService _characterService;
        private Dictionary<Character, CharacterFightData> _currentStatsCharacters;
        private Dictionary<Character, IFightComponent> _fightComponents;
        private IFightWindow _fightWindow;
        private const string FIGHT_WINDOW_VANILLA_PATH = "FightWindow";
        private const float ENEMY_TURN_DELAY = 0.5f;
        private FightTurnOwner _fightTurnOwner = FightTurnOwner.Player;

        private FightCharacter _playerCharacter;
        private FightCharacter _enemyCharacter;
        private AIFighter _aiFighter;

        private bool _isPlayerGuarding;
        private bool _isEnemyGuarding;

        public event Action<FightResult> OnFightEnded;

        public override void Initialize()
        {
            _characterService = NovelGame.Instance.GetService<CharacterService>();

            var ui = NovelGame.Instance.GetService<UIService>();

            var input = ResourceLoader.LoadCustomOrVanilla<FightWindow>(FIGHT_WINDOW_VANILLA_PATH);

            if (input == null)
            {
                return;
            }

            var prefab = Object.Instantiate(input);

            prefab.name = input.name;

            _fightWindow = prefab;

            ui.AddElementToUIContainer(prefab.gameObject);

            prefab.gameObject.SetActive(false);
        }

        public override void ResetState()
        {
            if (_fightWindow != null)
            {
                _fightWindow.OnTurnExecuted -= OnPlayerTurnExecuted;
            }
            HideCharacters();
            ClearupFightComponents();
            _currentStatsCharacters = null;
            _fightWindow.ResetState();
            _aiFighter = null;
            _isPlayerGuarding = false;
            _isEnemyGuarding = false;
        }

        public void TurnFight(FightCharacter playerCharacter, FightCharacter enemyCharacter)
        {
            _playerCharacter = playerCharacter;
            _enemyCharacter = enemyCharacter;

            _currentStatsCharacters = new();
            _fightComponents = new();
            ShowCharacter(playerCharacter.ReferenceCharacter);
            ShowCharacter(enemyCharacter.ReferenceCharacter);
            SetupCharacterForFight(playerCharacter);
            SetupCharacterForFight(enemyCharacter);

            IFightComponent playerComp = _fightComponents[playerCharacter.ReferenceCharacter];
            IFightComponent enemyComp = _fightComponents[enemyCharacter.ReferenceCharacter];

            _fightWindow.SetData(playerComp, enemyComp);

            _aiFighter = new AIFighter(_enemyCharacter, enemyComp, _playerCharacter, playerComp);

            _isPlayerGuarding = false;
            _isEnemyGuarding = false;

            _fightWindow.OnTurnExecuted += OnPlayerTurnExecuted;

            _fightWindow.Show();
            _fightTurnOwner = FightTurnOwner.Player;
        }

        private void OnPlayerTurnExecuted(PlayerAction action)
        {
            if (_fightTurnOwner != FightTurnOwner.Player)
            {
                return;
            }

            HandlePlayerAction(action);

            if (CheckFightEndConditions())
            {
                return;
            }

            _fightTurnOwner = FightTurnOwner.Enemy;
            _fightWindow.HidePanelAction();

            ExecuteEnemyTurn().Forget();
        }

        private void HandlePlayerAction(PlayerAction action)
        {
            IFightComponent enemyComp = _fightComponents[_enemyCharacter.ReferenceCharacter];
            float playerDamage = _playerCharacter.Damage;

            _isPlayerGuarding = false;

            switch (action)
            {
                case PlayerAction.Attack:
                    HandleAttackAction(enemyComp, playerDamage, _enemyCharacter);
                    break;
                case PlayerAction.Guard:
                    _isPlayerGuarding = true;
                    break;
                case PlayerAction.Wait:

                    break;
                case PlayerAction.Skill:

                    break;
            }
        }

        private void HandleEnemyAction(PlayerAction action)
        {
            IFightComponent playerComp = _fightComponents[_playerCharacter.ReferenceCharacter];
            float enemyDamage = _enemyCharacter.Damage;

            _isEnemyGuarding = false;

            switch (action)
            {
                case PlayerAction.Attack:
                    HandleAttackAction(playerComp, enemyDamage, _playerCharacter);
                    break;
                case PlayerAction.Guard:
                    _isEnemyGuarding = true;
                    break;
                case PlayerAction.Wait:

                    break;
                case PlayerAction.Skill:

                    break;
            }
        }

        private void HandleAttackAction(IFightComponent targetComponent, float baseDamage, FightCharacter targetCharacter)
        {
            float finalDamage = baseDamage;

            bool isTargetGuarding = targetCharacter == _playerCharacter ? _isPlayerGuarding : _isEnemyGuarding;

            if (isTargetGuarding)
            {
                float reduction = targetCharacter.GuardReductionPercentage;
                finalDamage *= (1f - reduction);

                if (targetCharacter == _playerCharacter)
                {
                    _isPlayerGuarding = false;
                }
                else
                {
                    _isEnemyGuarding = false;
                }
            }

            targetComponent.HealthComponent.TakeDamage(finalDamage);
        }

        private bool CheckFightEndConditions()
        {
            IFightComponent playerComp = _fightComponents[_playerCharacter.ReferenceCharacter];
            IFightComponent enemyComp = _fightComponents[_enemyCharacter.ReferenceCharacter];

            bool playerDead = playerComp.HealthComponent.CurrentHealth <= 0;
            bool enemyDead = enemyComp.HealthComponent.CurrentHealth <= 0;

            if (playerDead && enemyDead)
            {
                EndFight(FightResult.Tie);
                return true;
            }

            if (playerDead)
            {
                EndFight(FightResult.Defeat);
                return true;
            }

            if (enemyDead)
            {
                EndFight(FightResult.Victory);
                return true;
            }

            return false;
        }

        private void EndFight(FightResult result)
        {
            _fightWindow.OnTurnExecuted -= OnPlayerTurnExecuted;
            _fightWindow.Hide();
            HideCharacters();
            ClearupFightComponents();

            OnFightEnded?.Invoke(result);
        }

        private async UniTask ExecuteEnemyTurn()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(ENEMY_TURN_DELAY), DelayType.DeltaTime, PlayerLoopTiming.Update, CancellationToken.None);

            PlayerAction enemyAction = _aiFighter.DecideAction();

            HandleEnemyAction(enemyAction);

            if (CheckFightEndConditions())
            {
                return;
            }

            await UniTask.Delay(TimeSpan.FromSeconds(ENEMY_TURN_DELAY), DelayType.DeltaTime, PlayerLoopTiming.Update, CancellationToken.None);

            CompleteEnemyTurn();
        }

        private void CompleteEnemyTurn()
        {
            _fightTurnOwner = FightTurnOwner.Player;
            _fightWindow.ShowPanelAction();
        }

        private void SetupCharacterForFight(FightCharacter character)
        {
            CharacterFightData fightData = new(character);
            _currentStatsCharacters[character.ReferenceCharacter] = fightData;

            ICharacterRenderer characterRenderer = _characterService.GetWorldCharacter(character.ReferenceCharacter);
            IFightComponent fightComponent = characterRenderer.AddComponent<FightComponent>();
            fightComponent.AddComponents();
            fightComponent.HealthComponent.SetData(character.Health);
            fightComponent.ManaComponent.SetData(character.Mana);
            _fightComponents.Add(character.ReferenceCharacter, fightComponent);
        }

        private void ShowCharacter(Character character)
        {
            _characterService.ShowCharacter(character);
        }

        private void HideCharacter(Character character)
        {
            _characterService.HideCharacter(character);
        }

        private void ClearupFightComponents()
        {
            foreach (var component in _fightComponents)
            {
                try
                {
                    FightComponent fightComponent = component.Value as FightComponent;
                    Object.Destroy(fightComponent);
                }
                catch
                {
                    continue;
                }
            }

            _fightComponents.Clear();
            _fightComponents = null;
        }

        private void HideCharacters()
        {
            foreach (var fightComponent in _fightComponents)
            {
                Character character = fightComponent.Key;
                HideCharacter(character);
            }
        }
    }
}