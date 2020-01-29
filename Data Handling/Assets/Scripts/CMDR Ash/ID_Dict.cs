using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataHandling
{
    static class ID_Dict
    {
        //Dictionary for our key value pairs. Allows us to use a classtype to get a GENERIC_ID 
        public static Dictionary<Type, Int32> TypeIDs = new Dictionary<Type, Int32>()
        {
            { typeof(TileObjectBase), TileObjectBase.GENERIC_ID }
        };

        //Dictionary for our key value pairs. Allows us to use a classtype to get a GENERIC_ID 
        public static Dictionary<Int32, Type> GenIDs = new Dictionary<Int32, Type>()
        {
            {TileObjectBase.GENERIC_ID, typeof(TileObjectBase)}
        };

        //method to add

        //method to search
    }
}
