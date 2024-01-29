using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentProcessor.Web.Applications
{

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    [Serializable]
    public class DataParameter
    {
        public string label { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string value { get; set; }
    }

}
