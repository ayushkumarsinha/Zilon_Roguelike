﻿using System.Collections.Generic;

using Zilon.Core.Persons;
using Zilon.Core.Schemes;

namespace Zilon.Core.Tactics
{
    /// <summary>
    /// Реализация менеджера подсчёта очков.
    /// </summary>
    /// <seealso cref="Zilon.Core.Tactics.IScoreManager" />
    public class ScoreManager : IScoreManager
    {
        private const float TURN_INC = 1f;

        private float _turnCounter = 0;

        public ScoreManager()
        {
            Frags = new Dictionary<IMonsterScheme, int>();
            PlaceTypes = new Dictionary<ILocationScheme, int>();
        }

        /// <summary>Базовые очки, набранные игроком.</summary>
        public int BaseScores { get; private set; }

        /// <summary>Фраги по схемам монстров, добытые игроком.</summary>
        public IDictionary<IMonsterScheme, int> Frags { get; }

        public IDictionary<ILocationScheme, int> PlaceTypes { get; }

        /// <summary>Шаги, прожитые персонажем.</summary>
        public int Turns { get; set; }

        /// <summary>Засчитать убийство монстра.</summary>
        /// <param name="monster">Монстр, убитый игроком.</param>
        public void CountMonsterDefeat(MonsterPerson monster)
        {
            var monsterScheme = monster.Scheme;

            BaseScores += monsterScheme.BaseScore;

            if (!Frags.ContainsKey(monsterScheme))
            {
                Frags.Add(monsterScheme, 0);
            }

            Frags[monsterScheme]++;
        }

        /// <summary>Засчитать один прожитый шаг.</summary>
        public void CountTurn(ILocationScheme sectorScheme)
        {
            _turnCounter += TURN_INC;
            if (_turnCounter >= 1)
            {
                BaseScores++;
            }

            Turns++;

            if (!PlaceTypes.ContainsKey(sectorScheme))
            {
                PlaceTypes.Add(sectorScheme, 0);
            }

            PlaceTypes[sectorScheme]++;
        }
    }
}
