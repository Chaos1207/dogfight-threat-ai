using System.Collections;
using System.Linq;
using System.Reflection;
using DogfightThreatAI.Gameplay;
using DogfightThreatAI.Gameplay.View;
using DogfightThreatAI.Gameplay.World;
using DogfightThreatAI.UI.Menu;
using DogfightThreatAI.UI.Navigation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

namespace DogfightThreatAI.Tests.Menu
{
    public sealed class MenuToBattleTests
    {
        [UnityTest]
        public IEnumerator StartGameAndReturnMenuPreserveBothScenesAndResetAircraft()
        {
            Time.timeScale = 1;
            yield return SceneManager.LoadSceneAsync(SceneNavigation.StartMenuScene, LoadSceneMode.Single);
            var menu = Object.FindObjectOfType<MainMenuController>();
            Assert.That(menu, Is.Not.Null);
            Assert.That(menu.Phase, Is.EqualTo(MainMenuController.MenuPhase.AwaitInput));
            yield return new WaitForSecondsRealtime(.6f);
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(SceneNavigation.StartMenuScene), "Splash must wait for user input");
            Assert.That(Object.FindObjectOfType<PlayerController>(), Is.Null);

            menu.RevealMenu();
            yield return new WaitForSecondsRealtime(1.15f);
            Assert.That(menu.Phase, Is.EqualTo(MainMenuController.MenuPhase.Menu));
            var start = menu.GetComponentsInChildren<Button>().Single(button => button.name == "StartGame");
            Assert.That(start.IsInteractable(), Is.True);
            // Invoke the real Button pointer event, including its scene-bound menu action.
            ExecuteEvents.Execute(start.gameObject,
                new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left },
                ExecuteEvents.pointerClickHandler);
            float deadline = Time.realtimeSinceStartup + 15f;
            while (SceneManager.GetActiveScene().name != SceneNavigation.GameScene && Time.realtimeSinceStartup < deadline)
                yield return null;
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(SceneNavigation.GameScene));
            yield return null;

            Assert.That(Object.FindObjectOfType<MainMenuController>(), Is.Null, "Menu overlay must be unloaded");
            var map = Object.FindObjectOfType<BattleMapView>();
            Assert.That(map, Is.Not.Null);
            Assert.That(map.Map.IsValid, Is.True);
            Assert.That(map.Map.DisplayName, Is.EqualTo("河谷农田"));
            Assert.That(map.LoadedChunkCount, Is.GreaterThan(0));
            Assert.That(map.GetComponentsInChildren<Tilemap>().Any(chunk => chunk.GetUsedTilesCount() > 0), Is.True);
            Assert.That(Object.FindObjectOfType<DebugStarfield>(), Is.Null);

            var player = Object.FindObjectOfType<PlayerController>();
            Assert.That(player, Is.Not.Null);
            Assert.That(player.isActiveAndEnabled, Is.True);
            Assert.That(player.Ship, Is.Not.Null);
            Assert.That(player.GetComponentInChildren<SpriteRenderer>().sprite, Is.EqualTo(player.Ship.BodySprite));
            Assert.That(Camera.main, Is.Not.Null);
            Assert.That(Camera.main.GetComponent<CameraFollow>().isActiveAndEnabled, Is.True);
            var input = player.gameObject.AddComponent<IntegrationFlightInput>();
            typeof(PlayerController).GetField("inputSource", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(player, input);
            Vector3 initial = player.transform.position;
            yield return new WaitForSeconds(.6f);
            Assert.That(Vector3.Distance(initial, player.transform.position), Is.GreaterThan(.05f),
                "Flight simulation must continue after leaving the menu");

            var navigation = Object.FindObjectOfType<SceneNavigation>();
            Assert.That(navigation, Is.Not.Null);
            var back = navigation.GetComponentsInChildren<Button>().Single(button => button.name == "ReturnToMainMenu");
            ExecuteEvents.Execute(back.gameObject,
                new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left },
                ExecuteEvents.pointerClickHandler);
            deadline = Time.realtimeSinceStartup + 15f;
            while (SceneManager.GetActiveScene().name != SceneNavigation.StartMenuScene && Time.realtimeSinceStartup < deadline)
                yield return null;
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(SceneNavigation.StartMenuScene));
            Assert.That(Object.FindObjectOfType<PlayerController>(), Is.Null);
            Assert.That(Object.FindObjectOfType<BattleMapView>(), Is.Null);
            menu = Object.FindObjectOfType<MainMenuController>();
            Assert.That(menu.Phase, Is.EqualTo(MainMenuController.MenuPhase.AwaitInput));
            menu.RevealMenu();
            yield return new WaitForSecondsRealtime(1.15f);
            menu.GetComponentsInChildren<Button>().Single(button => button.name == "StartGame").onClick.Invoke();
            deadline = Time.realtimeSinceStartup + 15f;
            while (SceneManager.GetActiveScene().name != SceneNavigation.GameScene && Time.realtimeSinceStartup < deadline)
                yield return null;
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(SceneNavigation.GameScene));
            Assert.That(Object.FindObjectsOfType<PlayerController>().Length, Is.EqualTo(1));
            Assert.That(Object.FindObjectOfType<PlayerController>().transform.position.y, Is.EqualTo(0).Within(.1f),
                "Starting again must create a fresh aircraft at spawn");
        }

        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            var previous = SceneManager.GetActiveScene();
            var empty = SceneManager.CreateScene("Menu integration cleanup");
            SceneManager.SetActiveScene(empty);
            if (previous.IsValid() && previous.isLoaded) yield return SceneManager.UnloadSceneAsync(previous);
            Time.timeScale = 1;
        }
    }
}
