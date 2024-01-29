<%@ Page Title="Manage Account" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="InvoiceInIFrame.aspx.cs" Inherits="ExcellaLite.Invoices.InvoiceInIFrame" %>
<%@ Import Namespace="PaymentProcessor.Web.Applications" %>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">

    

    <%if(InvoiceStatusID == 0) { %>
        <%if(InvoiceID > 0) { %>
            <a href="/Invoices/MyInvoices"><< Back To Open Invoices</a>
        <% } %>
    <% } %>
    <%if(InvoiceStatusID == 1) { %>
        <%if(InvoiceID > 0) { %>
            <a href="/Invoices/MyInvoices?InvoiceStatusID=1"><< Back To In Transit Payments</a>
        <% } %>
    <% } %>
    <%if(InvoiceStatusID == 2) { %>
        <%if(InvoiceID > 0) { %>
            <a href="/Invoices/MyInvoices?InvoiceStatusID=2"><< Back To Closed Invoices</a>
        <% } %>
    <a href="/Invoices/MyInvoices?InvoiceStatusID=3">Archived Invoices</a>
    <% } %>

    <iframe scrolling="yes" frameborder="0" width="800" height="1000" src="DownloadInvoicePDF.ashx?IsAttachment=False&InvoiceID=<%=InvoiceID %>"></iframe> <br />   


    <%if(InvoiceStatusID == 0) { %>
        <%if(InvoiceID > 0) { %>
            <a href="/Invoices/MyInvoices"><< Back To Open Invoices</a>
        <% } %>
    <% } %>
    <%if(InvoiceStatusID == 1) { %>
        <%if(InvoiceID > 0) { %>
            <a href="/Invoices/MyInvoices?InvoiceStatusID=1"><< Back To In Transit Payments</a>
        <% } %>
    <% } %>
    <%if(InvoiceStatusID == 2) { %>
        <%if(InvoiceID > 0) { %>
            <a href="/Invoices/MyInvoices?InvoiceStatusID=2"><< Back To Closed Invoices</a>
        <% } %>
    <a href="/Invoices/MyInvoices?InvoiceStatusID=3">Archived Invoices</a>
    <% } %>

</asp:Content>
