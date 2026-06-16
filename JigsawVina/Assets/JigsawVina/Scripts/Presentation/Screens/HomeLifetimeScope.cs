using System;
using JigsawVina.Core.Services;
using VContainer;
using VContainer.Unity;

namespace JigsawVina.Presentation.Screens
{
    public class HomeLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponentInHierarchy<PictureSelectView>();
            builder.RegisterComponentInHierarchy<DifficultySelectView>();
            builder.RegisterComponentInHierarchy<CollectionView>();
            builder.RegisterComponentInHierarchy<DailyRewardView>();
            builder.Register<PictureSelectPresenter>(Lifetime.Singleton);
            builder.Register<DifficultySelectPresenter>(Lifetime.Singleton);
            builder.Register<CollectionPresenter>(Lifetime.Singleton);
            builder.Register<DailyRewardPresenter>(Lifetime.Singleton);
            builder.RegisterEntryPoint<HomeFlowController>();
        }
    }
}
