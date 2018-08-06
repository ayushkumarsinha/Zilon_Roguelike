﻿using Zilon.Core.Client;

namespace Zilon.Core.Commands
{
    public class NextTurnCommand: ActorCommandBase
    {
        public override bool CanExecute()
        {
            return true;
        }

        protected override void ExecuteTacticCommand()
        {
            
        }

        public NextTurnCommand(ISectorManager sectorManager, 
            IPlayerState playerState) :
            base(sectorManager, playerState)
        {
        }
    }
}