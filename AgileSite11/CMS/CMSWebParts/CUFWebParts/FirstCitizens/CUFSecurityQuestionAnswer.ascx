<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CUFSecurityQuestionAnswer.ascx.cs"
    Inherits="CMSApp.CMSWebParts.CUFWebParts.FirstCitizens.CUFSecurityQuestionAnswerControl" %>

<div id="divChallenge" class="form-horizontal" runat="server">
    <div id="divQuestionAnswer" class="form-group" runat="server">
        <div class="editing-form-value-cell">
            <asp:Label ID="lblSecurityQuestion" runat="server"></asp:Label>
        </div>
        <div class="editing-form-value-cell">
            <asp:TextBox ID="txtSecurityQuestionAnswer" runat="server"></asp:TextBox>
        </div>
        <br />
        <div id="divButton" class="form-group" runat="server">
            <cms:FormSubmitButton ID="btnSubmit" runat="server" Text="Submit" ResourceString=""
                OnClick="btnSubmit_Click" />
        </div>
    </div>
    <asp:Label ID="lblError" runat="server"></asp:Label>
</div>