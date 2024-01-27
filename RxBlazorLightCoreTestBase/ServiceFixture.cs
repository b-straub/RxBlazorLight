
using RxBlazorLightCore;
using static RxBlazorLightCoreTestBase.ServiceFixture.IntListVP;

namespace RxBlazorLightCoreTestBase
{
    public record CRUDTest(string Item, Guid Id);

    public partial class ServiceFixture : RxBLService
    {
        public IState ServiceState { get; }
        public IState<int> IntState { get; }

        public IState<int> IntStateAsyncX { get; }

        public IState<IEnumerable<CRUDTest>, IList<CRUDTest>> CRUDListState { get; }

        public IValueProviderVoid<int> Increment { get; }
        public IValueProvider<int> Add { get; }
        public IStateProvider<string> ChangeTest { get; }
        public IStateProvider ChangeTestSync { get; }

        public IValueProvider<(CMD CMD, CRUDTest? ITEM)> CRUDListCmds { get; }

        public string Test { get; private set; } = string.Empty;

        public ServiceFixture() 
        {
            ServiceState = this.CreateState();
            IntState = this.CreateState(-1);

            IntStateAsyncX = this.CreateState(10, s => new AsyncIntX(this, s));

            CRUDListState = this.CreateState<ServiceFixture, IEnumerable<CRUDTest>, IList<CRUDTest>>(new List<CRUDTest>());
            CRUDListCmds = new IntListVP(this, CRUDListState);

            Increment = new IncremementVP(this, IntState);
            Add = new AddVP(this, IntState);
            ChangeTest = new ChangeTestSP(this, ServiceState);
            ChangeTestSync = new ChangeTestSyncSP(this, ServiceState);
        }

        public void ClearTest()
        {
            Test = string.Empty;
        }
    }
}
