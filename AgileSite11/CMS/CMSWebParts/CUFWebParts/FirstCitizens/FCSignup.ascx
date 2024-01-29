<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FCSignup.ascx.cs" Inherits="CMSApp.CMSWebParts.CUFWebParts.FirstCitizens.FCSignup" %>

<asp:Label ID="lblMessage" runat="server"></asp:Label>
<table class="sign-up-form">
    <tr>
        <td>
            <label for="rtbLogin">User Name:</label>
        </td>
        <td>
            <telerik:RadTextBox ID="rtbLogin" runat="server"></telerik:RadTextBox>
            <asp:Image ID="imgUserNameHelp" runat="server" ImageUrl="~/CUFWeb/images/question_blue.png"
                Visible="true" class="formIcon" />
            <telerik:RadToolTip ID="rttuserName" runat="server"
                TargetControlID="imgUserNameHelp"
                RelativeTo="Element"
                Position="MiddleRight"
                RenderInPageRoot="true"
                EnableShadow="true"
                Width="250px"
                HideEvent="LeaveTargetAndToolTip"
                Text="For authentication purposes you will be required to create a User Name. Your User Name will be used to login into your eStatement/eBill record.">
            </telerik:RadToolTip>
            <asp:RequiredFieldValidator ID="rfvLogin" runat="server" ControlToValidate="rtbLogin"
                ErrorMessage="User Name is required." Display="Dynamic"></asp:RequiredFieldValidator>
        </td>
    </tr>
    <tr>
        <td>
            <label for="rtbPassword">Password:</label>
        </td>
        <td>
            <telerik:RadTextBox ID="rtbPassword" runat="server" class="none" TextMode="Password">
            </telerik:RadTextBox>
            <asp:Image ID="imgPasswordHelp" runat="server" ImageUrl="~/CUFWeb/images/question_blue.png"
                Visible="true" class="formIcon" />
            <telerik:RadToolTip ID="rttPassword" runat="server"
                TargetControlID="imgPasswordHelp"
                RelativeTo="Element"
                Position="MiddleRight"
                RenderInPageRoot="true"
                EnableShadow="true"
                Width="250px"
                HideEvent="LeaveTargetAndToolTip"
                Text="For your safety, the following requirements must be met when creating a password:<br /><ul><li>at least 1 alpha character</li><li>at least 1 numerical character</li><li>at least 1 special character</li><li>at least 8 characters</li></ul>">
            </telerik:RadToolTip>
            <asp:RequiredFieldValidator ID="rfvPassowrd" runat="server" ControlToValidate="rtbPassword"
                ErrorMessage="Password is required." Display="Dynamic"></asp:RequiredFieldValidator>
            <asp:RegularExpressionValidator ID="revPassword" ControlToValidate="rtbPassword"
                ValidationExpression="^(?=.*\d)(?=.*[a-zA-Z])(?=.*[!@#$%^&*-]).{8,255}$" runat="server"
                ErrorMessage="For your safety, the following requirements must be met when creating a password:at least 1 alpha character,at least 1 numerical character,at least 1 special character, a min length of 8 and max length of 255 characters."
                Display="Dynamic" />
        </td>
    </tr>
    <tr>
        <td>
            <label for="rtbPassword">Confirm Password:</label>
        </td>
        <td>
            <telerik:RadTextBox ID="rtbConfirmPassword" runat="server" class="none" TextMode="Password">
            </telerik:RadTextBox>
            <asp:RequiredFieldValidator ID="rfvConfirmPassword" runat="server" ControlToValidate="rtbConfirmPassword"
                ErrorMessage="Confirm Password is required." Display="Dynamic"></asp:RequiredFieldValidator>
            <asp:CompareValidator ID="cvPasswords"
                runat="server"
                ControlToCompare="rtbPassword"
                ControlToValidate="rtbConfirmPassword"
                ErrorMessage="The entered passwords do not match."
                Display="Dynamic" />
        </td>
    </tr>
    <tr>
        <td>
            <label for="rtbEmail">Email:</label>
        </td>
        <td>
            <telerik:RadTextBox ID="rtbEmail" runat="server"></telerik:RadTextBox>
            <asp:Image ID="imgEmailHelp" runat="server" ImageUrl="~/CUFWeb/images/question_blue.png"
                Visible="true" class="formIcon" />
            <telerik:RadToolTip ID="rttEmail" runat="server"
                TargetControlID="imgEmailHelp"
                RelativeTo="Element"
                Position="MiddleRight"
                RenderInPageRoot="true"
                EnableShadow="true"
                Width="250px"
                HideEvent="LeaveTargetAndToolTip"
                Text="Since this address is used for important financial communication, please provide email address that is checked frequently.">
            </telerik:RadToolTip>
            <asp:RequiredFieldValidator ID="rfvEmail" runat="server" ControlToValidate="rtbEmail"
                ErrorMessage="Email is required." Display="Dynamic"></asp:RequiredFieldValidator>
        </td>
    </tr>
    <tr>
        <td>
            <label for="rtbTaxID">SSN/Tax ID:</label>
        </td>
        <td>
            <telerik:RadTextBox ID="rtbTaxID" runat="server"></telerik:RadTextBox>
            <asp:Image ID="imgTaxIDHelp" runat="server" ImageUrl="~/CUFWeb/images/question_blue.png"
                Visible="true" class="formIcon" />
            <telerik:RadToolTip ID="rttTaxID" runat="server"
                TargetControlID="imgTaxIDHelp"
                RelativeTo="Element"
                Position="MiddleRight"
                RenderInPageRoot="true"
                EnableShadow="true"
                Width="250px"
                HideEvent="LeaveTargetAndToolTip"
                Text="For authentication purposes please enter your 9 digit SSN/Tax ID (without dashes) associated with any of your First Citizens’ accounts.">
            </telerik:RadToolTip>
            <asp:RequiredFieldValidator ID="rfvTaxID" runat="server" ControlToValidate="rtbTaxID"
                ErrorMessage="SSN / Tax ID is required." Display="Dynamic"></asp:RequiredFieldValidator>
        </td>
    </tr>
    <tr>
        <td>
            <label for="rtbAccountNumber">Account Number:</label>
        </td>
        <td>
            <telerik:RadTextBox ID="rtbAccountNumber" runat="server"></telerik:RadTextBox>
            <asp:Image ID="imgAccountNumberHelp" runat="server" ImageUrl="~/CUFWeb/images/question_blue.png"
                Visible="true" class="formIcon" />
            <telerik:RadToolTip ID="rttAccountNumber" runat="server"
                TargetControlID="imgAccountNumberHelp"
                RelativeTo="Element"
                Position="MiddleRight"
                RenderInPageRoot="true"
                EnableShadow="true"
                Width="250px"
                HideEvent="LeaveTargetAndToolTip"
                Text="For authentication purposes please enter your 10 digit account number (including leading zeros) associated with your SSN/Tax ID.">
            </telerik:RadToolTip>
            <asp:RequiredFieldValidator ID="rfvAccountNumber" runat="server" ControlToValidate="rtbAccountNumber"
                ErrorMessage="Account Number is required." Display="Dynamic"></asp:RequiredFieldValidator>
        </td>
    </tr>
    <tr>
        <td></td>
        <td>
            <telerik:RadButton ID="rbCreateAccount" runat="server" Text="Register" OnClick="rbCreateAccount_Click">
            </telerik:RadButton>
        </td>
    </tr>
</table>