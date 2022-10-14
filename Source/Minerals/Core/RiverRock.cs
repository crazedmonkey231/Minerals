using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minerals.Core
{
    public class RiverRock : DynamicMineral
    {

        public override float GrowthRate
        {
            get
            {
                float rate = 1f - submersibleFactor();
                return rate * rate;
            }
        }
    }
}
