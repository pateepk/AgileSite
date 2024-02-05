<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CUSQLDataSource.ascx.cs"
    Inherits="CMSApp.CMSWebParts.CUWebParts.CUSQLDataSource" %>
<%@ Register Src="CUSQLDataSourceInternal.ascx" TagName="SQLDataSource" TagPrefix="cu" %>
<cu:SQLDataSource ID="srcSQL" runat="server" />
<asp:Label ID="lblMsg" runat="server" Visible="false" />
<asp:Label ID="lblMsg2" runat="server" Visible="false" />