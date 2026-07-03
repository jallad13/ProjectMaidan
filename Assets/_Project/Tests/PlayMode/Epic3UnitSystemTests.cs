using System.Collections;
using System.Linq;
using NUnit.Framework;
using ProjectMaidan.Units;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ProjectMaidan.Tests
{
    public class Epic3UnitSystemTests
    {
        [UnityTest]
        public IEnumerator UnitStatsHealthDeathAndNavMeshAreConfigured()
        {
            yield return LoadMatch();
            UnitSpawner spawner = Object.FindFirstObjectByType<UnitSpawner>();
            UnitBase unit = spawner.Spawn(UnitRole.Frontline, true, new Vector3(-2f, 0.6f, -4.5f));
            yield return null;

            Assert.That(unit.UnitId, Is.EqualTo("ifrit"));
            Assert.That(unit.DisplayName, Is.EqualTo("Ifrit"));
            Assert.That(unit.Role, Is.EqualTo(UnitRole.Frontline));
            Assert.That(unit.HP, Is.EqualTo(120f));
            Assert.That(unit.Damage, Is.EqualTo(12f));
            Assert.That(unit.MoveSpeed, Is.EqualTo(3.5f));
            Assert.That(unit.AttackRange, Is.EqualTo(2f));
            Assert.That(unit.AttackRate, Is.EqualTo(1f));
            Assert.That(unit.ManaCost, Is.EqualTo(3));
            Assert.That(unit.Agent.enabled && unit.Agent.isOnNavMesh, Is.True);
            Assert.That(unit.Agent.speed, Is.EqualTo(unit.MoveSpeed));
            Assert.That(unit.HealthBar, Is.Not.Null);

            unit.TakeDamage(60f);
            Assert.That(unit.HP, Is.EqualTo(60f));
            Assert.That(unit.HealthBar.FillAmount, Is.EqualTo(0.5f).Within(0.01f));
            Assert.That(unit.HealthBar.FillColor, Is.EqualTo(Color.yellow));

            unit.Heal(1000f);
            Assert.That(unit.HP, Is.EqualTo(unit.MaxHP));

            bool deathNotified = false;
            void HandleDeath(UnitBase _) => deathNotified = true;
            UnitBase.OnUnitDied += HandleDeath;
            unit.TakeDamage(unit.MaxHP);
            yield return null;
            UnitBase.OnUnitDied -= HandleDeath;

            Assert.That(deathNotified, Is.True);
            Assert.That(UnitManager.Instance.GetUnitCount(true), Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator FrontlineMovesToCenterAndPrioritizesEnemyFrontline()
        {
            yield return LoadMatch();
            UnitSpawner spawner = Object.FindFirstObjectByType<UnitSpawner>();
            UnitBase player = spawner.Spawn(UnitRole.Frontline, true, new Vector3(0f, 0.6f, -4.5f));
            yield return null;

            float startZ = player.transform.position.z;
            yield return new WaitForSeconds(0.5f);
            Assert.That(player.transform.position.z, Is.GreaterThan(startZ + 0.2f));
            Assert.That(player.transform.position.x, Is.InRange(BattlefieldNavigation.MinX, BattlefieldNavigation.MaxX));

            Object.Destroy(player.gameObject);
            yield return null;

            UnitBase attacker = spawner.Spawn(UnitRole.Frontline, true, new Vector3(0f, 0.6f, -1.35f));
            UnitBase enemyRanged = spawner.Spawn(UnitRole.Ranged, false, new Vector3(0.7f, 0.6f, -0.2f));
            UnitBase enemyFrontline = spawner.Spawn(UnitRole.Frontline, false, new Vector3(-0.7f, 0.6f, -0.2f));
            yield return null;
            yield return new WaitForSeconds(0.2f);

            Assert.That(attacker.IsAlive, Is.True);
            Assert.That(enemyFrontline.HP, Is.LessThan(enemyFrontline.MaxHP));
            Assert.That(enemyRanged.HP, Is.EqualTo(enemyRanged.MaxHP));
        }

        [UnityTest]
        public IEnumerator SupportHealsLowestHealthAllyAndNeverDamagesEnemies()
        {
            yield return LoadMatch();
            UnitSpawner spawner = Object.FindFirstObjectByType<UnitSpawner>();
            UnitBase ally = spawner.Spawn(UnitRole.Frontline, true, new Vector3(0f, 0.6f, -4.2f));
            UnitBase support = spawner.Spawn(UnitRole.Support, true, new Vector3(1f, 0.6f, -4.4f));
            UnitBase enemy = spawner.Spawn(UnitRole.Frontline, false, new Vector3(0f, 0.6f, 4.4f));
            yield return null;

            ally.TakeDamage(40f);
            float enemyStartingHp = enemy.HP;
            yield return new WaitForSeconds(1.2f);

            Assert.That(support.Role, Is.EqualTo(UnitRole.Support));
            Assert.That(ally.HP, Is.GreaterThan(ally.MaxHP - 40f));
            Assert.That(ally.HP, Is.LessThanOrEqualTo(ally.MaxHP));
            Assert.That(enemy.HP, Is.EqualTo(enemyStartingHp));
            Assert.That(support.transform.position.z, Is.LessThan(-3f));
        }

        [UnityTest]
        public IEnumerator RangedPrioritizesEnemyRangedAndRetreatsFromDirectThreat()
        {
            yield return LoadMatch();
            UnitSpawner spawner = Object.FindFirstObjectByType<UnitSpawner>();
            UnitBase ranged = spawner.Spawn(UnitRole.Ranged, true, new Vector3(0f, 0.6f, -4.4f));
            UnitBase enemyRanged = spawner.Spawn(UnitRole.Ranged, false, new Vector3(0f, 0.6f, -1.2f));
            UnitBase enemyFrontline = spawner.Spawn(UnitRole.Frontline, false, new Vector3(1f, 0.6f, -3.4f));
            yield return null;

            float startZ = ranged.transform.position.z;
            yield return new WaitForSeconds(0.5f);

            Assert.That(ranged.AttackRange, Is.GreaterThan(enemyFrontline.AttackRange));
            Assert.That(ranged.AttackRate, Is.LessThan(enemyFrontline.AttackRate));
            Assert.That(enemyRanged.HP, Is.LessThan(enemyRanged.MaxHP));
            Assert.That(enemyFrontline.HP, Is.EqualTo(enemyFrontline.MaxHP));
            Assert.That(ranged.transform.position.z, Is.LessThanOrEqualTo(startZ));
        }

        [Test]
        public void BakedNavMeshCoversCenterAndBattlefieldBounds()
        {
            SceneManager.LoadScene("Match", LoadSceneMode.Single);

            Assert.That(NavMesh.SamplePosition(new Vector3(0f, 0.6f, 0f), out _, 0.5f, NavMesh.AllAreas), Is.True);
            Assert.That(NavMesh.SamplePosition(new Vector3(3.25f, 0.6f, 5.2f), out _, 0.5f, NavMesh.AllAreas), Is.True);
            Assert.That(NavMesh.SamplePosition(new Vector3(5f, 0.6f, 0f), out _, 0.5f, NavMesh.AllAreas), Is.False);

            NavMeshTriangulation triangulation = NavMesh.CalculateTriangulation();
            Assert.That(triangulation.vertices, Is.Not.Empty);
            Assert.That(triangulation.vertices.All(vertex =>
                vertex.x >= -4.01f && vertex.x <= 4.01f &&
                vertex.z >= -6.01f && vertex.z <= 6.01f), Is.True);
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
