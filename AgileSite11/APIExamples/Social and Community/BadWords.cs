using System.Collections;
using System.Linq;

using CMS.Protection;

namespace APIExamples
{
    /// <summary>
    /// Holds bad word API examples.
    /// </summary>
    /// <pageTitle>Bad words</pageTitle>
    internal class BadWordsMain
    {
        /// <heading>Creating a bad word</heading>
        private void CreateBadWord()
        {
            // Creates a new bad word object
            BadWordInfo newWord = new BadWordInfo();

            // Sets the bad word properties
            newWord.WordExpression = "testbadword";
            newWord.WordAction = BadWordActionEnum.ReportAbuse;
            newWord.WordIsGlobal = true;
            newWord.WordIsRegularExpression = false;

            // Saves the bad word to the database
            BadWordInfoProvider.SetBadWordInfo(newWord);
        }


        /// <heading>Updating a bad word</heading>
        private void GetAndUpdateBadWord()
        {
            // Gets the specific bad word
            var words = BadWordInfoProvider.GetBadWords().WhereEquals("WordExpression", "testbadword");
            BadWordInfo word = words.FirstOrDefault();

            if (word != null)
            {
                // Updates the bad word properties
                word.WordAction = BadWordActionEnum.Replace;
                word.WordReplacement = "good";

                // Saves the changes to the database
                BadWordInfoProvider.SetBadWordInfo(word);
            }
        }


        /// <heading>Updating multiple bad words</heading>
        private void GetAndBulkUpdateBadWords()
        {
            // Gets the bad words containing the word 'bad'
            var words = BadWordInfoProvider.GetBadWords().WhereContains("WordExpression", "bad");

            // Loops through individual bad words
            foreach (BadWordInfo word in words)
            {
                // Updates the properties
                word.WordAction = BadWordActionEnum.Replace;
                word.WordReplacement = "good";

                // Saves the updated bad word
                BadWordInfoProvider.SetBadWordInfo(word);
            }
        }        
        

        /// <heading>Checking text for a bad word</heading>
        private void CheckSingleBadWord()
        {
            // Gets the specific bad word
            var words = BadWordInfoProvider.GetBadWords().WhereEquals("WordExpression", "testbadword");

            if (words.Count > 0)
            {
                // Gets the object representing the bad word from the collection of results
                BadWordInfo badWord = words.FirstOrDefault();

                // Creates a string to be checked for the presence of the bad word
                string text = "This is a string containing the sample testbadword";

                // Creates a hashtable that will contain found bad words
                Hashtable foundWords = new Hashtable();

                // Modifies the string according to the found bad words and returns the action which should be performed
                BadWordActionEnum action = BadWordInfoProvider.CheckBadWord(badWord, null, null, ref text, foundWords, 0);

                if (foundWords.Count != 0)
                {
                    switch (action)
                    {
                        case BadWordActionEnum.Deny:
                            // Perform additional actions here
                            break;

                        case BadWordActionEnum.RequestModeration:
                            // Perform additional actions here
                            break;

                        case BadWordActionEnum.Remove:
                            // Perform additional actions here
                            break;

                        case BadWordActionEnum.Replace:
                            // Perform additional actions here
                            break;

                        case BadWordActionEnum.ReportAbuse:
                            // Perform additional actions here
                            break;

                        case BadWordActionEnum.None:
                            // Perform additional actions here
                            break;
                    }
                }
                else
                {
                    // The bad word is not present in the checked string
                }
            }
        }


        /// <heading>Checking text for all bad words defined in the system</heading>
        private void CheckAllBadWords()
        {
            // Creates a string to be checked for the presence of bad words
            string text = "This is a string containing the sample testbadword.";

            // Creates a hashtable that will contain found bad words
            Hashtable foundWords = new Hashtable();

            // Modifies the string according to the found bad words and returns the action which should be performed
            BadWordActionEnum action = BadWordInfoProvider.CheckAllBadWords(null, null, ref text, foundWords);

            if (foundWords.Count != 0)
            {
                switch (action)
                {
                    case BadWordActionEnum.Deny:
                        // Perform additional actions here
                        break;

                    case BadWordActionEnum.RequestModeration:
                        // Perform additional actions here
                        break;

                    case BadWordActionEnum.Remove:
                        // Perform additional actions here
                        break;

                    case BadWordActionEnum.Replace:
                        // Perform additional actions here
                        break;

                    case BadWordActionEnum.ReportAbuse:
                        // Perform additional actions here
                        break;

                    case BadWordActionEnum.None:
                        // Perform additional actions here
                        break;
                }
            }
            else
            {
                // The bad words are not present in the checked string
            }
        }


        /// <heading>Deleting a bad word</heading>
        private void DeleteBadWord()
        {
            // Gets the specific bad word
            var words = BadWordInfoProvider.GetBadWords().WhereEquals("WordExpression", "testbadword");
            BadWordInfo word = words.FirstOrDefault();

            if (word != null)
            {
                // Deletes the bad word
                BadWordInfoProvider.DeleteBadWordInfo(word);
            }
        }
    }
}
