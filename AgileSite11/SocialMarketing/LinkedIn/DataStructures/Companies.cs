using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;


namespace CMS.SocialMarketing.LinkedInInternal
{
    /// <summary>
    /// Represents a response body after searching companies.
    /// </summary>
    /// <remarks>This class is public for purposes of serialization only. Do not consider it a part of public API. The class' content may change without prior notification.</remarks>
    [XmlRoot("companies")]
    public class Companies
    {
        /// <summary>
        /// List of companies.
        /// </summary>
        [XmlElement("company")]
        public List<Company> CompanyList = new List<Company>();
    }
}
