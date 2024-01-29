<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CUNotificationAddEdit.ascx.cs"
    Inherits="CMSApp.CMSWebParts.CUWebParts.CUNotificationAddEdit" %>
<asp:Label ID="lblMessage" runat="server" /><br />
<table class="EditingFormTable">
    <thead>
    </thead>
    <tbody>
        <tr>
            <td class="EditingFormLabelCell">
                Friendly Name
            </td>
            <td class="EditingFormValueCell">
                <asp:TextBox ID="txtFriendlyName" runat="server" TextMode="SingleLine"></asp:TextBox>
            </td>
        </tr>
        <tr>
            <td class="EditingFormLabelCell">
                Subject
            </td>
            <td class="EditingFormValueCell">
                <asp:TextBox ID="txtSubject" runat="server" TextMode="SingleLine" Width="500px"></asp:TextBox>
                <asp:RequiredFieldValidator ID="rfvSubject" runat="server" ControlToValidate="txtSubject"
                    ErrorMessage="This field is required." />
            </td>
        </tr>
        <tr>
            <td class="EditingFormLabelCell">
                From Name
            </td>
            <td class="EditingFormValueCell">
                <asp:TextBox ID="txtFromName" runat="server" TextMode="SingleLine" Enabled="true"></asp:TextBox>
            </td>
        </tr>
        <tr>
            <td class="EditingFormLabelCell">
                From Email
            </td>
            <td class="EditingFormValueCell">
                <asp:TextBox ID="txtFromEmail" runat="server" TextMode="SingleLine" Enabled="true"></asp:TextBox>
                <asp:RequiredFieldValidator ID="rfvFromEmail" runat="server" ControlToValidate="txtFromEmail"
                    ErrorMessage="This field is required." />
            </td>
        </tr>
        <tr>
            <td class="EditingFormLabelCell">
                Reply to Email
            </td>
            <td class="EditingFormValueCell">
                <asp:TextBox ID="txtReplyToEmail" runat="server" TextMode="SingleLine" Enabled="true"></asp:TextBox>
                <asp:RequiredFieldValidator ID="rfvReplyToEmail" runat="server" ControlToValidate="txtReplyToEmail"
                    ErrorMessage="This field is required." />
            </td>
        </tr>
        <tr>
            <td class="EditingFormLabelCell">
                Body
            </td>
            <td class="EditingFormValueCell">
                <telerik:RadEditor ID="reBody" runat="server" ToolsFile="~/CMSWebParts/CUWebParts/RadEditorBasicToolsFile.xml">
                    <ImageManager ViewPaths="~/Uploads" UploadPaths="~/Uploads" DeletePaths="~/Uploads"
                        EnableAsyncUpload="true"></ImageManager>
                </telerik:RadEditor>
            </td>
        </tr>
        <tr>
            <td colspan="2">
                &nbsp;
            </td>
        </tr>
        <tr>
            <td class="EditingFormLabelCell">
                Send Test
            </td>
            <td class="EditingFormValueCell" style="vertical-align: text-top;">
                Email:
                <asp:TextBox ID="txtTestEmail" runat="server" TextMode="SingleLine" Enabled="true"></asp:TextBox>
                Name:
                <asp:TextBox ID="txtTestName" runat="server" TextMode="SingleLine" Enabled="true"
                    value="Test User"></asp:TextBox>
                <asp:Button ID="btnSendTest" runat="server" Text="SendTest" CausesValidation="true"
                    OnClick="SendTestBtn_Click" class="FormButton btn" />
            </td>
        </tr>
        <tr>
            <td class="EditingFormButtonLeftCell">
            </td>
            <td class="EditingFormButtonCell">
                <asp:Button ID="btnSave" runat="server" Text="Add New Notice" CausesValidation="true"
                    OnClick="SaveBtn_Click" class="FormButton btn btn-primary" />
                <asp:Button ID="btnCancel" runat="server" Text="Cancel" CausesValidation="false"
                    OnClick="CancelBtn_Click" class="FormButton btn" />
            </td>
        </tr>
    </tbody>
</table>
<br />
<h3>
    Tokens</h3>
<table border="1" width="100%">
    <tr>
        <th>
            Token
        </th>
        <th>
            Description
        </th>
    </tr>
    <tr>
        <td>
            ##Date##
        </td>
        <td>
            current date
        </td>
    </tr>
    <tr>
        <td>
            ##SubscriberFirstName##
        </td>
        <td>
            The name of the user the email will be sent to
        </td>
    </tr>
    <tr>
        <td>
            ##SubscriberEmail##
        </td>
        <td>
            The email of the user the email will be sent to
        </td>
    </tr>
    <tr>
        <td>
            ##TrackClick##
        </td>
        <td>
            Link Tracking E.G. &lt;a href="##TrackClick##http://www.apple.com"&gt;link&lt;/a&gt;
        </td>
    </tr>
</table>
