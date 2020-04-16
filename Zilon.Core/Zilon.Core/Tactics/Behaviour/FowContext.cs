﻿using System;
using System.Collections.Generic;

using Zilon.Core.Graphs;
using Zilon.Core.Tactics.Spatial;

namespace Zilon.Core.Tactics.Behaviour
{
    public sealed class FowContext : IFowContext
    {
        private readonly ISectorMap _sectorMap;
        private readonly IStaticObjectManager _staticObjectManager;

        public FowContext(ISectorMap sectorMap, IStaticObjectManager staticObjectManager)
        {
            _sectorMap = sectorMap ?? throw new ArgumentNullException(nameof(sectorMap));
            _staticObjectManager = staticObjectManager ?? throw new ArgumentNullException(nameof(staticObjectManager));
        }

        public IEnumerable<IGraphNode> GetNext(IGraphNode node)
        {
            return _sectorMap.GetNext(node);
        }

        public bool IsTargetVisible(IGraphNode baseNode, IGraphNode targetNode)
        {
            return _sectorMap.TargetIsOnLine(baseNode, targetNode);
        }
    }
}
