<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CUCycleManagement.ascx.cs"
    Inherits="CMSApp.CMSWebParts.CUWebParts.CUCycleManagement" %>
<div>
    <asp:Label ID="lblMessage" runat="server" EnableViewState="false" />
</div>
<br />
<table class="EditingFormTable">
    <thead>
    </thead>
    <tbody>
        <tr>
            <td class="EditingFormButtonCell">
                <asp:Button ID="btnCancel" runat="server" Text="Back to Cycles" CausesValidation="false"
                    OnClick="CancelBtn_Click" class="FormButton btn" />
            </td>
        </tr>
    </tbody>
</table>
