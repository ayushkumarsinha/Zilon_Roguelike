﻿namespace Zilon.Core.Persons
{
    /// <summary>
    /// Базовая реализация данных по развитию персонажа.
    /// </summary>
    public class EvolutionData : IEvolutionData
    {
        private IPerk[] _activePerks;

        private IPerk[] _archievedPerks;

        public EvolutionData()
        {
            _activePerks = new IPerk[0];
            _archievedPerks = new IPerk[0];
        }

        public IPerk[] ActivePerks => _activePerks;

        public IPerk[] ArchievedPerks => _archievedPerks;
    }
}
