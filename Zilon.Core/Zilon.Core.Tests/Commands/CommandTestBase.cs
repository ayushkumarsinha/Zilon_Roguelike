﻿using System.Linq;

using LightInject;

using Moq;

using NUnit.Framework;

using Zilon.Core.Client;
using Zilon.Core.Tactics;
using Zilon.Core.Tactics.Behaviour;
using Zilon.Core.Tactics.Spatial;
using Zilon.Core.Tests.Common;

namespace Zilon.Core.Tests.Commands
{
    [TestFixture]
    public abstract class CommandTestBase
    {
        private ServiceContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = new ServiceContainer();

            var testMap = new TestGridGenMap(3);

            var sectorMock = new Mock<ISector>();
            var sector = sectorMock.Object;

            var sectorManagerMock = new Mock<ISectorManager>();
            sectorManagerMock.SetupProperty(x => x.CurrentSector, sector);
            var sectorManager = sectorManagerMock.Object;

            var actorMock = new Mock<IActor>();
            var actorNode = testMap.Nodes.OfType<HexNode>().SelectBy(0, 0);
            actorMock.SetupGet(x => x.Node).Returns(actorNode);
            var actor = actorMock.Object;

            var actorVmMock = new Mock<IActorViewModel>();
            actorVmMock.SetupProperty(x => x.Actor, actor);
            var actorVm = actorVmMock.Object;

            var humanTaskSourceMock = new Mock<IHumanActorTaskSource>();
            var humanTaskSource = humanTaskSourceMock.Object;

            var playerStateMock = new Mock<IPlayerState>();
            playerStateMock.SetupProperty(x => x.ActiveActor, actorVm);
            playerStateMock.SetupProperty(x => x.TaskSource, humanTaskSource);
            var playerState = playerStateMock.Object;


            var gameLoopMock = new Mock<IGameLoop>();
            var gameLoop = gameLoopMock.Object;

            _container.Register(factory => sectorManager, new PerContainerLifetime());
            _container.Register(factory => humanTaskSourceMock, new PerContainerLifetime());
            _container.Register(factory => playerState, new PerContainerLifetime());
            _container.Register(factory => gameLoop, new PerContainerLifetime());

            RegisterSpecificServices(testMap, playerStateMock);
        }

        protected abstract void RegisterSpecificServices(IMap testMap, Mock<IPlayerState> playerStateMock);
    }
}