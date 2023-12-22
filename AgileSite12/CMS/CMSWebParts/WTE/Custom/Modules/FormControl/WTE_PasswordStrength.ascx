<%@ Control Language="C#" AutoEventWireup="true"  Codebehind="WTE_PasswordStrength.ascx.cs"
    Inherits="CMSApp.CMSWebParts.WTE.Custom.Modules.FormControl.WTE_PasswordStrength" %>

<script src="https://code.jquery.com/jquery-3.2.1.min.js"></script>
<script defer type="text/javascript" src="https://ajax.googleapis.com/ajax/libs/jqueryui/1.12.1/jquery-ui.min.js"></script>

    <script type="text/javascript">

        function togglePassword1(tbid, spanid) {
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

        function togglePassword() {
            var testinput = $('[id$=txtPassword]');
            let val = testinput.val();
            let inputtype = testinput.type;
            let inputtype2 = testinput.attr("type");
            testinput.attr('type', testinput.attr('type') === 'password' ? 'text' : 'password');
        }

        function togglePassword3() {
            var testinput = $('#<%=txtPassword.ClientID%>');
            let val = testinput.val();
            let inputtype = testinput.type;
            let inputtype2 = testinput.attr("type");
            testinput.attr('type', testinput.attr('type') === 'password' ? 'text' : 'password');
        }

        var tboxtype = 'password';
        function toggleTextBoxMode() {

            var spn = $('#stest');
            if (spn != null) {
                spn.removeClass('fa-eye').addClass('fa-eye-slash');
            }

            var spn2 = $('#stest2');
            if (spn2 != null) {
                spn2.html('<i class="fa-light fa-eye-slash"></i>');
            }

            var txtbox = $("testinput1");
            if (txtbox != null) {
                if (tboxtype == 'password') {
                    txtbox.attr('type', 'text');
                    txtbox.type = 'text';
                    tboxtype = 'text';
                    if (spn2 != null) {
                        spn2.html('<i class="fa-light fa-eye-slash"></i>');
                    }
                }
                else {
                    txtbox.attr('type', 'password');
                    txtbox.type = 'password';
                    tboxtype = 'password';
                    if (spn2 != null) {
                        spn2.html('<i class="fa-light fa-eye"></i>');
                    }
                }
            }
        }

    </script>

<div class="password-strength">
    <cms:CMSTextBox runat="server" ID="txtPassword" TextMode="Password"/><span class="pw-view" id="stogglepassword" onclick="togglePassword1('txtPassword','stogglepassword');return false;" runat="server"><i class="fa-light fa-eye"></i></span>
    <asp:LinkButton ID="lbtnShowPassword" OnClick="lbtnShowPassword_Clicked" runat="server" Text="click me!" Visible="false"><i class="fa-light fa-eye"></i></asp:LinkButton>
    <asp:Label ID="lblRequiredFieldMark" runat="server" Text="" Visible="false" />  
    <div class="password-strength-text">
        <cms:LocalizedLabel runat="server" ID="lblPasswStregth" CssClass="password-strength-hint" ResourceString="Membership.PasswordStrength" />
        <strong runat="server" ID="lblEvaluation" EnableViewState="false" ></strong>
    </div>
    <asp:Panel runat="server" ID="pnlPasswStrengthIndicator" CssClass="passw-strength-indicator">
        <asp:Panel runat="server" ID="pnlPasswIndicator">
            &nbsp;
        </asp:Panel>
    </asp:Panel>
    <cms:CMSRequiredFieldValidator ID="rfvPassword" runat="server" ControlToValidate="txtPassword"
        Display="Dynamic" EnableViewState="false" />
</div>
