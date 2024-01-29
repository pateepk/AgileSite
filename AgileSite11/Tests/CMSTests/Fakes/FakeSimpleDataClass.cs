using CMS.DataEngine;

namespace CMS.Tests
{
    /// <summary>
    /// Data class for testing purpose.
    /// </summary>
    /// <remarks>
    /// Doesn't modify the database.
    /// </remarks>
    internal sealed class FakeSimpleDataClass : SimpleDataClass
    {
        public FakeSimpleDataClass()
            : base(true)
        {
        }


        public override void Insert(bool initId = true)
        {

        }


        public override void Update()
        {

        }


        public override void Upsert(WhereCondition existingWhere)
        {
            
        }


        public override void Delete(bool preserveData = false)
        {

        }
    }
}