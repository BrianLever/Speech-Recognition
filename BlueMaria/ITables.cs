using System.Collections;
using System.Collections.Generic;

namespace BlueMaria
{
    public interface ITables
    {
        string Cmd_word { get; set; }
        string Sbl_word { get; set; }

        Hashtable Lang_c2k { get; set; }
        bool HasUnicodesInC2K { get; set; }
        Hashtable Lang_cmd { get; set; }
        Hashtable Lang_sbl { get; set; }
        Dictionary<string, string> Lang_fre { get; set; }

        IEnumerable<string> Languages { get; }
        IEnumerable<string> LanguageNames { get; }

        Dictionary<string, string> LanguagesHash { get; }

        string LoadLanguages();

        string checkfiles();
        void ClearTables();
        string SetupTables(string lang);

        /// <summary>
        /// Do all replacements on a text based on the .def table of replacements
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        string Replace(string input);
    }

}