using Cysharp.Threading.Tasks;
using JigsawVina.Core.Services;
using VContainer;
using VContainer.Unity;
using UnityEngine;

namespace JigsawVina.Presentation.Screens
{
    public class GameplayLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponentInHierarchy<PuzzlePlayingView>();
            builder.RegisterComponentInHierarchy<RewardSummaryView>();
            builder.Register<PuzzlePlayingPresenter>(Lifetime.Singleton);
            builder.Register<RewardSummaryPresenter>(Lifetime.Singleton);
            builder.RegisterEntryPoint<GameplayFlowController>();
        }
    }

}
