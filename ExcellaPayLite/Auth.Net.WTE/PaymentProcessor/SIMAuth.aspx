<%@ Page Language="C#" AutoEventWireup="false" CodeBehind="SIMAuth.aspx.cs" Inherits="PaymentProcessor.SIMAuth" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Credit Union Demo Sample Payment Page</title>
    <link href="/CMSPages/GetCSS.aspx?stylesheetname=CC_CCU" type="text/css" rel="stylesheet" />
    <meta name="author" content="WTE Solutions: G Dickinson" />
    <meta name="robots" content="noindex" />
    <meta name="googlebot" content="noindex" />
    <!--[if lt IE 9]>
  	<script src="/app_themes/global/html5shiv-m.js"></script>
 <![endif] -->
    <script type="text/javascript" src="/app_themes/CC_CCU/assets/js/jquery-1.9.1.min.js"></script>
    <link href="/favicon.ico" type="image/x-icon" rel="shortcut icon" />
    <link href="/favicon.ico" type="image/x-icon" rel="icon" />
    <link href="/CMSPages/GetResource.ashx?stylesheetfile=/App_Themes/ASCFortifiber/assets/css/core.css" type="text/css" rel="stylesheet" />
</head>
<body>

    <div class="content">
        Amount: <span runat="server" id="amountSpan"></span>
        <br />
        Description: <span runat="server" id="descriptionSpan"></span>
        <br />
        <br />
        <!--
By default, this sample code is designed to post to our test server for
developer accounts: https://test.authorize.net/gateway/transact.dll
for real accounts (even in test mode), please make sure that you are
posting to: https://secure.authorize.net/gateway/transact.dll
-->
        <form id="simForm" runat="server" method='post' action='https://test.authorize.net/gateway/transact.dll'>
            <input id="HiddenValue" type="hidden" value="Initial Value" runat="server" />
            <input type='hidden' runat="server" name='x_login' id='x_login' />
            <input type='hidden' runat="server" name='x_amount' id='x_amount' />
            <input type='hidden' runat="server" name='x_description' id='x_description' />
            <input type='hidden' runat="server" name='x_invoice_num' id='x_invoice_num' />
            <input type='hidden' runat="server" name='x_fp_sequence' id='x_fp_sequence' />
            <input type='hidden' runat="server" name='x_fp_timestamp' id='x_fp_timestamp' />
            <input type='hidden' runat="server" name='x_fp_hash' id='x_fp_hash' />
            <input type='hidden' runat="server" name='x_test_request' id='x_test_request' />
            <input type='hidden' name='x_show_form' value='PAYMENT_FORM' />
            <input type='submit' runat="server" id='buttonLabel' />
        </form>
        <br />
        &nbsp;<br />
        <br />
        &nbsp;<br />
        <br />
        &nbsp;<br />
    </div>

    <script type="text/javascript">
        document.forms["simForm"].submit();
    </script>
</body>
</html>