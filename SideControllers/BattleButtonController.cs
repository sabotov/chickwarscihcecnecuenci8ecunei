using BattlefieldScripts;
using LocalBattleSystem.Actions;
using LocalBattleSystem.Core;
using UnityEngine;
using UnityEngine.UI;

namespace LocalBattleSystem.SideControllers
{
    public class BattleButtonController : MonoBehaviour
    {
        [Header("Database")]
        [SerializeField] private LocalBattleDatabase database;

        [Header("Monster IDs")]
        [SerializeField] private string playerMonsterId = "hero_001";
        [SerializeField] private string enemyMonsterId = "slime_001";

        [Header("Optional references")]
        [SerializeField] private Button startBattleButton;
        [SerializeField] private FieldScriptWrapper fieldScriptWrapper;

        private void Reset()
        {
            startBattleButton = GetComponent<Button>();
        }

        private void OnEnable()
        {
            if (startBattleButton != null)
            {
                startBattleButton.onClick.AddListener(StartBattle);
            }
        }

        private void OnDisable()
        {
            if (startBattleButton != null)
            {
                startBattleButton.onClick.RemoveListener(StartBattle);
            }
        }

        public void StartBattle()
        {
            if (database == null)
            {
                Debug.LogError("BattleButtonController: LocalBattleDatabase reference is missing.");
                return;
            }

            if (!database.IsLoaded)
            {
                database.Load();
            }

            var player = database.CreateRuntimeMonster(playerMonsterId);
            var enemy = database.CreateRuntimeMonster(enemyMonsterId);

            if (player == null || enemy == null)
            {
                Debug.LogError($"BattleButtonController: Monster ID not found. player={playerMonsterId}, enemy={enemyMonsterId}");
                return;
            }

            var result = BattleActionResolver.RunOneVsOne(database, player, enemy);
            Debug.Log($"Battle finished. Winner ID={result.winnerId}, rounds={result.rounds}");

            foreach (var logLine in result.log)
            {
                Debug.Log(logLine);
            }

            HighlightTilesAfterBattle(result.winnerId == playerMonsterId);
        }

        private void HighlightTilesAfterBattle(bool playerWon)
        {
            if (fieldScriptWrapper == null || fieldScriptWrapper.tiles == null || fieldScriptWrapper.tiles.Count == 0)
            {
                return;
            }

            var color = playerWon ? Color.green : Color.red;
            var tile = fieldScriptWrapper.tiles[0];
            tile.SetTileHighlight(Tile.TileHighlightning.GreenBlinking, color);
        }
    }
}
