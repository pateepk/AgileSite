using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.UI.WebControls;

using CMS;
using CMS.Base;
using CMS.Base.Web.UI;
using CMS.ContactManagement;
using CMS.Core;
using CMS.DataEngine;
using CMS.FormEngine.Web.UI;
using CMS.Helpers;
using CMS.Membership;
using CMS.Newsletters.Web.UI;
using CMS.PortalEngine;
using CMS.PortalEngine.Internal;
using CMS.SiteProvider;
using CMS.UIControls;

[assembly: RegisterCustomClass("IssueContactGroupsExtender", typeof(IssueContactGroupsExtender))]

namespace CMS.Newsletters.Web.UI
{
    /// <summary>
    /// Extends Unsubscription listing unigrid.
    /// </summary>
    public class IssueContactGroupsExtender : ControlExtender<UniSelector>
    {
        private ObjectTransformationDataProvider mContactsDataProvider;
        private IUILinkProvider mUILinkProvider;
        private IssueInfo mIssue;
        private IContactGroupMemberService mContactGroupMemberService;


        /// <summary>
        /// Initializes extender.
        /// </summary>
        public override void OnInit()
        {
            mIssue = UIContext.Current.EditedObject as IssueInfo;

            mContactsDataProvider = new ObjectTransformationDataProvider();
            mContactsDataProvider.SetDefaultDataHandler(GetCountsDataHandler);

            mUILinkProvider = Service.Resolve<IUILinkProvider>();
            mContactGroupMemberService = Service.Resolve<IContactGroupMemberService>();

            Control.GridName = "~/App_Data/CMSModules/Newsletters/UI/Grids/newsletter_issue/contactgroups.xml";
            Control.OnAdditionalDataBound += Control_OnAdditionalDataBound;
            Control.UniGrid.OnAction += UniGrid_OnAction;
            Control.DynamicColumnName = false;
        }


        private void UniGrid_OnAction(string actionName, object actionArgument)
        {
            switch (actionName)
            {
                case "delete":
                    if (mIssue != null)
                    {
                        int contactGroupId = ValidationHelper.GetInteger(actionArgument, 0);

                        var issueContactGroupInfo = new IssueContactGroupInfo
                        {
                            ContactGroupID = contactGroupId,
                            IssueID = mIssue.IssueID
                        };

                        if (!issueContactGroupInfo.CheckPermissions(PermissionsEnum.Delete, SiteContext.CurrentSiteName, MembershipContext.AuthenticatedUser))
                        {
                            CMSPage.RedirectToAccessDenied(ModuleName.NEWSLETTER, "Delete");
                            return;
                        }

                        IssueContactGroupInfoProvider.DeleteIssueContactGroupInfo(issueContactGroupInfo);

                        string stringValue = ValidationHelper.GetString(Control.Value, "");
                        var items = stringValue.Split(new[] { Control.ValuesSeparator }, StringSplitOptions.RemoveEmptyEntries);

                        Control.Value = string.Join(Control.ValuesSeparator.ToString(), items.Except(new[] { contactGroupId.ToString() }).ToArray());
                        Control.Reload(true);
                    }
                    break;
            }
        }


        private IGeneralIndexable<int, IDataContainer> GetCountsDataHandler(string objectType, IEnumerable<int> contactGroupIds)
        {
            var contactGroupMembers = mContactGroupMemberService.GetCountOfContactsInContactGroup(contactGroupIds);
            var result = new SafeDictionary<int, IDataContainer>();

            foreach (var contactGroupMember in contactGroupMembers)
            {
                var container = new DataContainer();
                container.SetValue("Value", contactGroupMember.MembersCount);
                result[contactGroupMember.ContactGroupID] = container;
            }

            return result;
        }


        private object Control_OnAdditionalDataBound(object sender, string sourceName, object parameter, object value)
        {
            switch (sourceName)
            {
                case "actions":
                    {
                        int contactGroupId = ValidationHelper.GetInteger(parameter, 0);

                        var plcActions = new PlaceHolder();
                        plcActions.Controls.Add(GetEditButton(contactGroupId));
                        Control.UniGrid.ActionsID.Add(contactGroupId.ToString());
                        plcActions.Controls.Add(GetDeleteButton(contactGroupId));

                        return plcActions;
                    }

                case "contacts":
                    {
                        int contactGroupId = ValidationHelper.GetInteger(parameter, 0);

                        return new ObjectTransformation
                        {
                            ObjectType = ContactGroupInfo.OBJECT_TYPE,
                            ObjectID = contactGroupId,
                            DataProvider = mContactsDataProvider,
                            Transformation = "{% Value %}",
                            NoDataTransformation = "0"
                        };
                    }
                case "ItemName":
                {
                    return HTMLHelper.HTMLEncode((string)((DataRowView)parameter)["ContactGroupDisplayName"]);
                }
            }

            return value;
        }


        private CMSGridActionButton GetEditButton(int contactGroupId)
        {
            string editUrl =
                            URLHelper.GetAbsoluteUrl(
                                mUILinkProvider.GetSingleObjectLink(
                                    ModuleName.CONTACTMANAGEMENT,
                                    "EditContactGroup",
                                    new ObjectDetailLinkParameters
                                    {
                                        AllowNavigationToListing = true,
                                        ObjectIdentifier = contactGroupId
                                    }
                                )
                            );

            return new CMSGridActionButton
            {
                ToolTip = ResHelper.GetString("general.edit"),
                IconCssClass = "icon-edit",
                IconStyle = GridIconStyle.Allow,
                ID = "btnEdit",
                OnClientClick = string.Format("window.open({0})", ScriptHelper.GetString(editUrl))
            };
        }


        private CMSGridActionButton GetDeleteButton(int contactGroupId)
        {
            return new CMSGridActionButton
            {
                ToolTip = ResHelper.GetString("general.delete"),
                IconCssClass = "icon-bin",
                IconStyle = GridIconStyle.Critical,
                ID = "btnDelete",
                OnClientClick = string.Format("if (confirm({0})) {{ return {1}.command('delete', {2}); }}", ScriptHelper.GetLocalizedString("general.confirmdelete"), Control.UniGrid.GetJSModule(), contactGroupId)
            };
        }
    }
}