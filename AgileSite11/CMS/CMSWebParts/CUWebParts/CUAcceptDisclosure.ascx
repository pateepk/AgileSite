<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CUAcceptDisclosure.ascx.cs"
    Inherits="CMSApp.CMSWebParts.CUWebParts.CUAcceptDisclosure" %>
<script language="javascript" type="text/javascript">
    function AcceptTermsCheckBoxValidation(oSource, args) {
        var cbId = oSource.getAttribute("cbClientID");
        var myCheckBox = document.getElementById(cbId);
        if (!myCheckBox.checked) {
            args.IsValid = false;
        }
        else {
            args.IsValid = true;
        }
    }
</script>
<asp:Label ID="lblMsg" runat="server" EnableViewState="false" />
<asp:CheckBox ID="cbAcceptTerms" name="cbAcceptTerms" runat="server" />&nbsp;<b>I agree to the terms stated
    above</b>
<asp:CustomValidator ID="cvAcceptTerms"  ClientValidationFunction="AcceptTermsCheckBoxValidation"
    runat="server" ErrorMessage="You must accept terms to continue" OnServerValidate="cvAcceptTerms_ServerValidate"></asp:CustomValidator>
<table>
    <tr id="rowEmail" runat="server">
        <td>
            Email:
        </td>
        <td>
            <asp:TextBox ID="txtEmail" runat="server" />
            <asp:RequiredFieldValidator ID="rfvTxtEmail" runat="server" ControlToValidate="txtEmail"
                ErrorMessage="You must enter a valid email address" />
            <asp:RegularExpressionValidator ID="revTxtEmail" runat="server" ControlToValidate="txtEmail"
                ErrorMessage="You must enter a valid email address" ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*" />
        </td>
    </tr>
</table>
<asp:Button ID="btnContinue" runat="server" Text="Continue" class="btn btn-inverse"
    OnClick="Continue_Click" />
