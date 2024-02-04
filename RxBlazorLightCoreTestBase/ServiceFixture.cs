
using RxBlazorLightCore;
using static RxBlazorLightCoreTestBase.ServiceFixture.IntListVP;

namespace RxBlazorLightCoreTestBase
{
    public record CRUDTest(string Item, Guid Id);

    public partial class ServiceFixture : RxBLService
    {
        public IState<int> IntState { get; }

        public IState<int> IntStateAsyncX { get; }

        public IState<IEnumerable<CRUDTest>, IList<CRUDTest>> CRUDListState { get; }

        public IStateProvider<int> Increment { get; }
        public IStateTransformer<int> Add { get; }
        public IServiceStateProvider<string> ChangeTest { get; }
        public IServiceStateProvider ChangeTestSync { get; }

        public IStateTransformer<(CMD CMD, CRUDTest? ITEM)> CRUDListCmds { get; }

        public string Test { get; private set; } = string.Empty;

        public ServiceFixture() 
        {
            IntState = this.CreateState(-1);

            IntStateAsyncX = this.CreateState(10, s => new AsyncIntX(this, s));

            CRUDListState = this.CreateState<ServiceFixture, IEnumerable<CRUDTest>, IList<CRUDTest>>(new List<CRUDTest>());
            CRUDListCmds = new IntListVP(this, CRUDListState);

            Increment = new IncremementVP(this, IntState);
            Add = new AddVP(this, IntState);
            ChangeTest = new ChangeTestSP(this);
            ChangeTestSync = new ChangeTestSyncSP(this);
        }

        public void ClearTest()
        {
            Test = string.Empty;
        }
    }
}
