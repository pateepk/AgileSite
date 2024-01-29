using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Base;
using CMS.DataEngine.Internal;
using CMS.Helpers;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Container for the contact change data.
    /// </summary>
    /// <remarks>
    /// This API supports the framework infrastructure and is not intended to be used directly from your code.
    /// </remarks>
    public class ContactChangeData : IDataTransferObject
    {
        /// <summary>
        /// Creates new instance of <see cref="ContactChangeData"/>.
        /// </summary>
        public ContactChangeData()
        {
        }


        /// <summary>
        /// Creates new instance of <see cref="ContactChangeData"/> and set up its properties from given <paramref name="dataRowContainer"/>.
        /// </summary>
        /// <param name="dataRowContainer">Data row container containing the data used for creating the <see cref="ContactChangeData"/></param>
        /// <exception cref="ArgumentNullException"><paramref name="dataRowContainer"/> is <c>null</c></exception>
        public ContactChangeData(IDataContainer dataRowContainer)
        {
            if (dataRowContainer == null)
            {
                throw new ArgumentNullException("dataRowContainer");
            }

            ContactID = dataRowContainer["ContactChangeRecalculationQueueContactID"].ToInteger(0);
            ChangedColumns = dataRowContainer["ContactChangeRecalculationQueueChangedColumns"].ToString("").Split(new [] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            ContactIsNew = dataRowContainer["ContactChangeRecalculationQueueContactIsNew"].ToBoolean(false);
            ContactWasMerged = dataRowContainer["ContactChangeRecalculationQueueContactWasMerged"].ToBoolean(false);
        }


        /// <summary>
        /// Contact ID.
        /// </summary>
        public int ContactID
        {
            get;
            set;
        }

        
        /// <summary>
        /// List of columns which have changed. Is null when <see cref="ContactIsNew"/> is true.
        /// </summary>
        public IList<string> ChangedColumns
        {
            get;
            set;
        }


        /// <summary>
        /// If true the change represents a new contact creation. <see cref="ChangedColumns" /> are not included if contact is new.
        /// </summary>
        public bool ContactIsNew
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the value indicating if the contact was automatically merged.
        /// </summary>
        public bool ContactWasMerged
        {
            get;
            set;
        }


        /// <summary>
        /// Returns the string representation of the object.
        /// </summary>
        public override string ToString()
        {
            return String.Format(
@"
ContactID: {0}
ContactIsNew: {1}
ChangedColumns: {2}
ContactWasMerged: {3}", 
                ContactID, 
                ContactIsNew,
                (ChangedColumns != null) ? ChangedColumns.Join(", ") : null, 
                ContactWasMerged
            );
        }


        /// <summary>
        /// Returns true if the object equals to another.
        /// </summary>
        /// <param name="obj">Object to compare to</param>
        public override bool Equals(object obj)
        {
            var d = obj as ContactChangeData;

            return
                (d != null) &&
                (d.ContactIsNew == ContactIsNew) &&
                (d.ContactID == ContactID) &&
                (d.ContactWasMerged == ContactWasMerged) &&
                CompareColumns(d.ChangedColumns, ChangedColumns);
        }


        /// <summary>
        /// Compares the two lists of columns
        /// </summary>
        /// <param name="list1">First list</param>
        /// <param name="list2">Second list</param>
        private bool CompareColumns(IList<string> list1, IList<string> list2)
        {
            if ((list1 == null) || (list2 == null))
            {
                return ((list1 == null) && (list2 == null));
            }

            return (!list1.Except(list2).Any()) && (!list2.Except(list1).Any());
        }


        /// <summary>
        /// Gets the object hash code
        /// </summary>
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }


        /// <summary>
        /// Fills given <paramref name="dataContainer"/> with values from current <see cref="ContactChangeData"/>.
        /// </summary>  
        /// <param name="dataContainer">Data container to be filled</param>
        /// <exception cref="ArgumentNullException"><paramref name="dataContainer"/> is <c>null</c></exception>
        public void FillDataContainer(IDataContainer dataContainer)
        {
            if (dataContainer == null)
            {
                throw new ArgumentNullException("dataContainer");
            }

            dataContainer["ContactChangeRecalculationQueueContactID"] = ContactID;
            dataContainer["ContactChangeRecalculationQueueChangedColumns"] = ChangedColumns != null ? string.Join(";", ChangedColumns) : null;
            dataContainer["ContactChangeRecalculationQueueContactIsNew"] = ContactIsNew;
            dataContainer["ContactChangeRecalculationQueueContactWasMerged"] = ContactWasMerged;
        }
    }
}