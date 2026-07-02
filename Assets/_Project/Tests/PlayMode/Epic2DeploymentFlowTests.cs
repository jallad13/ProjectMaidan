using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using ProjectMaidan.Core;
using ProjectMaidan.UI;
using ProjectMaidan.Units;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace ProjectMaidan.Tests
{
    public class Epic2DeploymentFlowTests
    {
        [UnityTest]
        public IEnumerator CardToZoneToSpawnCooldownAndUnitCapFlowWorks()
        {
            AsyncOperation load = SceneManager.LoadSceneAsync("Match", LoadSceneMode.Single);
            while (!load.isDone)
            {
                yield return null;
            }
            yield return null;

            DeploymentController controller = Object.FindFirstObjectByType<DeploymentController>();
            UnitManager manager = UnitManager.Instance;
            UnitSpawner spawner = Object.FindFirstObjectByType<UnitSpawner>();
            DeploymentCardUI card = Object.FindObjectsByType<DeploymentCardUI>(FindObjectsSortMode.None)
                .Single(item => item.UnitName == "Ifrit");
            DeploymentZone[] zones = Object.FindObjectsByType<DeploymentZone>(FindObjectsSortMode.None)
                .Where(zone => zone.IsPlayerZone)
                .OrderBy(zone => zone.ZoneId)
                .ToArray();
            Text status = GameObject.Find("StatusMessage").GetComponent<Text>();

            Assert.That(controller, Is.Not.Null);
            Assert.That(manager, Is.Not.Null);
            Assert.That(spawner, Is.Not.Null);
            Assert.That(zones, Has.Length.EqualTo(6));

            FieldInfo cooldownField = typeof(DeploymentCardUI).GetField("_cooldownSeconds", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(cooldownField, Is.Not.Null);
            cooldownField.SetValue(card, 0.15f);

            for (int deployment = 0; deployment < UnitManager.MaxUnitsPerSide; deployment++)
            {
                float timeout = Time.realtimeSinceStartup + 1f;
                while (!card.IsAvailable && Time.realtimeSinceStartup < timeout)
                {
                    yield return null;
                }

                Assert.That(card.IsAvailable, Is.True, "Card did not recover from cooldown.");
                card.HandleTap();
                Assert.That(controller.SelectedCard, Is.SameAs(card));
                Assert.That(zones.All(zone => zone.IsHighlighted), Is.True, "Selecting a card must highlight all player zones.");
                Assert.That(zones[deployment].TryTap(), Is.True);
                Assert.That(manager.GetUnitCount(true), Is.EqualTo(deployment + 1));
                Assert.That(card.IsCoolingDown, Is.True);
                Assert.That(zones.All(zone => !zone.IsHighlighted), Is.True, "Zones must dehighlight after deployment.");
                yield return new WaitForSecondsRealtime(0.35f);
            }

            UnitBase firstUnit = manager.GetAllUnits().First(unit => unit.IsPlayerUnit);
            Assert.That(firstUnit.transform.localScale.x, Is.GreaterThan(0.8f), "Spawn animation did not reach full scale.");
            Assert.That(firstUnit.GetComponent<Renderer>().material.color.b, Is.GreaterThan(0.8f), "Player unit must be blue.");
            Assert.That(firstUnit.GetComponentInChildren<TextMesh>().text, Is.EqualTo("F"));

            float fifthTimeout = Time.realtimeSinceStartup + 1f;
            while (!card.IsAvailable && Time.realtimeSinceStartup < fifthTimeout)
            {
                yield return null;
            }

            card.HandleTap();
            Assert.That(zones[4].TryTap(), Is.True);
            yield return null;
            Assert.That(manager.GetUnitCount(true), Is.EqualTo(UnitManager.MaxUnitsPerSide));
            Assert.That(status.text, Is.EqualTo("Max units on field"));

            UnitBase opponent = spawner.Spawn(UnitRole.Ranged, false, new Vector3(0f, 0f, 3.6f));
            Assert.That(opponent, Is.Not.Null);
            Assert.That(opponent.IsPlayerUnit, Is.False);
            Assert.That(opponent.GetComponent<Renderer>().material.color.r, Is.GreaterThan(0.8f), "Opponent unit must be red.");
            Assert.That(opponent.GetComponentInChildren<TextMesh>().text, Is.EqualTo("R"));

        }
    }
}