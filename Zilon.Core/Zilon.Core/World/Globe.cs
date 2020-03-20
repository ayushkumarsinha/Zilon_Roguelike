﻿using System.Collections.Generic;

using Zilon.Core.CommonServices.Dices;
using Zilon.Core.Persons;
using Zilon.Core.World.NameGeneration;

namespace Zilon.Core.World
{
    /// <summary>
    /// Мир.
    /// </summary>
    public class Globe
    {
        public RandomName agentNameGenerator;

        public CityNameGenerator cityNameGenerator;

        public List<Realm> Realms = new List<Realm>();

        public Terrain Terrain { get; set; }

        public IList<SectorInfo> SectorInfos { get; set; }

        public IList<IPerson> Persons { get; set; }

        /// <summary>
        /// Список основных городов в мире.
        /// </summary>
        public List<Locality> Localities = new List<Locality>();

        /// <summary>
        /// Текущая итерация обработки мира.
        /// Нужна для отслеживания времени существования мира.
        /// </summary>
        public int Iteration { get; set; }

        public Globe()
        {
            SectorInfos = new List<SectorInfo>();
        }

        //public void Save(string path, string realmFileName = null, string branchFileName = null)
        //{
        //    var branchColors = new[] { Color.Red, Color.Blue, Color.Green, Color.Yellow,
        //        Color.Black, Color.Magenta, Color.Maroon, Color.LightGray };
        //    using (var realmBmp = new DirectBitmap(Terrain.Length, Terrain[0].Length))
        //    using (var branchmBmp = new DirectBitmap(Terrain.Length, Terrain[0].Length))
        //    {
        //        for (var x = 0; x < Terrain.Length; x++)
        //        {
        //            for (var y = 0; y < Terrain[0].Length; y++)
        //            {
        //                var cell = Terrain[x][y];
        //                if (LocalitiesCells.TryGetValue(cell, out var locality))
        //                {
        //                    var branch = locality.Branches.Single(b => b.Value > 0);

        //                    var owner = locality.Owner;
        //                    var mainColor = owner.Banner.MainColor;
        //                    var drawingColor = Color.FromArgb(mainColor.R, mainColor.G, mainColor.B);
        //                    realmBmp.SetPixel(x, y, drawingColor);
        //                    branchmBmp.SetPixel(x, y, branchColors[(int)branch.Key]);
        //                }
        //                else
        //                {
        //                    realmBmp.SetPixel(x, y, Color.White);
        //                    branchmBmp.SetPixel(x, y, Color.White);
        //                }
        //            }
        //        }

        //        realmFileName = realmFileName ?? "realms.bmp";
        //        realmBmp.Bitmap.Save(Path.Combine(path, realmFileName), ImageFormat.Bmp);

        //        branchFileName = branchFileName ?? "branches.bmp";
        //        branchmBmp.Bitmap.Save(Path.Combine(path, branchFileName), ImageFormat.Bmp);
        //    }
        //}

        public string GetLocalityName(IDice _dice)
        {
            return cityNameGenerator.Generate();
        }
    }
}
