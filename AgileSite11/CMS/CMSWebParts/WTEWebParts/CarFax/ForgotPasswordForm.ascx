<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ForgotPasswordForm.ascx.cs" Inherits="CMSApp.CMSWebParts.WTEWebParts.CarFax.ForgotPasswordForm" %>

<asp:Panel ID="pnlBody" runat="server" CssClass="LogonPageBackground">
    <table class="DialogPosition">
        <tr>
            <td>
                <cms:CMSUpdatePanel runat="server" ID="pnlUpdatePasswordRetrieval" UpdateMode="Conditional">
                    <ContentTemplate>
                        <asp:Panel ID="pnlPasswdRetrieval" runat="server" CssClass="LoginPanelPasswordRetrieval"
                            DefaultButton="btnPasswdRetrieval" Visible="true">
                            <table>
                                <tr>
                                    <td>
                                        <asp:Label ID="lblPasswdRetrieval" runat="server" EnableViewState="false" AssociatedControlID="txtPasswordRetrieval" />
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <cms:CMSTextBox ID="txtPasswordRetrieval" runat="server" />
                                        <cms:CMSButton class="btn btn-primary" ID="btnPasswdRetrieval" runat="server" EnableViewState="false" /><br />
                                        <cms:CMSRequiredFieldValidator ID="rqValue" runat="server" ControlToValidate="txtPasswordRetrieval"
                                            EnableViewState="false" />
                                    </td>
                                </tr>
                            </table>
                            <asp:Label ID="lblResult" runat="server" Visible="false" EnableViewState="false" />
                        </asp:Panel>
                    </ContentTemplate>
                </cms:CMSUpdatePanel>
            </td>
        </tr>
    </table>
</asp:Panel>
