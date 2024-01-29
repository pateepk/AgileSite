namespace CMS.Personas
{
    /// <summary>
    /// Simplifies creating implementations of operations on personas and nodes.
    /// </summary>
    public static class MultipleDocumentsActionFactory
    {
        /// <summary>
        /// Gets implementation of the specified mass action.
        /// </summary>
        /// <param name="massActionType">Specifies which mass action will be created</param>
        /// <returns>Mass action implementation</returns>
        public static IMultipleDocumentsAction GetActionImplementation(MultipleDocumentsActionTypeEnum massActionType)
        {
            switch (massActionType)
            {
                case MultipleDocumentsActionTypeEnum.Tag:
                    return new MultipleDocumentsTagger();
                case MultipleDocumentsActionTypeEnum.Untag:
                    return new MultipleDocumentsUntagger();
            }

            return null;
        }
    }
}