﻿using System;
using System.Linq;
using LightInject;

using Zilon.Core.MapGenerators;
using Zilon.Core.Schemes;
using Zilon.Core.Tactics;
using Zilon.Core.Tactics.Spatial;

namespace Zilon.Core.MassSectorGenerator
{
    class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            var startUp = new Startup();
            var serviceContainer = new ServiceContainer();
            startUp.RegisterServices(serviceContainer);


            var schemeService = serviceContainer.GetInstance<ISchemeService>();
            var allLocations = schemeService.GetSchemes<ILocationScheme>()
                .Where(x => x.SectorLevels != null).ToArray();

            var random = new Random();
            var iteration = 0;
            while (true)
            {
                var schemeCount = allLocations.Length;
                var randomSchemeIndex = random.Next(0, schemeCount);
                var sectorScheme = allLocations[randomSchemeIndex];
                var sectorLevelIndex = random.Next(0, sectorScheme.SectorLevels.Length);
                var sectorLevel = sectorScheme.SectorLevels[sectorLevelIndex];

                iteration++;

                using (var scopeContainer = serviceContainer.BeginScope())
                {
                    var sectorFactory = scopeContainer.GetInstance<ISectorGenerator>();
                    var sector = await sectorFactory.GenerateDungeonAsync(sectorLevel);

                    // Проверка

                    // Проверка сундуков.
                    // Сундуки не должны генерироваться на узлы, которые являются препятствием.
                    // Сундуки не должны генерироваться на узлы с выходом.
                    var containerManager = scopeContainer.GetInstance<IPropContainerManager>();
                    var allContainers = containerManager.Items;
                    foreach (var container in allContainers)
                    {
                        // Проверяем, что сундук не стоит на препятствии.
                        var hex = (HexNode)container.Node;
                        if (hex.IsObstacle)
                        {
                            throw new System.Exception();
                        }

                        // Проверяем, что сундук не на клетке с выходом.
                        var transitionNodes = sector.Map.Transitions.Keys;
                        var chestOnTransitionNode = transitionNodes.Contains(container.Node);
                        if (chestOnTransitionNode)
                        {
                            throw new System.Exception();
                        }
                    }

                    // Проверка монстров.
                    // Монстры не должны генерироваться на узлах с препятствием.
                    // Монстры не должны генерироваться на узлах с сундуками.
                    var actorManager = scopeContainer.GetInstance<IActorManager>();
                    var allMonsters = actorManager.Items;
                    var containerNodes = allContainers.Select(x => x.Node);
                    foreach (var actor in allMonsters)
                    {
                        var hex = (HexNode)actor.Node;
                        if (hex.IsObstacle)
                        {
                            throw new System.Exception();
                        }

                        var monsterIsOnContainer = containerNodes.Contains(actor.Node);
                        if (monsterIsOnContainer)
                        {
                            throw new System.Exception();
                        }
                    }
                }

                Console.WriteLine($"Iteration {iteration:D5} complete");
                Console.WriteLine($"{sectorScheme.Name.En} Level {sectorLevelIndex}");
            }
        }
    }
}