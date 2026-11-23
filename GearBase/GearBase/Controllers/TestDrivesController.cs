using GearBase.DAO;
using GearBase.Mapeamento;
using Microsoft.AspNetCore.Mvc;

namespace GearBase.Controllers
{
    [Route("api/test-drives")]
    public class TestDrivesController : CrudController<TestDrive>
    {
        public TestDrivesController() : base(new TestDriveDAO()) { }

        protected override void DefinirId(TestDrive objeto, int id)
        {
            objeto.TestDriveId = id;
        }
    }
}
