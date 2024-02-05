<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CUNoticeAddEdit.ascx.cs"
    Inherits="CMSApp.CMSWebParts.CUWebParts.CUNoticeAddEdit" %>

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
                Table Name
            </td>
            <td class="EditingFormValueCell">
                <asp:TextBox ID="txtTableName" runat="server" TextMode="SingleLine"></asp:TextBox>
            </td>
        </tr>
        <tr>
            <td class="EditingFormLabelCell">
                HTML
            </td>
            <td class="EditingFormValueCell">
                <telerik:RadEditor ID="reHTML" runat="server" ToolsFile="~/CMSWebParts/CUWebParts/RadEditorBasicToolsFile.xml">
                    <ImageManager ViewPaths="~/Uploads" UploadPaths="~/Uploads" DeletePaths="~/Uploads"
                        EnableAsyncUpload="true"></ImageManager>
                </telerik:RadEditor>
            </td>
        </tr>
        <tr>
            <td class="EditingFormLabelCell">
                HTML print
            </td>
            <td class="EditingFormValueCell">
                <telerik:RadEditor ID="reHTMLPrint" runat="server" ToolsFile="~/CMSWebParts/CUWebParts/RadEditorBasicToolsFile.xml">
                    <ImageManager ViewPaths="~/Uploads" UploadPaths="~/Uploads" DeletePaths="~/Uploads"
                        EnableAsyncUpload="true"></ImageManager>
                </telerik:RadEditor>
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
