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
        public string Folder { get; private set; }

        public Query(string name, string folder)
        {
            Name = name;
            Folder = folder;
        }
    }
}
