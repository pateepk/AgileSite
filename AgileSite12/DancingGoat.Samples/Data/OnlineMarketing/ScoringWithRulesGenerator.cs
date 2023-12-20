using System.Linq;

using CMS.ContactManagement;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.Membership;
using CMS.OnlineForms;
using CMS.SiteProvider;

namespace CMS.DancingGoat.Samples
{
    /// <summary>
    /// Contains methods for generating sample scoring objects with rules of various types.
    /// </summary>
    internal class ScoringWithRulesGenerator
    {
        private readonly SiteInfo mSite;


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="site">Site that will contain generated objects</param>
        public ScoringWithRulesGenerator(SiteInfo site)
        {
            mSite = site;
        }


        /// <summary>
        /// Performs scoring objects and rules generation.
        /// </summary>
        public void Generate()
        {
            var score = ScoreInfoProvider.GetScores()
                .WhereEquals("ScoreName", "EngagementAndBusinessFit")
                .WhereFalse("ScoreBelongsToPersona")
                .TopN(1)
                .FirstOrDefault();

            if (score != null)
            {
                return;
            }

            score = new ScoreInfo
            {
                ScoreDisplayName = "Engagement and business fit",
                ScoreName = "EngagementAndBusinessFit",
                ScoreDescription = "Measures the fit and interest of B2B prospects on the site. Fit is measured by demographics and geographics. Interest is measured by behavior on the site that can be tied to B2B activities, such as visiting the 'Partnership' section of the site or providing a phone number.",
                ScoreEnabled = true,
                ScoreStatus = ScoreStatusEnum.RecalculationRequired,
                ScoreEmailAtScore = 20,
                ScoreNotificationEmail = "sales@localhost.local"
            };
            ScoreInfoProvider.SetScoreInfo(score);


            var partnershipDocument = DocumentHelper.GetDocuments()
                                                    .All()
                                                    .Culture("en-US")
                                                    .Path("/Partnership")
                                                    .Columns("NodeID")
                                                    .OnCurrentSite()
                                                    .TopN(1)
                                                    .FirstOrDefault();

            if (partnershipDocument != null)
            {
                var partnershipSectionVisit = GenerateRule(
                    "Visited the Partnership section",
                    5,
                    score.ScoreID,
                    "<condition>\r\n  <activity name=\"pagevisit\">\r\n    <field name=\"ActivityCreated\">\r\n      <settings>\r\n        <seconddatetime>1/1/0001 12:00:00 AM</seconddatetime>\r\n      </settings>\r\n    </field>\r\n    <field name=\"ActivityNodeID\">\r\n      <value>" + partnershipDocument.NodeID + "</value>\r\n    </field>\r\n    <field name=\"ActivityURL\">\r\n      <settings>\r\n        <operator>0</operator>\r\n      </settings>\r\n    </field>\r\n    <field name=\"ActivityTitle\">\r\n      <settings>\r\n        <operator>0</operator>\r\n      </settings>\r\n    </field>\r\n    <field name=\"ActivityComment\">\r\n      <settings>\r\n        <operator>0</operator>\r\n      </settings>\r\n    </field>\r\n    <field name=\"ActivityCampaign\">\r\n      <settings>\r\n        <operator>0</operator>\r\n      </settings>\r\n    </field>\r\n    <field name=\"ActivityURLReferrer\">\r\n      <settings>\r\n        <operator>0</operator>\r\n      </settings>\r\n    </field>\r\n    <field name=\"ActivityABVariantName\">\r\n      <settings>\r\n        <operator>0</operator>\r\n      </settings>\r\n    </field>\r\n    <field name=\"ActivityMVTCombinationName\">\r\n      <settings>\r\n        <operator>0</operator>\r\n      </settings>\r\n    </field>\r\n  </activity>\r\n  <wherecondition>(ActivityType='pagevisit') AND ([ActivityNodeID] = " + partnershipDocument.NodeID + ")</wherecondition>\r\n</condition>",
                    RuleTypeEnum.Activity,
                    "pagevisit",
                    false
                    );
                partnershipSectionVisit.RuleIsRecurring = true;
                partnershipSectionVisit.RuleMaxPoints = 15;
                RuleInfoProvider.SetRuleInfo(partnershipSectionVisit);
            }

            var customerRegistrationForm = BizFormInfoProvider.GetBizFormInfo("BusinessCustomerRegistration", mSite.SiteID);
            if (customerRegistrationForm != null)
            {
                GenerateRule(
                    "Submitted the business registration form",
                    15,
                    score.ScoreID,
                    BuildMacroRuleCondition("{%Rule(\"(Contact.SubmittedForm(\\\"" + customerRegistrationForm.FormName + "\\\", ToInt(0)))\", \"&lt;rules&gt;&lt;r pos=\\\"0\\\" par=\\\"\\\" op=\\\"and\\\" n=\\\"CMSContactHasSubmittedSpecifiedFormInLastXDays\\\" &gt;&lt;p n=\\\"_perfectum\\\"&gt;&lt;t&gt;has&lt;/t&gt;&lt;v&gt;&lt;/v&gt;&lt;r&gt;0&lt;/r&gt;&lt;d&gt;select operation&lt;/d&gt;&lt;vt&gt;text&lt;/vt&gt;&lt;tv&gt;0&lt;/tv&gt;&lt;/p&gt;&lt;p n=\\\"days\\\"&gt;&lt;t&gt;#enter days&lt;/t&gt;&lt;v&gt;0&lt;/v&gt;&lt;r&gt;0&lt;/r&gt;&lt;d&gt;enter days&lt;/d&gt;&lt;vt&gt;text&lt;/vt&gt;&lt;tv&gt;0&lt;/tv&gt;&lt;/p&gt;&lt;p n=\\\"item\\\"&gt;&lt;t&gt;" + customerRegistrationForm.FormName + "&lt;/t&gt;&lt;v&gt;" + customerRegistrationForm.FormName + "&lt;/v&gt;&lt;r&gt;1&lt;/r&gt;&lt;d&gt;select form&lt;/d&gt;&lt;vt&gt;text&lt;/vt&gt;&lt;tv&gt;0&lt;/tv&gt;&lt;/p&gt;&lt;/r&gt;&lt;/rules&gt;\")%}"),
                    RuleTypeEnum.Macro,
                    belongsToPersona: false
                    );
            }

            GenerateRule(
                "Provided phone number",
                10,
                score.ScoreID,
                "<condition>\r\n  <attribute name=\"ContactBusinessPhone\">\r\n    <params>\r\n      <ContactBusinessPhoneOperator>9</ContactBusinessPhoneOperator>\r\n    </params>\r\n  </attribute>\r\n  <wherecondition>([ContactBusinessPhone] &lt;&gt; N'' AND [ContactBusinessPhone] IS NOT NULL)</wherecondition>\r\n</condition>",
                RuleTypeEnum.Attribute,
                "ContactBusinessPhone"
                );

            RecalculateScores();
        }


        private string BuildMacroRuleCondition(string macroCondition)
        {
            return "<condition>\r\n  <macro>\r\n    <value>" + MacroSecurityProcessor.AddSecurityParameters(macroCondition, MacroIdentityOption.FromUserInfo(UserInfoProvider.AdministratorUser), null) + "</value>\r\n  </macro>\r\n</condition>";
        }


        private RuleInfo GenerateRule(string displayName, int value, int scoreId, string ruleCondition, RuleTypeEnum ruleType, string ruleParameter = null, bool belongsToPersona = true)
        {
            var rule = new RuleInfo
            {
                RuleScoreID = scoreId,
                RuleDisplayName = displayName,
                RuleName = ValidationHelper.GetCodeName(displayName, 100),
                RuleValue = value,
                RuleType = ruleType,
                RuleParameter = ruleParameter,
                RuleCondition = ruleCondition,
                RuleBelongsToPersona = belongsToPersona
            };
            RuleInfoProvider.SetRuleInfo(rule);

            return rule;
        }


        private void RecalculateScores()
        {
            var scores = ScoreInfoProvider.GetScores()
                                          .WhereEquals("ScoreStatus", ScoreStatusEnum.RecalculationRequired)
                                          .WhereFalse("ScoreBelongsToPersona");
            foreach (var score in scores)
            {
                new ScoreAsyncRecalculator(score).RunAsync();
            }
        }
    }
}
