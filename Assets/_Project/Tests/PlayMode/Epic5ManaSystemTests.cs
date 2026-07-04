using System.Collections;
using System.Linq;
using NUnit.Framework;
using ProjectMaidan.Core;
using ProjectMaidan.UI;
using ProjectMaidan.Units;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace ProjectMaidan.Tests
{
    public class Epic5ManaSystemTests
    {
        [UnityTest]
        public IEnumerator MatchStartsWithIndependentManaPoolsAndVisibleHud()
        {
            yield return LoadScene("Match");

            ManaSystem playerMana = ManaSystem.Instance;
            ManaSystem opponentMana = ManaSystem.OpponentInstance;
            ManaHUD hud = Object.FindFirstObjectByType<ManaHUD>();

            Assert.That(playerMana, Is.Not.Null);
            Assert.That(opponentMana, Is.Not.Null);
            Assert.That(opponentMana, Is.Not.SameAs(playerMana));
            Assert.That(playerMana.CurrentMana, Is.EqualTo(5));
            Assert.That(opponentMana.CurrentMana, Is.EqualTo(5));
            Assert.That(playerMana.MaxMana, Is.EqualTo(10));
            Assert.That(opponentMana.MaxMana, Is.EqualTo(10));
            Assert.That(hud, Is.Not.Null);
            Assert.That(hud.FillAmount, Is.EqualTo(0.5f).Within(0.01f));
            Assert.That(hud.ManaText, Is.EqualTo("5"));
            Assert.That(GameObject.Find("StatusMessage").GetComponent<TMP_Text>(), Is.Not.Null);

            Assert.That(playerMana.TryDeductMana(3), Is.True);
            Assert.That(playerMana.CurrentMana, Is.EqualTo(2));
            Assert.That(opponentMana.CurrentMana, Is.EqualTo(5),
                "The opponent must own an independent mana pool.");
            Assert.That(hud.FillAmount, Is.EqualTo(0.2f).Within(0.01f));
            Assert.That(hud.ManaText, Is.EqualTo("2"));
        }

        [UnityTest]
        public IEnumerator DeploymentDeductsManaAndBlocksUnaffordableCards()
        {
            yield return LoadScene("Match");

            DeploymentController controller = Object.FindFirstObjectByType<DeploymentController>();
            DeploymentCardUI[] cards = Object.FindObjectsByType<DeploymentCardUI>(FindObjectsSortMode.None);
            DeploymentCardUI roc = cards.Single(card => card.UnitName == "Roc");
            DeploymentCardUI ifrit = cards.Single(card => card.UnitName == "Ifrit");
            DeploymentZone zone = Object.FindObjectsByType<DeploymentZone>(FindObjectsSortMode.None)
                .Where(item => item.IsPlayerZone)
                .OrderBy(item => item.ZoneId)
                .First();
            TMP_Text status = GameObject.Find("StatusMessage").GetComponent<TMP_Text>();
            ManaSystem mana = ManaSystem.Instance;

            Assert.That(mana.CurrentMana, Is.EqualTo(5));
            Assert.That(roc.ManaCost, Is.EqualTo(4));
            Assert.That(roc.IsAffordable, Is.True);

            roc.HandleTap();
            Assert.That(controller.SelectedCard, Is.SameAs(roc));
            Assert.That(zone.TryTap(), Is.True);
            Assert.That(mana.CurrentMana, Is.EqualTo(1));
            Assert.That(roc.IsAffordable, Is.False);
            Assert.That(ifrit.IsAffordable, Is.False);
            Assert.That(ifrit.GetComponent<Button>().interactable, Is.False);
            Assert.That(controller.SelectedCard, Is.Null);

            ifrit.OnPointerClick(null);
            Assert.That(controller.SelectedCard, Is.Null);
            Assert.That(status.text, Is.EqualTo("Not enough mana"));

            mana.AddMana(2);
            Assert.That(mana.CurrentMana, Is.EqualTo(3));
            Assert.That(ifrit.IsAffordable, Is.True);
            Assert.That(roc.IsAffordable, Is.False);
        }

        [UnityTest]
        public IEnumerator RegenCarriesPartialTimeIntoImmediateComebackRate()
        {
            yield return LoadScene("MainMenu");

            MatchConfig config = ScriptableObject.CreateInstance<MatchConfig>();
            config.StartingMana = 5;
            config.MaxMana = 10;
            config.ManaRegenRate = 0.2f;
            config.ComebackRegenRate = 0.1f;
            config.ComebackThreshold = 1;

            GameObject manaObject = new("TestPlayerMana");
            ManaSystem mana = manaObject.AddComponent<ManaSystem>();
            mana.Configure(config);
            yield return null;

            Assert.That(mana.CurrentMana, Is.EqualTo(5));
            yield return new WaitForSeconds(0.12f);
            Assert.That(mana.CurrentMana, Is.EqualTo(5));

            GameObject zoneObject = new("TestCenterZone", typeof(BoxCollider), typeof(CenterTerritoryZone));
            GameObject territoryObject = new("TestTerritorySystem", typeof(TerritorySystem));
            CenterTerritoryZone zone = zoneObject.GetComponent<CenterTerritoryZone>();
            TerritorySystem territory = territoryObject.GetComponent<TerritorySystem>();
            territory.Configure(config, zone, null);
            territory.ResetScores();
            UnitBase opponent = CreateInactiveUnit("OpponentScorer", false);
            zone.RegisterUnit(opponent);

            territory.EvaluateTick();
            Assert.That(mana.IsComebackActive, Is.True);
            Assert.That(mana.CurrentRegenRate, Is.EqualTo(0.1f).Within(0.001f));
            yield return null;
            Assert.That(mana.CurrentMana, Is.EqualTo(6),
                "Changing to comeback regen must preserve the accumulated partial interval.");

            Object.Destroy(opponent.gameObject);
            Object.Destroy(territoryObject);
            Object.Destroy(zoneObject);
            Object.Destroy(manaObject);
            Object.Destroy(config);
        }

        [UnityTest]
        public IEnumerator ComebackStatePulsesTrailingSideAndRegenStopsAtCap()
        {
            yield return LoadScene("Match");

            ManaSystem playerMana = ManaSystem.Instance;
            ManaSystem opponentMana = ManaSystem.OpponentInstance;
            TerritorySystem territory = Object.FindFirstObjectByType<TerritorySystem>();
            CenterTerritoryZone zone = territory.CenterZone;
            ManaHUD hud = Object.FindFirstObjectByType<ManaHUD>();
            UnitBase opponent = CreateInactiveUnit("OpponentController", false);

            territory.ResetScores();
            zone.Clear();
            zone.RegisterUnit(opponent);
            for (int tick = 0; tick < 30; tick++)
            {
                territory.EvaluateTick();
            }

            Assert.That(playerMana.IsComebackActive, Is.True);
            Assert.That(opponentMana.IsComebackActive, Is.False);
            Assert.That(playerMana.CurrentRegenRate, Is.EqualTo(2.4f).Within(0.001f));
            Assert.That(hud.IsPulsing, Is.True);

            float minimumAlpha = 1f;
            float maximumAlpha = 0f;
            float pulseSampleEnd = Time.realtimeSinceStartup + 0.6f;
            while (Time.realtimeSinceStartup < pulseSampleEnd)
            {
                minimumAlpha = Mathf.Min(minimumAlpha, hud.PulseAlpha);
                maximumAlpha = Mathf.Max(maximumAlpha, hud.PulseAlpha);
                yield return null;
            }

            Assert.That(maximumAlpha - minimumAlpha, Is.GreaterThan(0.15f));

            playerMana.AddMana(playerMana.MaxMana);
            Assert.That(playerMana.CurrentMana, Is.EqualTo(playerMana.MaxMana));
            yield return new WaitForSeconds(0.1f);
            Assert.That(playerMana.CurrentMana, Is.EqualTo(playerMana.MaxMana));

            territory.ResetScores();
            zone.Clear();
            Object.Destroy(opponent.gameObject);
            yield return null;

            UnitBase player = CreateInactiveUnit("PlayerController", true);
            zone.RegisterUnit(player);
            for (int tick = 0; tick < 30; tick++)
            {
                territory.EvaluateTick();
            }

            Assert.That(playerMana.IsComebackActive, Is.False);
            Assert.That(opponentMana.IsComebackActive, Is.True);
            Assert.That(hud.IsPulsing, Is.False);
            Assert.That(hud.PulseAlpha, Is.EqualTo(1f).Within(0.01f));

            Object.Destroy(player.gameObject);
        }

        private static UnitBase CreateInactiveUnit(string objectName, bool isPlayer)
        {
            var unitObject = new GameObject(objectName);
            unitObject.SetActive(false);
            var unit = unitObject.AddComponent<FrontlineUnit>();
            unit.IsPlayerUnit = isPlayer;
            return unit;
        }

        private static IEnumerator LoadScene(string sceneName)
        {
            AsyncOperation load = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            while (!load.isDone)
            {
                yield return null;
            }

            yield return null;
        }
    }
}
