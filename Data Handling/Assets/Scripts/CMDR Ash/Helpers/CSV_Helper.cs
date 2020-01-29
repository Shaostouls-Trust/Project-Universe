using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;

namespace DataHandling.Helpers
{
    class TileMap : CsvHelper.Configuration.ClassMap<TileObjectBase>
    {
        public TileMap()
        {
            Map(m => m.GENREF).Index(0);
            Map(m => m.iRefGuid).Index(1);
            Map(m => m.ObjectPosition).Index(2);
            Map(m => m.ObjectRotation).Index(3);
            Map(m => m.ObjectScale).Index(4);
            Map(m => m.mapPrimary).Index(5);
            Map(m => m.mapSecondary).Index(6);
            Map(m => m.mapTertiary).Index(7);
            Map(m => m.mapQuartiary).Index(8);
            Map(m => m.mapDetailPrimary).Index(9);
            Map(m => m.mapDetailSecondary).Index(10);
            Map(m => m.emissiveColor).Index(11);
            Map(m => m.AnimationFrame).Index(12);
            Map(m => m.decorationString).Index(13);

        }
    }
}