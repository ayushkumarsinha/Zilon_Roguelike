﻿using System.Collections.Generic;
using System.Linq;

using Zilon.Core.Persons;
using Zilon.Core.Players;
using Zilon.Core.Schemes;
using Zilon.Core.Tactics;
using Zilon.Core.Tactics.Behaviour.Bots;
using Zilon.Core.Tactics.Spatial;

namespace Zilon.Core.MapGenerators
{
    public class SectorProceduralGenerator : ISectorProceduralGenerator
    {
        private readonly IActorManager _actorManager;
        private readonly IPropContainerManager _propContainerManager;
        private readonly ISectorGeneratorRandomSource _randomSource;
        private readonly IBotPlayer _botPlayer;
        private readonly ISchemeService _schemeService;
        private readonly IDropResolver _dropResolver;
        private readonly IMapFactory _mapFactory;

        public SectorProceduralGenerator(IActorManager actorManager,
            IPropContainerManager propContainerManager,
            ISectorGeneratorRandomSource randomSource,
            IBotPlayer botPlayer,
            ISchemeService schemeService,
            IDropResolver dropResolver,
            IMapFactory mapFactory)
        {
            _actorManager = actorManager;
            _propContainerManager = propContainerManager;
            _randomSource = randomSource;
            _botPlayer = botPlayer;
            _schemeService = schemeService;
            _dropResolver = dropResolver;
            _mapFactory = mapFactory;
        }

        public ISector Generate()
        {
            var map = _mapFactory.Create();

            var sector = new Sector(map,
                _actorManager,
                _propContainerManager,
                _dropResolver,
                _schemeService);

            var monsterRegions = map.Regions.Where(x => x != map.StartRegion);
            CreateRoomMonsters(sector, monsterRegions);

            CreateChests(monsterRegions);

            return sector;
        }

        private void CreateChests(IEnumerable<MapRegion> rooms)
        {
            var defaultDropTable = _schemeService.GetScheme<IDropTableScheme>("default");
            var survivalDropTable = _schemeService.GetScheme<IDropTableScheme>("survival");

            foreach (var room in rooms)
            {
                var absNodeIndex = room.Nodes.Count();
                var containerNode = room.Nodes[absNodeIndex / 2];
                var container = new DropTablePropChest(containerNode,
                    new[] { defaultDropTable, survivalDropTable },
                    _dropResolver);
                _propContainerManager.Add(container);
            }
        }

        private void CreateRoomMonsters(ISector sector, IEnumerable<MapRegion> regions)
        {
            var monsterScheme = _schemeService.GetScheme<IMonsterScheme>("rat");

            //TODO Учесть вероятность, что монстр может инстанцироваться на сундук
            foreach (var region in regions)
            {
                // В каждую комнату генерируем по 2 монстра
                // первый ходит по маршруту

                var startNode1 = (HexNode)region.Nodes.FirstOrDefault();
                var actor1 = CreateMonster(monsterScheme, startNode1);

                var finishPatrolNode = region.Nodes.Last();
                var patrolRoute = new PatrolRoute(startNode1, finishPatrolNode);
                sector.PatrolRoutes[actor1] = patrolRoute;

                // второй произвольно бродит

                var startNode2 = (HexNode)region.Nodes.Skip(3).FirstOrDefault();
                CreateMonster(monsterScheme, startNode2);
            }
        }

        private IActor CreateMonster(IMonsterScheme monsterScheme, HexNode startNode)
        {
            var person = new MonsterPerson(monsterScheme);
            var actor = new Actor(person, _botPlayer, startNode);
            _actorManager.Add(actor);
            return actor;
        }
    }
}
