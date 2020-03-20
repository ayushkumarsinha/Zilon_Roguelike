﻿using System;
using System.Collections.Generic;
using Zilon.Core.Graphs;

using Zilon.Core.Graphs;

namespace Zilon.Core.Tactics
{
    public sealed class MonsterSectorFowData : ISectorFowData
    {
        /// <inheritdoc/>
        public IEnumerable<SectorMapFowNode> Nodes => Array.Empty<SectorMapFowNode>();

        /// <inheritdoc/>
        public void AddNodes(IEnumerable<SectorMapFowNode> nodes)
        {
            // Ничего не делаем. Просто метод для соблюдения интерфейса.
        }

        /// <inheritdoc/>
        public SectorMapFowNode GetNode(IGraphNode node)
        {
            // Возвращаем null, потому что этот объект не предполагает хранение чего-либо.
            return null;
        }
    }
}
