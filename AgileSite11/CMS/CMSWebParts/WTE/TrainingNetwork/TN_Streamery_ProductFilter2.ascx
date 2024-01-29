<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TN_Streamery_ProductFilter2.ascx.cs" Inherits="CMSApp.CMSWebParts.WTE.TrainingNetwork.TN_Streamery_ProductFilter2" %>

<div class="drop-menu_item">
    <input type="radio" name="dropmenu" id="drop1" value="drop1" />
    <label for="drop1">
        <span class="btn btn-icon">
            <svg height="24" width="24" fill="none" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg">
                <g stroke="#7BCDBC" stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5">
                    <path d="m16.395 14.5h4.211l-4.211 5h4.211" />
                    <path d="m21 9.5-2.5-5-2.5 5" />
                    <path d="m16.419 8.662h4.162" />
                    <path d="m12 19.5h-9" />
                    <path d="m12 14.5h-9" />
                    <path d="m12 9.5h-9" />
                    <path d="m12 4.5h-9" />
                </g></svg>
            <span>Sort</span>
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
            <svg height="24" width="24" fill="none" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg">
                <g stroke="#567FE0" stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5">
                    <path d="m14.5 13.5 5.207-5.207c.188-.188.293-.442.293-.707v-2.586c0-.552-.448-1-1-1h-14c-.552 0-1 .448-1 1v2.586c0 .265.105.52.293.707l5.207 5.207" />
                    <path d="m9.5 13.5v6.249c0 .813.764 1.41 1.553 1.213l2.5-.625c.556-.139.947-.639.947-1.213v-5.624" />
                </g></svg>
            <span>Filter</span>
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


		<fieldset>
        <asp:LinkButton ID="lbtnApplyFilter" runat="server" CssClass="btn btn-icon" OnClick="lbtnApplyFilter_Click">
            <svg height="24" width="24" fill="none" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg">
                <g stroke="#567FE0" stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5">
                    <path d="m14.5 13.5 5.207-5.207c.188-.188.293-.442.293-.707v-2.586c0-.552-.448-1-1-1h-14c-.552 0-1 .448-1 1v2.586c0 .265.105.52.293.707l5.207 5.207" />
                    <path d="m9.5 13.5v6.249c0 .813.764 1.41 1.553 1.213l2.5-.625c.556-.139.947-.639.947-1.213v-5.624" />
                </g>
            </svg>
            <span>Apply Filter</span>
        </asp:LinkButton>
    </fieldset>
    </div>

</div>

<div class="drop-menu_item">
    <asp:LinkButton ID="lbtnReset" runat="server" CssClass="btn btn-icon" OnClick="lbtnReset_Click">
        <svg width="24" height="24" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg">
            <g fill="none">
                <path stroke="#F0717C" stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M18.364 5.636l-12.728 12.728 12.728-12.728Z"></path>
                <path stroke="#F0717C" stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M12 3v0c-4.971 0-9 4.029-9 9v0c0 4.971 4.029 9 9 9v0c4.971 0 9-4.029 9-9v0c0-4.971-4.029-9-9-9Z"></path>
            </g></svg>
        <span>Reset</span>
    </asp:LinkButton>
</div>
