using System.Collections.Generic;
using System.Linq;
using JigsawVina.Core.Data;
using JigsawVina.Core.Services;
using NUnit.Framework;

namespace JigsawVina.Tests
{
    public class DropRewardTests
    {
        [Test]
        public void RollDropRewards_Success_ReturnsRewardAndIncrementsCounter()
        {
            var save = new PlayerSave();
            var random = new FakeRandomSource(0.29f, 2);
            var service = CreateService(
                random,
                CreateItem(10, "consumable", true, 999),
                CreateDrop(10, 0.30f, 0.10f, 0.20f, 1, 2));

            var rewards = service.RollDropRewards(1001, save);

            Assert.That(rewards, Has.Count.EqualTo(1));
            Assert.That(rewards[0].ItemId, Is.EqualTo(10));
            Assert.That(rewards[0].Amount, Is.EqualTo(2));
            Assert.That(save.DailyDropCounts.Single().ItemId, Is.EqualTo(10));
            Assert.That(save.DailyDropCounts.Single().Count, Is.EqualTo(1));
            Assert.That(random.RangeCalls, Is.EqualTo(1));
            Assert.That(random.LastMaxExclusive, Is.EqualTo(3));
        }

        [Test]
        public void RollDropRewards_RollEqualToRate_FailsWithoutCounter()
        {
            var save = new PlayerSave();
            var random = new FakeRandomSource(0.30f);
            var service = CreateService(
                random,
                CreateItem(10, "consumable", true, 999),
                CreateDrop(10, 0.30f, 0.10f, 0.20f));

            var rewards = service.RollDropRewards(1001, save);

            Assert.That(rewards, Is.Empty);
            Assert.That(save.DailyDropCounts, Is.Empty);
            Assert.That(random.RangeCalls, Is.Zero);
        }

        [Test]
        public void RollDropRewards_DecaysAndClampsToMinimumRate()
        {
            var save = new PlayerSave
            {
                DailyDropCounts = new List<DailyDropCount>
                {
                    new() { ItemId = 10, Count = 50 }
                }
            };
            var random = new FakeRandomSource(0.19f);
            var service = CreateService(
                random,
                CreateItem(10, "consumable", true, 999),
                CreateDrop(10, 0.60f, 0.10f, 0.20f));

            var rewards = service.RollDropRewards(1001, save);

            Assert.That(rewards, Has.Count.EqualTo(1));
            Assert.That(save.DailyDropCounts.Single().Count, Is.EqualTo(51));
        }

        [Test]
        public void RollDropRewards_OwnedKeyItem_IsExcludedBeforeRandom()
        {
            var save = new PlayerSave
            {
                OwnedItemIds = new List<int> { 101 },
                DailyDropCounts = new List<DailyDropCount>
                {
                    new() { ItemId = 101, Count = 2 }
                }
            };
            var random = new FakeRandomSource(0f);
            var service = CreateService(
                random,
                CreateItem(101, "key_item", false, 1),
                CreateDrop(101, 1f, 0f, 1f));

            var rewards = service.RollDropRewards(1001, save);

            Assert.That(rewards, Is.Empty);
            Assert.That(random.FloatCalls, Is.Zero);
            Assert.That(save.DailyDropCounts.Single().Count, Is.EqualTo(2));
        }

        [Test]
        public void RollDropRewards_FullConsumable_IsExcludedBeforeRandom()
        {
            var save = new PlayerSave
            {
                Inventory = new List<InventoryItem>
                {
                    new() { ItemId = 10, Amount = 3 }
                }
            };
            var random = new FakeRandomSource(0f);
            var service = CreateService(
                random,
                CreateItem(10, "consumable", true, 3),
                CreateDrop(10, 1f, 0f, 1f));

            var rewards = service.RollDropRewards(1001, save);

            Assert.That(rewards, Is.Empty);
            Assert.That(random.FloatCalls, Is.Zero);
            Assert.That(save.DailyDropCounts, Is.Empty);
        }

        [Test]
        public void RollDropRewards_PartialConsumableCapacity_ReturnsAmountThatFits()
        {
            var save = new PlayerSave
            {
                Inventory = new List<InventoryItem>
                {
                    new() { ItemId = 10, Amount = 2 }
                }
            };
            var random = new FakeRandomSource(0f, 3);
            var service = CreateService(
                random,
                CreateItem(10, "consumable", true, 3),
                CreateDrop(10, 1f, 0f, 1f, 1, 3));

            var rewards = service.RollDropRewards(1001, save);

            Assert.That(rewards, Has.Count.EqualTo(1));
            Assert.That(rewards[0].Amount, Is.EqualTo(1));
            Assert.That(save.DailyDropCounts.Single().Count, Is.EqualTo(1));
        }

        [Test]
        public void RollDropRewards_ActiveEntriesRollIndependently()
        {
            var save = new PlayerSave();
            var random = new FakeRandomSource(new[] { 0.1f, 0.9f });
            var staticData = new FakeStaticDataService(
                new[]
                {
                    CreateItem(10, "consumable", true, 999),
                    CreateItem(11, "consumable", true, 999)
                },
                new[]
                {
                    CreateDrop(10, 0.5f, 0f, 0.5f),
                    CreateDrop(11, 0.5f, 0f, 0.5f)
                });
            var service = new DropRewardService(staticData, random);

            var rewards = service.RollDropRewards(1001, save);

            Assert.That(rewards.Select(reward => reward.ItemId), Is.EqualTo(new[] { 10 }));
            Assert.That(random.FloatCalls, Is.EqualTo(2));
            Assert.That(save.DailyDropCounts.Single().ItemId, Is.EqualTo(10));
        }

        private static DropRewardService CreateService(
            FakeRandomSource random,
            ItemDto item,
            DropTableItemConfig drop)
        {
            return new DropRewardService(
                new FakeStaticDataService(new[] { item }, new[] { drop }),
                random);
        }

        private static ItemDto CreateItem(
            int itemId,
            string itemType,
            bool isConsumable,
            int maxStack)
        {
            return new ItemDto
            {
                id = itemId,
                display_name = $"Item {itemId}",
                item_type = itemType,
                is_consumable = isConsumable,
                max_stack = maxStack,
                status = "active"
            };
        }

        private static DropTableItemConfig CreateDrop(
            int itemId,
            float baseRate,
            float decay,
            float minRate,
            int amountMin = 1,
            int amountMax = 1)
        {
            return new DropTableItemConfig(
                itemId,
                $"drop_{itemId}",
                $"Drop {itemId}",
                1001,
                itemId,
                baseRate,
                decay,
                minRate,
                amountMin,
                amountMax,
                "active");
        }

        private sealed class FakeRandomSource : IRandomSource
        {
            private readonly Queue<float> _floats;
            private readonly int _rangeResult;

            public int FloatCalls { get; private set; }
            public int RangeCalls { get; private set; }
            public int LastMaxExclusive { get; private set; }

            public FakeRandomSource(float value, int rangeResult = 1)
                : this(new[] { value }, rangeResult)
            {
            }

            public FakeRandomSource(IEnumerable<float> values, int rangeResult = 1)
            {
                _floats = new Queue<float>(values);
                _rangeResult = rangeResult;
            }

            public float NextFloat()
            {
                FloatCalls++;
                return _floats.Dequeue();
            }

            public int NextRange(int minInclusive, int maxExclusive)
            {
                RangeCalls++;
                LastMaxExclusive = maxExclusive;
                return _rangeResult;
            }
        }

        private sealed class FakeStaticDataService : IStaticDataService
        {
            private readonly List<ItemDto> _items;
            private readonly List<DropTableItemConfig> _dropItems;

            public FakeStaticDataService(
                IEnumerable<ItemDto> items,
                IEnumerable<DropTableItemConfig> dropItems)
            {
                _items = items.ToList();
                _dropItems = dropItems.ToList();
            }

            public IReadOnlyList<PictureConfig> GetAllPictures() => new List<PictureConfig>();
            public PictureConfig GetPictureById(int id) => default;
            public PictureDifficultyConfig GetPictureDifficulty(int pictureId, int difficultyId) => default;
            public ItemDto GetItemById(int id) => _items.FirstOrDefault(item => item.id == id);
            public IReadOnlyList<ItemDto> GetAllItems() => _items;
            public IReadOnlyList<PictureDifficultyConfig> GetPictureDifficulties(int pictureId) => new List<PictureDifficultyConfig>();
            public IReadOnlyList<PictureDifficultyConfig> GetAllPictureDifficulties() => new List<PictureDifficultyConfig>();
            public IReadOnlyList<DropTableConfig> GetAllDropTables() => new List<DropTableConfig>();
            public IReadOnlyList<DropTableItemConfig> GetDropTableItems(int dropTableId) =>
                _dropItems.Where(item => item.DropTableId == dropTableId).ToList();
            public IReadOnlyList<DropTableItemConfig> GetAllDropTableItems() => _dropItems;
            public IReadOnlyList<DailyRewardConfig> GetDailyRewards() => new List<DailyRewardConfig>();
        }
    }
}
