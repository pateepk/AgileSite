<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CUReplicatingUser.ascx.cs" Inherits="CMSApp.CMSWebParts.CUWebParts.CUReplicatingUser" %>
<asp:Label ID="lblMsg" runat="server" EnableViewState="false" />
<asp:Button ID="btnReturnToAdmin" runat="server" Text="Return to Admin" CausesValidation="false"
     OnClick="ReturnToAdmin_Click" class="FormButton btn btn-primary" Visible="false" />