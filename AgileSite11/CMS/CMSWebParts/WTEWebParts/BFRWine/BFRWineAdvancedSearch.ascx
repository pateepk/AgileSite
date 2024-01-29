<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="BFRWineAdvancedSearch.ascx.cs" Inherits="CMSApp.CMSWebParts.WTEWebParts.BFRWine.BFRWineAdvancedSearch" %>

<asp:Label ID="lblMessage" runat="server" EnableViewState="false"></asp:Label>
<table width="100%">
    <tr>
        <th width="20%">
            Producer
        </th>
        <td>
            <asp:DropDownList ID="ddlProducer" runat="server" />
        </td>
    </tr>
    <tr>
        <th>
            Country
        </th>
        <td>
            <asp:DropDownList ID="ddlCountry" runat="server" />
        </td>
    </tr>
    <tr>
        <th>
            Varietal
        </th>
        <td>
            <asp:DropDownList ID="ddlVarietal" runat="server" />
        </td>
    </tr>
    <tr>
        <th>
            Wine Category
        </th>
        <td>
            <asp:CheckBoxList ID="cblCategory" runat="server" RepeatDirection="Horizontal" RepeatColumns="4"
                RepeatLayout="Table" TextAlign="Right" >
                <asp:ListItem Text="Sparkling" Value="Sparkling" />
                <asp:ListItem Text="Desert" Value="Desert" />
                <asp:ListItem Text="Red Wine" Value="Red Wine" />
                <asp:ListItem Text="White Wine" Value="White Wine" />
                <asp:ListItem Text="Rose Wine" Value="Rose Wine" />
                <asp:ListItem Text="Kosher" Value="Kosher" />
                <asp:ListItem Text="Fortified" Value="Fortified" />
                <asp:ListItem Text="Cocktail Ingredient" Value="Cocktail Ingredient" />
            </asp:CheckBoxList>
        </td>
    </tr>
    <tr>
        <th>
            Vintage
        </th>
        <td>
            <asp:DropDownList ID="ddlVintage" runat="server" />
        </td>
    </tr>
    <tr>
        <th>
            Agricultural Practice
        </th>
        <td>
            <asp:CheckBoxList ID="cblAgriculturalPractice" runat="server" RepeatDirection="Horizontal"
                RepeatColumns="4" RepeatLayout="Table" TextAlign="Right">
                <asp:ListItem Text="Organic" Value="Organic" />
                <asp:ListItem Text="Sustainable" Value="Sustainable" />
                <asp:ListItem Text="Natural" Value="Natural" />
                <asp:ListItem Text="Biodynamic" Value="Biodynamic" />
            </asp:CheckBoxList>
        </td>
    </tr>
    <tr>
        <th>
            SRP
        </th>
        <td>
            <asp:DropDownList ID="ddlSRP" runat="server" AutoPostBack="false">
                <asp:ListItem Text="Any" Value=""></asp:ListItem>
                <asp:ListItem Text="Under $11.00" Value="1"></asp:ListItem>
                <asp:ListItem Text="$11.00 - $15.99" Value="2"></asp:ListItem>
                <asp:ListItem Text="$16.00 - $19.99" Value="3"></asp:ListItem>
                <asp:ListItem Text="$20.00 - $29.99" Value="4"></asp:ListItem>
                <asp:ListItem Text="$30.00 - $49.99" Value="5"></asp:ListItem>
                <asp:ListItem Text="Over $50.00" Value="6"></asp:ListItem>
            </asp:DropDownList>
        </td>
    </tr>
    <tr>
        <th>
            Point Rating
        </th>
        <td>
            <asp:DropDownList ID="ddlPointRating" runat="server" AutoPostBack="false" />
        </td>
    </tr>
    <tr>
        <th>
            Wine Reviewer
        </th>
        <td>
            <asp:DropDownList ID="ddlWineReviewer" runat="server" AutoPostBack="false">
                <asp:ListItem Text="Any" Value=""></asp:ListItem>
                <asp:ListItem Value="Beverage Testing Institut" Text="Beverage Testing Institute" />
                <asp:ListItem Value="Burghound / Allan Meadows" Text="Burghound / Allan Meadows" />
                <asp:ListItem Value="Decanter" Text="Decanter" />
                <asp:ListItem Value="El Mundo Vino" Text="El Mundo Vino" />
                <asp:ListItem Value="Falstaff" Text="Falstaff" />
                <asp:ListItem Value="Gambero Rosso" Text="Gambero Rosso" />
                <asp:ListItem Value="International Wine Cellar / Stephen Tanzer" Text="International Wine Cellar / Stephen Tanzer" />
                <asp:ListItem Value="James Halliday" Text="James Halliday" />
                <asp:ListItem Value="James Suckling" Text="James Suckling" />
                <asp:ListItem Value="Jancis Robinson" Text="Jancis Robinson" />
                <asp:ListItem Value="Jean-Marc Quarin" Text="Jean-Marc Quarin" />
                <asp:ListItem Value="Jeremy Oliver" Text="Jeremy Oliver" />
                <asp:ListItem Value="La Revue du Vin de France" Text="La Revue du Vin de France" />
                <asp:ListItem Value="The Wine Advocate" Text="The Wine Advocate" />
                <asp:ListItem Value="View from the Cellar / John Gilman" Text="View from the Cellar / John Gilman" />
                <asp:ListItem Value="Vinous / Anthoru Galloni" Text="Vinous / Anthoru Galloni" />
                <asp:ListItem Value="Wine & Spirits Magazine" Text="Wine & Spirits Magazine" />
                <asp:ListItem Value="Wine Enthusiast" Text="Wine Enthusiast" />
                <asp:ListItem Value="Wine Spectator" Text="Wine Spectator" />
            </asp:DropDownList>
        </td>
    </tr>
    <tr>
        <td colspan="2">
            <asp:Button ID="search" runat="server" Text="Search"  CssClass="ContentButton" EnableViewState="false" />
            <asp:Button ID="btnClear" runat="server" Text="Clear"  CssClass="ContentButton" EnableViewState="false" CausesValidation="false" OnClick="btnClear_Click" />
        </td>
    </tr>
</table>
