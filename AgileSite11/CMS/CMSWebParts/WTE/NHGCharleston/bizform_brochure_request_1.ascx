<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="bizform_brochure_request_1.ascx.cs" Inherits="NHG_C.CMSWebParts_BizForms_bizform_brochure_request_1_NHGC" %>

<p>
    <span class="requestFormPhoneNumber">
        <asp:Label ID="lblPhoneNumber" runat="server"></asp:Label></span><br />
    Thank you for your interest in <strong>
        <asp:Label ID="requestName" runat="server" /></strong>.
</p>

<cms:BizForm ID="viewBiz" runat="server" IsLiveSite="true" OnOnBeforeSave="viewBiz_OnBeforeSave" OnOnAfterSave="viewBiz_OnAfterSave" />
<asp:Literal ID="ltlDebug" runat="server"></asp:Literal>