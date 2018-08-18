﻿using System.Collections.Generic;
using System.Linq;

using FluentAssertions;

using Moq;

using NUnit.Framework;

using Zilon.Core.Players;
using Zilon.Core.Tactics;
using Zilon.Core.Tactics.Behaviour;
using Zilon.Core.Tactics.Behaviour.Bots;
using Zilon.Core.Tactics.Spatial;
using Zilon.Core.Tests.Common;

namespace Zilon.Core.Tests.Tactics.Behaviour.Bots
{
    /// <summary>
    /// Тесты для проверки корректности обхода точек одним актёром.
    /// </summary>
    [TestFixture]
    public class PatrolLogicOneActorBypassTests
    {
        private const int _expectedIdleDuration = 1;

        private IMapNode _factActorNode;
        private IMap _map;
        private IPlayer _player;
        private IActor _actor;
        private IPatrolRoute _patrolRoute3Points;
        private IPatrolRoute _patrolRoute2DiagPoints;
        private IActorManager _actorList;
        private IDecisionSource _decisionSource;

        [SetUp]
        public void SetUp()
        {
            _map = new TestGridGenMap();


            var playerMock = new Mock<IPlayer>();
            _player = playerMock.Object;


            var actorMock = new Mock<IActor>();
            actorMock.SetupGet(x => x.Node).Returns(() => _factActorNode);
            actorMock.Setup(x => x.MoveToNode(It.IsAny<IMapNode>()))
                .Callback<IMapNode>(node => _factActorNode = node);
            actorMock.SetupGet(x => x.Owner).Returns(_player);
            _actor = actorMock.Object;

            // Маршрут из 3-х точек
            var patrolRoute3PointsMock = new Mock<IPatrolRoute>();
            var route3Points = new IMapNode[] {
                _map.Nodes.OfType<HexNode>().SelectBy(1, 1),
                _map.Nodes.OfType<HexNode>().SelectBy(5, 3),
                _map.Nodes.OfType<HexNode>().SelectBy(3, 5)
            };
            patrolRoute3PointsMock
                .SetupGet(x => x.Points)
                .Returns(route3Points);
            _patrolRoute3Points = patrolRoute3PointsMock.Object;

            // Маршрут из 2-х точек по диагонали комнаты
            var patrolRoute2DiagPointsMock = new Mock<IPatrolRoute>();
            var route2DiagPoints = new IMapNode[] {
                _map.Nodes.OfType<HexNode>().SelectBy(0, 0),
                _map.Nodes.OfType<HexNode>().SelectBy(9, 9)
            };
            patrolRoute2DiagPointsMock
                .SetupGet(x => x.Points)
                .Returns(route2DiagPoints);
            _patrolRoute2DiagPoints = patrolRoute2DiagPointsMock.Object;


            var actors = new List<IActor> { _actor };
            var actorListMock = new Mock<IActorManager>();
            actorListMock.SetupGet(x => x.Actors).Returns(actors);
            _actorList = actorListMock.Object;


            var decisionSourceMock = new Mock<IDecisionSource>();
            decisionSourceMock.Setup(x => x.SelectIdleDuration(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(_expectedIdleDuration);
            _decisionSource = decisionSourceMock.Object;
        }

        /// <summary>
        /// Тест проверяет, что актёр, следуемый логике патрулирования будет
        /// корректно обходить ключевые точки.
        /// В точка должен быть простой на 1 ход.
        /// Изначально актёр начинает патрулирование с первой точки обхода.
        /// </summary>
        [Test]
        public void GetCurrentTask_StartOnFirstPoint_ActorWalkThroughRount()
        {
            // ARRANGE


            _factActorNode = _map.Nodes.OfType<HexNode>().SelectBy(1, 1);

            var expectedActorPositions = new IMapNode[] {
                _map.Nodes.OfType<HexNode>().SelectBy(2, 2),
                _map.Nodes.OfType<HexNode>().SelectBy(2, 3),
                _map.Nodes.OfType<HexNode>().SelectBy(3, 3),
                _map.Nodes.OfType<HexNode>().SelectBy(4, 3),
                _map.Nodes.OfType<HexNode>().SelectBy(5, 3),

                _map.Nodes.OfType<HexNode>().SelectBy(5, 3),

                _map.Nodes.OfType<HexNode>().SelectBy(4, 3),
                _map.Nodes.OfType<HexNode>().SelectBy(4, 4),
                _map.Nodes.OfType<HexNode>().SelectBy(3, 5),

                _map.Nodes.OfType<HexNode>().SelectBy(3, 5),

                _map.Nodes.OfType<HexNode>().SelectBy(3, 4),
                _map.Nodes.OfType<HexNode>().SelectBy(2, 3),
                _map.Nodes.OfType<HexNode>().SelectBy(2, 2),
                _map.Nodes.OfType<HexNode>().SelectBy(1, 1),

                _map.Nodes.OfType<HexNode>().SelectBy(1, 1),
            };

            var tacticalActUsageService = CreateTacticalActUsageService();

            var logic = new PatrolLogic(_actor, 
                _patrolRoute3Points, 
                _map, 
                _actorList,
                _decisionSource, 
                tacticalActUsageService);



            // ACT
            for (var round = 0; round < expectedActorPositions.Count() + 1; round++)
            {
                var task = logic.GetCurrentTask();


                // ASSERT
                task.Should().NotBeNull();
                switch (round)
                {
                    case 5:
                    case 9:
                    case 14:
                        task.Should().BeOfType<IdleTask>($"На {round} итерации ожидается задача на простой.");
                        break;

                    default:
                        task.Should().BeOfType<MoveTask>($"На {round} итерации ожидается задача на перемещение.");
                        break;
                }

                task.Execute();

                if (round < expectedActorPositions.Count())
                {
                    _factActorNode.Should().Be(expectedActorPositions[round],
                        $"На {round} итерации неожиданные координаты актёра.");
                }
                else
                {
                    _factActorNode.Should().Be(expectedActorPositions[0],
                        $"На {round} итерации актёр должен начать маршрут заново.");
                }
            }

        }


        /// <summary>
        /// Тест проверяет, что актёр, следуемый логике патрулирования будет
        /// корректно обходить ключевые точки.
        /// В точка должен быть простой на 1 ход.
        /// Изначально актёр начинает патрулирование в стороне от маршрута патрулирования.
        /// Ожидается, что актёр в первую очередь посетит ближайшую точку патрулирования
        /// и продолжит обход в порядке точек патрулирования.
        /// </summary>
        [Test]
        public void GetCurrentTask_StartOnSideOnRoute_ActorWalkThroughRount()
        {
            // ARRANGE


            _factActorNode = _map.Nodes.OfType<HexNode>().SelectBy(0, 0);

            var tacticalActUsageService = CreateTacticalActUsageService();

            var logic = new PatrolLogic(_actor,
                _patrolRoute3Points,
                _map,
                _actorList,
                _decisionSource,
                tacticalActUsageService);

            const int expectedStepsToPatrolPoint_1_1 = 2;
            var expectedNode = _patrolRoute3Points.Points.First();



            // ACT
            for (var round = 0; round < expectedStepsToPatrolPoint_1_1; round++)
            {
                var task = logic.GetCurrentTask();

                task.Execute();
            }



            // ASSERT
            var factHexNode = (HexNode)_factActorNode;
            var expectedHexNode = (HexNode)expectedNode;
            factHexNode.OffsetX.Should().Be(expectedHexNode.OffsetX);
            factHexNode.OffsetY.Should().Be(expectedHexNode.OffsetY);
        }

        /// <summary>
        /// Тест проверяет, что актёр, следуемый логике патрулирования будет
        /// корректно обходить ключевые точки.
        /// В точка должен быть простой на 1 ход.
        /// Изначально актёр начинает патрулирование в стороне от маршрута патрулирования.
        /// Ожидается, что актёр в первую очередь посетит ближайшую точку патрулирования
        /// и продолжит обход в порядке точек патрулирования.
        /// Отличается от предыдущего тем, что стартовая точка лежит рядов со второй точкой маршрута.
        /// </summary>
        [Test]
        public void GetCurrentTask_StartOnSideOnRoute2_ActorWalkThroughRount()
        {
            // ARRANGE


            _factActorNode = _map.Nodes.OfType<HexNode>().SelectBy(5, 1);

            var tacticalActUsageService = CreateTacticalActUsageService();

            var logic = new PatrolLogic(_actor,
                _patrolRoute3Points,
                _map,
                _actorList,
                _decisionSource,
                tacticalActUsageService);

            const int expectedStepsToPatrolPoint_5_3 = 2;
            var expectedNode = _patrolRoute3Points.Points[1];  // вторая точка маршрута



            // ACT
            for (var round = 0; round < expectedStepsToPatrolPoint_5_3; round++)
            {
                var task = logic.GetCurrentTask();

                task.Execute();
            }



            // ASSERT
            var factHexNode = (HexNode)_factActorNode;
            var expectedHexNode = (HexNode)expectedNode;
            factHexNode.OffsetX.Should().Be(expectedHexNode.OffsetX);
            factHexNode.OffsetY.Should().Be(expectedHexNode.OffsetY);
        }

        /// <summary>
        /// Тест проверяет, что актёр, следуемый логике патрулирования будет
        /// корректно обходить ключевые точки.
        /// В точка должен быть простой на 1 ход.
        /// Изначально актёр начинает патрулирование в стороне от маршрута патрулирования.
        /// Ожидается, что актёр в первую очередь посетит ближайшую точку патрулирования
        /// и продолжит обход в порядке точек патрулирования.
        /// Отличается от предыдущего тем, что стартовая точка лежит рядом с третьей точкой маршрута.
        /// </summary>
        [Test]
        public void GetCurrentTask_StartOnSideOnRoute3_ActorWalkThroughRount()
        {
            // ARRANGE


            _factActorNode = _map.Nodes.OfType<HexNode>().SelectBy(2, 6);

            var tacticalActUsageService = CreateTacticalActUsageService();

            var logic = new PatrolLogic(_actor,
                _patrolRoute3Points,
                _map,
                _actorList,
                _decisionSource,
                tacticalActUsageService);

            const int expectedStepsToPatrolPoint_3_5 = 2;
            var expectedNode = _patrolRoute3Points.Points[2];  // третья точка маршрута



            // ACT
            for (var round = 0; round < expectedStepsToPatrolPoint_3_5; round++)
            {
                var task = logic.GetCurrentTask();

                task.Execute();
            }



            // ASSERT
            var factHexNode = (HexNode)_factActorNode;
            var expectedHexNode = (HexNode)expectedNode;
            factHexNode.OffsetX.Should().Be(expectedHexNode.OffsetX);
            factHexNode.OffsetY.Should().Be(expectedHexNode.OffsetY);
        }

        /// <summary>
        /// Тест проверяет, что актёр, следуемый логике патрулирования будет
        /// корректно обходить ключевые точки.
        /// В точка должен быть простой на 1 ход.
        /// Изначально актёр начинает патрулирование с первой точки обхода.
        /// </summary>
        [Test]
        public void GetCurrentTask_2PointsDiag_ReturningToStartPoint()
        {
            // ARRANGE

            const int expectedStepsToSecondPoint = 10;
            const int expectedWaiting = 1;
            const int expectedReturningStep = expectedStepsToSecondPoint +
               expectedWaiting + 1;

            var expectedSecondPoint = _patrolRoute2DiagPoints.Points.Last();

            _factActorNode = _map.Nodes.OfType<HexNode>().SelectBy(0, 0);

            var tacticalActUsageService = CreateTacticalActUsageService();

            var logic = new PatrolLogic(_actor,
                _patrolRoute3Points,
                _map,
                _actorList,
                _decisionSource,
                tacticalActUsageService);



            // ACT
            for (var round = 0; round < expectedReturningStep; round++)
            {
                var task = logic.GetCurrentTask();

                task.Execute();
            }

            _factActorNode.Should().NotBe(expectedSecondPoint);
        }

        private ITacticalActUsageService CreateTacticalActUsageService()
        {
            var tacticalActUsageServiceMock = new Mock<ITacticalActUsageService>();
            var tacticalActUsageService = tacticalActUsageServiceMock.Object;
            return tacticalActUsageService;
        }
    }
}