using System;

using CMS.Base;
using CMS.Helpers;

namespace CMS.OnlineMarketing.Web.UI
{
    /// <summary>
    /// Helps with the A/B test variants in the Pages application UI.
    /// </summary>
    public class ABTestUIVariantHelper : AbstractHelper<ABTestUIVariantHelper>
    {
        private const string AB_TEST_UI_SESSION_KEY_PREFIX = "ABTestUIVariantHelper_";


        /// <summary>
        /// Persists identifier of the A/B variant to be displayed in Pages UI for given node.
        /// </summary>
        /// <param name="pageId">Identifies page for which to persist A/B variant.</param>
        /// <param name="targetVariant">A/B variant identifier that should be persisted for the usage in the UI for given <paramref name="pageId"/>.</param>
        /// <seealso cref="GetPersistentVariantIdentifier(int)"/>
        public static void SetPersistentVariantIdentifier(int pageId, Guid targetVariant)
        {
            HelperObject.SetPersistentVariantIdentifierInternal(pageId, targetVariant);
        }


        /// <summary>
        /// Gets identifier of the A/B variant (original is considered to be an A/B variant as well) persisted for usage in the UI for given node.
        /// If null then no variant was persisted for the given node.
        /// </summary>
        /// <param name="pageId">Identifies page for which to retrieve A/B variant.</param>
        /// <seealso cref="SetPersistentVariantIdentifier(int, Guid)"/>
        public static Guid? GetPersistentVariantIdentifier(int pageId)
        {
            return HelperObject.GetPersistentVariantIdentifierInternal(pageId);
        }


        /// <summary>
        /// Removes persisted variant identifier for given <paramref name="pageId"/>.
        /// </summary>
        /// <seealso cref="SetPersistentVariantIdentifier(int, Guid)"/>
        public static void RemovePersistentVariantIdentifier(int pageId)
        {
            HelperObject.RemovePersistentVariantIdentifierInternal(pageId);
        }


        /// <summary>
        /// Persists identifier of the A/B variant to be displayed in Pages UI for given node.
        /// </summary>
        /// <param name="pageId">Identifies page for which to persist A/B variant.</param>
        /// <param name="targetVariant">A/B variant identifier that should be persisted for the usage in the UI for given <paramref name="pageId"/>.</param>
        /// <seealso cref="GetPersistentVariantIdentifierInternal(int)"/>
        protected virtual void SetPersistentVariantIdentifierInternal(int pageId, Guid targetVariant)
        {
            SessionHelper.SetValue($"{AB_TEST_UI_SESSION_KEY_PREFIX}{pageId}", targetVariant, true);
        }


        /// <summary>
        /// Gets identifier of the A/B variant (original is considered to be an A/B variant as well) persisted for usage in the UI for given node.
        /// If null then no variant was persisted for the given node.
        /// </summary>
        /// <param name="pageId">Identifies page for which to retrieve A/B variant.</param>
        /// <seealso cref="SetPersistentVariantIdentifierInternal(int, Guid)"/>
        protected virtual Guid? GetPersistentVariantIdentifierInternal(int pageId)
        {
            return SessionHelper.GetValue($"{AB_TEST_UI_SESSION_KEY_PREFIX}{pageId}") as Guid?;
        }


        /// <summary>
        /// Removes persisted variant identifier for given <paramref name="pageId"/>.
        /// </summary>
        /// <seealso cref="SetPersistentVariantIdentifier(int, Guid)"/>
        protected virtual void RemovePersistentVariantIdentifierInternal(int pageId)
        {
            SessionHelper.Remove($"{AB_TEST_UI_SESSION_KEY_PREFIX}{pageId}");
        }
    }
}
