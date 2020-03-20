﻿using System.Collections.Generic;

namespace Zilon.Core.World
{
    /// <summary>
    /// Город.
    /// Это абстракция над одним или несколькими секторами.
    /// Нужна для того, чтобы показать, что некоторый участок мира принадлежит
    /// определённому государству и имеет наименование.
    /// </summary>
    public class Locality
    {
        /// <summary>
        /// Наименование административного образования.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Государство-владелец города / населенного пункта.
        /// </summary>
        public Realm Owner { get; set; }

        /// <summary>
        /// Строковое представление города для отладки.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{Name} [{Owner}]";
        }

        /// <summary>
        /// Текущее население населенного пункта.
        /// </summary>
        public int Population { get; set; }

        /// <summary>
        /// Узел провинции, в котором находится населенный пункт.
        /// </summary>
        public TerrainCell Cell { get; set; }

        /// <summary>
        /// Специализация населенного пункта.
        /// </summary>
        public Dictionary<BranchType, int> Branches { get; set; }
    }
}
