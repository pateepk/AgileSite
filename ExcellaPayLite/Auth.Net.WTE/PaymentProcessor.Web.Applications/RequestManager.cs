namespace PaymentProcessor.Web.Applications
{

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    [Serializable]
    public class RequestManager
    {

        private string _errormessage = string.Empty;
        private int _errorcode = 0;
        private bool _iserror = false;

        public RequestManager()
        {

        }

        public void SetError(int errorcode, string errormessage)
        {
            _errorcode = errorcode;
            _errormessage = errormessage;
            _iserror = true;
        }

        public string ErrorMessage
        {
            get
            {
                return _errormessage;
            }
        }

        public int ErrorCode
        {
            get
            {
                return _errorcode;
            }
        }

        public bool IsError
        {
            get
            {
                return _iserror;
            }
        }

    }

}
