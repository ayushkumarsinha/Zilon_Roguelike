﻿using System;

using Zilon.Core.Schemes;
using Zilon.Core.Tactics;

namespace Zilon.Core.StaticObjectModules
{
    public class PropDepositModule : IPropDepositModule
    {
        private readonly IPropContainer _propContainer;
        private readonly IDropTableScheme _dropTableScheme;
        private readonly IDropResolver _dropResolver;
        private readonly string[] _toolTags;

        private readonly int _exhaustingValue;
        private int _exhaustingCounter;

        /// <inheritdoc/>
        public event EventHandler Mined;

        public PropDepositModule(IPropContainer propContainer,
            IDropTableScheme dropTableScheme,
            IDropResolver dropResolver,
            string[] toolTags,
            int exhaustingValue,
            DepositMiningDifficulty depositMiningDifficulty)
        {
            _propContainer = propContainer ?? throw new ArgumentNullException(nameof(propContainer));
            _dropTableScheme = dropTableScheme ?? throw new ArgumentNullException(nameof(dropTableScheme));
            _dropResolver = dropResolver ?? throw new ArgumentNullException(nameof(dropResolver));
            _toolTags = toolTags ?? throw new ArgumentNullException(nameof(toolTags));

            _exhaustingValue = exhaustingValue;
            _exhaustingCounter = exhaustingValue;
            Difficulty = depositMiningDifficulty;
        }

        /// <inheritdoc/>
        public string[] GetToolTags()
        {
            return _toolTags;
        }

        /// <inheritdoc/>
        public bool IsExhausted { get => Stock <= 0; }

        /// <inheritdoc/>
        public bool IsActive { get; set; }

        /// <inheritdoc/>
        public string Key { get => nameof(IPropDepositModule); }
        public DepositMiningDifficulty Difficulty { get; }
        public float Stock { get => (float)_exhaustingCounter / _exhaustingValue; }

        /// <inheritdoc/>
        public void Mine()
        {
            if (_exhaustingCounter <= 0)
            {
                throw new InvalidOperationException("Попытка выполнить добычу в исчерпанных залежах");
            }

            var props = _dropResolver.Resolve(new[] { _dropTableScheme });
            foreach (var prop in props)
            {
                _propContainer.Content.Add(prop);
                _propContainer.IsActive = true;
            }

            _exhaustingCounter--;

            DoMined();
        }

        private void DoMined()
        {
            var eventArgs = new EventArgs();
            Mined?.Invoke(this, eventArgs);
        }
    }
}
