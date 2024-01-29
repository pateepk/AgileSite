<%@ Page Language="C#" AutoEventWireup="false" CodeBehind="braintree_recure.aspx.cs" Inherits="PaymentProcessor.braintree_recure" %>

<%@ Import Namespace="PaymentProcessor" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>WTE - WAF Payment</title>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <meta name="author" content="WTE Solutions" />
    <meta name="robots" content="noindex" />
    <meta name="googlebot" content="noindex" />
    <link href="/favicon.ico" type="image/x-icon" rel="shortcut icon" />
    <link href="/favicon.ico" type="image/x-icon" rel="icon" />
    <link href="http://www.myrunner.com/app_themes/myrunner/assets/css/core.css" type="text/css" rel="stylesheet" /> 
    <link href="http://www.myrunner.com/ExcellaPay/js/dist/css/formValidation.min.css" type="text/css" rel="stylesheet" /> 
    
</head>
<body>

<section class="excellapay">
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

        <h3>Order Information</h3>
        <div class="OrderDetails">
            <div class="form-group">
                <label for="x_invoice_num">Invoice Number</label>
                <asp:Label ID="lblInvoiceNumber" class="" runat="server"></asp:Label>
            </div>
            
            <div class="form-group">
                    <label for="x_description">Description</label>
                    <asp:Label ID="lblDescription" class="" runat="server"></asp:Label>
            </div>
            
            <div class="form-group">
                <label for="x_amount">Total</label>
                <div class="input-prepend">
                    <div class="add-on">$</div>
                    <input type="text" class="form-control input-medium" runat="server" name="x_amount" id="x_amount" />
                </div>
            </div>
        </div>

        <h3>Billing Information</h3>
        <div id="errorMessages" class="ErrorLabel">
            <asp:Literal ID="ltlErrors" runat="server"></asp:Literal>
        </div>
        
        <div class="CustomerBillingInfo">
            <div class="form-group">
                <label for="x_first_name">First Name</label>
                <input runat="server" type="text" id="x_first_name" name="x_first_name" class="form-control" maxlength="50" />
            </div>
            
            <div class="form-group">
                <label for="x_last_name">Last Name</label>
                <input runat="server" type="text" id="x_last_name" name="x_last_name" class="form-control" maxlength="50" />
            </div>
            
            <div class="form-group">
                <label for="x_company">Company</label>
                <input runat="server" type="text" id="x_company" name="x_company" maxlength="50" class="form-control" placeholder="optional" />
            </div>
            
            <div class="form-group">
                <label for="x_email">Email</label>
                <input runat="server" type="email" id="x_email" name="x_email" maxlength="255" class="form-control" />      
            </div>
            
            <div class="form-group">
                <label for="x_address">Address</label>
                <asp:TextBox id="x_address" TextMode="multiline" Columns="10" Rows="2" runat="server" class="form-control" />
                <small>Include full address, including apt number, etc.</small>
            </div>
            
            <div class="form-group">
                <label for="x_city">City</label>
                <input runat="server" type="text" id="x_city" name="x_city" maxlength="40" class="form-control" />
            </div>
            
            <div class="form-group">
                <label for="x_state">State/Province</label>
                <input runat="server" type="text" id="x_state" name="x_state" maxlength="40" class="form-control input-medium" />
                <small>2 character abbreviation preferred, but not required</small>
            </div>
            
            <div class="form-group">
                <label for="x_zip">Zip/Postal Code</label>
                <input runat="server" type="text" id="x_zip" name="x_zip" maxlength="20" class="form-control input-small" />
            </div>
            
            <div class="form-group">
                <label for="x_country">Country</label>
                <input runat="server" type="text" id="x_country" name="x_country" maxlength="60" value="US" class="form-control" />
            </div>
        </div>


        <h3>Payment Information</h3>
        <div class="PaymentMethod">
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
        </div>

        <input type="hidden" name="x_method_available" value="true" />
        <div id="divCreditCardInformation">
            <fieldset class="CreditCardInfo"><legend>Credit Card Information</legend>
                <div class="form-group">
                    <label for="x_card_num">Card Number</label>
                    <input runat="server" type="text" id="x_card_num" name="x_card_num" class="form-control input-medium" maxlength="16" />
                    <small>Enter number without spaces or dashes</small>
                    <small>We never store your credit card information</small>
                </div>
                
                <div class="form-group">
                    <label for="x_exp_date_MM">Expiration Date</label>
                    
                    <input runat="server" type="text" id="x_exp_date_MM" name="x_exp_date_MM" class="form-control input-mini" maxlength="4" placeholder="MM" />
                        &nbsp;/&nbsp;
                    <input runat="server" type="text" id="x_exp_date_YY" name="x_exp_date_YY" class="form-control input-mini" maxlength="4" placeholder="YY" />
                </div>

                <div class="form-group">
                    <label for="x_card_code">Credit Card Security Code</label>
                    <input runat="server" type="text" id="x_card_code" name="x_card_code" class="form-control input-mini" maxlength="4" />
                    <small class="inline"><a href="#ccv-toggle-window" id="ccv-toggle-link">What is this?</a></small>
                    <div id="ccv-toggle-window" class="toggle-window">
                        <small>The three- or four-digit code assigned to a customer’s credit card number. This number is found either on the back of the card or on the front of the card at the end of the credit card number.</small>
                            <img src="img/ccv.gif" width="296" height="172" alt="security code locations">
                        
                    </div>
                </div>
            </fieldset>
        </div>

        <div id="divBankAccountInformation" class="bank-info">
            <fieldset class="BankAccountInfo"><legend>Bank Account Information</legend>
                <div class="form-group">
                    <label for="x_bank_acct_name">Name On Account</label>
                    <input runat="server" type="text" id="x_bank_acct_name" name="x_bank_acct_name" maxlength="22" />
                </div>
                
                <div class="form-group">
                    <label for="x_bank_name">Name of Bank</label>
                    <input runat="server" type="text" id="x_bank_name" name="x_bank_name" maxlength="20" />
                </div>
                
                 <div class="form-group">
                    <label for="x_bank_acct_type">Bank Account Type</label>
                    <select id="x_bank_acct_type" name="x_bank_acct_type" size="1">
                        <option value="CK">Personal Checking</option>
                        <option value="SA">Personal Savings</option>
                        <option value="BC">Business Checking</option>
                    </select>
                </div>
               
                <div class="form-group">
                    <label for="x_bank_acct_num">Bank Account Number</label>
                    <input runat="server" type="text" id="x_bank_acct_num" name="x_bank_acct_num" maxlength="20" />
                    <small>Enter number without spaces or dashes</small>
                </div>
                
                <div class="form-group">
                    <label for="x_bank_aba_code">ABA Routing Number</label>
                    <input runat="server" type="text" id="x_bank_aba_code" name="x_bank_aba_code" maxlength="9" />
                    <small class="inline"><a href="#aba-toggle-window" id="aba-toggle-link">What is this?</a></small>
                    <small>Enter number without spaces or dashes</small>
                    <div id="aba-toggle-window" class="toggle-window">
                        <small>The ABA Routing Number is a nine-digit number that identifies the financial institution associated with a bank account. This number is located at the bottom left corner of a check.</small>
                        <img src="img/ABA-routing-check.gif" alt="ABA Routing Check image" width="292" height="174">
                    </div>
                </div>                
            </fieldset>
        </div>

        <div class="tableButtons">
            <input runat="server" type="submit" class="btn btn-primary btn-large" id="btnSubmit" value="Submit" onclick="return onSubmit();" />
            <input style="display: none;" runat="server" type="button" id="btnCancelOrder" value="Cancel Order" onclick="return cancelOrder_onClick();" />
        </div>

        <div class="epayFoot">
            <p class="center">
                <b>Note: area</b><br />
                <small>&copy; 2015 WTE Solutions.com | All Rights Reserved</small>
            </p>
        </div>
    </asp:Panel>

                
    <asp:Panel runat="server" ID="pnlResult">
        <div id="divPage" class="Page">
            <div id="divMerchantHeader" class="receipt_header">
                <h1>WTE Solutions Receipt</h1>
            </div>
            
            <div id="divThankYou"><h3>Thank you for your payment!</h3></div>
            <div id="divReceiptMsg"><p>Your Web Applicaiton Firewall is currently being provisioned. Next Steps: Make sure you knave control of your DNS entries and a .PFX exort of your site's SSL Certificate.  </p></div>
            <p><asp:Label ID="lblRDescription" runat="server"></asp:Label></p>
            <p><asp:Label ID="lblRTransactionId" runat="server"></asp:Label></p>

                               
            <hr />
        </div>

    </asp:Panel>
    </form>
        
    </section>
    
    <script type="text/javascript" src="js/jquery.min.js"></script>
   
    
    
   

    
    <script>
    $(document).ready(function() {
        // toggle aba message on excellapay
        $('#ccv-toggle-link').click(function(){
            $('#ccv-toggle-window').slideToggle();
        });
   
        
        // toggle aba message on excellapay
        $('#aba-toggle-link').click(function(){
            $('#aba-toggle-window').slideToggle();
        });
   
      });
    </script>


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
                    oDivCC.style.display = ' ';
                    if (null != oDivEcheck) { oDivEcheck.style.display = 'none'; }
                    if (null != otdVme) { otdVme.style.display = 'none'; }
                    otdSubmit.style.display = ' ';
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
                    list = new Array('x_card_num', 'x_exp_date_MM', 'x_exp_date_YY', 'x_card_code', 'x_auth_code');
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
            var expMM = document.getElementById('x_exp_date_MM');
            var expYY = document.getElementById('x_exp_date_YY');
            var cvv = document.getElementById('x_card_code');
            var amnt = document.getElementById('x_amount');

            if (amnt.value == "") { return focusAndShowRequiredMessage(amnt, 'Amount') }
            if (addr.value == "") { return focusAndShowRequiredMessage(addr, 'Address') }
            if (city.value == "") { return focusAndShowRequiredMessage(city, 'City') }
            if (state.value == "") { return focusAndShowRequiredMessage(state, 'State') }
            if (zip.value == "") { return focusAndShowRequiredMessage(zip, 'Zip') }
            if (cardNum.value == "") { return focusAndShowRequiredMessage(cardNum, 'Card Number') }
            if (expMM.value == "") { return focusAndShowRequiredMessage(expMM, 'Expiration Date Month') }
            if (expYY.value == "") { return focusAndShowRequiredMessage(expYY, 'Expiration Date Year') }
            if (cvv.value == "") { return focusAndShowRequiredMessage(cvv, 'Card Code') }

            /*Minimum amount of $1*/
            if (amnt.value <= 1) {return focusAndShowCustomMessage(amnt, 'The Amount field must be greater than $1.00. Please update the value and try again.')}

            return true;
            
        }
        function focusAndShowRequiredMessage(focusOnField, friendlyFieldName) {
            var msg = 'The ' + friendlyFieldName + ' field is required. Please enter a value and try again.';
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
