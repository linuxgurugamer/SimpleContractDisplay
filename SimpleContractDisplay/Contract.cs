using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ContractParser;

namespace SimpleContractDisplay
{
    public class Contract
    {
        internal bool selected;
        internal bool active;
        internal bool manual;
        internal string manualContract;
        internal string manualTitle;
        internal contractContainer contractContainer;

        internal Contract(contractContainer cc)
        {
            selected = false;
            active = true;
            manual = false;
            contractContainer = cc;
        }
        internal Contract(string title, string contract)
        {
            selected = false;
            active = true;
            manual = true;
            manualTitle = title;
            manualContract = contract;
        }
    }

}
