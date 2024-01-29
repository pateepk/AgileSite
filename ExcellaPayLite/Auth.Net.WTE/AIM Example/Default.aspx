<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="AIM_Example._Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head id="Head1" runat="server">
    <title>Sample AIM Implementation</title>
</head>
<body>
    <P> This sample code is designed to generate a post using Authorize.net's
    Advanced Integration Method (AIM) and display the results of this post to
    the screen. </P>
    <P> For details on how this is accomplished, please review the readme file,
    the comments in the sample code, and the Authorize.net AIM API documentation
    found at http://developer.authorize.net </P>
    <HR />
    
    <span runat="server" id="resultSpan"></span>
    
</body>
