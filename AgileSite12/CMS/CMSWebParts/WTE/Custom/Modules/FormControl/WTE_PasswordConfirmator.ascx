<%@ Control Language="C#" AutoEventWireup="true" Inherits="CMSApp.CMSWebParts.WTE.Custom.Modules.FormControl.WTE_PasswordConfirmator"  Codebehind="WTE_PasswordConfirmator.ascx.cs" %>
<%@ Register Src="WTE_PasswordStrength.ascx" TagName="PasswordStrength" TagPrefix="cms" %>
<div>

    <script type="text/javascript">
        function toggleConfirmPassword(tbid, spanid) {
            //var testinput = $('#testinput1');
            // input is a server control...
            var inputname = '[id$=' + tbid + ']';
            var input = $(inputname);

            var spanname = '[id$=' + spanid + ']';
            var span = $(spanname);

            if (input != null) {
                var inputtype = input.type;
                var inputtype2 = input.attr('type');
                if (inputtype2 == 'password') {
                    input.attr('type', 'text');
                    if (span != null) {
                        span.html('<i class="fa-light fa-eye-slash"></i>');
                    }
                }
                else {
                    input.attr('type', 'password');
                    if (span != null) {
                        span.html('<i class="fa-light fa-eye"></i>');
                    }
                }
            }

            //let val = testinput.val();
            //let inputtype = testinput.type;
            //let inputtype2 = testinput.attr("type");
            //testinput.attr('type', testinput.attr('type') === 'password' ? 'text' : 'password');
        }
    </script>

    <cms:PasswordStrength runat="server" ID="passStrength" AllowEmpty="true" /> 
    <div class="ConfirmationSeparator">
    </div>
    <div class="pw-confirm">
    <cms:LocalizedLabel ID="lblConfirmPassword" runat="server" ResourceString="general.confirmpassword" AssociatedControlID="txtConfirmPassword" EnableViewState="false" Display="false" />
    <cms:CMSTextBox ID="txtConfirmPassword" runat="server" TextMode="Password" MaxLength="100" /><span class="pw-view" id="stoggleconfirmpw1" onclick="toggleConfirmPassword('txtConfirmPassword','stoggleconfirmpw1');return false;" runat="server"><i class="fa-light fa-eye"></i></span>
    </div>
    <br />
    <cms:CMSRequiredFieldValidator ID="rfvConfirmPassword" ValidationGroup="ConfirmRegForm" runat="server"
        ControlToValidate="txtConfirmPassword" Display="Dynamic" />
</div>
