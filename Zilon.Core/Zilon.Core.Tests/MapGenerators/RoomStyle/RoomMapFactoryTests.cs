﻿using System;
using System.Linq;

using FluentAssertions;

using NUnit.Framework;

using Zilon.Core.CommonServices.Dices;
using Zilon.Core.MapGenerators.RoomStyle;
using Zilon.Core.Tactics.Spatial;

namespace Zilon.Core.Tests.MapGenerators.RoomStyle
{
    [TestFixture]
    public class DungeonMapFactoryTests
    {
        /// <summary>
        /// Тест проверяет, что карта из цепочки комнат строится без ошибок.
        /// </summary>
        [Test]
        public void Create_SimpleSnakeMaze_NoExceptions()
        {
            var roomGenerator = new TestSnakeRoomGenerator();
            var factory = new RoomMapFactory(roomGenerator);



            // ACT
            Action act = async () =>
            {
                var map = await factory.CreateAsync(null);
            };



            // ARRANGE
            act.Should().NotThrow();
        }

        /// <summary>
        /// Тест проверяет, что произвольная карта строится без ошибок. И за допустимое время.
        /// </summary>
        [Test]
        [Timeout(3 * 60 * 1000)]
        [TestCase(123)]
        [TestCase(1)]
        [TestCase(8674)]
        [TestCase(1000)]
        public void Create_RealRandom_NoExceptions(int diceSeed)
        {
            // ARRANGE
            var dice = new Dice(diceSeed);
            var randomSource = new RoomGeneratorRandomSource(dice);
            var roomGenerator = new RoomGenerator(randomSource);
            var factory = new RoomMapFactory(roomGenerator);



            // ACT
            Action act = async () =>
            {
                var map = await factory.CreateAsync(null);
            };



            // ARRANGE
            act.Should().NotThrow();
        }

        /// <summary>
        /// Тест проверяет, что карта из цепочки комнат строится без ошибок.
        /// </summary>
        [Test]
        public async System.Threading.Tasks.Task Create_RealRandom_NoOverlapNodesAsync()
        {
            // ARRANGE
            var dice = new Dice(3245);
            var randomSource = new RoomGeneratorRandomSource(dice);
            var roomGenerator = new RoomGenerator(randomSource);
            var factory = new RoomMapFactory(roomGenerator);



            // ACT
            var map = await factory.CreateAsync(null);



            // ARRANGE
            var hexNodes = map.Nodes.Cast<HexNode>().ToArray();
            foreach (var node in hexNodes)
            {
                var sameNode = hexNodes.Where(x => x != node && x.OffsetX == node.OffsetX && x.OffsetY == node.OffsetY);
                sameNode.Should().BeEmpty();
            }
        }
    }
}