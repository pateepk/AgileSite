<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="bizform_brochure_request_INLINE.ascx.cs" Inherits="NHG_C.CMSWebParts_BizForms_bizform_brochure_request_INLINE_NHGC" %>

<asp:Label ID="lblPhoneNumber" runat="server" Visible="false" />
<asp:Label ID="requestName" runat="server" Visible="false" />

<cms:BizForm ID="viewBiz" runat="server" IsLiveSite="true" OnOnBeforeSave="viewBiz_OnBeforeSave" OnOnAfterSave="viewBiz_OnAfterSave" />
<asp:Literal ID="ltlDebug" runat="server"></asp:Literal>