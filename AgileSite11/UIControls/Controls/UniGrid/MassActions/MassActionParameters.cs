using System;
using System.Collections.Generic;
using System.Linq;

namespace CMS.UIControls
{
    /// <summary>
    /// Represents session parameters that are required for performing a mass action.
    /// </summary>
    /// <remarks>Instance of this class is supposed to be provided by the system.</remarks>
    [Serializable]
    public sealed class MassActionParameters : IEquatable<MassActionParameters>
    {
        private IEnumerable<int> mIDs;


        /// <summary>
        /// Info identifiers on which mass action will be executed.
        /// </summary>
        public IEnumerable<int> IDs
        {
            get
            {
                return mIDs;
            }
            private set
            {
                mIDs = value ?? Enumerable.Empty<int>();
            }
        }


        /// <summary>
        /// Type of interaction when performing the mass action.
        /// </summary>
        public MassActionTypeEnum Behavior
        {
            get;
            private set;
        }


        /// <summary>
        /// Type of info used by mass actions.
        /// </summary>
        public string ObjectType
        {
            get;
            private set;
        }


        /// <summary>
        /// URI on which user will be redirected after performing mass action. 
        /// </summary>
        /// <remarks>Applicable when <see cref="Behavior"/> is set to <see cref="MassActionTypeEnum.Redirect"/>.</remarks>
        public Uri ReturnUrl
        {
            get;
            private set;
        }


        /// <summary>
        /// Script for reloading underlying grid.
        /// </summary>
        /// <remarks>Applicable when <see cref="Behavior"/> is set to <see cref="MassActionTypeEnum.OpenModal"/>.</remarks>
        public string ReloadScript
        {
            get;
            private set;
        }


        /// <summary>
        /// Where condition that indicates which items will be deleted when <see cref="MassActionScopeEnum.AllItems"/> was selected.
        /// </summary>
        internal string WhereCondition
        {
            get;
            private set;
        }


        /// <summary>
        /// Initializes new instance of the <see cref="MassActionParameters"/> with properties that are independent on a mass action.
        /// </summary>
        /// <param name="objectType">Type of info used by mass actions.</param>
        /// <param name="returnUrl">URI on which user will be redirected after performing mass action.</param>
        /// <param name="reloadScript">Script for reloading underlying grid.</param>
        /// <param name="whereCondition">Where condition that indicates which items will be deleted when <see cref="MassActionScopeEnum.AllItems"/> was selected.</param>
        internal MassActionParameters(string objectType, Uri returnUrl, string reloadScript, string whereCondition)
        {
            ObjectType = objectType;
            ReturnUrl = returnUrl;
            ReloadScript = reloadScript;
            WhereCondition = whereCondition;
        }


        /// <summary>
        /// Initializes new instance of the <see cref="MassActionParameters"/> with all properties in class.
        /// </summary>
        /// <param name="ids">Info identifiers on which mass action will be executed.</param>
        /// <param name="behavior">Type of interaction when performing the mass action.</param>
        /// <param name="generalParameters">Parameters that are independent on a mass action.</param>
        /// <remarks><paramref name="generalParameters"/> is expected not to be <c>null</c>.</remarks>
        internal MassActionParameters(IEnumerable<int> ids, MassActionTypeEnum behavior, MassActionParameters generalParameters)
            : this(generalParameters.ObjectType, generalParameters.ReturnUrl, generalParameters.ReloadScript, generalParameters.WhereCondition)
        {
            IDs = ids;
            Behavior = behavior;
        }


        /// <summary>
        /// Compares two object instances for equality.
        /// </summary>
        /// <param name="parameters">Instance to compare.</param>
        public override bool Equals(object parameters)
        {
            return Equals(parameters as MassActionParameters);
        }


        /// <summary>
        /// Compares two mass action parameters for equality.
        /// </summary>
        /// <param name="parameters">Mass action parameters to compare.</param>
        public bool Equals(MassActionParameters parameters)
        {
            if (parameters == null)
            {
                return false;
            }

            return Behavior.Equals(parameters.Behavior) &&
                   AreEqual(ObjectType, parameters.ObjectType) &&
                   AreEqual(ReturnUrl, parameters.ReturnUrl) &&
                   AreEqual(ReloadScript, parameters.ReloadScript) &&
                   AreEqual(WhereCondition, parameters.WhereCondition) &&
                   AreEqual(IDs, parameters.IDs);                        
        }      
        

        /// <summary>
        /// Returns hash code for instance values.
        /// </summary>
        public override int GetHashCode()
        {
            const int MULTIPLIER = 7;
            int hash = 13;
            hash = IDs.Aggregate(hash, (aggregatedHash, id) => aggregatedHash * MULTIPLIER + id);
            hash = (hash * MULTIPLIER) + Behavior.GetHashCode();
            hash = (hash * MULTIPLIER) + ObjectType.GetHashCode();
            hash = (hash * MULTIPLIER) + ReturnUrl.GetHashCode();
            hash = (hash * MULTIPLIER) + ReloadScript.GetHashCode();
            hash = (hash * MULTIPLIER) + WhereCondition.GetHashCode();

            return hash;
        }


        /// <summary>
        /// Returns a <see cref="string"/> which represents the object instance.
        /// </summary>
        public override string ToString()
        {
            return String.Format(
                "Behavior: {1}{0}Object type: {2}{0}Return URL: {3}{0}Reload script: {4}{0}IDs: {5}{0}",
                Environment.NewLine,
                Behavior,
                ObjectType,
                ReturnUrl,
                ReloadScript,
                String.Join(", ", IDs));
        }


        /// <summary>
        /// Returns true if two instances are equal.
        /// </summary>
        private static bool AreEqual(object first, object second)
        {
            return ReferenceEquals(first, second) || (first != null && first.Equals(second));
        }


        /// <summary>
        /// Returns true if two sequences are equal.
        /// </summary>
        private static bool AreEqual(IEnumerable<int> first, IEnumerable<int> second)
        {
            return ReferenceEquals(first, second) || (first != null && first.SequenceEqual(second));
        }
    }
}
