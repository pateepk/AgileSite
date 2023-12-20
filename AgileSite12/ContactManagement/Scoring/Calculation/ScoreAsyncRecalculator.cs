using System;
using System.Data;
using System.Security.Principal;
using System.Threading.Tasks;

using CMS.Base;
using CMS.Core;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Recalculates score for ID given in constructor. Recalculation is performed in another thread.
    /// </summary>
    public class ScoreAsyncRecalculator
    {
        private readonly ScoreInfo mScore;
        private WindowsIdentity mWindowsIdentity;


        /// <summary>
        /// Initializes a new instance of the <see cref="ScoreAsyncRecalculator"/> class.
        /// </summary>
        /// <param name="score">Score to be evaluated</param>
        /// <exception cref="ArgumentNullException"><paramref name="score"/> is null</exception>
        public ScoreAsyncRecalculator(ScoreInfo score)
        {
            if (score == null)
            {
                throw new ArgumentNullException(nameof(score));
            }
            mScore = score;
        }


        /// <summary>
        /// Executes asynchronous recalculation of score.
        /// </summary>
        /// <returns>Recalculation task.</returns>
        /// <remarks>Note that the method does not check if the score is already being recalculated. That is, multiple recalculations for the same score may be running at one time. 
        /// However, once all the recalculations are finished, all the contacts will have valid score values. The downside is that multiple notification emails may have been sent.</remarks>
        public Task RunAsync()
        {
            // Store windows identity
            mWindowsIdentity = WindowsIdentity.GetCurrent();

            return Task.Factory.StartNew(CMSThread.Wrap(Run), TaskCreationOptions.LongRunning);
        }


        /// <summary>
        /// Recalculates score for scoreID passed within closure.
        /// </summary>
        private void Run()
        {
            // Impersonate current thread
            WindowsImpersonationContext ctx = mWindowsIdentity.Impersonate();

            DataSet oldScoreValues = ScoreContactRuleInfoProvider.GetContactsWithScore().WhereEquals("ScoreID", mScore.ScoreID);

            var scoreCalculationManager = Service.Resolve<IScoreRecalculator>();

            scoreCalculationManager.RecalculateScoreRulesForAllContacts(mScore);

            ScoreInfoProvider.ProcessTriggers(ScoreInfoProvider.GetScoreInfo(mScore.ScoreID), null, oldScoreValues);

            // Undo impersonation
            ctx.Undo();
        }
    }
}
