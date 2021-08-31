using System.Collections.Generic;
using System.Xml;
using System.Linq;

namespace Primes.Updater.Files
{
    public static class XML
    {
        public static bool GetFirstChildOfName(this XmlNode parent, string name, out XmlNode child)
        {
            child = null;

            var children = parent.ChildNodes;

            foreach (XmlNode fChild in children)
            {
                if (fChild.Name == name)
                {
                    child = fChild;
                    return true;
                }
            }

            return false;
        }
    }
}
