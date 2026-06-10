using JigsawVina.Presentation.Screens;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using VContainer.Unity;

namespace JigsawVina.Tests
{
    public class LifetimeScopeRegistrationTests
    {
        [Test]
        public void HomeLifetimeScope_LivesInPresentationScreensNamespace()
        {
            Assert.That(typeof(HomeLifetimeScope).Namespace, Is.EqualTo("JigsawVina.Presentation.Screens"));
            Assert.That(typeof(LifetimeScope).IsAssignableFrom(typeof(HomeLifetimeScope)), Is.True);
        }

        [Test]
        public void GameplayLifetimeScope_LivesInPresentationScreensNamespace()
        {
            Assert.That(typeof(GameplayLifetimeScope).Namespace, Is.EqualTo("JigsawVina.Presentation.Screens"));
            Assert.That(typeof(LifetimeScope).IsAssignableFrom(typeof(GameplayLifetimeScope)), Is.True);
        }

        [Test]
        public void HomeScene_UsesPresentationScreensHomeLifetimeScope()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Home.unity", OpenSceneMode.Additive);
            try
            {
                var scope = FindInScene<HomeLifetimeScope>(scene.GetRootGameObjects());

                Assert.That(scope, Is.Not.Null);
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void GameplayScene_UsesPresentationScreensGameplayLifetimeScope()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Gameplay.unity", OpenSceneMode.Additive);
            try
            {
                var scope = FindInScene<GameplayLifetimeScope>(scene.GetRootGameObjects());

                Assert.That(scope, Is.Not.Null);
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static T FindInScene<T>(UnityEngine.GameObject[] rootObjects)
            where T : UnityEngine.Component
        {
            foreach (var rootObject in rootObjects)
            {
                var component = rootObject.GetComponentInChildren<T>(true);
                if (component != null)
                {
                    return component;
                }
            }

            return null;
        }
    }
}
