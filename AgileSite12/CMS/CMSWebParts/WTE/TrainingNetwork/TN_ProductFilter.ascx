<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TN_ProductFilter.ascx.cs" Inherits="CMSApp.CMSWebParts.WTE.TrainingNetwork.TN_ProductFilter" %>

<asp:Label ID="lblMsg" runat="server" />

<div class="filter-box">
    <div class="auto-grid">

        <div class="inline-form-group">
            <label>Sort By: </label>
            <asp:DropDownList ID="lstSortBy" runat="server" AutoPostBack="true" />
        </div>

        <div class="inline-form-group">
            <label>Keywords: </label>
            <asp:TextBox ID="txtKeywords" runat="server" OnTextChanged="txtKeywords_OnTextChaged" AutoPostBack="true" />
        </div>

        <div class="inline-form-group">
            <label>Catalog / Category: </label>
            <asp:DropDownList ID="lstCategory" runat="server" AutoPostBack="true" />
        </div>

        <div class="inline-form-group">
            <label>Language</label>
            <asp:DropDownList ID="lstLanguage" runat="server" AutoPostBack="true">
                <asp:ListItem Value="0" Text="All" />
                <asp:ListItem Value="1" Text="English" />
                <asp:ListItem Value="2" Text="Spanish" />
            </asp:DropDownList>
        </div>

        <div class="inline-form-group">
            <label>Recently Added</label>
            <asp:DropDownList ID="lstRecentlyAdded" runat="server" AutoPostBack="true">
                <asp:ListItem Value="0" Text="Any" />
                <asp:ListItem Value="1" Text="30 Days" />
                <asp:ListItem Value="2" Text="90 Days" />
                <asp:ListItem Value="3" Text="this year" />
                <asp:ListItem Value="4" Text="last year" />
            </asp:DropDownList>
        </div>
    </div>
</div>