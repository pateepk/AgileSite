<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="WTE_UniPager.ascx.cs" Inherits="CMSApp.CMSWebParts.WTE.Custom.WTE_UniPager" %>

<div id="divMain" runat="server" class="pager-control">
    <cms:UniPager ID="pagerElem" runat="server">
        <LayoutTemplate>
            <!-- this get ignores if there is layout -->
            <div class="pager-control_numbers">
                <asp:PlaceHolder runat="server" ID="plcFirstPage"></asp:PlaceHolder>
                <asp:PlaceHolder runat="server" ID="plcPreviousPage"></asp:PlaceHolder>
                <asp:PlaceHolder runat="server" ID="plcPreviousGroup"></asp:PlaceHolder>
                <asp:PlaceHolder runat="server" ID="plcPageNumbers"></asp:PlaceHolder>
                <asp:PlaceHolder runat="server" ID="plcNextGroup"></asp:PlaceHolder>
                <asp:PlaceHolder runat="server" ID="plcNextPage"></asp:PlaceHolder>
                <asp:PlaceHolder runat="server" ID="plcLastPage"></asp:PlaceHolder>
                <asp:PlaceHolder runat="server" ID="plcDirectPage"></asp:PlaceHolder>
            </div>
            <div id="divCurrPage" runat="server" class="pager-control_count">
                Page <span><%# Eval("CurrentPage") %></span> of <span><%# Eval("Pages") %></span>
            </div>
        </LayoutTemplate>
    </cms:UniPager>
    <div id="divPageSize" runat="server" class="pager-control_count">
        <asp:Label ID="lblPageSize" runat="server" EnableViewState="false" AssociatedControlID="drpPageSize" />
        <cms:CMSDropDownList ID="drpPageSize" CssClass="select-css" runat="server" AutoPostBack="true" />
    </div>
</div>