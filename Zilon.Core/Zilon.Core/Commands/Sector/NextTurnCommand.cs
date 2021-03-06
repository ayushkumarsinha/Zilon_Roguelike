﻿using Zilon.Core.Client;
using Zilon.Core.Tactics;
using Zilon.Core.Tactics.Behaviour;

namespace Zilon.Core.Commands
{
    public class NextTurnCommand : ActorCommandBase
    {
        public NextTurnCommand(
            ISectorManager sectorManager,
            ISectorUiState playerState) : base(sectorManager, playerState)
        {
        }

        public override bool CanExecute()
        {
            return true;
        }

        protected override void ExecuteTacticCommand()
        {
            var intention = new Intention<IdleTask>(actor => new IdleTask(actor, 1000));
            PlayerState.TaskSource.Intent(intention);
        }
    }
}