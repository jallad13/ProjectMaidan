using System.Collections;
using System.Linq;
using NUnit.Framework;
using ProjectMaidan.Core;
using ProjectMaidan.UI;
using ProjectMaidan.Units;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ProjectMaidan.Tests
{
    public class Epic7AIOpponentTests
    {
        [UnityTest]
        public IEnumerator AiIsGatedDuringPreMatchAndDeploysCenterFrontlineWhenLosing()
        {
            yield return LoadScene("Match");

            MatchManager manager = MatchManager.Instance;
            AIController ai = Object.FindFirstObjectByType<AIController>(FindObjectsInactive.Include);
            TerritorySystem territory = Object.FindFirstObjectByType<TerritorySystem>();
            CenterTerritoryZone center = territory.CenterZone;
            ManaSystem opponentMana = ManaSystem.OpponentInstance;
            UnitBase playerScorer = CreateInactiveUnit("PlayerTerritoryLead", true);

            Assert.That(manager.CurrentState, Is.EqualTo(MatchState.PreMatch));
            Assert.That(ai.enabled, Is.False, "AI must not make decisions during the countdown.");
            Assert.That(UnitManager.Instance.GetUnitCount(false), Is.EqualTo(0));
            Assert.That(ai.RunDecisionCycle(allowRandomOverride: false), Is.Null);
            Assert.That(opponentMana.CurrentMana, Is.EqualTo(5));

            territory.ResetScores();
            center.Clear();
            center.RegisterUnit(playerScorer);
            for (int tick = 0; tick < ai.AggressiveDefendThreshold; tick++)
            {
                territory.EvaluateTick();
            }

            Assert.That(manager.TransitionTo(MatchState.InMatch), Is.True);
            int manaBefore = opponentMana.CurrentMana;
            UnitBase deployed = ai.RunDecisionCycle(allowRandomOverride: false);
            yield return null;

            Assert.That(deployed, Is.Not.Null);
            Assert.That(deployed.Role, Is.EqualTo(UnitRole.Frontline));
            Assert.That(deployed.IsPlayerUnit, Is.False);
            Assert.That(ai.LastDeploymentZone.ZoneId, Is.EqualTo(ZoneId.CenterFront));
            Assert.That(opponentMana.CurrentMana, Is.EqualTo(manaBefore - deployed.ManaCost));
            Assert.That(UnitManager.Instance.GetUnitCount(false), Is.EqualTo(1));

            Object.Destroy(playerScorer.gameObject);
        }

        [UnityTest]
        public IEnumerator AiTargetsRearSupportWithRangedUnit()
        {
            yield return LoadScene("Match");
            Assert.That(MatchManager.Instance.TransitionTo(MatchState.InMatch), Is.True);

            AIController ai = Object.FindFirstObjectByType<AIController>();
            UnitSpawner spawner = Object.FindFirstObjectByType<UnitSpawner>();
            UnitBase support = spawner.Spawn(
                UnitRole.Support,
                true,
                new Vector3(3f, 0f, -4.6f));
            yield return null;

            UnitBase deployed = ai.RunDecisionCycle(allowRandomOverride: false);
            yield return null;

            Assert.That(support, Is.Not.Null);
            Assert.That(deployed, Is.Not.Null);
            Assert.That(deployed.Role, Is.EqualTo(UnitRole.Ranged));
            Assert.That(ai.LastDeploymentZone.IsPlayerZone, Is.False);
            Assert.That(Mathf.Abs(ai.LastDeploymentZone.transform.position.x - support.transform.position.x),
                Is.LessThanOrEqualTo(1.1f));
        }

        [UnityTest]
        public IEnumerator AiRespectsOpponentUnitCapWithoutSpendingMana()
        {
            yield return LoadScene("Match");
            Assert.That(MatchManager.Instance.TransitionTo(MatchState.InMatch), Is.True);

            AIController ai = Object.FindFirstObjectByType<AIController>();
            UnitSpawner spawner = Object.FindFirstObjectByType<UnitSpawner>();
            DeploymentZone[] opponentZones = Object.FindObjectsByType<DeploymentZone>(FindObjectsSortMode.None)
                .Where(zone => !zone.IsPlayerZone)
                .OrderBy(zone => zone.ZoneId)
                .ToArray();

            for (int index = 0; index < UnitManager.MaxUnitsPerSide; index++)
            {
                spawner.Spawn(
                    UnitRole.Frontline,
                    false,
                    opponentZones[index].transform.position);
            }

            yield return null;
            Assert.That(UnitManager.Instance.GetUnitCount(false), Is.EqualTo(UnitManager.MaxUnitsPerSide));
            int manaBefore = ManaSystem.OpponentInstance.CurrentMana;

            UnitBase extraDeployment = ai.RunDecisionCycle(allowRandomOverride: false);
            yield return null;

            Assert.That(extraDeployment, Is.Null);
            Assert.That(UnitManager.Instance.GetUnitCount(false), Is.EqualTo(UnitManager.MaxUnitsPerSide));
            Assert.That(ManaSystem.OpponentInstance.CurrentMana, Is.EqualTo(manaBefore));
        }

        [UnityTest]
        public IEnumerator AiRunsForFiveSecondsWithoutErrors()
        {
            yield return LoadScene("Match");
            AIController ai = Object.FindFirstObjectByType<AIController>(FindObjectsInactive.Include);
            Assert.That(ai.enabled, Is.False);
            Assert.That(MatchManager.Instance.TransitionTo(MatchState.InMatch), Is.True);

            yield return new WaitForSecondsRealtime(5f);

            Assert.That(MatchManager.Instance.CurrentState, Is.EqualTo(MatchState.InMatch));
            Assert.That(UnitManager.Instance.GetUnitCount(false), Is.GreaterThanOrEqualTo(1));
            Assert.That(ManaSystem.OpponentInstance.CurrentMana, Is.LessThan(5));
        }

        [UnityTest]
        public IEnumerator BootstrapMenuAndSecondMatchResetAllState()
        {
            yield return LoadScene("Bootstrap");
            yield return WaitForActiveScene("MainMenu");

            Assert.That(GameManager.Instance, Is.Not.Null);
            MainMenuController menu = Object.FindFirstObjectByType<MainMenuController>();
            Assert.That(menu, Is.Not.Null);
            menu.StartMatch();
            yield return WaitForActiveScene("Match");
            yield return null;

            AssertFreshPreMatch();
            Assert.That(MatchManager.Instance.TransitionTo(MatchState.InMatch), Is.True);
            AIController ai = Object.FindFirstObjectByType<AIController>();
            Assert.That(ai.RunDecisionCycle(allowRandomOverride: false), Is.Not.Null);
            yield return null;
            Assert.That(UnitManager.Instance.GetUnitCount(false), Is.EqualTo(1));

            SceneManager.LoadScene("MainMenu");
            yield return WaitForActiveScene("MainMenu");
            MainMenuController secondMenu = Object.FindFirstObjectByType<MainMenuController>();
            Assert.That(secondMenu, Is.Not.Null);
            secondMenu.StartMatch();
            yield return WaitForActiveScene("Match");
            yield return null;

            AssertFreshPreMatch();
        }

        private static void AssertFreshPreMatch()
        {
            TerritorySystem territory = Object.FindFirstObjectByType<TerritorySystem>();
            Assert.That(MatchManager.Instance.CurrentState, Is.EqualTo(MatchState.PreMatch));
            Assert.That(territory.PlayerScore, Is.EqualTo(0));
            Assert.That(territory.OpponentScore, Is.EqualTo(0));
            Assert.That(ManaSystem.Instance.CurrentMana, Is.EqualTo(5));
            Assert.That(ManaSystem.OpponentInstance.CurrentMana, Is.EqualTo(5));
            Assert.That(UnitManager.Instance.GetAllUnits(), Is.Empty);
            Assert.That(Object.FindFirstObjectByType<AIController>(FindObjectsInactive.Include).enabled, Is.False);
        }

        private static UnitBase CreateInactiveUnit(string objectName, bool isPlayer)
        {
            GameObject unitObject = new(objectName);
            unitObject.SetActive(false);
            UnitBase unit = unitObject.AddComponent<FrontlineUnit>();
            unit.IsPlayerUnit = isPlayer;
            return unit;
        }

        private static IEnumerator LoadScene(string sceneName)
        {
            Time.timeScale = 1f;
            AsyncOperation load = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            while (!load.isDone)
            {
                yield return null;
            }

            yield return null;
        }

        private static IEnumerator WaitForActiveScene(string sceneName)
        {
            float timeout = Time.realtimeSinceStartup + 5f;
            while (SceneManager.GetActiveScene().name != sceneName &&
                   Time.realtimeSinceStartup < timeout)
            {
                yield return null;
            }

            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(sceneName));
        }
    }
}
