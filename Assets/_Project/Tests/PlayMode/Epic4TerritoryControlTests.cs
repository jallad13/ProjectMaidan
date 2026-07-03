using System.Collections;
using NUnit.Framework;
using ProjectMaidan.Core;
using ProjectMaidan.UI;
using ProjectMaidan.Units;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ProjectMaidan.Tests
{
    public class Epic4TerritoryControlTests
    {
        [Test]
        public void TerritoryTickCanBeEvaluatedInIsolation()
        {
            var zoneObject = new GameObject("IsolatedCenterZone", typeof(BoxCollider), typeof(CenterTerritoryZone));
            var systemObject = new GameObject("IsolatedTerritorySystem", typeof(TerritorySystem));
            var config = ScriptableObject.CreateInstance<MatchConfig>();
            config.WinThreshold = 3;
            var zone = zoneObject.GetComponent<CenterTerritoryZone>();
            var system = systemObject.GetComponent<TerritorySystem>();
            system.Configure(config, zone, null);
            system.ResetScores();

            UnitBase player = CreateInactiveUnit("PlayerStub", true);
            UnitBase opponent = CreateInactiveUnit("OpponentStub", false);
            TerritoryTick observedTick = null;
            void HandleTick(TerritoryTick tick) => observedTick = tick;
            TerritorySystem.OnTerritoryTick += HandleTick;

            zone.RegisterUnit(player);
            TerritoryTick playerTick = system.EvaluateTick();
            Assert.That(playerTick.Control, Is.EqualTo(TerritoryControl.PlayerA));
            Assert.That(playerTick.PlayerACount, Is.EqualTo(1));
            Assert.That(playerTick.PlayerBCount, Is.EqualTo(0));
            Assert.That(system.PlayerScore, Is.EqualTo(1));

            zone.RegisterUnit(opponent);
            TerritoryTick contestedTick = system.EvaluateTick();
            Assert.That(contestedTick.Control, Is.EqualTo(TerritoryControl.Contested));
            Assert.That(system.PlayerScore, Is.EqualTo(1));
            Assert.That(system.OpponentScore, Is.EqualTo(0));

            zone.UnregisterUnit(player);
            TerritoryTick opponentTick = system.EvaluateTick();
            Assert.That(opponentTick.Control, Is.EqualTo(TerritoryControl.PlayerB));
            Assert.That(opponentTick.PlayerACount, Is.EqualTo(0));
            Assert.That(opponentTick.PlayerBCount, Is.EqualTo(1));
            Assert.That(system.OpponentScore, Is.EqualTo(1));
            Assert.That(observedTick, Is.SameAs(opponentTick));

            TerritorySystem.OnTerritoryTick -= HandleTick;
            Object.DestroyImmediate(player.gameObject);
            Object.DestroyImmediate(opponent.gameObject);
            Object.DestroyImmediate(systemObject);
            Object.DestroyImmediate(zoneObject);
            Object.DestroyImmediate(config);
        }

        [UnityTest]
        public IEnumerator ScoreHudLineAndWinThresholdStaySynchronized()
        {
            yield return LoadMatch();
            TerritorySystem system = Object.FindFirstObjectByType<TerritorySystem>();
            MatchManager manager = MatchManager.Instance;
            TerritoryScoreHUD hud = Object.FindFirstObjectByType<TerritoryScoreHUD>();
            TerritoryLineVisual line = Object.FindFirstObjectByType<TerritoryLineVisual>();
            CenterTerritoryZone zone = system.CenterZone;
            UnitBase player = CreateInactiveUnit("PlayerScorer", true);

            Assert.That(system, Is.Not.Null);
            Assert.That(manager, Is.Not.Null);
            Assert.That(hud, Is.Not.Null);
            Assert.That(line, Is.Not.Null);
            Assert.That(system.WinThreshold, Is.EqualTo(120));
            Assert.That(zone.GetComponent<BoxCollider>().isTrigger, Is.True);

            system.ResetScores();
            zone.Clear();
            zone.RegisterUnit(player);

            MatchResult result = null;
            void HandleMatchEnded(MatchResult matchResult) => result = matchResult;
            MatchManager.OnMatchEnded += HandleMatchEnded;

            for (int tick = 0; tick < 60; tick++)
            {
                system.EvaluateTick();
            }

            Assert.That(manager.PlayerScore, Is.EqualTo(60));
            Assert.That(manager.OpponentScore, Is.EqualTo(0));
            Assert.That(hud.PlayerScoreText, Is.EqualTo("60 / 120"));
            Assert.That(hud.OpponentScoreText, Is.EqualTo("0 / 120"));
            Assert.That(hud.PlayerStyle, Is.EqualTo(FontStyles.Bold));
            Assert.That(hud.OpponentStyle, Is.EqualTo(FontStyles.Normal));

            yield return new WaitForSeconds(0.6f);
            Assert.That(line.TargetZ, Is.EqualTo(BattlefieldNavigation.MaxZ * 0.5f).Within(0.05f));
            Assert.That(line.CurrentNormalizedPosition, Is.GreaterThan(0f));
            Assert.That(line.CurrentNormalizedPosition, Is.LessThan(0.55f));

            for (int tick = 60; tick < system.WinThreshold; tick++)
            {
                system.EvaluateTick();
            }

            Assert.That(manager.PlayerScore, Is.EqualTo(120));
            Assert.That(manager.CurrentState, Is.EqualTo(MatchState.PostMatch));
            Assert.That(result, Is.Not.Null);
            Assert.That(result.WinnerId, Is.EqualTo("player"));
            Assert.That(result.WinCondition, Is.EqualTo("score"));

            MatchManager.OnMatchEnded -= HandleMatchEnded;
            Object.Destroy(player.gameObject);
        }

        [UnityTest]
        public IEnumerator CenterTriggerTracksRealUnitEnterAndExit()
        {
            yield return LoadMatch();
            TerritorySystem system = Object.FindFirstObjectByType<TerritorySystem>();
            UnitSpawner spawner = Object.FindFirstObjectByType<UnitSpawner>();
            CenterTerritoryZone zone = system.CenterZone;

            UnitBase unit = spawner.Spawn(UnitRole.Frontline, true, new Vector3(0f, 0.6f, 0f));
            yield return new WaitForFixedUpdate();

            Assert.That(zone.PlayerCount, Is.EqualTo(1));
            Assert.That(zone.OpponentCount, Is.EqualTo(0));

            Object.Destroy(unit.gameObject);
            yield return new WaitForFixedUpdate();
            Assert.That(zone.PlayerCount, Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator ControlChangeFlashesLineAndOpponentLeadHighlightsHud()
        {
            yield return LoadMatch();
            TerritorySystem system = Object.FindFirstObjectByType<TerritorySystem>();
            TerritoryScoreHUD hud = Object.FindFirstObjectByType<TerritoryScoreHUD>();
            TerritoryLineVisual line = Object.FindFirstObjectByType<TerritoryLineVisual>();
            CenterTerritoryZone zone = system.CenterZone;
            UnitBase player = CreateInactiveUnit("PlayerController", true);
            UnitBase opponent = CreateInactiveUnit("OpponentController", false);

            system.ResetScores();
            zone.Clear();
            zone.RegisterUnit(player);
            system.EvaluateTick();
            Assert.That(line.IsFlashing, Is.False);

            zone.UnregisterUnit(player);
            zone.RegisterUnit(opponent);
            system.EvaluateTick();
            system.EvaluateTick();

            Assert.That(line.IsFlashing, Is.True);
            Assert.That(hud.OpponentScoreText, Is.EqualTo("2 / 120"));
            Assert.That(hud.OpponentStyle, Is.EqualTo(FontStyles.Bold));
            Assert.That(hud.PlayerStyle, Is.EqualTo(FontStyles.Normal));

            yield return new WaitForSeconds(0.6f);
            Assert.That(line.TargetZ, Is.LessThan(0f));
            Assert.That(line.CurrentNormalizedPosition, Is.LessThan(0f));

            Object.Destroy(player.gameObject);
            Object.Destroy(opponent.gameObject);
        }

        private static UnitBase CreateInactiveUnit(string objectName, bool isPlayer)
        {
            var unitObject = new GameObject(objectName);
            unitObject.SetActive(false);
            var unit = unitObject.AddComponent<FrontlineUnit>();
            unit.IsPlayerUnit = isPlayer;
            return unit;
        }

        private static IEnumerator LoadMatch()
        {
            AsyncOperation load = SceneManager.LoadSceneAsync("Match", LoadSceneMode.Single);
            while (!load.isDone)
            {
                yield return null;
            }

            yield return null;
        }
    }
}
