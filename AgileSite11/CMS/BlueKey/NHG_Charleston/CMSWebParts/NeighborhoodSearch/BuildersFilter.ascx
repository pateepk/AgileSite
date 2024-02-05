<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="BuildersFilter.ascx.cs" Inherits="NHG_C.CMSWebParts_BuildersFilter" %>

<asp:Button ID="btnFilterCustom" CssClass="btnBuilderCustomFilter" runat="server" OnClick="btnFilter_Click" /><br /><br />
<a name="filter"></a>
<asp:Label ID="ddDebugLabel" runat="server"></asp:Label>

<script type="text/javascript" src="/productionFiles/js/table_sorter.aspx"></script>

<script type="text/javascript">
    var j = jQuery.noConflict();

    j(document).ready(function() {

        j(".table_sorter").tablesorter({ widgets: ['zebra'] });
    });
</script>


