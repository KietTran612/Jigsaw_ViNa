using VContainer;
using VContainer.Unity;
using JigsawVina.Core.Services;

namespace JigsawVina.Presentation.App
{
    public class ProjectLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<LocalDateProvider>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<UnityRandomSource>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<DropRewardService>(Lifetime.Singleton).AsImplementedInterfaces();

            builder.Register<AudioService>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<LocalizationService>(Lifetime.Singleton).AsImplementedInterfaces();

            builder.Register(c => new SaveDataService(c.Resolve<ILocalDateProvider>()), Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register(c => new StaticDataService(), Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<RewardApplier>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<DailyRewardService>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<ProgressionService>(Lifetime.Singleton);
            builder.Register<GameSessionService>(Lifetime.Singleton);
            builder.Register<SceneLoader>(Lifetime.Singleton);
        }
    }
}
