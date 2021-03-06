﻿using System;
using System.Collections.Generic;
using System.Linq;

using Zilon.Core.PersonModules;
using Zilon.Core.Persons;
using Zilon.Core.Tactics;
using Zilon.Core.Tactics.Behaviour;
using Zilon.Core.Tactics.Spatial;

namespace Zilon.Bot.Players.Logics
{
    public sealed class DefeatTargetLogicState : LogicStateBase
    {
        private const int REFRESH_COUNTER_VALUE = 3;

        private IAttackTarget _target;

        private int _refreshCounter;

        private MoveTask _moveTask;

        private readonly ISectorMap _map;
        private readonly ISectorManager _sectorManager;
        private readonly ITacticalActUsageService _actService;

        public DefeatTargetLogicState(ISectorManager sectorManager,
                                      ITacticalActUsageService actService)
        {
            if (sectorManager is null)
            {
                throw new ArgumentNullException(nameof(sectorManager));
            }

            _map = sectorManager.CurrentSector.Map;
            _sectorManager = sectorManager;
            _actService = actService ?? throw new ArgumentNullException(nameof(actService));
        }

        private IAttackTarget GetTarget(IActor actor)
        {
            //TODO Убрать дублирование кода с IntruderDetectedTrigger
            // Этот фрагмент уже однажды был использован неправильно,
            // что привело к трудноуловимой ошибке.
            var intruders = CheckForIntruders(actor);

            var orderedIntruders = intruders.OrderBy(x => _map.DistanceBetween(actor.Node, x.Node));
            var nearbyIntruder = orderedIntruders.FirstOrDefault();

            return nearbyIntruder;
        }

        private IEnumerable<IActor> CheckForIntruders(IActor actor)
        {
            foreach (var target in _sectorManager.CurrentSector.ActorManager.Items)
            {
                if (target.Owner == actor.Owner)
                {
                    continue;
                }

                if (target.Person.CheckIsDead())
                {
                    continue;
                }

                var isVisible = LogicHelper.CheckTargetVisible(_map, actor.Node, target.Node);
                if (!isVisible)
                {
                    continue;
                }

                yield return target;
            }
        }

        private AttackParams CheckAttackAvailability(IActor actor, IAttackTarget target)
        {
            if (actor.Person.GetModuleSafe<ICombatActModule>() is null)
            {
                throw new NotSupportedException();
            }

            var inventory = actor.Person.GetModuleSafe<IInventoryModule>();

            var act = SelectActHelper.SelectBestAct(actor.Person.GetModule<ICombatActModule>().CalcCombatActs(), inventory);

            var isInDistance = act.CheckDistance(actor.Node, target.Node, _map);
            var targetIsOnLine = _map.TargetIsOnLine(actor.Node, target.Node);

            var attackParams = new AttackParams
            {
                IsAvailable = isInDistance && targetIsOnLine,
                TacticalAct = act
            };

            return attackParams;
        }

        public override IActorTask GetTask(IActor actor, ILogicStrategyData strategyData)
        {
            if (_target == null)
            {
                _target = GetTarget(actor);
            }

            var targetCanBeDamaged = _target.CanBeDamaged();
            if (!targetCanBeDamaged)
            {
                Complete = true;
                return null;
            }

            var attackParams = CheckAttackAvailability(actor, _target);
            if (attackParams.IsAvailable)
            {
                var act = attackParams.TacticalAct;
                var attackTask = new AttackTask(actor, _target, act, _actService);
                return attackTask;
            }
            else
            {
                // Маршрут до цели обновляем каждые 3 хода.
                // Для оптимизации.
                // Эффект потери цели.

                if (_refreshCounter > 0 && _moveTask?.CanExecute() == true)
                {
                    _refreshCounter--;
                    return _moveTask;
                }
                else
                {
                    _refreshCounter = REFRESH_COUNTER_VALUE;
                    var targetIsOnLine = _map.TargetIsOnLine(actor.Node, _target.Node);

                    if (targetIsOnLine)
                    {
                        _moveTask = new MoveTask(actor, _target.Node, _map);
                        return _moveTask;
                    }
                    else
                    {
                        // Цел за пределами видимости. Считается потерянной.
                        return null;
                    }
                }
            }
        }

        protected override void ResetData()
        {
            _refreshCounter = 0;
            _target = null;
            _moveTask = null;
        }

        private class AttackParams
        {
            public bool IsAvailable { get; set; }
            public ITacticalAct TacticalAct { get; set; }
        }
    }
}
