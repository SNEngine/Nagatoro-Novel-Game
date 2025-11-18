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
using Object = UnityEngine.Object;

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
        private FightTurnOwner _fightTurnOwner = FightTurnOwner.Player;

        private FightCharacter _playerCharacter;
        private FightCharacter _enemyCharacter;

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
            _fightWindow.SetData(_fightComponents[playerCharacter.ReferenceCharacter], _fightComponents[enemyCharacter.ReferenceCharacter]);

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

            _fightTurnOwner = FightTurnOwner.Enemy;
            _fightWindow.Hide();

            ExecuteEnemyTurn();
        }

        private void HandlePlayerAction(PlayerAction action)
        {
            IFightComponent enemyComp = _fightComponents[_enemyCharacter.ReferenceCharacter];

            switch (action)
            {
                case PlayerAction.Attack:
                    HandleAttackAction(enemyComp, 10f);
                    break;
                case PlayerAction.Guard:
                    // Logic for Guard
                    break;
                case PlayerAction.Wait:
                    // Logic for Wait
                    break;
                case PlayerAction.Skill:
                    // Logic for Skill
                    break;
            }
        }

        private void HandleAttackAction(IFightComponent target, float damage)
        {
            target.HealthComponent.TakeDamage(damage);
        }

        private void ExecuteEnemyTurn()
        {
            // Здесь должна быть логика хода противника (AI, анимация, задержка)

            // Сейчас просто немедленно завершаем ход противника
            CompleteEnemyTurn();
        }

        private void CompleteEnemyTurn()
        {
            _fightTurnOwner = FightTurnOwner.Player;
            _fightWindow.Show();
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
                    UnityEngine.Object.Destroy(fightComponent);
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