<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="AMPSubscriptionImporter.ascx.cs" Inherits="CMSApp.CMSWebParts.WTEWebParts.CarFax.AMPSubscriptionImporter" %>

<div id="divUploaderMain" runat="server">
<table class="EditingFormTable">
<tr>
    <td class="EditingFormLabelCell"><label>Existing List:</label></td>
<td>
    <div class="formItem" id="divSelectNewsLetterList" runat="server">
	
        <asp:DropDownList ID="ddlSelectNewsLetter" runat="server" OnSelectedIndexChanged="OnSelectNewsLetterIndexChanged"
            AutoPostBack="true" />
    </div>
	</td>
</tr>
<tr>
    <td colspan="2"><hr></td>
</tr>
<tr>
    <td class="EditingFormLabelCell">
        <label><asp:Literal ID="litNameCaption" Text="Enter Newsletter List Name:" runat="server" /></label>
    </td>
    <td>
        <div class="formItem" id="divNewsletterName" runat="server">
            
            <asp:TextBox ID="txtNewsLetterlistName" runat="server" />
            <asp:RequiredFieldValidator ID="rfvNewsLetter" class="error" ControlToValidate="txtNewsLetterlistName"
                runat="server" Display="Dynamic" ErrorMessage="Please enter a Name, or select and existing newsletter list" />
        </div>
	</td>
</tr>
<tr>
    <td colspan="2"><hr></td>
</tr>
<tr>
    <td class="EditingFormLabelCell"><label>File Upload:</label></td>
    <td>
        <div class="formItem" id="divImportUpload" runat="server">
            <table>
                <tr>
                    <td class="file-input">
                        <asp:FileUpload ID="ExcelUpload" runat="server" />
                    </td>
                </tr>
                <tr>
                    <td>
                        <br>
                        <asp:Button ID="btnUpload" runat="server" OnClick="btnUploadClicked" Text="Import" />
            
                        <asp:RequiredFieldValidator ID="rfvFileName" ControlToValidate="ExcelUpload" runat="server" Display="Dynamic" class="error" ErrorMessage="Please select a file" />
                        <asp:RegularExpressionValidator ID="revFileExtension" runat="server" ControlToValidate="ExcelUpload" CssClass="Error" Display="dynamic" ErrorMessage="Only .xls, .xlsx, .csv, or .txt is current supported" ValidationExpression=".*(\.[Xx][Ll][Ss][Xx]|\.[Xx][Ll][Ss]|\.[Cc][Ss][Vv]|\.[Tt][Xx][Tt])" />
                    </td>
                </tr>
            </table>
        </div>
	</td>
</tr>
</table>

<div class="formItem" id="divContinue" runat="server">
    <asp:Literal ID="litMessage" runat="server" />
    <br />
    <asp:Button ID="btnContinue" runat="server" OnClick="btnContinueClicked" CausesValidation="false"
            Text="Next step" />
</div>
</div>
