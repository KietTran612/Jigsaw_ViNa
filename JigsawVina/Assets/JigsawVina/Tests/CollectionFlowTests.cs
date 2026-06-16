using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JigsawVina.Core.Data;
using JigsawVina.Core.Services;
using JigsawVina.Presentation.Screens;
using NUnit.Framework;
using UnityEngine;

namespace JigsawVina.Tests
{
    public class CollectionFlowTests
    {
        [Test]
        public void CollectionPresenter_BuildsOwnedItemWithDeduplicatedSortedSources()
        {
            Type viewType = typeof(PictureSelectView).Assembly.GetType(
                "JigsawVina.Presentation.Screens.CollectionView");
            Type presenterType = typeof(PictureSelectView).Assembly.GetType(
                "JigsawVina.Presentation.Screens.CollectionPresenter");
            Assert.That(viewType, Is.Not.Null);
            Assert.That(presenterType, Is.Not.Null);

            var holder = new GameObject("CollectionView");
            try
            {
                var view = holder.AddComponent(viewType);
                var staticData = new CollectionStaticDataService();
                var saveData = new MockSaveDataService
                {
                    SaveData = new PlayerSave
                    {
                        OwnedItemIds = new List<int> { 101 }
                    }
                };

                object presenter = Activator.CreateInstance(
                    presenterType,
                    view,
                    saveData,
                    staticData);
                var models = (IEnumerable)presenterType
                    .GetProperty("CurrentModels", BindingFlags.Instance | BindingFlags.Public)
                    ?.GetValue(presenter);
                Assert.That(models, Is.Not.Null);

                var modelList = models.Cast<object>().ToList();
                Assert.That(modelList, Has.Count.EqualTo(1));
                object model = modelList[0];
                Assert.That(
                    (int)model.GetType().GetProperty("ItemId")?.GetValue(model),
                    Is.EqualTo(101));

                var sources = ((IEnumerable)model.GetType()
                        .GetProperty("Sources")?.GetValue(model))
                    .Cast<object>()
                    .ToList();
                Assert.That(sources, Has.Count.EqualTo(2));
                Assert.That(GetInt(sources[0], "PictureId"), Is.EqualTo(1));
                Assert.That(GetInt(sources[0], "DifficultyId"), Is.EqualTo(0));
                Assert.That(GetInt(sources[1], "PictureId"), Is.EqualTo(2));
                Assert.That(GetInt(sources[1], "DifficultyId"), Is.EqualTo(1));

                ((IDisposable)presenter).Dispose();
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(holder);
            }
        }

        [Test]
        public void PictureSelectView_ExposesCollectionNavigationContract()
        {
            Assert.That(
                typeof(PictureSelectView).GetEvent("OnCollectionRequested"),
                Is.Not.Null);
            Assert.That(
                typeof(PictureSelectView).GetMethod(
                    "RequestPictureSelection",
                    new[] { typeof(int) }),
                Is.Not.Null);
            Assert.That(
                typeof(PictureSelectView).GetMethod(
                    "FocusCard",
                    new[] { typeof(int) }),
                Is.Not.Null);
            Assert.That(
                typeof(PictureSelectCard).GetProperty("PictureId"),
                Is.Not.Null);
            Assert.That(
                typeof(PictureSelectCard).GetMethod("Highlight", Type.EmptyTypes),
                Is.Not.Null);
        }

        [Test]
        public void HomeFlow_CollectionNavigation_UsesUnlockState()
        {
            var pictureObject = new GameObject("PictureView");
            var difficultyObject = new GameObject("DifficultyView");
            var collectionObject = new GameObject("CollectionView");
            try
            {
                var pictureView = pictureObject.AddComponent<PictureSelectView>();
                var difficultyView = difficultyObject.AddComponent<DifficultySelectView>();
                var collectionView = collectionObject.AddComponent<CollectionView>();
                var staticData = new CollectionStaticDataService();
                var saveData = new MockSaveDataService
                {
                    SaveData = new PlayerSave
                    {
                        OwnedItemIds = new List<int> { 101 }
                    }
                };
                var progression = new ProgressionService(staticData, saveData);
                var collectionPresenter = new CollectionPresenter(
                    collectionView,
                    saveData,
                    staticData);
                var controller = new HomeFlowController(
                    pictureView,
                    difficultyView,
                    null,
                    null,
                    collectionView,
                    collectionPresenter,
                    progression);
                int selectedPictureId = 0;
                pictureView.OnPictureSelected += id => selectedPictureId = id;
                controller.Start();

                collectionView.RequestNavigation(1);

                Assert.That(selectedPictureId, Is.EqualTo(1));
                Assert.That(pictureView.gameObject.activeSelf, Is.False);
                Assert.That(difficultyView.gameObject.activeSelf, Is.True);
                Assert.That(collectionView.gameObject.activeSelf, Is.False);

                difficultyView.SetActive(false);
                pictureView.SetActive(false);
                collectionView.SetActive(true);
                selectedPictureId = 0;

                collectionView.RequestNavigation(2);

                Assert.That(selectedPictureId, Is.Zero);
                Assert.That(pictureView.gameObject.activeSelf, Is.True);
                Assert.That(difficultyView.gameObject.activeSelf, Is.False);
                Assert.That(collectionView.gameObject.activeSelf, Is.False);

                controller.Dispose();
                collectionPresenter.Dispose();
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(pictureObject);
                UnityEngine.Object.DestroyImmediate(difficultyObject);
                UnityEngine.Object.DestroyImmediate(collectionObject);
            }
        }

        [Test]
        public void CollectionPresenter_Dispose_UnsubscribesNavigation()
        {
            var holder = new GameObject("CollectionView");
            try
            {
                var view = holder.AddComponent<CollectionView>();
                var presenter = new CollectionPresenter(
                    view,
                    new MockSaveDataService(),
                    new CollectionStaticDataService());
                int requestedPictureId = 0;
                presenter.OnNavigateToPictureRequested +=
                    id => requestedPictureId = id;

                presenter.Dispose();
                view.RequestNavigation(2);

                Assert.That(requestedPictureId, Is.Zero);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(holder);
            }
        }

        private static int GetInt(object target, string propertyName)
        {
            return (int)target.GetType().GetProperty(propertyName)?.GetValue(target);
        }

        private sealed class CollectionStaticDataService : IStaticDataService
        {
            private readonly List<PictureConfig> _pictures = new()
            {
                new PictureConfig(
                    1, "pic1", "Pic 1", "", "", "",
                    true, "sequential", new List<int>()),
                new PictureConfig(
                    2, "pic2", "Pic 2", "", "", "",
                    false, "sequential", new List<int> { 999 })
            };

            private readonly List<PictureDifficultyConfig> _difficulties = new()
            {
                new PictureDifficultyConfig(
                    2, 1, "Normal", 8, 6, 2, 60, 0, 20,
                    new List<int>(), 1002),
                new PictureDifficultyConfig(
                    1, 0, "Easy", 6, 4, 1, 30, 0, 10,
                    new List<int> { 101 }, 1001)
            };

            private readonly List<DropTableItemConfig> _dropItems = new()
            {
                new DropTableItemConfig(
                    1, "drop1", "Drop 1", 1001, 101,
                    0.5f, 0.1f, 0.2f, 1, 1, "active"),
                new DropTableItemConfig(
                    2, "drop2", "Drop 2", 1002, 101,
                    0.5f, 0.1f, 0.2f, 1, 1, "active")
            };

            public IReadOnlyList<PictureConfig> GetAllPictures() => _pictures;
            public PictureConfig GetPictureById(int id) =>
                _pictures.FirstOrDefault(picture => picture.Id == id);
            public PictureDifficultyConfig GetPictureDifficulty(int pictureId, int difficultyId) =>
                _difficulties.First(difficulty =>
                    difficulty.PictureId == pictureId &&
                    difficulty.DifficultyId == difficultyId);
            public ItemDto GetItemById(int id) => id == 101
                ? new ItemDto
                {
                    id = 101,
                    display_name = "Key 101",
                    description = "Description",
                    item_type = "key_item",
                    status = "active"
                }
                : null;
            public IReadOnlyList<ItemDto> GetAllItems() =>
                new List<ItemDto> { GetItemById(101) };
            public IReadOnlyList<PictureDifficultyConfig> GetPictureDifficulties(int pictureId) =>
                _difficulties.Where(difficulty => difficulty.PictureId == pictureId).ToList();
            public IReadOnlyList<PictureDifficultyConfig> GetAllPictureDifficulties() =>
                _difficulties;
            public IReadOnlyList<DropTableConfig> GetAllDropTables() =>
                new List<DropTableConfig>();
            public IReadOnlyList<DropTableItemConfig> GetDropTableItems(int dropTableId) =>
                _dropItems.Where(item => item.DropTableId == dropTableId).ToList();
            public IReadOnlyList<DropTableItemConfig> GetAllDropTableItems() =>
                _dropItems;
            public IReadOnlyList<DailyRewardConfig> GetDailyRewards() => new List<DailyRewardConfig>();
        }
    }
}
