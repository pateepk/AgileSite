<%@ Page Title="Manage Account" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="MyInvoices.aspx.cs" Inherits="ExcellaLite.Invoices.MyInvoices" %>
<%@ Import Namespace="PaymentProcessor.Web.Applications" %>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">

    <script type="text/javascript">

        var LastDivID = '';
        var LastDivContent = '';
        var LastItemDivID = '';
        var LastItemDivContent = '';
        var AllInvoiceIDs = '<%=AllInvoiceIDs%>';
        var WebServices_PayPartialURL = '<%=WebServices_PayPartialURL%>';
        var WebServices_InvoiceArchiveURL = '<%=WebServices_InvoiceArchiveURL%>';
        var WebServices_InvoiceItemDeleteURL = '<%=WebServices_InvoiceItemDeleteURL%>';
        var WebServices_InvoiceItemPaidURL = '<%=WebServices_InvoiceItemPaidURL%>';
        var WebServices_InvoiceItemFailURL = '<%=WebServices_InvoiceItemFailURL%>';
        var WebServices_InvoiceNoteInsertURL = '<%=WebServices_InvoiceNoteInsertURL%>';

        function ResetButtonBack(ErrorMessage) {

            if (LastDivID.length > 0) {
                $(LastDivID).html(LastDivContent);
            }
            LastDivID = '';
            LastDivContent = '';

            var ids = AllInvoiceIDs.split(',');
            for (var i = 0; i < ids.length; i++) {
                if (ids[i].length > 0) {
                    $('#PayPartialButton_' + ids[i]).prop('disabled', false);
                    $('#PayFullButton_' + ids[i]).prop('disabled', false);
                }
            }
            if (ErrorMessage.length > 0) {
                $('#ErrorMessageHolder').html(ErrorMessage);
            } else {
                $('#ErrorMessageHolder').html('<br>');
            }
        }

        function PaymentItemDelete(invoiceItemID, isConfirmed) {

            var InvoiceNote = ''; 

            $('#ErrorMessageItemHolder').html('<br>');

            var r = false;
            if (isConfirmed) {
                r = true;
            } else {
                OpenDialogConfirm('Only in transit payment can be deleted. Are you sure want to delete a payment (ID=' + invoiceItemID + ')?', 'PaymentItemDelete(' + invoiceItemID + ', true);');
                r = false;
            }

            if (r == true) {
                var postdata = '';
                postdata += WSAddParameter('InvoiceItemID', invoiceItemID);
                postdata += WSAddParameter('InvoiceNote', InvoiceNote);
                $.ajax({
                    url: WebServices_InvoiceItemDeleteURL,
                    type: 'POST',
                    data: postdata,
                    success: function (data) {
                        var xmldata = WSGetJSONObjectFromReturn(data);
                        if (xmldata.success) {
                            window.location.reload();
                        } else {
                            $('#ErrorMessageItemHolder').html(xmldata.ErrorMessage + '<br>');
                        }
                    }
                });
            }
        }

        // for inserting a note on Invoice reading local data and submit to webservice)
        function InsertNote(invoiceID) {
            var InvoiceNote = $('#InvoiceNote_' + invoiceID).val();
            if (InvoiceNote.length > 0) {
                InvoiceNoteInsert(invoiceID, 0, InvoiceNote);
            }
        }

        // for inserting a note on Invoice (webservice)
        function InvoiceNoteInsert(invoiceID, invoiceItemID, InvoiceNote) {

            var postdata = '';
            postdata += WSAddParameter('InvoiceID', invoiceID);
            postdata += WSAddParameter('InvoiceItemID', invoiceItemID);
            postdata += WSAddParameter('InvoiceNote', InvoiceNote);

            $.ajax({
                url: WebServices_InvoiceNoteInsertURL,
                type: 'POST',
                data: postdata,
                success: function (data) {
                    var xmldata = WSGetJSONObjectFromReturn(data);
                    if (xmldata.success) {
                        // note inserted
                    } else {
                        // failed
                    }
                }
            });

        }

        function ProcessPay(invoiceID) {
            var InvoiceNote = $('#Partial_InvoiceNote_' + invoiceID).val();

            if (InvoiceNote.length > 0) {

                $('#ErrorMessageHolder').html('<br>');
                $('#Partial_Amount_' + invoiceID).prop('disabled', true);
                $('#ProcessPayButton_' + invoiceID).prop('disabled', true);
                $('#AjaxLoader_' + invoiceID).show();
                var ids = AllInvoiceIDs.split(',');
                for (var i = 0; i < ids.length; i++) {
                    if (ids[i].length > 0) {
                        if ($.isNumeric(ids[i]) && ids[i] != invoiceID) {
                            $('#PayPartialButton_' + ids[i]).prop('disabled', true);
                            $('#PayFullButton_' + ids[i]).prop('disabled', true);
                        }
                    }
                }



                var postdata = '';
                postdata += WSAddParameter('InvoiceID', invoiceID);
                postdata += WSAddParameter('Amount', $('#Partial_Amount_' + invoiceID).val());
                postdata += WSAddParameter('InvoiceNote', InvoiceNote);
                $.ajax({
                    url: WebServices_PayPartialURL,
                    type: 'POST',
                    data: postdata,
                    success: function (data) {
                        var xmldata = WSGetJSONObjectFromReturn(data);
                        if (xmldata.success) {

                            if (xmldata.InvoiceItemID > 0) {
                                // window.location = "/Invoices/MyInvoices/?InvoiceID=" + invoiceID + '&InvoiceItemID=' + xmldata.InvoiceItemID;
                                var detailLink = "/Invoices/MyInvoices/?InvoiceID=" + invoiceID + '&InvoiceItemID=' + xmldata.InvoiceItemID;
                                DialogPaymentAfterPayment(detailLink);
                                UpdateRow(invoiceID, xmldata);
                                //$('#TableRowInvoice_' + invoiceID).remove();
                                ResetButtonBack('');
                            } else {
                                ResetButtonBack(xmldata.ErrorMessage);
                            }
                        } else {
                            ResetButtonBack('');
                        }
                    }
                });
            } else {
                OpenDialogAlert('Note for partial pay is required.');
            }

        }

        function PayCancel() {
            CloseDialogAlert();
            if (LastDivID.length > 0) {
                $(LastDivID).html(LastDivContent);
            }
            LastDivID = '';
            LastDivContent = '';
        }

        function ProcessItemPayNowCancel() {
            if (LastItemDivID.length > 0) {
                $(LastItemDivID).html(LastItemDivContent);
            }
            LastItemDivID = '';
            LastItemDivContent = '';
        }

        function ProcessItemFailNow(invoiceItemID, amount, isConfirmed) {

            var r = false;
            if (isConfirmed) {
                r = true;
            } else {
                OpenDialogConfirm('Are you sure to FAIL Payment Item ID ' + invoiceItemID + ' with amount ' + amount + '?', 'ProcessItemFailNow(' + invoiceItemID + ',\'' + amount + '\', true);');
                r = false;
            }

            if (r == true) {
                var PaidNote = ($('#ProcessItemNote_' + invoiceItemID).val());

                var postdata = '';
                postdata += WSAddParameter('InvoiceItemID', invoiceItemID);
                postdata += WSAddParameter('PaidNote', PaidNote);

                $.ajax({
                    url: WebServices_InvoiceItemFailURL,
                    type: 'POST',
                    data: postdata,
                    success: function (data) {
                        var xmldata = WSGetJSONObjectFromReturn(data);
                        if (xmldata.success) {
                            if (xmldata.InvoiceItemID > 0) {
                                window.location.reload();
                            } else {
                                //ResetButtonBack(xmldata.ErrorMessage);
                            }
                        } else {
                            //ResetButtonBack('');
                        }
                    }
                });

            }
        }

        function ProcessItemPayNow(invoiceItemID, amount, isConfirmed) {

            var r = false;
            if (isConfirmed) {
                r = true;
            } else {
                OpenDialogConfirm('Are you sure to process Payment Item ID ' + invoiceItemID + ' with amount ' + amount + '?', 'ProcessItemPayNow(' + invoiceItemID + ',\'' + amount + '\', true);');
                r = false;
            }

            if (r == true) {
                var PaidNote = ($('#ProcessItemNote_' + invoiceItemID).val());

                var postdata = '';
                postdata += WSAddParameter('InvoiceItemID', invoiceItemID);
                postdata += WSAddParameter('PaidNote', PaidNote);
                $.ajax({
                    url: WebServices_InvoiceItemPaidURL,
                    type: 'POST',
                    data: postdata,
                    success: function (data) {
                        var xmldata = WSGetJSONObjectFromReturn(data);
                        if (xmldata.success) {
                            if (xmldata.InvoiceItemID > 0) {
                                window.location.reload();
                            } else {
                                //ResetButtonBack(xmldata.ErrorMessage);
                            }
                        } else {
                            //ResetButtonBack('');
                        }
                    }
                });

            }
        }

        function ArchiveInvoice(invoiceID) {
            var postdata = '';
            postdata += WSAddParameter('InvoiceID', invoiceID);
            $.ajax({
                url: WebServices_InvoiceArchiveURL,
                type: 'POST',
                data: postdata,
                success: function (data) {
                    var xmldata = WSGetJSONObjectFromReturn(data);
                    if (xmldata.success) {
                        if (xmldata.InvoiceID > 0) {
                            window.location.reload();
                        } else {
                            //ResetButtonBack(xmldata.ErrorMessage);
                        }
                    } else {
                        OpenDialogAlert('Error archiving');
                    }
                }
            });
        }

        function PayPartial(invoiceID, amount, intransit, paid, due, isFull) {
            DialogPaymentAfterPaymentClose();
            if (LastDivID.length > 0) {
                $(LastDivID).html(LastDivContent);
            }
            LastDivID = '#Pay_' + invoiceID;
            LastDivContent = $(LastDivID).html();

            var strHTML = '$&nbsp;<input type=\"text\" style=\"width:55px; height:12px; \" value=\"' + (amount - intransit - paid) + '\" id=\"Partial_Amount_' + invoiceID + '\">&nbsp;<input id=\"ProcessPayButton_' + invoiceID + '\" onclick=\"ProcessPay(' + invoiceID + ');\" type=\"button\" class=\"ui-button ui-widget ui-corner-all\" value=\"Pay\">&nbsp;<input type=\"button\" class=\"ui-button ui-widget ui-corner-all\" onclick=\"PayCancel();\" value=\"Cancel\">&nbsp;<img id=\"AjaxLoader_' + invoiceID + '\" style=\"display:none;\" src=\"/Images/ajax.gif\"><br /><b>Note is required:</b> <input id=\"Partial_InvoiceNote_' + invoiceID + '\" type=\"text\" >';
            $(LastDivID).html(strHTML);
            if (isFull) {
                $('#Partial_Amount_' + invoiceID).attr('readonly', true);
            } else {
                $('#Partial_Amount_' + invoiceID).select();
            }
        }

        var secondsLeft = 10;
        var handlerCountdown = 0;

        function invoicePaymentDialogCountdown()
        {
            clearTimeout(handlerCountdown);
            secondsLeft = secondsLeft - 1;
            $('#invoicePaymentDialogCountdown').html('<b>' + secondsLeft + '</b>' + (secondsLeft > 1 ? " seconds" : " second"));
            if(secondsLeft <=0)
            {
                $('#dialogPayment').dialog("close");
            } else
            {
                handlerCountdown = setTimeout("invoicePaymentDialogCountdown()", 1000);
            }
        }

        function DetailInvoiceFromDialog(detailLink) {
            window.location = detailLink;
        }

        function DialogPaymentAfterPaymentClose() {
            clearTimeout(handlerCountdown);
            $('#dialogPayment').dialog("close");
        }

        function DialogPaymentAfterPayment(detailLink) {
            $('#dialogPayment').dialog("close"); // close previous dialog
            $('#dialogPayment').dialog({
                width: 500
            });

            var strHTML = '<input type="button" id="dialogPaymentDetail" onclick="DetailInvoiceFromDialog(\'' + detailLink + '\');" class="ui-button ui-widget ui-corner-all" value="Payments Detail" />';
            strHTML += '<input type="button" id="dialogPaymentClose" onclick="DialogPaymentAfterPaymentClose();" class="ui-button ui-widget ui-corner-all" value="Close Window" /><br />';

            $('#invoiceLinkDetail').html(strHTML);
            secondsLeft = 10;
            invoicePaymentDialogCountdown();
        }

        function UpdateRow(invoiceID, xmldata)
        {
            $('#InvoiceTotalInvoice_' + invoiceID).html(xmldata.InvoiceTotalDollar);
            $('#AmountInTransitInvoice_' + invoiceID).html(xmldata.AmountInTransitDollar);
            $('#AmountDueInvoice_' + invoiceID).html(xmldata.AmountDueDollar);
            $('#AmountPaidInvoice_' + invoiceID).html(xmldata.AmountPaidDollar);
            $('#InvoiceStatusInvoice_' + invoiceID).html(xmldata.InvoiceStatus);
            if (xmldata.InvoiceTotal - xmldata.AmountPaid - xmldata.AmountInTransit <= 0) {
                $('#ActionInvoice_' + invoiceID).html('<%=SV.Common.WaitingForProcess%>');
            }
        }

        function PayFull(invoiceID, amount, intransit, paid, due) {
            DialogPaymentAfterPaymentClose();
            // directly pay now
            //PayPartial(invoiceID, amount, intransit, paid, due, true);
            $('#PayFullButton_' + invoiceID).prop('disabled', true);

            var InvoiceNote = '';

            var postdata = '';
            postdata += WSAddParameter('InvoiceID', invoiceID);
            postdata += WSAddParameter('Amount', (amount - intransit - paid));
            postdata += WSAddParameter('InvoiceNote', InvoiceNote);
            $.ajax({
                url: WebServices_PayPartialURL,
                type: 'POST',
                data: postdata,
                success: function (data) {
                    var xmldata = WSGetJSONObjectFromReturn(data);
                    if (xmldata.success) {

                        if (xmldata.InvoiceItemID > 0) {
                            var detailLink = "/Invoices/MyInvoices/?InvoiceID=" + invoiceID + '&InvoiceItemID=' + xmldata.InvoiceItemID;
                            // window.location = detailLink;
                            DialogPaymentAfterPayment(detailLink);
                            //$('#TableRowInvoice_' + invoiceID).remove();
                            UpdateRow(invoiceID, xmldata);
                        } else {
                            if (xmldata.ErrorMessage.length > 0) {
                                $('#ErrorMessageHolder').html(xmldata.ErrorMessage);
                            } else {
                                $('#ErrorMessageHolder').html('<br>');
                            }
                            $('#PayFullButton_' + invoiceID).removeAttr('disabled');
                        }
                    } else {
                        $('#PayFullButton_' + invoiceID).removeAttr('disabled');
                    }
                }
            });

        }



        function ProcessItemNow(invoiceItemID, amount) {
            
            if (LastItemDivID.length > 0) {
                $(LastItemDivID).html(LastItemDivContent);
            }
            LastItemDivID = '#ProcessItemNowHolder_' + invoiceItemID;
            LastItemDivContent = $(LastItemDivID).html();
            var isfail = $('#ChkFailProcess_' + invoiceItemID).is(':checked');
            var strHTML;
            
            if (isfail) {
                strHTML = '<b>Fail Note:</b> <input type=\"text\" style=\"width:200px; height:12px; \" maxlength=\"80\" value=\"\" id=\"ProcessItemNote_' + invoiceItemID + '\">&nbsp;<input id=\"ProcessitemNowButton_' + invoiceItemID + '\" onclick=\"ProcessItemFailNow(' + invoiceItemID + ',\'' + amount + '\');\" type=\"button\" class=\"ui-button ui-widget ui-corner-all\" value=\"Fail\">&nbsp;<input id=\"ProcessitemNowButton_' + invoiceItemID + '\" onclick=\"ProcessItemPayNowCancel();\" type=\"button\" class=\"ui-button ui-widget ui-corner-all\" value=\"Cancel\">&nbsp;<img id=\"AjaxLoaderItem_' + invoiceItemID + '\" style=\"display:none;\" src=\"/Images/ajax.gif\">';
            } else {
                strHTML = '<b>Note:</b> <input type=\"text\" style=\"width:200px; height:12px; \" maxlength=\"80\" value=\"\" id=\"ProcessItemNote_' + invoiceItemID + '\">&nbsp;<input id=\"ProcessitemNowButton_' + invoiceItemID + '\" onclick=\"ProcessItemPayNow(' + invoiceItemID + ',\'' + amount + '\');\" type=\"button\" class=\"ui-button ui-widget ui-corner-all\" value=\"Process\">&nbsp;<input id=\"ProcessitemNowButton_' + invoiceItemID + '\" onclick=\"ProcessItemPayNowCancel();\" type=\"button\" class=\"ui-button ui-widget ui-corner-all\" value=\"Cancel\">&nbsp;<img id=\"AjaxLoaderItem_' + invoiceItemID + '\" style=\"display:none;\" src=\"/Images/ajax.gif\">';
            }
            $(LastItemDivID).html(strHTML);
            $('#ProcessItemNote_' + invoiceItemID).select();
        }

        function main() {

        }

        $(document).ready(function () {
            main();
        });


    </script>

    <br /><br />

    <%if(InvoiceStatusID == 0) { %>
        <h1>Open Invoices</h1>
        <%if(InvoiceID > 0) { %>
            <a href="/Invoices/MyInvoices"><< Back To Open Invoices</a>
        <% } %>
    <% } %>

    <%if(InvoiceStatusID == 1) { %>
        <h1>In Transit Payments</h1>
        <%if(InvoiceID > 0) { %>
            <a href="/Invoices/MyInvoices?InvoiceStatusID=1"><< Back To In Transit Payments</a>
        <% } %>
    <% } %>

    <%if(InvoiceStatusID == 2) { %>
        <h1>Closed Invoices</h1>
        <%if(InvoiceID > 0) { %>
            <a href="/Invoices/MyInvoices?InvoiceStatusID=2"><< Back To Closed Invoices</a>
        <% } %>
    <a href="/Invoices/MyInvoices?InvoiceStatusID=3">Archived Invoices</a>
    <% } %>

    <%if(InvoiceStatusID == 3) { %>
        <h1>Archived Invoices</h1>
        <%if(InvoiceID > 0) { %>
            <a href="/Invoices/MyInvoices?InvoiceStatusID=3"><< Back To Archived Invoices</a>
        <% } %>
    <% } %>
    
        
    <asp:Literal ID="CustNumLiteral" runat="server"></asp:Literal>

    <span id="ErrorMessageHolder" style="color:#FF0000; font-weight:bold;"><br /></span>
    <asp:Repeater ID="Repeater1" runat="server">
        <HeaderTemplate>
             <table class="table-nice">
                <tr>
                   <th><b>Invoice</b></th>
                   <th><b>Status</b></th>
<%--               <%# user.isUserAdministrator ? "<th><b>Customer</b></th>": "" %>--%>
 <%--              <th><b>Invoice</b></th>
                   <th><b>Date</b></th>
                   <th><b>Project</b></th>--%>
                   <th><b>Due Date</b></th>
<%--                   <th><b>Terms</b></th>--%>
                   <th><b>Amount</b></th>
                   <th><b>In Transit</b></th>
                   <th><b>Paid</b></th>
                   <th><b>Balance</b></th>
                   <th><b>View</b></th>
                    <%if(InvoiceStatusID != 3) { %>
                   <th><b>Action</b></th>
                    <% } %>
                </tr>
        </HeaderTemplate>
        <ItemTemplate>
             <tr id="TableRowInvoice_<%# DataBinder.Eval(Container.DataItem, DRspWTE_excellalite_invoices_GetByUserID.Columns.invoiceID)%>">
                <td> <%# DataBinder.Eval(Container.DataItem, Columns.InvoiceDetailLink)%> </td>
                <td id="InvoiceStatusInvoice_<%# DataBinder.Eval(Container.DataItem, DRspWTE_excellalite_invoices_GetByUserID.Columns.invoiceID)%>"> <%# DataBinder.Eval(Container.DataItem, DRspWTE_excellalite_invoices_GetByUserID.Columns.InvoiceStatus)%> </td>
<%--            <%# user.isUserAdministrator ? "<td>" + DataBinder.Eval(Container.DataItem, DRspWTE_excellalite_invoices_GetByUserID.Columns.CustNum) + "</td>": "" %>--%>
<%--            <td> <%# DataBinder.Eval(Container.DataItem, DRspWTE_excellalite_invoices_GetByUserID.Columns.CustNum)%> </td>
                <td> <%# DataBinder.Eval(Container.DataItem, DRspWTE_excellalite_invoices_GetByUserID.Columns.Invoice)%> </td>
                <td> <%# DataBinder.Eval(Container.DataItem, DRspWTE_excellalite_invoices_GetByUserID.Columns.invoiceDate, SV.Common.SimpleDateFormat)%> </td>
                <td> <%# DataBinder.Eval(Container.DataItem, DRspWTE_excellalite_invoices_GetByUserID.Columns.Project)%> </td>--%>
                <td> <%# DataBinder.Eval(Container.DataItem, DRspWTE_excellalite_invoices_GetByUserID.Columns.DateDue, SV.Common.SimpleDateFormat)%> </td>
<%--            <td> <%# DataBinder.Eval(Container.DataItem, DRspWTE_excellalite_invoices_GetByUserID.Columns.Terms)%> </td>--%>
                <td id="InvoiceTotalInvoice_<%# DataBinder.Eval(Container.DataItem, DRspWTE_excellalite_invoices_GetByUserID.Columns.invoiceID)%>" align="right"> <%# DataBinder.Eval(Container.DataItem, DRspWTE_excellalite_invoices_GetByUserID.Columns.InvoiceTotal, SV.Common.CurrencyFormat)%> </td>
                <td id="AmountInTransitInvoice_<%# DataBinder.Eval(Container.DataItem, DRspWTE_excellalite_invoices_GetByUserID.Columns.invoiceID)%>" align="right"> <%# DataBinder.Eval(Container.DataItem, DRspWTE_excellalite_invoices_GetByUserID.Columns.AmountInTransit, SV.Common.CurrencyFormat)%> </td>
                <td id="AmountPaidInvoice_<%# DataBinder.Eval(Container.DataItem, DRspWTE_excellalite_invoices_GetByUserID.Columns.invoiceID)%>" align="right"> <%# DataBinder.Eval(Container.DataItem, DRspWTE_excellalite_invoices_GetByUserID.Columns.AmountPaid, SV.Common.CurrencyFormat)%> </td>
                <td id="AmountDueInvoice_<%# DataBinder.Eval(Container.DataItem, DRspWTE_excellalite_invoices_GetByUserID.Columns.invoiceID)%>" align="right"> <%# DataBinder.Eval(Container.DataItem, DRspWTE_excellalite_invoices_GetByUserID.Columns.AmountDue, SV.Common.CurrencyFormat)%> </td>
                <td> <a href="/Invoices/InvoiceInIFrame.aspx?InvoiceID=<%# DataBinder.Eval(Container.DataItem, DRspWTE_excellalite_invoices_GetByUserID.Columns.invoiceID)%>"><%# DataBinder.Eval(Container.DataItem, DRspWTE_excellalite_invoices_GetByUserID.Columns.docname)%></a></td>
                <%if(InvoiceStatusID != 3) { %>
                <td id="ActionInvoice_<%# DataBinder.Eval(Container.DataItem, DRspWTE_excellalite_invoices_GetByUserID.Columns.invoiceID)%>" >
                    <div  id="Pay_<%# DataBinder.Eval(Container.DataItem, DRspWTE_excellalite_invoices_GetByUserID.Columns.invoiceID)%>">
                    <%# DataBinder.Eval(Container.DataItem, Columns.PartialPayButton) %>&nbsp;
                    <%# DataBinder.Eval(Container.DataItem, Columns.FullPayButton) %>
                    <%--<%# DataBinder.Eval(Container.DataItem, Columns.InsertNoteButton) %>--%>
                    <br />
                    </div>
                </td>
                <% } %>
             </tr>
        </ItemTemplate>
        <FooterTemplate>
             </table>
        </FooterTemplate>
    </asp:Repeater>

    <asp:Repeater ID="Repeater3" runat="server">
        <HeaderTemplate>
            <br />
             <table class="table-nice">
                <caption>Invoice Notes</caption>
                <tr>
                   <th><b>Date</b></th>
                   <th><b>Note By</b></th>
                   <th><b>Note</b></th>
                </tr>
        </HeaderTemplate>
        <ItemTemplate>
             <tr>
                 <td> <%# DataBinder.Eval(Container.DataItem, DRspWTE_excellalite_invoices_notes_GetAllNotes.Columns.CreatedDate)%> </td>
                 <td> <%# DataBinder.Eval(Container.DataItem, DRspWTE_excellalite_invoices_notes_GetAllNotes.Columns.FullName)%> </td>
                 <td> <%# DataBinder.Eval(Container.DataItem, DRspWTE_excellalite_invoices_notes_GetAllNotes.Columns.invoiceNote)%> </td>
             </tr>
        </ItemTemplate>
        <FooterTemplate>

             </table>
        </FooterTemplate>
    </asp:Repeater>


    <span id="ErrorMessageItemHolder" style="color:#FF0000; font-weight:bold;"><br /></span>
    <asp:Repeater ID="Repeater2" runat="server">
        <HeaderTemplate>
             <table class="table-nice">
                <caption>Invoice Activity</caption>
                <tr>
                   <th title="Click to delete payment item when still in transit."><b>Delete</b></th>
                   <th title="Payment Item ID"><b>ID</b></th>
                   <th title="Payment Item Created Date"><b>Create</b></th>
                   <th title="Payment Item Status"><b>Status</b></th>
                   <th title="Requested By"><b>Request</b></th>
                   <th title="Amount Paid"><b>Amount</b></th>
                   <th title="Processed By"><b>Process</b></th>
                   <th title="Processed Date"><b>Date</b></th>
                   <th><b>Note</b></th>
                </tr>
        </HeaderTemplate>
        <ItemTemplate>
             <tr >
                <td> <%# DataBinder.Eval(Container.DataItem, ItemsColumns.ItemDeleteLink)%> </td>
                <td> <%# DataBinder.Eval(Container.DataItem, DRspWTE_excellalite_invoices_items_GetByInvoiceID.Columns.invoiceItemID)%> </td>
                <td> <%# DataBinder.Eval(Container.DataItem, DRspWTE_excellalite_invoices_items_GetByInvoiceID.Columns.CreatedDate, SV.Common.SimpleDateFormat)%> </td>
                <td> <%# DataBinder.Eval(Container.DataItem, DRspWTE_excellalite_invoices_items_GetByInvoiceID.Columns.InvoiceItemStatus)%> </td>
                <td> <%# DataBinder.Eval(Container.DataItem, DRspWTE_excellalite_invoices_items_GetByInvoiceID.Columns.PaymentUser)%> </td>
                <td> <%# DataBinder.Eval(Container.DataItem, DRspWTE_excellalite_invoices_items_GetByInvoiceID.Columns.Amount, SV.Common.CurrencyFormat)%> </td>
                <%# (Eval(ItemsColumns.ItemProcessLink).ToString().Length > 0) ? String.Format("<td width=\"100%\" colspan=\"3\"><span id=\"ProcessItemNowHolder_{1}\">{0}</span></td>",DataBinder.Eval(Container.DataItem, ItemsColumns.ItemProcessLink), DataBinder.Eval(Container.DataItem, DRspWTE_excellalite_invoices_items_GetByInvoiceID.Columns.invoiceItemID)) :  String.Format("<td>{0}</td><td>{1}</td><td>{2}</td>", DataBinder.Eval(Container.DataItem, DRspWTE_excellalite_invoices_items_GetByInvoiceID.Columns.PaidUser), DataBinder.Eval(Container.DataItem, DRspWTE_excellalite_invoices_items_GetByInvoiceID.Columns.PaidDate, SV.Common.SimpleDateFormat), DataBinder.Eval(Container.DataItem, DRspWTE_excellalite_invoices_items_GetByInvoiceID.Columns.PaidNote)) %>
             </tr>
        </ItemTemplate>
        <FooterTemplate>
             <tr><td colspan="5">&nbsp;</td></tr>   
             <tr><td colspan="4">Total Amount</td><td align="right" ><%=String.Format(SV.Common.CurrencyFormat,TotalItemsAmount) %></td></tr>    
             <tr><td colspan="4">Total Paid Amount</td><td align="right" ><%=String.Format(SV.Common.CurrencyFormat,TotalPaidAmount) %></td></tr>
             <tr><td colspan="4">Total In Transit Amount</td><td align="right" ><%=String.Format(SV.Common.CurrencyFormat,TotalInTransitAmount) %></td></tr>
             <tr><td colspan="4">Total Due Amount</td><td align="right" ><%=String.Format(SV.Common.CurrencyFormat,TotalDueAmount) %></td></tr>
             </table>
        </FooterTemplate>
    </asp:Repeater>

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
    <% } %>

<div id="dialogPayment" style="display:none; " title="Thank you for your prompt payment!">
  
  <p style="font-size:small; align-content:center;">This invoice has been submitted for payment and will appear in your In Transit Payments list.</p>
  <div style="text-align:center;">
    This window will be closed in <span id="invoicePaymentDialogCountdown"></span>
    <div id="invoiceLinkDetail"></div>
  </div>
</div>


</asp:Content>
