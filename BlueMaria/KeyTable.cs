using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueMaria
{
	class KeyTable
	{
		public string Name { get; } = "";
		public Hashtable Table { get; } = new Hashtable();
        public bool HasUnicodes { get; } = false;

        // DRAGAN: this is a quick fix, we should remove hashtables, use dicts instead for all
        public Dictionary<string, string> Dictionary =>
            Table
            .Cast<DictionaryEntry>()
            .ToDictionary(kvp => (string) kvp.Key, kvp => (string) kvp.Value);

        public KeyTable(string nm, Hashtable ht, bool hasUnicodes = false) {
			Name = nm;
			Table = ht;
            HasUnicodes = hasUnicodes;

        }
	}
}
