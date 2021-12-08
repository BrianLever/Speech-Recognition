using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BlueMaria.Utilities
{
    public static class TextExtensions
    {

        private static char[] _punctuation =
            Enumerable.Range(0, 256).Select(x => (char)x).Where(x => char.IsPunctuation(x)).ToArray(); // .Cast<char>() // char.MaxValue
        private static char[] _whiteSpace =
            Enumerable.Range(0, 256).Select(x => (char)x).Where(x => char.IsWhiteSpace(x)).ToArray(); // .Cast<char>() // char.MaxValue
        private static char[] _separator =
            Enumerable.Range(0, 256).Select(x => (char)x).Where(x => char.IsSeparator(x)).ToArray(); // .Cast<char>() // char.MaxValue
        private static char[] _allSeparators = _punctuation.Concat(_whiteSpace).Concat(_separator).Distinct().ToArray();


        //public static char GetShiftedKey(this char c)
        //{
        //    uint code = UnsafeInputMethods.CharToCode(c);
        //    return GetShiftedKey(code);
        //}
        //public static char GetShiftedKey(this uint code)
        //{
        //    ////UnsafeInputMethods.ToAscii((uint)c, System.Windows.Forms.Keys.ShiftKey);
        //    //uint code = UnsafeInputMethods.CharToCode(c);
        //    return UnsafeInputMethods.ToAscii(code, System.Windows.Forms.Keys.LShiftKey);
        //}

        public static string ReverseCase(this string input)
        {
            return new string(
                input.Select(c => char.IsLetter(c) ? (char.IsUpper(c) ?
                char.ToLower(c) : char.ToUpper(c)) : c).ToArray());
        }

        public static bool IsAnySeparator(this char c) =>
            char.IsSeparator(c) || char.IsWhiteSpace(c) || char.IsPunctuation(c);

        // https://stackoverflow.com/questions/521146/c-sharp-split-string-but-keep-split-chars-separators/3143036#3143036
        public static IEnumerable<string> SplitAndKeep(this string s, char[] delims)
        {
            int start = 0, index;

            while ((index = s.IndexOfAny(delims, start)) != -1)
            {
                if (index - start > 0)
                    yield return s.Substring(start, index - start);
                yield return s.Substring(index, 1);
                start = index + 1;
            }

            if (start < s.Length)
            {
                yield return s.Substring(start);
            }
        }

        public static IEnumerable<string> SplitWords(this string s)
        {
            //var punctuation = text.Where(Char.IsPunctuation).Distinct().ToArray();
            var words = s.Split().Select(x => x.Trim(_punctuation));
            return words;
        }

        public static string ReplaceWords(this string input, Dictionary<string, string> dict)
        {
            var text = input;
            var inputWords = text.SplitWords().Select(x => x.ToLower()).Distinct().ToDictionary(x => x);
            foreach (var pair in dict)
            {
                var hasAllWords = pair.Key.SplitWords().All(x => inputWords.ContainsKey(x.ToLower()));
                if (!hasAllWords) continue;

                var parts = text.Split(new[] { pair.Key }, StringSplitOptions.None);
                if (parts.Length <= 1) continue;

                StringBuilder s = new StringBuilder();
                for (int i = 0; i < parts.Length; ++i)
                {
                    s.Append(parts[i]);
                    if (!(i + 1 < parts.Length))
                    {
                        break;
                    }
                    if ((parts[i].Length > 0 && !parts[i].Last().IsAnySeparator()) ||
                        (parts[i + 1].Length > 0 && !parts[i + 1].First().IsAnySeparator()))
                    {
                        s.Append(pair.Key);
                        continue;
                    }
                    s.Append(pair.Value);
                }
                text = s.ToString();
            }
            return text;
        }

        public static IEnumerable<string> SplitWordsAndReplace(this string s, Dictionary<string, string> dict)
        {
            return SplitWordsAndReplace(s, _allSeparators, dict);
        }

        public static IEnumerable<string> SplitWordsAndReplace(this string s, char[] delimiters, Dictionary<string, string> dict)
        {
            int start = 0, index;
            string value;
            while ((index = s.IndexOfAny(delimiters, start)) != -1)
            {
                if (index - start > 0)
                {
                    string word = s.Substring(start, index - start);

                    if (dict.TryGetValue(word.ToLower(), out  value))
                        yield return value;
                    else
                        yield return word;
                }
                yield return s.Substring(index, 1);
                start = index + 1;
            }

            if (start < s.Length)
            {
                yield return s.Substring(start);
            }
        }

        public static IEnumerable<string> ReadLines(this Stream stream) =>
            ReadLines(stream, Encoding.UTF8);

        public static IEnumerable<string> ReadLines(
            this Stream stream, Encoding encoding)
        {
            //using (var stream = streamProvider())
            using (var reader = new StreamReader(stream, encoding))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    yield return line;
                }
            }
        }

    }
}
