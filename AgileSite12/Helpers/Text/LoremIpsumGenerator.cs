using System;
using System.Text;

namespace CMS.Helpers
{
    /// <summary>
    /// Lorem ipsum text generator.
    /// </summary>
    public class LoremIpsumGenerator
    {
        #region "Variables"

        /// <summary>
        /// List of available words.
        /// </summary>
        protected static string[] mWords = null;

        /// <summary>
        /// Random generator.
        /// </summary>
        protected static Random mRandom = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Random generator.
        /// </summary>
        protected static Random Random
        {
            get
            {
                if (mRandom == null)
                {
                    mRandom = new Random();
                }

                return mRandom;
            }
        }


        /// <summary>
        /// List of available words.
        /// </summary>
        public static string[] Words
        {
            get
            {
                if (mWords == null)
                {
                    mWords = new string[]
                                 {
                                     "consetetur", "sadipscing", "elitr", "sed", "diam", "nonumy", "eirmod",
                                     "tempor", "invidunt", "ut", "labore", "et", "dolore", "magna", "aliquyam", "erat", "sed", "diam", "voluptua",
                                     "at", "vero", "eos", "et", "accusam", "et", "justo", "duo", "dolores", "et", "ea", "rebum", "stet", "clita",
                                     "kasd", "gubergren", "no", "sea", "takimata", "sanctus", "est", "lorem", "ipsum", "dolor", "sit", "amet",
                                     "lorem", "ipsum", "dolor", "sit", "amet", "consetetur", "sadipscing", "elitr", "sed", "diam", "nonumy", "eirmod",
                                     "tempor", "invidunt", "ut", "labore", "et", "dolore", "magna", "aliquyam", "erat", "sed", "diam", "voluptua",
                                     "at", "vero", "eos", "et", "accusam", "et", "justo", "duo", "dolores", "et", "ea", "rebum", "stet", "clita",
                                     "kasd", "gubergren", "no", "sea", "takimata", "sanctus", "est", "lorem", "ipsum", "dolor", "sit", "amet",
                                     "lorem", "ipsum", "dolor", "sit", "amet", "consetetur", "sadipscing", "elitr", "sed", "diam", "nonumy", "eirmod",
                                     "tempor", "invidunt", "ut", "labore", "et", "dolore", "magna", "aliquyam", "erat", "sed", "diam", "voluptua",
                                     "at", "vero", "eos", "et", "accusam", "et", "justo", "duo", "dolores", "et", "ea", "rebum", "stet", "clita",
                                     "kasd", "gubergren", "no", "sea", "takimata", "sanctus", "est", "lorem", "ipsum", "dolor", "sit", "amet", "duis",
                                     "autem", "vel", "eum", "iriure", "dolor", "in", "hendrerit", "in", "vulputate", "velit", "esse", "molestie",
                                     "consequat", "vel", "illum", "dolore", "eu", "feugiat", "nulla", "facilisis", "at", "vero", "eros", "et",
                                     "accumsan", "et", "iusto", "odio", "dignissim", "qui", "blandit", "praesent", "luptatum", "zzril", "delenit",
                                     "augue", "duis", "dolore", "te", "feugait", "nulla", "facilisi", "lorem", "ipsum", "dolor", "sit", "amet",
                                     "consectetuer", "adipiscing", "elit", "sed", "diam", "nonummy", "nibh", "euismod", "tincidunt", "ut", "laoreet",
                                     "dolore", "magna", "aliquam", "erat", "volutpat", "ut", "wisi", "enim", "ad", "minim", "veniam", "quis",
                                     "nostrud", "exerci", "tation", "ullamcorper", "suscipit", "lobortis", "nisl", "ut", "aliquip", "ex", "ea",
                                     "commodo", "consequat", "duis", "autem", "vel", "eum", "iriure", "dolor", "in", "hendrerit", "in", "vulputate",
                                     "velit", "esse", "molestie", "consequat", "vel", "illum", "dolore", "eu", "feugiat", "nulla", "facilisis", "at",
                                     "vero", "eros", "et", "accumsan", "et", "iusto", "odio", "dignissim", "qui", "blandit", "praesent", "luptatum",
                                     "zzril", "delenit", "augue", "duis", "dolore", "te", "feugait", "nulla", "facilisi", "nam", "liber", "tempor",
                                     "cum", "soluta", "nobis", "eleifend", "option", "congue", "nihil", "imperdiet", "doming", "id", "quod", "mazim",
                                     "placerat", "facer", "possim", "assum", "lorem", "ipsum", "dolor", "sit", "amet", "consectetuer", "adipiscing",
                                     "elit", "sed", "diam", "nonummy", "nibh", "euismod", "tincidunt", "ut", "laoreet", "dolore", "magna", "aliquam",
                                     "erat", "volutpat", "ut", "wisi", "enim", "ad", "minim", "veniam", "quis", "nostrud", "exerci", "tation",
                                     "ullamcorper", "suscipit", "lobortis", "nisl", "ut", "aliquip", "ex", "ea", "commodo", "consequat", "duis",
                                     "autem", "vel", "eum", "iriure", "dolor", "in", "hendrerit", "in", "vulputate", "velit", "esse", "molestie",
                                     "consequat", "vel", "illum", "dolore", "eu", "feugiat", "nulla", "facilisis", "at", "vero", "eos", "et", "accusam",
                                     "et", "justo", "duo", "dolores", "et", "ea", "rebum", "stet", "clita", "kasd", "gubergren", "no", "sea",
                                     "takimata", "sanctus", "est", "lorem", "ipsum", "dolor", "sit", "amet", "lorem", "ipsum", "dolor", "sit",
                                     "amet", "consetetur", "sadipscing", "elitr", "sed", "diam", "nonumy", "eirmod", "tempor", "invidunt", "ut",
                                     "labore", "et", "dolore", "magna", "aliquyam", "erat", "sed", "diam", "voluptua", "at", "vero", "eos", "et",
                                     "accusam", "et", "justo", "duo", "dolores", "et", "ea", "rebum", "stet", "clita", "kasd", "gubergren", "no",
                                     "sea", "takimata", "sanctus", "est", "lorem", "ipsum", "dolor", "sit", "amet", "lorem", "ipsum", "dolor", "sit",
                                     "amet", "consetetur", "sadipscing", "elitr", "at", "accusam", "aliquyam", "diam", "diam", "dolore", "dolores",
                                     "duo", "eirmod", "eos", "erat", "et", "nonumy", "sed", "tempor", "et", "et", "invidunt", "justo", "labore",
                                     "stet", "clita", "ea", "et", "gubergren", "kasd", "magna", "no", "rebum", "sanctus", "sea", "sed", "takimata",
                                     "ut", "vero", "voluptua", "est", "lorem", "ipsum", "dolor", "sit", "amet", "lorem", "ipsum", "dolor", "sit",
                                     "amet", "consetetur", "sadipscing", "elitr", "sed", "diam", "nonumy", "eirmod", "tempor", "invidunt", "ut",
                                     "labore", "et", "dolore", "magna", "aliquyam", "erat", "consetetur", "sadipscing", "elitr", "sed", "diam",
                                     "nonumy", "eirmod", "tempor", "invidunt", "ut", "labore", "et", "dolore", "magna", "aliquyam", "erat", "sed",
                                     "diam", "voluptua", "at", "vero", "eos", "et", "accusam", "et", "justo", "duo", "dolores", "et", "ea",
                                     "rebum", "stet", "clita", "kasd", "gubergren", "no", "sea", "takimata", "sanctus", "est", "lorem", "ipsum"
                                 };
                }
                return mWords;
            }
            set
            {
                mWords = value;
            }
        }

        #endregion "Properties"


        #region "Methods"

        /// <summary>
        /// Gets the paragraphs.
        /// </summary>
        /// <param name="paragraphs">Number of paragraphs</param>
        /// <param name="wordsPerParagraph">Maximum number of words per paragraph</param>
        /// <param name="charsPerParagraph">Maximum number of characters per paragraph</param>
        /// <param name="maxWords">Maximum number of words</param>
        /// <param name="maxChars">Maximum number of characters</param>
        public static string GetText(int paragraphs, int wordsPerParagraph, int charsPerParagraph, int maxWords, int maxChars)
        {
            StringBuilder result = new StringBuilder();

            result.Append("Lorem ipsum dolor sit amet");

            // Init missing settings
            if (paragraphs <= 0)
            {
                paragraphs = 1;
            }
            if (maxChars <= 0)
            {
                maxChars = (charsPerParagraph > 0 ? charsPerParagraph * paragraphs : 1000);
            }
            if (wordsPerParagraph <= 0)
            {
                wordsPerParagraph = (paragraphs > 1 ? maxWords / paragraphs : int.MaxValue);
            }
            if (maxWords <= 0)
            {
                maxWords = ((wordsPerParagraph > 0) && (wordsPerParagraph != int.MaxValue) ? wordsPerParagraph * paragraphs : maxChars / 5);
            }
            if (charsPerParagraph <= 0)
            {
                charsPerParagraph = (paragraphs > 1 ? maxChars / paragraphs : int.MaxValue);
            }

            int currentChars = 26;
            int currentWords = 5;

            int totalChars = currentChars;
            int totalWords = currentWords;

            // Generate the paragraphs
            for (int i = 0; i < paragraphs; i++)
            {
                if (i > 0)
                {
                    result.Append("\r\n");
                }

                while ((totalChars < maxChars) && (totalWords < maxWords) && (currentChars < charsPerParagraph) && (currentWords < wordsPerParagraph))
                {
                    string word = GetWord();

                    result.Append(" ");
                    result.Append(word);

                    int newChars = word.Length + 1;

                    currentWords++;
                    totalWords++;

                    currentChars += newChars;
                    totalChars += newChars;
                }
            }

            result.Append(".");

            return result.ToString();
        }


        /// <summary>
        /// Gets the lorem ipsum text with specified number of words. Always starts with "Lorem ipsum dolor sit amet".
        /// </summary>
        /// <param name="chars">Number of characters</param>
        public static string GetTextByLength(int chars)
        {
            return GetText(0, 0, 0, 0, chars);
        }


        /// <summary>
        /// Gets the lorem ipsum text with specified number of words. Always starts with "Lorem ipsum dolor sit amet".
        /// </summary>
        /// <param name="words">Number of words</param>
        public static string GetTextByNumberOfWords(int words)
        {
            return GetText(0, 0, 0, words, 0);
        }


        /// <summary>
        /// Gets the random word.
        /// </summary>
        public static string GetWord()
        {
            return Words[Random.Next(Words.Length)];
        }

        #endregion
    }
}