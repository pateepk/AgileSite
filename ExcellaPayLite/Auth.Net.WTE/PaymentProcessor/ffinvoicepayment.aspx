<%@ Page Language="C#" AutoEventWireup="false" CodeBehind="ffinvoicepayment.aspx.cs" Inherits="PaymentProcessor.ffinvoicepayment" %>

<%@ Import Namespace="PaymentProcessor" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Forifiber Payments</title>
    <meta charset="utf" />
    <meta name="author" content="WTE Solutions: G Dickinson" />
    <meta name="robots" content="noindex" />
    <meta name="googlebot" content="noindex" />
    <link href="/favicon.ico" type="image/x-icon" rel="shortcut icon" />
    <link href="/favicon.ico" type="image/x-icon" rel="icon" />
    <link href="/CMSPages/GetResource.ashx?stylesheetfile=/App_Themes/ASCFortifiber/assets/css/core.css" type="text/css" rel="stylesheet" />
</head>
<body>

    <section class="main2">
        <div class="inner">

            <!-- <h1>Fortifiber</h1> -->

            <form id="PaymentForm" runat="server">
                <asp:Panel runat="server" ID="pnlForm">
                    <input type="hidden" runat="server" id="HiddenValue" value="Initial Value" />
                    <input type="hidden" runat="server" name="x_login" id="x_login" />
                    <input type="hidden" runat="server" name="x_description" id="x_description" />
                    <input type="hidden" runat="server" name="x_invoice_num" id="x_invoice_num" />
                    <input type="hidden" runat="server" name="x_fp_sequence" id="x_fp_sequence" />
                    <input type="hidden" runat="server" name="x_fp_timestamp" id="x_fp_timestamp" />
                    <input type="hidden" runat="server" name="x_fp_hash" id="x_fp_hash" />
                    <input type="hidden" runat="server" name="x_cust_id" id="x_cust_id" />
                    <input type="hidden" runat="server" name="x_test_request" id="x_test_request" />
                    <input type="hidden" name="x_show_form" value="pf_receipt" />

                    <h2>Order Information</h2>
                    <table class="OrderDetails">
                        <tr>
                            <td>
                                <label for="x_invoice_num"><span class="hidden"></span>Invoice Number:</label></td>
                            <td>
                                <asp:Label ID="lblInvoiceNumber" runat="server"></asp:Label></td>
                            <td>
                                <label for="x_description"><span class="hidden"></span>Description:</label></td>
                            <td>
                                <asp:Label ID="lblDescription" runat="server"></asp:Label></td>
                        </tr>
                        <tr>
                            <td>
                                <label for="x_amount"><span class="hidden"></span>Total:</label></td>
                            <td colspan="3">
                                $<input type="text" runat="server" name="x_amount" id="x_amount" /></td>
                        </tr>
                    </table>

                    <h2>Billing Information</h2>
                    <div id="errorMessages"style="color:red;font-weight:bold;font-size:14px;"><asp:Literal ID="ltlErrors" runat="server"></asp:Literal></div>
                    <table class="CustomerBillingInfo">
                        <tr>
                            <td>
                                <label for="x_first_name"><span class="Hidden"></span>First Name:</label></td>
                            <td>
                                <input runat="server" type="text" id="x_first_name" name="x_first_name" maxlength="50" /><span class="Comment"></span></td>
                            <td>
                                <label for="x_last_name"><span class="Hidden"></span>Last Name:</label></td>
                            <td>
                                <input runat="server" type="text" id="x_last_name" name="x_last_name" maxlength="50" /><span class="Comment"></span></td>
                        </tr>
                        <tr>
                            <td>
                                <label for="x_company"><span class="Hidden"></span>Company:</label></td>
                            <td>
                                <input runat="server" type="text" id="x_company" name="x_company" maxlength="50" /><span class="Comment"></span></td>
                            <td>
                                <label for="x_email"><span class="Hidden"></span>Email:</label></td>
                            <td>
                                <input runat="server" type="text" id="x_email" name="x_email" maxlength="255" /><span class="Comment"></span></td>
                        </tr>
                        <tr>
                            <td>
                                <label for="x_address"><span class="Hidden"></span>Address:</label></td>
                            <td>
                                <input runat="server" type="text" id="x_address" name="x_address" maxlength="60" /><span class="Comment"></span></td>
                            <td>
                                <label for="x_city"><span class="Hidden"></span>City:</label></td>
                            <td>
                                <input runat="server" type="text" id="x_city" name="x_city" maxlength="40" /><span class="Comment"></span></td>
                        </tr>
                        <tr>
                            <td>
                                <label for="x_state"><span class="Hidden"></span>State/Province:</label></td>
                            <td>
                                <input runat="server" type="text" id="x_state" name="x_state" maxlength="40" /><span class="Comment"></span></td>
                            <td>
                                <label for="x_zip"><span class="Hidden"></span>Zip/Postal Code:</label></td>
                            <td>
                                <input runat="server" type="text" id="x_zip" name="x_zip" maxlength="20" /><span class="Comment"></span></td>
                        </tr>
                        <tr>
                            <td>
                                <label for="x_country"><span class="Hidden"></span>Country:</label></td>
                            <td>
                                <input runat="server" type="text" id="x_country" name="x_country" maxlength="60" value="US" /><span class="Comment"></span></td>
                            <td colspan="2"></td>
                        </tr>
                        <tr>
                            <td>
                                <label for="x_phone"><span class="Hidden"></span>Phone:</label></td>
                            <td>
                                <input runat="server" type="text" id="x_phone" name="x_phone" maxlength="25" /><span class="Comment"></span></td>
                            <td>
                                <label for="x_fax"><span class="Hidden"></span>Fax:</label></td>
                            <td>
                                <input runat="server" type="text" id="x_fax" name="x_fax" maxlength="25" /><span class="Comment"></span></td>
                        </tr>
                    </table>

                    <h2>Payment Information</h2>
                    <table class="PaymentMethod">
                        <tr>
                            <td>Pay by</td>
                            <td>
                                <input runat="server" id="pmCC" type="radio" class="input_radio" name="x_method" value="cc" checked="true" onclick="paymentMethod_onClick()" /></td>
                            <td>
                                <label for="pmCC">Credit/Debit Card</label></td>
                            <td id="tdPayByBankAccount1" runat="server">
                                <input runat="server" id="pmEcheck" type="radio" class="input_radio" name="x_method" value="echeck" onclick="paymentMethod_onClick()" />
                            </td>
                            <td id="tdPayByBankAccount2" runat="server">
                                <label for="pmEcheck">
                                    Bank Account <small>(USA only)</small>
                                </label>
                            </td>
                        </tr>
                    </table>

                    <input type="hidden" name="x_method_available" value="true" />
                    <div id="divCreditCardInformation">
                        <h3>Credit Card Information</h3>
                        <table class="CreditCardInfo">
                            <tr>
                                <td>
                                    <label for="x_card_num"><span class="Hidden">(enter number without spaces or dashes)</span>Card Number:</label></td>
                                <td>
                                    <input runat="server" type="text" id="x_card_num" name="x_card_num" maxlength="16" /><span class="req">*</span><span class="Comment">(enter number without spaces or dashes)</span>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <label for="x_exp_date"><span class="Hidden">(mmyy)</span>Expiration Date:</label></td>
                                <td>
                                    <input runat="server" type="text" id="x_exp_date" name="x_exp_date" maxlength="20" /><span class="req">*</span><span class="Comment">(mmyy)</span></td>
                            </tr>
                            <tr>
                                <td>
                                    <label for="x_card_code"><span class="Hidden"><a id="aCardCodeWhatsThis" href="https://account.authorize.net/help/Miscellaneous/Pop-up_Terms/Virtual_Terminal/Card_Code.htm" target="_blank" onclick="javascript:return PopupLink(this);">What's this?</a></span>Card Code:</label>
                                </td>
                                <td>
                                    <input runat="server" type="text" id="x_card_code" name="x_card_code" maxlength="4" /><span class="req">*</span><span class="Comment"><a id="aCardCodeWhatsThis" href="https://account.authorize.net/help/Miscellaneous/Pop-up_Terms/Virtual_Terminal/Card_Code.htm" target="_blank" onclick="javascript:return PopupLink(this);">What's this?</a></span>
                                </td>
                            </tr>
                        </table>
                    </div>

                    <div id="divBankAccountInformation">
                        <h3>Bank Account Information</h3>
                        <table class="BankAccountInfo">
                            <tr>
                                <td>
                                    <label for="x_bank_name"><span class="Hidden"></span>Bank Name:</label></td>
                                <td>
                                    <input runat="server" type="text" id="x_bank_name" name="x_bank_name" maxlength="20" /><span class="Comment"></span></td>
                            </tr>
                            <tr>
                                <td>
                                    <label for="x_bank_acct_num"><span class="Hidden">(enter number without spaces or dashes)</span>Bank Account Number:</label></td>
                                <td>
                                    <input runat="server" type="text" id="x_bank_acct_num" name="x_bank_acct_num" maxlength="20" /><span class="req">*</span><span class="Comment">(enter number without spaces or dashes)</span>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <label for="x_bank_aba_code"><span class="Hidden"><a id="aAbaWhatsThis" href="https://account.authorize.net/help/Miscellaneous/Pop-up_Terms/ALL/ABA_Routing_Number.htm" target="_blank" onclick="javascript:return PopupLink(this);">What's this?</a></span>ABA Routing Number:</label>
                                </td>
                                <td>
                                    <input runat="server" type="text" id="x_bank_aba_code" name="x_bank_aba_code" maxlength="9" /><span class="req">*</span><span class="Comment"><a id="aAbaWhatsThis" href="https://account.authorize.net/help/Miscellaneous/Pop-up_Terms/ALL/ABA_Routing_Number.htm" target="_blank" onclick="javascript:return PopupLink(this);">What's this?</a></span>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <label for="x_bank_acct_name"><span class="Hidden"></span>Name On Account:</label></td>
                                <td>
                                    <input runat="server" type="text" id="x_bank_acct_name" name="x_bank_acct_name" maxlength="22" /><span class="Comment"></span></td>
                            </tr>
                            <tr>
                                <td>
                                    <label for="x_bank_acct_type"><span class="Hidden"></span>Bank Account Type:</label></td>
                                <td>
                                    <select id="x_bank_acct_type" name="x_bank_acct_type" size="1">
                                        <option value="CK">Personal Checking</option>
                                        <option value="SA">Personal Savings</option>
                                        <option value="BC">Business Checking</option>
                                    </select><span class="Comment"></span>
                                </td>
                            </tr>
                        </table>
                    </div>

                    <table class="tableButtons">
                        <tr>
                            <td>
                                <input runat="server" type="submit" class="btn btn-primary btn-large" id="btnSubmit" value="Submit" onclick="return onSubmit();" />
                            </td>
                            <td style="display: none;">
                                <input runat="server" type="button" id="btnCancelOrder" value="Cancel Order" onclick="return cancelOrder_onClick();" />
                            </td>
                        </tr>
                    </table>

                    <div class="epayFoot">
                        <p style="text-align: center;">
                            <b>Payments will be posted to your account the next business day</b><br />
                            <small>&copy; <%=year %> Fortifiber | All Rights Reserved</small>
                        </p>
                    </div>
                </asp:Panel>

                
                <asp:Panel runat="server" ID="pnlResult">
                    <div id="divPage" class="Page">
                        <div id="divMerchantHeader" class="HeaderFooter1">
                            <h1>Fortifiber eBilling Application</h1>
                        </div>
                        
                        <div id="divThankYou"><h2>Thank you for your payment!</h2></div>

                        <div id="divReceiptMsg">You may print this receipt page for your records. A receipt has also been emailed to you.</div>
                        <h3>Order Information</h3>
                        <table id="tablePaymentDetails1Rcpt" role="presentation" class="OrderDetails">
                            <tbody>
                                <tr>
                                    <td>
                                        <label><span class="Hidden"></span>Merchant:</label></td>
                                    <td>
                                        <asp:Label ID="lblRMerchant" runat="server"></asp:Label></td>
                                </tr>
                            </tbody>
                        </table>
                        
                        <table id="tablePaymentDetails2Rcpt" role="presentation" cellspacing="0">
                            <tbody>
                                <tr>
                                    <td id="tdPaymentDetails2Rcpt1">
                                        <table role="presentation" class="OrderDetails">
                                            <tbody>
                                                <tr>
                                                    <td><label for=""><span class="Hidden"></span>Date/Time:</label></td>
                                                    <td><asp:Label ID="lblRDateTime" runat="server"></asp:Label></td>
                                                </tr>

                                                <tr>
                                                    <td>
                                                        <label for="x_cust_id"><span class="Hidden"></span>Customer ID:</label></td>
                                                    <td>
                                                        <asp:Label ID="lblRCustomerId" runat="server"></asp:Label></td>
                                                </tr>
                                                <tr id="tdPaymentDetails2Rcpt2">
                                                    <td><label for="x_invoice_num"><span class="Hidden"></span>Invoice Number:</label></td>
                                                    <td><asp:Label ID="lblRInvoiceNumber" runat="server"></asp:Label></td>
                                                </tr>
                                            </tbody>
                                        </table>
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                        <hr />
                        <table id="tableBillingShipping" role="presentation">
                            <tbody>
                                <tr>
                                    <td id="tdBillingInformation">
                                        <h3>Billing Information</h3>
                                        <div id="divBillingInformation">
                                            <b><asp:Label ID="lblRFirstName" runat="server"></asp:Label>&nbsp;<asp:Label ID="lblRLastName" runat="server"></asp:Label></b><br />
                                            <asp:Label ID="lblRCompany" runat="server"></asp:Label><br />
                                            <asp:Label ID="lblRAddress" runat="server"></asp:Label><br />
                                            <asp:Label ID="lblRCity" runat="server"></asp:Label>, <asp:Label ID="lblRState" runat="server"></asp:Label> <asp:Label ID="lblRZip" runat="server"></asp:Label> <asp:Label ID="lblRCountry" runat="server"></asp:Label><br />
                                            Email: <asp:Label ID="lblREmail" runat="server"></asp:Label><br />
                                            Phone: <asp:Label ID="lblRPhone" runat="server"></asp:Label><br />
                                        </div>
                                    </td>
                                </tr>
                            </tbody>
                        </table>

                        <hr />
                        <div id="divOrderDetailsBottomR">
                            <table id="tableOrderDetailsBottom" role="presentation" class="OrderDetails cl3">
                                <tbody>
                                    <tr>
                                        <td><label for="x_amount"><span class="Hidden"></span>Total:</label></td>
                                        <td class="DescrColTotal">&nbsp; </td>
                                        <td class="DataColTotal"><asp:Label ID="lblRAmount" runat="server"></asp:Label></td>
                                    </tr>
                                </tbody>
                            </table>
                        </div>
                        

                        
                        <table cellspacing="0" role="presentation">
                            <tbody>
                                <tr>
                                    <td class="PaymentSection1">
                                        <table role="presentation" class="OrderDetails">
                                            <tbody>
                                                <tr>
                                                    <td><label>Payment Method</label></td>
                                                    <td><asp:Label ID="lblRMethod" runat="server"></asp:Label></td>
                                                </tr>
                                                <tr>
                                                    <td>
                                                        <label for=""><span class="Hidden"></span>Date/Time:</label></td>
                                                    <td>
                                                        <asp:Label ID="lblRDateTime2" runat="server"></asp:Label></td>
                                                </tr>
                                                <tr>
                                                    <td>
                                                        <label for=""><span class="Hidden"></span>Transaction ID:</label></td>
                                                    <td>
                                                        <asp:Label ID="lblRTransactionId" runat="server"></asp:Label></td>
                                                </tr>
                                                <tr>
                                                    <td>
                                                        <label for="x_auth_code"><span class="Hidden"></span>Authorization Code:</label></td>
                                                    <td>
                                                        <asp:Label ID="lblRAuthCode" runat="server"></asp:Label></td>
                                                </tr>
                                                <tr>
                                                    <td>
                                                        <label for="x_method"><span class="Hidden"></span>Payment Method:</label></td>
                                                    <td>
                                                        <asp:Label ID="lblRMethod2" runat="server"></asp:Label></td>
                                                </tr>

                                            </tbody>
                                        </table>
                                    </td>
                                    <td class="PaymentSection2">
                                        <table role="presentation" class="OrderDetails">
                                        </table>
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                    </div>

                </asp:Panel>
            </form>
        </div>
    </section>

    <script>
        var g_submitClicked = false;
        function paymentMethod_onClick() {
            var oPmCC = document.getElementById('pmCC');
            var oPmEcheck = document.getElementById('pmEcheck');
            var oPmVme = document.getElementById('pmVme');
            var oDivCC = document.getElementById('divCreditCardInformation');
            var oDivEcheck = document.getElementById('divBankAccountInformation');
            var otdVme = document.getElementById('tdVme');
            var otdSubmit = document.getElementById('tdSubmit');
            var oDivBillInfo = document.getElementById('divBillingInformation');
            var oDivShipInfo = document.getElementById('divShippingInformation');
            if ((null != oDivCC && null != oDivEcheck) ||
                (null != oDivCC && null != otdVme)) {
                if (null != oPmCC && oPmCC.checked) {
                    oDivCC.style.display = '';
                    if (null != oDivEcheck) { oDivEcheck.style.display = 'none'; }
                    if (null != otdVme) { otdVme.style.display = 'none'; }
                    otdSubmit.style.display = '';
                    if (null != oDivShipInfo) { oDivShipInfo.style.display = ''; }
                    if (null != oDivBillInfo) { oDivBillInfo.style.display = ''; }
                }
                if (null != oPmEcheck && oPmEcheck.checked) {
                    if (null != oDivCC) { oDivCC.style.display = 'none'; }
                    oDivEcheck.style.display = '';
                    if (null != otdVme) { otdVme.style.display = 'none'; }
                    otdSubmit.style.display = '';
                    if (null != oDivShipInfo) { oDivShipInfo.style.display = ''; }
                    if (null != oDivBillInfo) { oDivBillInfo.style.display = ''; }
                }
                if (null != oPmVme && oPmVme.checked) {
                    if (null != oDivCC) { oDivCC.style.display = 'none'; }
                    if (null != oDivEcheck) { oDivEcheck.style.display = 'none'; }
                    otdVme.style.display = '';
                    otdSubmit.style.display = 'none';
                    if (null != oDivShipInfo) { oDivShipInfo.style.display = 'none'; }
                    if (null != oDivBillInfo) { oDivBillInfo.style.display = 'none'; }
                }
            }
        }

        function PopupLink(oLink) {
            if (null != oLink) {
                window.open(oLink.href, null, 'height=350, width=450, scrollbars=1, resizable=1');
                return false;
            }
            return true;
        }
        function ClearHiddenCCEcheck() {
            var oDivCC = document.getElementById('divCreditCardInformation');
            var oDivEcheck = document.getElementById('divBankAccountInformation');
            if (null != oDivCC && null != oDivEcheck) {
                var oFld;
                var list = new Array();
                if ('none' == oDivCC.style.display) {
                    list = new Array('x_card_num', 'x_exp_date', 'x_card_code', 'x_auth_code');
                }
                else if ('none' == oDivEcheck.style.display) {
                    list = new Array('x_bank_name', 'x_bank_acct_num', 'x_bank_aba_code', 'x_bank_acct_name', 'x_drivers_license_num', 'x_drivers_license_state', 'x_drivers_license_dob', 'x_customer_tax_id');
                }
                for (i = 0; i < list.length; i++) {
                    oFld = document.getElementById(list[i]);
                    if (null != oFld && 'text' == oFld.type) oFld.value = '';
                }
            }
        }
        function onSubmit() {
            if (g_submitClicked) {
                return false;
            }
            if (validateFields()) {
                var oBtnSubmit = document.getElementById('btnSubmit');
                oBtnSubmit.value = 'Sending...';
                ClearHiddenCCEcheck();
                return true;
            }
            else{
                return false;
            }


           
        }
        function MoreLessText(elemId, bMore) {
            if (bMore) {
                document.getElementById(elemId + 'More').style.display = '';
                document.getElementById(elemId + 'Less').style.display = 'none';
            } else {
                document.getElementById(elemId + 'More').style.display = 'none';
                document.getElementById(elemId + 'Less').style.display = '';
            }
            return false;
        }
        function validateFields() {
            document.getElementById("errorMessages").innerHTML = '';

            var addr = document.getElementById('x_address');
            var city = document.getElementById('x_city');
            var state = document.getElementById('x_state');
            var zip = document.getElementById('x_zip');
            var cardNum = document.getElementById('x_card_num');
            var exp = document.getElementById('x_exp_date');
            var cvv = document.getElementById('x_card_code');
            var amnt = document.getElementById('x_amount');

            if (amnt.value == "") { return focusAndShowRequiredMessage(amnt, 'Amount') }
            if (addr.value == "") { return focusAndShowRequiredMessage(addr, 'Address') }
            if (city.value == "") { return focusAndShowRequiredMessage(city, 'City') }
            if (state.value == "") { return focusAndShowRequiredMessage(state, 'State') }
            if (zip.value == "") { return focusAndShowRequiredMessage(zip, 'Zip') }
            if (cardNum.value == "") { return focusAndShowRequiredMessage(cardNum, 'Card Number') }
            if (exp.value == "") { return focusAndShowRequiredMessage(exp, 'Expiration Date') }
            if (cvv.value == "") { return focusAndShowRequiredMessage(cvv, 'Card Code') }

            /*Minimum amount of $1*/
            if (amnt.value <= 1) {return focusAndShowCustomMessage(amnt, 'The Amount field must be greater than $1.00. Please update the value and try again.')}

            return true;
            
        }
        function focusAndShowRequiredMessage(focusOnField, friendlyFieldName) {
            var msg = 'The ' + friendlyFieldName + ' field is required. Please enter a vlue and try again.';
            focusOnField.focus();

            document.getElementById("errorMessages").innerHTML = msg;
            return false;
        }
        function focusAndShowCustomMessage(focusOnField, msg) {
            focusOnField.focus();

            document.getElementById("errorMessages").innerHTML = msg;
            return false;
        }


    </script>
    <script>
        paymentMethod_onClick();
        //document.forms["simForm"].submit();
    </script>
</body>
</html>
