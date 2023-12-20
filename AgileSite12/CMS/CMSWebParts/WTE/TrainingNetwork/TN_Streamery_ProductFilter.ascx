<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TN_Streamery_ProductFilter.ascx.cs" Inherits="CMSApp.CMSWebParts.WTE.TrainingNetwork.TN_Streamery_ProductFilter" %>

<div class="drop-menu_item">
    <input type="radio" name="dropmenu" id="drop1" value="drop1" />
    <label for="drop1">
        <span class="btn btn-icon"><span class="icon icon-sort-a-z"></span>
            Sort
        </span>
    </label>

    <div class="drop-menu_cnt">
        <fieldset>
            <h5>Sort By</h5>
            <div class="form-group">
                <asp:RadioButtonList ID="lstSortBy" runat="server" AutoPostBack="true" CssClass="radio-vert" RepeatLayout="Flow" RepeatDirection="Vertical" RepeatColumns="1" />
            </div>
        </fieldset>
    </div>
</div>

<div class="drop-menu_item">
    <input type="radio" name="dropmenu" id="drop2" value="drop2" />
    <label for="drop2">
        <span class="btn btn-icon">
            <span class="icon icon-filter"></span>
            Filter
        </span>
    </label>

    <div class="drop-menu_cnt">
        <fieldset>
            <h5>Search</h5>
            <div class="form-group">
                <asp:TextBox ID="txtKeywords" runat="server" OnTextChanged="txtKeywords_OnTextChaged" AutoPostBack="true" />
            </div>
        </fieldset>

        <fieldset>
            <h5>Catalog</h5>
            <div class="form-group">
                <asp:DropDownList ID="lstCategory" runat="server" AutoPostBack="true" />
            </div>
        </fieldset>

        <fieldset>
            <h5>Language</h5>
            <div class="form-group">
                <asp:RadioButtonList ID="lstLanguage" CssClass="radio-vert" runat="server" AutoPostBack="true" RepeatLayout="Flow" RepeatDirection="Vertical" RepeatColumns="1">
                    <asp:ListItem Value="0" Text="All" />
                    <asp:ListItem Value="1" Text="English" />
                    <asp:ListItem Value="2" Text="Spanish" />
                </asp:RadioButtonList>
            </div>
        </fieldset>

        <fieldset>
            <h5>Recently Added</h5>
            <div class="form-group">

                <asp:RadioButtonList ID="lstRecentlyAdded" CssClass="radio-vert" runat="server" AutoPostBack="true" RepeatLayout="Flow" RepeatDirection="Vertical" RepeatColumns="1">
                    <asp:ListItem Value="0" Text="Any" />
                    <asp:ListItem Value="1" Text="30 Days" />
                    <asp:ListItem Value="2" Text="90 Days" />
                    <asp:ListItem Value="3" Text="this year" />
                    <asp:ListItem Value="4" Text="last year" />
                </asp:RadioButtonList>
            </div>
        </fieldset>

    </div>
</div>