using System;
using System.Collections.ObjectModel;
using System.Linq;

using CMS.ContactManagement;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.Membership;
using CMS.Newsletters;
using CMS.OnlineMarketing;
using CMS.Personas;
using CMS.SiteProvider;

namespace CMS.DancingGoat.Samples
{
    /// <summary>
    /// Contains methods for generating sample personas with rules of various types.
    /// </summary>
    internal class PersonaWithRulesGenerator
    {
        private readonly SiteInfo mSite;
        private readonly Random mRandom = new Random();
        private const string COFFEE_GEEK_PESONA_CONTACT_GROUP_NAME = "IsInPersona_Martina_TheCoffeeGeek";


        /// <summary>
        /// Represents Tony, the Cafe Owner.
        /// </summary>
        public const string PERSONA_CAFE_OWNER = "Tony_The_Cafe_Owner";

        /// <summary>
        /// Represents Martina, the Coffee Geek.
        /// </summary>
        public const string PERSONA_COFEE_GEEK = "Martina_The_Cofee_Geek";


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="site">Site that will contain generated objects</param>
        public PersonaWithRulesGenerator(SiteInfo site)
        {
            mSite = site;
        }


        /// <summary>
        /// Generates two personas with rules.
        /// </summary>
        public void Generate()
        {
            GenerateCoffeeGeekPersona();
            GenerateCafeOwnerPersona();
            AssignPersonaToSKUs(PERSONA_CAFE_OWNER, new[]
            {
                "Hario Vacuum Pot",
                "Macap M4D",
                "Guatemala Finca El Injerto",
                "Nicaragua Dipilto"
            });
            AssignPersonaToSKUs(PERSONA_COFEE_GEEK, new[]
            {
                "Anfim Super Caimano",
                "Espro Press",
                "Hario Buono Kettle",
                "AeroPress"
            });

            RecalculatePersonas();
            GenerateBannerPersonalizationVariantsMacrosAndEnableVariants();
            GeneratePersonaContactHistory();
        }


        private void GenerateCoffeeGeekPersona()
        {
            var coffeeGeekPersona = PersonaInfoProvider.GetPersonaInfoByCodeName(PERSONA_COFEE_GEEK);
            if (coffeeGeekPersona != null)
            {
                return;
            }

            coffeeGeekPersona = new PersonaInfo
            {
                PersonaDisplayName = "Martina, the Coffee Geek",
                PersonaName = PERSONA_COFEE_GEEK,
                PersonaDescription = "Martina is 28, she's an online entrepreneur and a foodie girl who likes to blog about her gastronomic experiences. \r\n\r\nThe preparation of her coffee has to be perfect.  She knows all the technical bits that go into the process. Not to leave things to chance, she also owns a professional espresso machine and a grinder.\r\n\r\nMartina drinks a cappuccino or a filtered coffee in the morning and then an espresso or machiato after each meal.",
                PersonaPointsThreshold = 15,
                PersonaPictureMetafileGUID = new Guid("8A3AF6F7-0914-42E1-9641-B7F2E04AED1B"),
                PersonaEnabled = true
            };
            PersonaInfoProvider.SetPersonaInfo(coffeeGeekPersona);

            SubscribeCoffeeGeekContactGroupToEmailCampaign(coffeeGeekPersona);

            var dancingGoatNewsletter = NewsletterInfoProvider.GetNewsletterInfo("DancingGoatNewsletter", mSite.SiteID);
            if (dancingGoatNewsletter != null)
            {
                GenerateRule(
                    "Is subscribed to the Dancing goat newsletter",
                    10,
                    coffeeGeekPersona.PersonaScoreID,
                    "<condition>\r\n  <activity name=\"newslettersubscription\">\r\n    <field name=\"ActivityItemID\">\r\n      <value>" + dancingGoatNewsletter.NewsletterID + "</value>\r\n    </field>\r\n    <field name=\"ActivityCreated\">\r\n      <settings>\r\n        <seconddatetime>1/1/0001 12:00:00 AM</seconddatetime>\r\n      </settings>\r\n    </field>\r\n    <field name=\"ActivityURL\">\r\n      <settings>\r\n        <operator>0</operator>\r\n      </settings>\r\n    </field>\r\n    <field name=\"ActivityTitle\">\r\n      <settings>\r\n        <operator>0</operator>\r\n      </settings>\r\n    </field>\r\n    <field name=\"ActivityComment\">\r\n      <settings>\r\n        <operator>0</operator>\r\n      </settings>\r\n    </field>\r\n    <field name=\"ActivityCampaign\">\r\n      <settings>\r\n        <operator>0</operator>\r\n      </settings>\r\n    </field>\r\n    <field name=\"ActivityURLReferrer\">\r\n      <settings>\r\n        <operator>0</operator>\r\n      </settings>\r\n    </field>\r\n  </activity>\r\n  <wherecondition>(ActivityType='newslettersubscription') AND ([ActivityItemID] = " + dancingGoatNewsletter.NewsletterID + ")</wherecondition>\r\n</condition>",
                    RuleTypeEnum.Activity,
                    "newslettersubscription"
                    );
            }

            const string filePath = "/Campaign-assets/Cafe-promotion/America-s-coffee-poster";
            GenerateRule(
                "Downloaded the America's coffee poster file",
                5,
                coffeeGeekPersona.PersonaScoreID,
                BuildMacroRuleCondition("<condition>\r\n  <macro>\r\n    <value>{%Rule(\"(Contact.VisitedPage(\\\"" + filePath + "\\\", ToInt(0)))\", \"&lt;rules&gt;&lt;r pos=\\\"0\\\" par=\\\"\\\" op=\\\"and\\\" n=\\\"CMSContactHasDownloadedSpecifiedFileInLastXDays\\\" &gt;&lt;p n=\\\"_perfectum\\\"&gt;&lt;t&gt;has&lt;/t&gt;&lt;v&gt;&lt;/v&gt;&lt;r&gt;0&lt;/r&gt;&lt;d&gt;select operation&lt;/d&gt;&lt;vt&gt;text&lt;/vt&gt;&lt;tv&gt;0&lt;/tv&gt;&lt;/p&gt;&lt;p n=\\\"days\\\"&gt;&lt;t&gt;#enter days&lt;/t&gt;&lt;v&gt;0&lt;/v&gt;&lt;r&gt;0&lt;/r&gt;&lt;d&gt;enter days&lt;/d&gt;&lt;vt&gt;text&lt;/vt&gt;&lt;tv&gt;0&lt;/tv&gt;&lt;/p&gt;&lt;p n=\\\"item\\\"&gt;&lt;t&gt;" + filePath + "&lt;/t&gt;&lt;v&gt;" + filePath + "&lt;/v&gt;&lt;r&gt;1&lt;/r&gt;&lt;d&gt;select file&lt;/d&gt;&lt;vt&gt;text&lt;/vt&gt;&lt;tv&gt;1&lt;/tv&gt;&lt;/p&gt;&lt;/r&gt;&lt;/rules&gt;\")%}</value>\r\n  </macro>\r\n</condition>"),
                RuleTypeEnum.Macro
                );

            GenerateRule(
                "Spent between $1 and $100",
                10,
                coffeeGeekPersona.PersonaScoreID,
                BuildMacroRuleCondition("{%Rule(\"(Contact.SpentMoney(ToDouble(1), ToDouble(100), ToInt(90)))\", \"&lt;rules&gt;&lt;r pos=\\\"0\\\" par=\\\"\\\" op=\\\"and\\\" n=\\\"CMSContactHasSpentMoneyInTheStoreInTheLastXDays\\\" &gt;&lt;p n=\\\"money1\\\"&gt;&lt;t&gt;1&lt;/t&gt;&lt;v&gt;1&lt;/v&gt;&lt;r&gt;1&lt;/r&gt;&lt;d&gt;enter value&lt;/d&gt;&lt;vt&gt;double&lt;/vt&gt;&lt;tv&gt;1&lt;/tv&gt;&lt;/p&gt;&lt;p n=\\\"days\\\"&gt;&lt;t&gt;90&lt;/t&gt;&lt;v&gt;90&lt;/v&gt;&lt;r&gt;0&lt;/r&gt;&lt;d&gt;enter days&lt;/d&gt;&lt;vt&gt;integer&lt;/vt&gt;&lt;tv&gt;1&lt;/tv&gt;&lt;/p&gt;&lt;p n=\\\"money2\\\"&gt;&lt;t&gt;100&lt;/t&gt;&lt;v&gt;100&lt;/v&gt;&lt;r&gt;1&lt;/r&gt;&lt;d&gt;enter value&lt;/d&gt;&lt;vt&gt;double&lt;/vt&gt;&lt;tv&gt;1&lt;/tv&gt;&lt;/p&gt;&lt;p n=\\\"_perfectum\\\"&gt;&lt;t&gt;has&lt;/t&gt;&lt;v&gt;&lt;/v&gt;&lt;r&gt;0&lt;/r&gt;&lt;d&gt;select operation&lt;/d&gt;&lt;vt&gt;text&lt;/vt&gt;&lt;tv&gt;0&lt;/tv&gt;&lt;/p&gt;&lt;/r&gt;&lt;/rules&gt;\")%}"),
                RuleTypeEnum.Macro
                );
        }


        private static void SubscribeCoffeeGeekContactGroupToEmailCampaign(PersonaInfo persona)
        {
            var issue = IssueInfoProvider.GetIssues()
                                         .WhereIn("IssueNewsletterID", NewsletterInfoProvider.GetNewsletters()
                                                                                             .WhereEquals("NewsletterName", NewslettersDataGenerator.NEWSLETTER_COFFEE_CLUB_MEMBERSHIP)
                                                                                             .Column("NewsletterID"))
                                         .TopN(1).FirstOrDefault();
            if (issue == null)
            {
                return;
            }

            var contactGroup = CreateContactGroup(persona);

            var issueContactGroupInfo = IssueContactGroupInfoProvider.GetIssueContactGroupInfo(issue.IssueID, contactGroup.ContactGroupID);
            if (issueContactGroupInfo != null)
            {
                return;
            }

            IssueContactGroupInfoProvider.SetIssueContactGroupInfo(new IssueContactGroupInfo
            {
                IssueID = issue.IssueID,
                ContactGroupID = contactGroup.ContactGroupID,
            });
        }

        private static ContactGroupInfo CreateContactGroup(PersonaInfo persona)
        {
            var contactGroup = ContactGroupInfoProvider.GetContactGroupInfo(COFFEE_GEEK_PESONA_CONTACT_GROUP_NAME);
            if (contactGroup != null)
            {
                ContactGroupInfoProvider.DeleteContactGroupInfo(contactGroup);
            }

            contactGroup = new ContactGroupInfo();
            contactGroup.ContactGroupDisplayName = "Is in persona 'Martina, the Coffee Geek'";
            contactGroup.ContactGroupName = COFFEE_GEEK_PESONA_CONTACT_GROUP_NAME;
            contactGroup.ContactGroupEnabled = true;

            string rule = string.Format("{{%Rule(\"(Contact.IsInPersona(\\\"{0}\\\"))\", \"<rules><r pos=\\\"0\\\" par=\\\"\\\" op=\\\"and\\\" n=\\\"ContactIsInPersona\\\" ><p n=\\\"_is\\\"><t>is</t><v></v><r>0</r><d>select operation</d><vt>text</vt><tv>0</tv></p><p n=\\\"personaguid\\\"><t>{1}</t><v>{0}</v><r>1</r><d>select persona</d><vt>text</vt><tv>0</tv></p></r></rules>\") %}}", persona.PersonaGUID, persona.PersonaDisplayName);
            string signedRule = MacroSecurityProcessor.AddSecurityParameters(rule, MacroIdentityOption.FromUserInfo(UserInfoProvider.AdministratorUser), null);

            contactGroup.ContactGroupDynamicCondition = signedRule;
            ContactGroupInfoProvider.SetContactGroupInfo(contactGroup);

            return contactGroup;
        }


        private void GenerateCafeOwnerPersona()
        {
            var coffeeOwnerPersona = PersonaInfoProvider.GetPersonaInfoByCodeName(PERSONA_CAFE_OWNER);
            if (coffeeOwnerPersona != null)
            {
                return;
            }

            coffeeOwnerPersona = new PersonaInfo
            {
                PersonaDisplayName = "Tony, the Cafe Owner",
                PersonaName = PERSONA_CAFE_OWNER,
                PersonaDescription = "Tony has been running his own cafe for the last 7 years. He always looks at ways of improving the service he provides.\r\n\r\nHe offers coffee that he sources from several roasteries. In addition to that, he also sells brewing machines, accessories and grinders for home use.",
                PersonaPointsThreshold = 15,
                PersonaPictureMetafileGUID = new Guid("220C65BA-2CED-4347-9615-8CF69EAC20E5"),
                PersonaEnabled = true
            };
            PersonaInfoProvider.SetPersonaInfo(coffeeOwnerPersona);

            var partnershipDocument = DocumentHelper.GetDocuments()
                                                    .All()
                                                    .Culture("en-US")
                                                    .Path("/Partnership")
                                                    .Columns("NodeID")
                                                    .OnCurrentSite()
                                                    .FirstObject;

            if (partnershipDocument != null)
            {
                var partnershipSectionVisit = GenerateRule(
                    "Visited the Partnership section",
                    5,
                    coffeeOwnerPersona.PersonaScoreID,
                    "<condition>\r\n  <activity name=\"pagevisit\">\r\n    <field name=\"ActivityCreated\">\r\n      <settings>\r\n        <seconddatetime>1/1/0001 12:00:00 AM</seconddatetime>\r\n      </settings>\r\n    </field>\r\n    <field name=\"ActivityNodeID\">\r\n      <value>" + partnershipDocument.NodeID + "</value>\r\n    </field>\r\n    <field name=\"ActivityURL\">\r\n      <settings>\r\n        <operator>0</operator>\r\n      </settings>\r\n    </field>\r\n    <field name=\"ActivityTitle\">\r\n      <settings>\r\n        <operator>0</operator>\r\n      </settings>\r\n    </field>\r\n    <field name=\"ActivityComment\">\r\n      <settings>\r\n        <operator>0</operator>\r\n      </settings>\r\n    </field>\r\n    <field name=\"ActivityCampaign\">\r\n      <settings>\r\n        <operator>0</operator>\r\n      </settings>\r\n    </field>\r\n    <field name=\"ActivityURLReferrer\">\r\n      <settings>\r\n        <operator>0</operator>\r\n      </settings>\r\n    </field>\r\n    <field name=\"ActivityABVariantName\">\r\n      <settings>\r\n        <operator>0</operator>\r\n      </settings>\r\n    </field>\r\n    <field name=\"ActivityMVTCombinationName\">\r\n      <settings>\r\n        <operator>0</operator>\r\n      </settings>\r\n    </field>\r\n  </activity>\r\n  <wherecondition>(ActivityType='pagevisit') AND ([ActivityNodeID] = " + partnershipDocument.NodeID + ")</wherecondition>\r\n</condition>",
                    RuleTypeEnum.Activity,
                    "pagevisit"
                    );
                partnershipSectionVisit.RuleIsRecurring = true;
                partnershipSectionVisit.RuleMaxPoints = 15;
                RuleInfoProvider.SetRuleInfo(partnershipSectionVisit);
            }
            
            var customerRegistrationForm = ProviderHelper.GetInfoByName(PredefinedObjectType.BIZFORM, "BusinessCustomerRegistration", mSite.SiteID);
            if (customerRegistrationForm != null)
            {
                var formName = customerRegistrationForm.GetValue("FormName");
                GenerateRule(
                    "Submitted the business registration form",
                    15,
                    coffeeOwnerPersona.PersonaScoreID,
                    BuildMacroRuleCondition("{%Rule(\"(Contact.SubmittedForm(\\\"" + formName + "\\\", ToInt(0)))\", \"&lt;rules&gt;&lt;r pos=\\\"0\\\" par=\\\"\\\" op=\\\"and\\\" n=\\\"CMSContactHasSubmittedSpecifiedFormInLastXDays\\\" &gt;&lt;p n=\\\"_perfectum\\\"&gt;&lt;t&gt;has&lt;/t&gt;&lt;v&gt;&lt;/v&gt;&lt;r&gt;0&lt;/r&gt;&lt;d&gt;select operation&lt;/d&gt;&lt;vt&gt;text&lt;/vt&gt;&lt;tv&gt;0&lt;/tv&gt;&lt;/p&gt;&lt;p n=\\\"days\\\"&gt;&lt;t&gt;#enter days&lt;/t&gt;&lt;v&gt;0&lt;/v&gt;&lt;r&gt;0&lt;/r&gt;&lt;d&gt;enter days&lt;/d&gt;&lt;vt&gt;text&lt;/vt&gt;&lt;tv&gt;0&lt;/tv&gt;&lt;/p&gt;&lt;p n=\\\"item\\\"&gt;&lt;t&gt;" + formName + "&lt;/t&gt;&lt;v&gt;" + formName + "&lt;/v&gt;&lt;r&gt;1&lt;/r&gt;&lt;d&gt;select form&lt;/d&gt;&lt;vt&gt;text&lt;/vt&gt;&lt;tv&gt;0&lt;/tv&gt;&lt;/p&gt;&lt;/r&gt;&lt;/rules&gt;\")%}"),
                    RuleTypeEnum.Macro,
                    belongsToPersona: false
                    );
            }

            GenerateRule(
                "Gmail penalization",
                -10,
                coffeeOwnerPersona.PersonaScoreID,
                "<condition>\r\n  <attribute name=\"ContactEmail\">\r\n    <value>gmail.com</value>\r\n    <params>\r\n      <ContactEmailOperator>6</ContactEmailOperator>\r\n    </params>\r\n  </attribute>\r\n  <wherecondition>[ContactEmail] LIKE N'%gmail.com'</wherecondition>\r\n</condition>",
                RuleTypeEnum.Attribute,
                "ContactEmail"
                );
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


        private static void AssignPersonaToSKUs(string personaName, string[] skuNames)
        {
            var persona = PersonaInfoProvider.GetPersonaInfoByCodeName(personaName);
            if (persona == null)
            {
                return;
            }

            var docs = DocumentHelper.GetDocuments()
                                     .All()
                                     .AllCultures()
                                     .WhereIn("SKUName", skuNames)
                                     .Columns("NodeID")
                                     .OnCurrentSite();

            foreach (var doc in docs)
            {
                PersonaNodeInfoProvider.RemovePersonaFromNode(persona.PersonaID, doc.NodeID);
                PersonaNodeInfoProvider.AddPersonaToNode(persona.PersonaID, doc.NodeID);
            }
        }


        private void RecalculatePersonas()
        {
            // Recalculate persona scores. Contacts are than reassigned to correct personas automatically using persona handlers.
            var personaScores = ScoreInfoProvider.GetScores()
                                                 .WhereEquals("ScoreStatus", ScoreStatusEnum.RecalculationRequired)
                                                 .WhereTrue("ScoreBelongsToPersona");

            foreach (var personaScore in personaScores)
            {
                new ScoreAsyncRecalculator(personaScore).RunAsync();
            }
        }


        /// <summary>
        /// Enables banner variants and adds the macro to the display condition.
        /// Does not overwrite data.
        /// </summary>
        private void GenerateBannerPersonalizationVariantsMacrosAndEnableVariants()
        {
            const string predefinedMacro = "{%// Macro will be added by generator%}";
            var BannerVariantB2B = ContentPersonalizationVariantInfoProvider.GetContentPersonalizationVariant("TheCafeOwnerBannerVariant_dd20f145-3779-4748-9bc8-d383eecfbd15");
            var BannerVariantB2C = ContentPersonalizationVariantInfoProvider.GetContentPersonalizationVariant("TheCoffeeGeekBannerVariant_d862b458-e5a5-432f-a3fe-60f495acad9f");

            // Do not overwrite if not default
            if ((BannerVariantB2B == null) || (BannerVariantB2C == null) ||
                BannerVariantB2B.VariantEnabled || BannerVariantB2C.VariantEnabled ||
                (BannerVariantB2B.VariantDisplayCondition != predefinedMacro) || (BannerVariantB2C.VariantDisplayCondition != predefinedMacro))
            {
                return;
            }

            var personaB2B = PersonaInfoProvider.GetPersonaInfoByCodeName(PERSONA_CAFE_OWNER);
            var personaB2C = PersonaInfoProvider.GetPersonaInfoByCodeName(PERSONA_COFEE_GEEK);

            if ((personaB2B == null) || (personaB2C == null))
            {
                return;
            }

            const string macroFormat = @"{{%Rule(""(Contact.IsInPersona(\""{0}\""))"", ""<rules><r pos=\""0\"" par=\""\"" op=\""and\"" n=\""ContactIsInPersona\"" ><p n=\""_is\""><t>is</t><v></v><r>0</r><d>select operation</d><vt>text</vt><tv>0</tv></p><p n=\""personaguid\""><t>{1}</t><v>{0}</v><r>1</r><d>select persona</d><vt>text</vt><tv>0</tv></p></r></rules>"")%}}";
            var macroIdentityOption = MacroIdentityOption.FromUserInfo(UserInfoProvider.AdministratorUser);
            BannerVariantB2B.VariantDisplayCondition = MacroSecurityProcessor.AddSecurityParameters(string.Format(macroFormat, personaB2B.PersonaGUID, personaB2B.PersonaDisplayName), macroIdentityOption, null);
            BannerVariantB2C.VariantDisplayCondition = MacroSecurityProcessor.AddSecurityParameters(string.Format(macroFormat, personaB2C.PersonaGUID, personaB2C.PersonaDisplayName), macroIdentityOption, null);
            BannerVariantB2B.VariantEnabled = true;
            BannerVariantB2C.VariantEnabled = true;

            ContentPersonalizationVariantInfoProvider.SetContentPersonalizationVariant(BannerVariantB2B);
            ContentPersonalizationVariantInfoProvider.SetContentPersonalizationVariant(BannerVariantB2C);
        }


        private void GeneratePersonaContactHistory()
        {
            // Delete old records
            PersonaContactHistoryInfoProvider.GetPersonaContactHistory().ForEachObject(PersonaContactHistoryInfoProvider.DeletePersonaContactHistoryInfo);

            GeneratePersonaContactHistoryRecords();
        }


        private void GeneratePersonaContactHistoryRecords()
        {
            int contactsWithoutPersona = 2658;
            int contactsInB2BPersona = 426;
            int contactsInB2CPersona = 1037;

            var personaB2B = PersonaInfoProvider.GetPersonaInfoByCodeName(PERSONA_CAFE_OWNER);
            var personaB2C = PersonaInfoProvider.GetPersonaInfoByCodeName(PERSONA_COFEE_GEEK);
            
            DateTime now = DateTime.Now;
            DateTime to = now.AddDays(30);
            DateTime from = now.AddDays(-60);

            var hits = new Collection<PersonaContactHistoryInfo>();

            for (DateTime time = from; time < to; time = time.AddDays(1))
            {
                IncreaseContactsCount(time, now, ref contactsWithoutPersona, ref contactsInB2BPersona, ref contactsInB2CPersona);
                ChangeCountsIfCampaign1IsRunning(time, now, ref contactsInB2CPersona);
                ChangeCountsIfCampaign2IsSendingMails(time, now, ref contactsWithoutPersona, ref contactsInB2CPersona);
                ChangeCountsWhenPersonaRulesChanged(time, now, ref contactsWithoutPersona, ref contactsInB2BPersona, ref contactsInB2CPersona);

                hits.Add(CreatePersonaContactHistoryInfo(contactsWithoutPersona, time, null));
                hits.Add(CreatePersonaContactHistoryInfo(contactsInB2BPersona, time, personaB2B.PersonaID));
                hits.Add(CreatePersonaContactHistoryInfo(contactsInB2CPersona, time, personaB2C.PersonaID));
            }

            PersonaContactHistoryInfoProvider.BulkInsert(hits);
        }


        private PersonaContactHistoryInfo CreatePersonaContactHistoryInfo(int contactsCount, DateTime time, int? personaID)
        {
            return new PersonaContactHistoryInfo
            {
                PersonaContactHistoryContacts = contactsCount,
                PersonaContactHistoryDate = time,
                PersonaContactHistoryPersonaID = personaID
            };
        }


        private void IncreaseContactsCount(DateTime time, DateTime now, ref int contactsWithoutPersona, ref int contactsInB2BPersona, ref int contactsInB2CPersona)
        {
            // Before this date the personas were set differently
            // After the change more contacts are recognized correctly as B2C visitors
            if (time < now.AddDays(-45))
            {
                contactsWithoutPersona += mRandom.Next(80, 120);
                contactsInB2BPersona += mRandom.Next(3, 12);
                contactsInB2CPersona += mRandom.Next(8, 20);
            }
            else
            {
                contactsWithoutPersona += mRandom.Next(50, 80);
                contactsInB2BPersona += mRandom.Next(2, 5);
                contactsInB2CPersona += mRandom.Next(25, 60);
            }
        }


        private void ChangeCountsIfCampaign1IsRunning(DateTime time, DateTime now, ref int contactsInB2CPersona)
        {
            // In this time range the "Cafe sample promotion test" campaign was running
            if (time > now.AddDays(-14) && time < now.AddDays(-8))
            {
                contactsInB2CPersona += mRandom.Next(20, 40);
            }
        }


        private void ChangeCountsIfCampaign2IsSendingMails(DateTime time, DateTime now, ref int contactsWithoutPersona, ref int contactsInB2CPersona)
        {
            // In this time range mails in the "Cafe sample promotion" campaign were being sent
            if (time > now.AddDays(-14) && time < now.AddDays(-11))
            {
                contactsWithoutPersona += mRandom.Next(60, 100);
                contactsInB2CPersona += mRandom.Next(90, 150);
            }
        }


        private void ChangeCountsWhenPersonaRulesChanged(DateTime time, DateTime now, ref int contactsWithoutPersona, ref int contactsInB2BPersona, ref int contactsInB2CPersona)
        {
            if (time.Date > now.Date.AddDays(-45) && time.Date <= now.Date.AddDays(-44))
            {
                contactsWithoutPersona -= 235;
                contactsInB2BPersona -= 127;
                contactsInB2CPersona += 382;
            }
        }
    }
}