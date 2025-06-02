using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Piktosaur.Models
{
    public class Query
    {
        public string Name { get; private set; }
        public string[] Folders { get; private set; }

        public Query(string name, string[] folders)
        {
            Name = name;
            Folders = folders;
        }
    }
}
