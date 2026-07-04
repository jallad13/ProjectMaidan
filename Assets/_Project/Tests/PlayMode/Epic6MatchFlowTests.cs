using System.Collections;
using System.Linq;
using System.Reflection;
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
    public class Epic6MatchFlowTests
    {
        [UnityTest]
        public IEnumerator PreMatchGatesSystemsAndCountdownDrivesInMatch()
        {
            yield return LoadMatch();

            MatchManager manager = MatchManager.Instance;
            TerritorySystem territory = Object.FindFirstObjectByType<TerritorySystem>();
            ManaSystem[] manaSystems = Object.FindObjectsByType<ManaSystem>(FindObjectsSortMode.None);
            DeploymentZone[] zones = Object.FindObjectsByType<DeploymentZone>(FindObjectsSortMode.None);
            AIController ai = Object.FindFirstObjectByType<AIController>(FindObjectsInactive.Include);
            CountdownOverlay countdown = Object.FindFirstObjectByType<CountdownOverlay>();
            TimerHUD timer = Object.FindFirstObjectByType<TimerHUD>();

            Assert.That(manager.CurrentState, Is.EqualTo(MatchState.PreMatch));
            Assert.That(manager.TransitionTo(MatchState.PostMatch), Is.False,
                "The state machine must reject skipped transitions.");
            Assert.That(territory.enabled, Is.False);
            Assert.That(manaSystems, Has.Length.EqualTo(2));
            Assert.That(manaSystems.All(system => !system.IsRegenEnabled), Is.True);
            Assert.That(zones, Has.Length.EqualTo(12));
            Assert.That(zones.All(zone => !zone.IsInteractionEnabled), Is.True);
            Assert.That(ai, Is.Not.Null);
            Assert.That(ai.enabled, Is.False);
            Assert.That(countdown, Is.Not.Null);
            Assert.That(countdown.IsVisible, Is.True);
            Assert.That(countdown.CurrentText, Is.EqualTo("3"));
            Assert.That(Time.timeScale, Is.EqualTo(0f));

            yield return new WaitForSecondsRealtime(1.05f);
            Assert.That(countdown.CurrentText, Is.EqualTo("2"));
            yield return new WaitForSecondsRealtime(1.05f);
            Assert.That(countdown.CurrentText, Is.EqualTo("1"));
            yield return new WaitForSecondsRealtime(1.05f);
            Assert.That(countdown.CurrentText, Is.EqualTo("GO!"));
            yield return new WaitForSecondsRealtime(0.9f);

            Assert.That(manager.CurrentState, Is.EqualTo(MatchState.InMatch));
            Assert.That(territory.enabled, Is.True);
            Assert.That(manaSystems.All(system => system.IsRegenEnabled), Is.True);
            Assert.That(zones.All(zone => zone.IsInteractionEnabled), Is.True);
            Assert.That(ai.enabled, Is.True);
            Assert.That(countdown.IsVisible, Is.False);
            Assert.That(Time.timeScale, Is.EqualTo(1f));
            Assert.That(manager.TimeRemaining, Is.InRange(119f, 120f));
            Assert.That(timer, Is.Not.Null);
            Assert.That(timer.TimerText, Is.EqualTo("2:00"));
        }

        [UnityTest]
        public IEnumerator TimerExpiryUsesUnitHpTiebreakerAndFreezesPostMatch()
        {
            yield return LoadMatch();

            MatchManager manager = MatchManager.Instance;
            TerritorySystem territory = Object.FindFirstObjectByType<TerritorySystem>();
            TimerHUD timer = Object.FindFirstObjectByType<TimerHUD>();
            MatchConfig config = ScriptableObject.CreateInstance<MatchConfig>();
            config.MatchDuration = 1f;
            config.WinThreshold = 120;
            manager.Configure(config, territory);

            GameObject playerObject = new("PlayerHpTiebreaker");
            UnitBase player = playerObject.AddComponent<FrontlineUnit>();
            player.IsPlayerUnit = true;
            UnitManager.Instance.Register(player);

            MatchResult observedResult = null;
            void HandleMatchEnded(MatchResult result) => observedResult = result;
            MatchManager.OnMatchEnded += HandleMatchEnded;

            Assert.That(manager.EvaluateWinner(), Is.EqualTo("player"));
            Assert.That(manager.TransitionTo(MatchState.InMatch), Is.True);
            Assert.That(timer.TimerText, Is.EqualTo("0:01"));
            Assert.That(timer.CurrentColor.r, Is.GreaterThan(0.9f));

            yield return new WaitForSecondsRealtime(1.2f);

            Assert.That(manager.TimeRemaining, Is.EqualTo(0f));
            Assert.That(manager.CurrentState, Is.EqualTo(MatchState.PostMatch));
            Assert.That(observedResult, Is.Not.Null);
            Assert.That(observedResult.WinnerId, Is.EqualTo("player"));
            Assert.That(observedResult.WinCondition, Is.EqualTo("time"));
            Assert.That(Time.timeScale, Is.EqualTo(0f));

            MatchManager.OnMatchEnded -= HandleMatchEnded;
            Object.Destroy(playerObject);
            Object.Destroy(config);
        }

        [UnityTest]
        public IEnumerator ResultScreenFadesInWithFinalScoreAndThresholdCondition()
        {
            yield return LoadMatch();

            MatchManager manager = MatchManager.Instance;
            TerritorySystem territory = Object.FindFirstObjectByType<TerritorySystem>();
            CenterTerritoryZone zone = territory.CenterZone;
            ResultScreen resultScreen = Object.FindFirstObjectByType<ResultScreen>();
            UnitBase player = CreateInactiveUnit("ResultPlayer", true);
            UnitBase opponent = CreateInactiveUnit("ResultOpponent", false);

            Assert.That(manager.TransitionTo(MatchState.InMatch), Is.True);
            territory.ResetScores();
            zone.Clear();
            zone.RegisterUnit(player);
            for (int tick = 0; tick < 67; tick++)
            {
                territory.EvaluateTick();
            }

            zone.UnregisterUnit(player);
            zone.RegisterUnit(opponent);
            for (int tick = 0; tick < 53; tick++)
            {
                territory.EvaluateTick();
            }

            manager.EndMatch(new MatchResult
            {
                WinnerId = "player",
                WinCondition = "score"
            });

            Assert.That(resultScreen, Is.Not.Null);
            Assert.That(resultScreen.ContinueInteractable, Is.False);
            Assert.That(resultScreen.Alpha, Is.LessThan(0.1f));
            yield return new WaitForSecondsRealtime(0.6f);

            Assert.That(resultScreen.OutcomeText, Is.EqualTo("VICTORY"));
            Assert.That(resultScreen.ScoreText, Is.EqualTo("67 vs 53"));
            Assert.That(resultScreen.WinConditionText, Is.EqualTo("Score Threshold"));
            Assert.That(resultScreen.Alpha, Is.EqualTo(1f).Within(0.01f));
            Assert.That(resultScreen.ContinueInteractable, Is.True);

            Object.Destroy(player.gameObject);
            Object.Destroy(opponent.gameObject);
        }

        [UnityTest]
        public IEnumerator DeploymentCardsUseWiredTmpLabels()
        {
            yield return LoadMatch();

            DeploymentCardUI[] cards = Object.FindObjectsByType<DeploymentCardUI>(FindObjectsSortMode.None);
            string[] fieldNames = { "_nameText", "_roleText", "_manaText", "_cooldownText" };
            Assert.That(cards, Has.Length.EqualTo(3));

            foreach (string fieldName in fieldNames)
            {
                FieldInfo field = typeof(DeploymentCardUI).GetField(
                    fieldName,
                    BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.That(field, Is.Not.Null);
                Assert.That(field.FieldType, Is.EqualTo(typeof(TMP_Text)));
                Assert.That(cards.All(card => field.GetValue(card) is TMP_Text), Is.True,
                    $"{fieldName} must reference a TMP label on every deployment card.");
            }
        }

        private static UnitBase CreateInactiveUnit(string objectName, bool isPlayer)
        {
            GameObject unitObject = new(objectName);
            unitObject.SetActive(false);
            UnitBase unit = unitObject.AddComponent<FrontlineUnit>();
            unit.IsPlayerUnit = isPlayer;
            return unit;
        }

        private static IEnumerator LoadMatch()
        {
            Time.timeScale = 1f;
            AsyncOperation load = SceneManager.LoadSceneAsync("Match", LoadSceneMode.Single);
            while (!load.isDone)
            {
                yield return null;
            }

            yield return null;
        }
    }
}
