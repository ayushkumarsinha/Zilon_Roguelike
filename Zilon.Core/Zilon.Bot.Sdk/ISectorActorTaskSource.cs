﻿using Zilon.Core.Tactics.Behaviour;

namespace Zilon.Bot.Sdk
{
    public interface ISectorActorTaskSource: IActorTaskSource
    {
        void Configure(IBotSettings botSettings);
    }
}
