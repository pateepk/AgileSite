<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ImportOldContent.aspx.cs" Inherits="NHG_C.BlueKey_Temporary_ImportOldContent" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
  <head runat="server">
      <title></title>
  </head>
  <body>
      <form id="form1" runat="server">
          <asp:button ID="btnMigrateDeveloers" Text="Migrate Developers" OnClick="btnMigrateDevelopers_click" runat="server" Enabled="false" />
          <br /><br />
          <asp:button ID="btnMigrateNeighborhoods" Text="Migrate Neighborhoods" OnClick="btnMigrateNeighborhoods_click" runat="server" Enabled="false" />
          <br /><br />
          <asp:button ID="btnMigrateInventory" Text="Migrate Inventory" OnClick="btnMigrateInventory_click" runat="server" Enabled="false" />
          <br /><br />
          <asp:button ID="btnMigratePages" Text="Migrate Pages" OnClick="btnMigratePages_click" runat="server" Enabled="false" />
          <br /><br />
          <asp:button ID="btnMigrateBlogs" Text="Migrate Blogs" OnClick="btnMigrateBlogs_click" runat="server" Enabled="false" />
          <div>
              <asp:Literal ID="dbg" runat="server" />
          </div>
      </form>
  </body>
</html>
